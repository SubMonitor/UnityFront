using System;

namespace SubMonitor.App.DTO
{
    [Serializable]
    public sealed class RegisterRequestDto
    {
        public string email;
        public string password;
        public string first_name;
        public string last_name;
        public string patronymic;
    }

    [Serializable]
    public sealed class LoginRequestDto
    {
        public string email;
        public string password;
    }

    [Serializable]
    public sealed class RegisterResponseEnvelopeDto
    {
        public string message;
        public RegisterResponseDto data;
    }

    [Serializable]
    public sealed class RegisterResponseDto
    {
        public int user_id;
        public string email;
        public string error;
    }

    [Serializable]
    public sealed class TokenResponseDto
    {
        public string access_token;
        public string token_type;
    }

    [Serializable]
    public sealed class UserProfileDto
    {
        public int id;
        public string email;
        public string first_name;
        public string last_name;
        public string patronymic;
        public string created_at;
    }
}
