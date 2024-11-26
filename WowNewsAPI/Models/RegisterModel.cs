namespace WowNewsAPI.Models
{
    public class RegisterModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class AuthResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }

}
