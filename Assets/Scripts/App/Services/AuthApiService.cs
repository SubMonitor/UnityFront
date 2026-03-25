using System.Collections.Generic;
using System.Threading.Tasks;
using SubMonitor.App.Config;
using SubMonitor.App.Core;
using SubMonitor.App.DTO;
using SubMonitor.App.Session;

namespace SubMonitor.App.Services
{
    public sealed class AuthApiService
    {
        private readonly ApiClient _apiClient;
        private readonly SessionStore _sessionStore;

        public AuthApiService(ApiClient apiClient, SessionStore sessionStore)
        {
            _apiClient = apiClient;
            _sessionStore = sessionStore;
        }

        public async Task<ApiResult<RegisterResponseDto>> RegisterAsync(RegisterRequestDto request)
        {
            ApiResult<RegisterResponseEnvelopeDto> result =
                await _apiClient.PostJsonAsync<RegisterRequestDto, RegisterResponseEnvelopeDto>(
                    ApiConfig.ApiPrefix + "/auth/reg",
                    request,
                    false);

            if (!result.IsSuccess)
            {
                return ApiResult<RegisterResponseDto>.Failure(result.FailureKind, result.ErrorMessage, result.StatusCode, result.ValidationErrors);
            }

            if (result.Data == null || result.Data.data == null)
            {
                return ApiResult<RegisterResponseDto>.Failure(ApiFailureKind.Empty, "Сервер не вернул данные регистрации.", result.StatusCode);
            }

            if (!string.IsNullOrWhiteSpace(result.Data.data.error))
            {
                return ApiResult<RegisterResponseDto>.Failure(ApiFailureKind.Validation, result.Data.data.error, result.StatusCode);
            }

            return ApiResult<RegisterResponseDto>.Success(result.Data.data, result.StatusCode);
        }

        public async Task<ApiResult<TokenResponseDto>> LoginAsync(LoginRequestDto request)
        {
            var form = new Dictionary<string, string>
            {
                { "username", request.email ?? string.Empty },
                { "password", request.password ?? string.Empty }
            };

            ApiResult<TokenResponseDto> result =
                await _apiClient.PostFormAsync<TokenResponseDto>(ApiConfig.ApiPrefix + "/auth/login", form);

            if (result.IsSuccess && result.Data != null)
            {
                _sessionStore.SaveSession(result.Data.access_token, request.email);
            }

            return result;
        }

        public Task<ApiResult<UserProfileDto>> GetProfileAsync()
        {
            return _apiClient.GetAsync<UserProfileDto>(ApiConfig.ApiPrefix + "/me");
        }

        public void Logout()
        {
            _sessionStore.Clear();
        }
    }
}
