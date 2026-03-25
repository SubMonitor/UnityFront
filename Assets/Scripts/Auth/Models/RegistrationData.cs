namespace SubMonitor.Auth.Models
{
    public sealed class RegistrationData
    {
        public string Email { get; }
        public string Password { get; }
        public string ConfirmPassword { get; }

        public RegistrationData(string email, string password, string confirmPassword)
        {
            Email = email;
            Password = password;
            ConfirmPassword = confirmPassword;
        }
    }
}
