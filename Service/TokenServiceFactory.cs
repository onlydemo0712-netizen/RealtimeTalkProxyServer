using AetherCore.Utility;
using AetherCore.Utility.Attributes;
using AetherCore.Utility.JWT;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Service.Interface;

namespace Service
{
    [AutoInject(ServiceLifetime.Scoped)]
    public class TokenServiceFactory : ITokenServiceFactory
    {
        private IOptionsMonitor<JwtOptions> _jwtOptionsMonitor;
        public TokenServiceFactory(IOptionsMonitor<JwtOptions> jwtOptionsMonitor)
        {
            _jwtOptionsMonitor = jwtOptionsMonitor;
        }
        
        public ITokenService Create(string optionName)
        {
            JwtOptions jwtOptions = _jwtOptionsMonitor.Get(optionName); // 確保有對應的設定存在

            return new JwtGenerator(jwtOptions.Secret, jwtOptions.Issuer, jwtOptions.Audience);
        }
    }
}
