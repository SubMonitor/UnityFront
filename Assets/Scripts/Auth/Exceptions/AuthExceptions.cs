using System;

namespace SubMonitor.Auth.Exceptions
{
    public abstract class AuthException : Exception
    {
        protected AuthException(string message) : base(message)
        {
        }
    }

    public abstract class AuthValidationException : AuthException
    {
        public string FieldName { get; }

        protected AuthValidationException(string fieldName, string message) : base(message)
        {
            FieldName = fieldName;
        }
    }

    public sealed class EmptyFieldException : AuthValidationException
    {
        public EmptyFieldException(string fieldName) : base(fieldName, $"Поле '{fieldName}' не может быть пустым.")
        {
        }
    }

    public sealed class InvalidEmailException : AuthValidationException
    {
        public InvalidEmailException(string email) : base("Email", $"Некорректный email: {email}")
        {
        }
    }

    public sealed class WeakPasswordException : AuthValidationException
    {
        public WeakPasswordException() : base("Password", "Пароль должен быть не короче 6 символов и содержать буквы и цифры.")
        {
        }
    }

    public sealed class PasswordMismatchException : AuthValidationException
    {
        public PasswordMismatchException() : base("ConfirmPassword", "Пароль и подтверждение пароля не совпадают.")
        {
        }
    }

    public abstract class AuthServiceException : AuthException
    {
        protected AuthServiceException(string message) : base(message)
        {
        }
    }

    public sealed class UserAlreadyExistsException : AuthServiceException
    {
        public UserAlreadyExistsException(string email) : base($"Пользователь с email '{email}' уже существует.")
        {
        }
    }

    public sealed class InvalidCredentialsException : AuthServiceException
    {
        public InvalidCredentialsException() : base("Неверный email или пароль.")
        {
        }
    }
}
