namespace Common.DTO.Auth
{
    public class LoginResponse
    {
        public bool Success { get; set; }   = false;
        public string Token { get; set; }   = "";        
    }
}
