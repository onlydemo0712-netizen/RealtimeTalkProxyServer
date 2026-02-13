using System.Text.Encodings.Web;
using System.Text.Json;

namespace ElderAIServer.Common.Prompts
{
    public class PromptTemplate
    {
        public string Version { get; set; } = "1.0";
        public JsonElement Template { get; set; }
    }

    public static class PromptTemplateLoader
    {
        private static readonly JsonSerializerOptions _jsonOpt = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling         = JsonCommentHandling.Skip,
            AllowTrailingCommas         = true
        };

        public static PromptTemplate Load(string name)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Prompts", $"{name}.json");
            var json = File.ReadAllText(path);

            var tpl = JsonSerializer.Deserialize<PromptTemplate>(json, _jsonOpt);
            if (tpl == null)
                throw new InvalidOperationException($"Prompt template deserialize failed: {path}");

            // 防呆：避免 JSON 沒有 Template 或為 null
            if (tpl.Template.ValueKind == JsonValueKind.Undefined || tpl.Template.ValueKind == JsonValueKind.Null)
                throw new InvalidOperationException($"Prompt template 'Template' is missing or null: {path}");

            return tpl;
        }
    }

    public static class PromptTemplateApplier
    {
        /// <summary>
        /// 舊版相容：直接用字串模板做替換
        /// </summary>
        public static string Apply(string template, Dictionary<string, string> args)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));
            if (args == null) throw new ArgumentNullException(nameof(args));

            foreach (var kv in args)
                template = template.Replace($"{{{{{kv.Key}}}}}", kv.Value ?? string.Empty);

            return template;
        }

        /// <summary>
        /// 新版：Template 是 JsonElement（JSON object），先轉成 JSON 字串再套變數。
        /// </summary>
        public static string Apply(PromptTemplate tpl, Dictionary<string, string> args, bool indented = true)
        {
            if (tpl == null) throw new ArgumentNullException(nameof(tpl));
            if (args == null) throw new ArgumentNullException(nameof(args));

            var rawJson     = tpl.Template.GetRawText();
            var replaced    = Apply(rawJson, args);

            try
            {
                using var doc = JsonDocument.Parse(replaced);

                var opt = new JsonSerializerOptions
                {
                    WriteIndented   = indented,
                    Encoder         = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // 保留中文，不轉 \uXXXX
                };

                return JsonSerializer.Serialize(doc.RootElement, opt);
            }
            catch
            {
                return replaced;
            }
        }

        /// <summary>
        /// 方便你用：輸出單行 JSON（好存/好傳輸）
        /// </summary>
        public static string ApplyAndMinify(PromptTemplate tpl, Dictionary<string, string> args)
            => Apply(tpl, args, indented: false);

        /// <summary>
        /// 方便你用：輸出漂亮縮排（好 debug）
        /// </summary>
        public static string ApplyAndPretty(PromptTemplate tpl, Dictionary<string, string> args)
            => Apply(tpl, args, indented: true);
    }
}
