using UnityEngine;

namespace SubMonitor.App.Config
{
    public static class ApiConfig
    {
        public const string DefaultBaseUrl = "http://31.29.180.7:5173";
        public const string ApiPrefix = "/api/v1";
        public const int RequestTimeoutSeconds = 20;
        private const string BaseUrlOverrideKey = "submonitor.api.base_url";

        public static string BaseUrl
        {
            get
            {
                string overridden = PlayerPrefs.GetString(BaseUrlOverrideKey, string.Empty);
                return string.IsNullOrWhiteSpace(overridden) ? DefaultBaseUrl : overridden;
            }
        }

        public static string BuildUrl(string relativePath)
        {
            string normalizedBaseUrl = BaseUrl.TrimEnd('/');
            string normalizedPath = relativePath.StartsWith("/") ? relativePath : "/" + relativePath;
            return normalizedBaseUrl + normalizedPath;
        }

        public static void OverrideBaseUrl(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                PlayerPrefs.DeleteKey(BaseUrlOverrideKey);
            }
            else
            {
                PlayerPrefs.SetString(BaseUrlOverrideKey, baseUrl.Trim());
            }

            PlayerPrefs.Save();
        }
    }
}
