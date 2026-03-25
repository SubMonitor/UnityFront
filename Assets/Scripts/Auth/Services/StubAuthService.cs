using System;
using System.Collections.Generic;
using SubMonitor.Auth.Exceptions;
using SubMonitor.Auth.Models;
using SubMonitor.Auth.Validation;

namespace SubMonitor.Auth.Services
{
    public sealed class StubAuthService : IAuthService
    {
        private static readonly Dictionary<string, string> Users = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "demo@strelka.app", "Demo123" }
        };

        public void Login(AuthCredentials credentials)
        {
            AuthDataValidator.ValidateLogin(credentials);

            string normalizedEmail = credentials.Email.Trim();
            if (!Users.TryGetValue(normalizedEmail, out string savedPassword))
            {
                throw new InvalidCredentialsException();
            }

            if (!string.Equals(savedPassword, credentials.Password, StringComparison.Ordinal))
            {
                throw new InvalidCredentialsException();
            }
        }

        public void Register(RegistrationData registrationData)
        {
            AuthDataValidator.ValidateRegistration(registrationData);

            string normalizedEmail = registrationData.Email.Trim();
            if (Users.ContainsKey(normalizedEmail))
            {
                throw new UserAlreadyExistsException(normalizedEmail);
            }

            // Заглушка хранилища: в реальном проекте здесь должна быть запись в backend/БД.
            Users[normalizedEmail] = registrationData.Password;
        }
    }
}
