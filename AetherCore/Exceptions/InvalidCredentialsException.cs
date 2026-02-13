using AetherCore.Utility.Exceptions;

namespace AetherCore.Exceptions
{
    public class InvalidCredentialsException : CustomException
    {
        public InvalidCredentialsException()
            : base($"Invalid account or password.") { }
    }
}
