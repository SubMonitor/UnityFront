using System.Threading.Tasks;
using SubMonitor.App.Config;
using SubMonitor.App.Core;
using SubMonitor.App.DTO;

namespace SubMonitor.App.Services
{
    public sealed class SubscriptionApiService
    {
        private readonly ApiClient _apiClient;

        public SubscriptionApiService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public Task<ApiResult<SubscriptionDto[]>> GetAllAsync(int offset = 0, int limit = 100)
        {
            return _apiClient.GetArrayAsync<SubscriptionDto>(
                ApiConfig.ApiPrefix + "/subs/get/all/" + offset + "/" + limit);
        }

        public Task<ApiResult<SubscriptionDto[]>> GetActiveAsync()
        {
            return _apiClient.GetArrayAsync<SubscriptionDto>(ApiConfig.ApiPrefix + "/subs/get/active");
        }

        public Task<ApiResult<SubscriptionDto>> AddAsync(SubscriptionRequestDto request)
        {
            return _apiClient.PostJsonAsync<SubscriptionRequestDto, SubscriptionDto>(
                ApiConfig.ApiPrefix + "/subs/add",
                request);
        }

        public Task<ApiResult<SubscriptionDto>> UpdateAsync(int subscriptionId, SubscriptionUpdateRequestDto request)
        {
            return _apiClient.PutJsonAsync<SubscriptionUpdateRequestDto, SubscriptionDto>(
                ApiConfig.ApiPrefix + "/subs/update/" + subscriptionId,
                request);
        }

        public Task<ApiResult> DeleteAsync(int subscriptionId)
        {
            return _apiClient.DeleteAsync(ApiConfig.ApiPrefix + "/subs/delete/" + subscriptionId);
        }

        public Task<ApiResult<bool>> SetActiveAsync(int subscriptionId, bool isActive)
        {
            return _apiClient.GetBooleanAsync(
                ApiConfig.ApiPrefix + "/subs/setactive/" + subscriptionId + "/" + isActive.ToString().ToLowerInvariant());
        }

        public Task<ApiResult<string[]>> GetCategoriesAsync()
        {
            return _apiClient.GetArrayAsync<string>(ApiConfig.ApiPrefix + "/subs/categories");
        }

        public Task<ApiResult<SubscriptionInsightsDto>> GetInsightsAsync()
        {
            return _apiClient.GetAsync<SubscriptionInsightsDto>(ApiConfig.ApiPrefix + "/subs/insights");
        }

        public Task<ApiResult<SubscriptionUsageStatusDto>> RecordUsageAsync(SubscriptionUsageLogRequestDto request)
        {
            return _apiClient.PostJsonAsync<SubscriptionUsageLogRequestDto, SubscriptionUsageStatusDto>(
                ApiConfig.ApiPrefix + "/subs/usage",
                request);
        }

        public Task<ApiResult<SubscriptionActionPlanDto>> GetActionPlanAsync(int subscriptionId, string action = "pause")
        {
            return _apiClient.GetAsync<SubscriptionActionPlanDto>(
                ApiConfig.ApiPrefix + "/subs/" + subscriptionId + "/action-plan?action=" + action);
        }
    }
}
