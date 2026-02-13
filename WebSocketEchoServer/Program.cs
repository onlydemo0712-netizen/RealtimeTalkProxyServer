using Common.DTO.Auth;
using Common.Setting;
using ElderAIServer.Convention;
using Microsoft.OpenApi.Models;
using OpenAIProxyService.Controllers;
using OpenAIProxyService.Extens;
using OpenAIProxyService.Extension;
using OpenAIProxyService.Websockets;
using System.Security.Cryptography;
using System.Text;
using AetherCore.Settings;
using AetherCore.Utility;
using AetherCore.Utility.Filter;
using AetherCore.WebSockets;

var builder = WebApplication.CreateBuilder(args);

/********************************************
 * 加上 Filter 
 * ******************************************/
builder.Services.AddExceptionHandling<ProxyServiceErrorFilter>();

/********************************************
 * 註冊AutoMapper
 * ******************************************/
// 直接指定 Profile
builder.Services.AddAutoMapper(
    cfg => cfg.AddMaps(typeof(AuthProfile).Assembly),
    typeof(AuthProfile).Assembly
);

builder.Services.AddAutoMapperProfiles(LoggerFactory.Create(builder =>
{
    builder.AddConsole(); // 或其他你需要的設定
}));

/********************************************
 * 加上Database Settings
 * ******************************************/
//builder.Services.InitialMongoDB(builder.Configuration);
await builder.Services.InitialMongoDBEntity(builder.Configuration);

/********************************************
 * 設定記憶體快取
 * ******************************************/
builder.Services.AddCacheSettings(builder.Configuration);

/********************************************
 * register & Inject
 * ******************************************/
// 自動註冊 AppSettings，掃描指定前綴的assembly，找到帶有 AppSettingsAttribute 的類別並註冊
builder.Services.AutoRegisterAppSettings(builder.Configuration, new[] { "Common"});
// 自動註冊服務，掃描指定前綴的assembly，找到帶有 AutoInjectAttribute 的類別並註冊
builder.Services.AutoInject(new[] { "DataAccess", "Repository", "Service", "ElderAIServer" });
// 手動註冊服務
builder.Services.SlaveServices();

/********************************************
 * 註冊 JWT 認證
 * ******************************************/

JwtOptions adminJwtOptions = new JwtOptions
{
    JwtName                     = "AdminJwt",
    ValidateIssuer              = true,
    ValidateAudience            = true,
    ValidateLifetime            = true,
    ValidateIssuerSigningKey    = true,
    Issuer                      = builder.Configuration["Jwt:Admin:Issuer"],
    Audience                    = builder.Configuration["Jwt:Admin:Audience"],
    Secret                      = builder.Configuration["Jwt:Admin:Secret"]
};

JwtOptions appJwtOptions = new JwtOptions
{
    JwtName                     = "AppJwt",    
    ValidateIssuer              = true,
    ValidateAudience            = true,
    ValidateLifetime            = true,
    ValidateIssuerSigningKey    = true,
    Issuer                      = builder.Configuration["Jwt:App:Issuer"],
    Audience                    = builder.Configuration["Jwt:App:Audience"],
    Secret                      = builder.Configuration["Jwt:App:Secret"]
};

// 第一個 JwtOptions 為預設方案
var jwtOptions = new List<JwtOptions>
{
    appJwtOptions,
    adminJwtOptions
};

builder.Services.AddJwtAuthentication(jwtOptions);

/********************************************
 * 加上Controller
 * ******************************************/
builder.Services.AddControllers(o =>
{
    o.Filters.Add<DisabledApiFilter>();

    o.Conventions.Add(new AutoAuthorize());             // 使用 ListAuthorizeConvention 來自動套用 Authorize
    o.Conventions.Add(new AutoDisableApi());            // 搭配 DisabledApiFilter 來開關API
});

/********************************************
 * Swagger 設定
 * ******************************************/
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.GenerateSwaggerDoc(builder.Environment);                  // 建立Swagger分頁 與API 要出現的分頁

    c.OperationFilter<ControllerAddSummaryFilter>();            // 使用 Operation Filter 來給API加上註解
    c.OperationFilter<EndpointMetadataAuthorizeLockFilter>();   // 讓每個 API 依照授權(AutoAuthorize)需求自動帶出鎖頭

    foreach (var option in jwtOptions)
    {
        // 加入 JWT 認證設定
        c.AddSecurityDefinition(option.JwtName, new OpenApiSecurityScheme
        {
            Type            = SecuritySchemeType.Http,
            Scheme          = "bearer",
            BearerFormat    = "JWT",
            In              = ParameterLocation.Header,
            Description     = "請輸入 " + option.JwtName + " 的 JWT",
        });
    }    
});

var app = builder.Build();

/***********************************************************************************************
* 啟用 WebSockets，設置保活間隔與收包緩衝
************************************************************************************************/
var wsOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(20),
    ReceiveBufferSize = 64 * 1024
};

app.UseWebSockets(wsOptions);

// WebSocket 端點：/ws
app.Map<OpenAIProxyWsHub>("/ws");
app.MapGet("/", () => "WS backend running").ExcludeFromDescription();

/***********************************************************************************************
* 啟用 Swagger
************************************************************************************************/
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoints();
    });
}

/***********************************************************************************************
* 測試
************************************************************************************************/
var key = Environment.GetEnvironmentVariable("OpenAISettings__ApiKey")
          ?? builder.Configuration["OpenAISettings:ApiKey"];

if (string.IsNullOrWhiteSpace(key))
{
    app.Logger.LogError("OpenAI key missing (null/empty).");
}
else
{
    var safePreview = key.Length <= 6 ? key : key[..6];
    app.Logger.LogInformation("OpenAI key loaded. Length={Len}, Prefix={Prefix}***", key.Length, safePreview);

    var hash = Utils.ComputeSha256Hash(key);
    app.Logger.LogInformation("OpenAI key SHA256 = {Hash}", hash);
}

/***********************************************************************************************
* 其他
************************************************************************************************/
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();              // 先認證身分
app.UseAuthorization();               // 再授權權限

app.MapControllers();

await app.RunAsync();