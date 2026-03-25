using SubMonitor.Auth.Models;

namespace SubMonitor.Auth.Services
{
    public interface IAuthService
    {
        void Login(AuthCredentials credentials);
        void Register(RegistrationData registrationData);
    }
}
