using System;

namespace SubMonitor.SubscriptionsUI
{
    [Serializable]
    public class SubscriptionItem
    {
        public string Title;
        public string ExpireDate;
        public bool IsExpanded;

        public SubscriptionItem(string title, string expireDate, bool isExpanded = false)
        {
            Title = title;
            ExpireDate = expireDate;
            IsExpanded = isExpanded;
        }
    }
}
