using UnityEngine;

namespace SubMonitor.App.Session
{
    public sealed class SessionStore
    {
        private const string AccessTokenKey = "strelka.session.access_token";
        private const string UserEmailKey = "strelka.session.user_email";

        public bool HasToken()
        {
            return !string.IsNullOrWhiteSpace(GetAccessToken());
        }

        public string GetAccessToken()
        {
            return PlayerPrefs.GetString(AccessTokenKey, string.Empty);
        }

        public string GetUserEmail()
        {
            return PlayerPrefs.GetString(UserEmailKey, string.Empty);
        }

        public void SaveSession(string accessToken, string userEmail)
        {
            PlayerPrefs.SetString(AccessTokenKey, accessToken ?? string.Empty);
            PlayerPrefs.SetString(UserEmailKey, userEmail ?? string.Empty);
            PlayerPrefs.Save();
        }

        public void Clear()
        {
            PlayerPrefs.DeleteKey(AccessTokenKey);
            PlayerPrefs.DeleteKey(UserEmailKey);
            PlayerPrefs.Save();
        }
    }
}
