namespace SubMonitor.Auth.Models
{
    public sealed class AuthCredentials
    {
        public string Email { get; }
        public string Password { get; }

        public AuthCredentials(string email, string password)
        {
            Email = email;
            Password = password;
        }
    }
}
