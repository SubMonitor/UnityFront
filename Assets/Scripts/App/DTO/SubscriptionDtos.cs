using System;

namespace SubMonitor.App.DTO
{
    [Serializable]
    public sealed class SubscriptionRequestDto
    {
        public string name;
        public float cost;
        public string billing_cycle;
        public string payment_date;
        public bool is_next_date;
        public string category;
        public string comment;
    }

    [Serializable]
    public sealed class SubscriptionUpdateRequestDto
    {
        public string name;
        public float cost;
        public string billing_cycle;
        public string payment_date;
        public bool is_next_date;
        public string category;
        public string comment;
        public bool is_active;
    }

    [Serializable]
    public sealed class SubscriptionDto
    {
        public int id;
        public int user_id;
        public string name;
        public float cost;
        public string billing_cycle;
        public string last_payment_date;
        public string next_payment_date;
        public string category;
        public string comment;
        public bool is_active;
        public string created_at;
    }

    [Serializable]
    public sealed class SubscriptionUsageLogRequestDto
    {
        public int subscription_id;
        public string signal;
        public string note;
    }

    [Serializable]
    public sealed class SubscriptionInsightsSummaryDto
    {
        public int total_subscriptions;
        public int active_subscriptions;
        public float monthly_total;
        public float yearly_total;
        public float upcoming_30_days_total;
        public float savings_opportunity_total;
        public int needs_attention_count;
    }

    [Serializable]
    public sealed class SubscriptionCategorySpendDto
    {
        public string category;
        public float monthly_cost;
        public float yearly_cost;
        public int subscriptions_count;
        public float share_percent;
    }

    [Serializable]
    public sealed class SubscriptionForecastPointDto
    {
        public string month;
        public float total_cost;
        public int charges_count;
    }

    [Serializable]
    public sealed class SubscriptionUpcomingChargeDto
    {
        public int subscription_id;
        public string service_name;
        public string charge_date;
        public int days_left;
        public float cost;
        public string billing_cycle;
    }

    [Serializable]
    public sealed class SubscriptionAlertDto
    {
        public string code;
        public string severity;
        public int subscription_id;
        public string service_name;
        public string title;
        public string message;
        public string recommended_action;
        public float potential_savings;
    }

    [Serializable]
    public sealed class SubscriptionRecommendationDto
    {
        public int subscription_id;
        public string service_name;
        public string category;
        public string reason;
        public string[] alternative_services;
        public float estimated_yearly_savings;
        public string action_hint;
    }

    [Serializable]
    public sealed class SubscriptionUsageStatusDto
    {
        public int subscription_id;
        public string service_name;
        public string status;
        public string status_label;
        public int usage_score;
        public string last_signal;
        public string last_recorded_at;
        public float monthly_cost;
        public string recommended_action;
    }

    [Serializable]
    public sealed class SubscriptionInsightsDto
    {
        public SubscriptionInsightsSummaryDto summary;
        public SubscriptionCategorySpendDto[] category_breakdown;
        public SubscriptionForecastPointDto[] yearly_forecast;
        public SubscriptionUpcomingChargeDto[] upcoming_charges;
        public SubscriptionAlertDto[] alerts;
        public SubscriptionRecommendationDto[] recommendations;
        public SubscriptionUsageStatusDto[] usage_reviews;
    }

    [Serializable]
    public sealed class SubscriptionActionPlanDto
    {
        public int subscription_id;
        public string service_name;
        public string action;
        public string subject;
        public string body;
        public string copy_text;
        public string copy_hint;
        public string[] steps;
    }
}
