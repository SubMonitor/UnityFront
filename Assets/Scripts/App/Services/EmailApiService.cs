using System.Threading.Tasks;
using SubMonitor.App.Config;
using SubMonitor.App.Core;
using SubMonitor.App.DTO;

namespace SubMonitor.App.Services
{
    public sealed class EmailApiService
    {
        private readonly ApiClient _apiClient;

        public EmailApiService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResult<EmailServerDto[]>> GetServersAsync()
        {
            ApiResult<EmailServersResponseDto> result =
                await _apiClient.GetAsync<EmailServersResponseDto>(ApiConfig.ApiPrefix + "/email/servers");

            if (!result.IsSuccess)
            {
                return ApiResult<EmailServerDto[]>.Failure(result.FailureKind, result.ErrorMessage, result.StatusCode, result.ValidationErrors);
            }

            return ApiResult<EmailServerDto[]>.Success(result.Data != null ? result.Data.servers : new EmailServerDto[0], result.StatusCode);
        }

        public Task<ApiResult<EmailConnectResponseDto>> ConnectAsync(EmailConnectRequestDto request)
        {
            return _apiClient.PostJsonAsync<EmailConnectRequestDto, EmailConnectResponseDto>(
                ApiConfig.ApiPrefix + "/email/connect",
                request);
        }

        public async Task<ApiResult<EmailAccountDto[]>> GetAccountsAsync()
        {
            ApiResult<EmailAccountsResponseDto> result =
                await _apiClient.GetAsync<EmailAccountsResponseDto>(ApiConfig.ApiPrefix + "/email/accounts");

            if (!result.IsSuccess)
            {
                return ApiResult<EmailAccountDto[]>.Failure(result.FailureKind, result.ErrorMessage, result.StatusCode, result.ValidationErrors);
            }

            return ApiResult<EmailAccountDto[]>.Success(result.Data != null ? result.Data.accounts : new EmailAccountDto[0], result.StatusCode);
        }

        public Task<ApiResult> DeleteAsync(int accountId)
        {
            return _apiClient.DeleteAsync(ApiConfig.ApiPrefix + "/email/accounts/" + accountId);
        }

        public Task<ApiResult<EmailSearchResponseDto>> SearchAsync(int accountId, EmailSearchRequestDto request)
        {
            return _apiClient.PostJsonAsync<EmailSearchRequestDto, EmailSearchResponseDto>(
                ApiConfig.ApiPrefix + "/email/accounts/" + accountId + "/search",
                request);
        }

        public Task<ApiResult<string[]>> GetFoldersAsync(int accountId)
        {
            return _apiClient.GetArrayAsync<string>(ApiConfig.ApiPrefix + "/email/accounts/" + accountId + "/folders");
        }

        public Task<ApiResult<EmailDetailEnvelopeDto>> GetEmailDetailAsync(int accountId, string uid, string folder)
        {
            string encodedUid = System.Uri.EscapeDataString(uid ?? string.Empty);
            string encodedFolder = System.Uri.EscapeDataString(folder ?? "INBOX");
            return _apiClient.GetAsync<EmailDetailEnvelopeDto>(
                ApiConfig.ApiPrefix + "/email/accounts/" + accountId + "/emails/" + encodedUid + "?folder=" + encodedFolder);
        }

        public Task<ApiResult<SubscriptionRequestDto>> ParseSubscriptionAsync(int accountId, string uid, string folder)
        {
            string encodedUid = System.Uri.EscapeDataString(uid ?? string.Empty);
            string encodedFolder = System.Uri.EscapeDataString(folder ?? "INBOX");
            return _apiClient.GetAsync<SubscriptionRequestDto>(
                ApiConfig.ApiPrefix + "/email/accounts/" + accountId + "/emails/" + encodedUid + "/parse?folder=" + encodedFolder);
        }
    }
}
