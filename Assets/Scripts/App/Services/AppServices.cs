using SubMonitor.App.Session;

namespace SubMonitor.App.Services
{
    public sealed class AppServices
    {
        public SessionStore SessionStore { get; }
        public ApiClient ApiClient { get; }
        public AuthApiService Auth { get; }
        public EmailApiService Emails { get; }
        public SubscriptionApiService Subscriptions { get; }

        public AppServices()
        {
            SessionStore = new SessionStore();
            ApiClient = new ApiClient(SessionStore);
            Auth = new AuthApiService(ApiClient, SessionStore);
            Emails = new EmailApiService(ApiClient);
            Subscriptions = new SubscriptionApiService(ApiClient);
        }
    }

    public static class ServiceRegistry
    {
        private static AppServices _current;

        public static AppServices Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new AppServices();
                }

                return _current;
            }
        }

        public static void Reset()
        {
            _current = null;
        }
    }
}
