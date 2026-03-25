using System.Text.RegularExpressions;
using SubMonitor.Auth.Exceptions;
using SubMonitor.Auth.Models;

namespace SubMonitor.Auth.Validation
{
    public static class AuthDataValidator
    {
        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static void ValidateLogin(AuthCredentials credentials)
        {
            EnsureNotEmpty("Email", credentials?.Email);
            EnsureNotEmpty("Password", credentials?.Password);
            ValidateEmail(credentials.Email);
            ValidatePasswordStrength(credentials.Password);
        }

        public static void ValidateRegistration(RegistrationData registrationData)
        {
            EnsureNotEmpty("Email", registrationData?.Email);
            EnsureNotEmpty("Password", registrationData?.Password);
            EnsureNotEmpty("ConfirmPassword", registrationData?.ConfirmPassword);
            ValidateEmail(registrationData.Email);
            ValidatePasswordStrength(registrationData.Password);
            ValidatePasswordsMatch(registrationData.Password, registrationData.ConfirmPassword);
        }

        public static void ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new EmptyFieldException("Email");
            }

            if (!EmailRegex.IsMatch(email.Trim()))
            {
                throw new InvalidEmailException(email);
            }
        }

        public static void ValidatePasswordStrength(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new EmptyFieldException("Password");
            }

            if (password.Length < 6)
            {
                throw new WeakPasswordException();
            }

            bool hasLetter = false;
            bool hasDigit = false;

            for (int i = 0; i < password.Length; i++)
            {
                if (char.IsLetter(password[i]))
                {
                    hasLetter = true;
                }

                if (char.IsDigit(password[i]))
                {
                    hasDigit = true;
                }

                if (hasLetter && hasDigit)
                {
                    return;
                }
            }

            throw new WeakPasswordException();
        }

        public static void ValidatePasswordsMatch(string password, string confirmPassword)
        {
            if (!string.Equals(password, confirmPassword))
            {
                throw new PasswordMismatchException();
            }
        }

        private static void EnsureNotEmpty(string fieldName, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new EmptyFieldException(fieldName);
            }
        }
    }
}
