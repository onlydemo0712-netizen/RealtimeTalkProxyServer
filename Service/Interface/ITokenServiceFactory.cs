using AetherCore.Utility.JWT;

namespace Service.Interface
{
    public interface ITokenServiceFactory
    {
        ITokenService Create(string tokenType);
    }
}
