using System;

namespace SubMonitor.App.DTO
{
    [Serializable]
    public sealed class EmailServersResponseDto
    {
        public EmailServerDto[] servers;
    }

    [Serializable]
    public sealed class EmailServerDto
    {
        public string key;
        public string name;
        public string help_url;
        public bool requires_custom_host;
    }

    [Serializable]
    public sealed class EmailConnectRequestDto
    {
        public string email;
        public string password;
        public string server_key;
        public string custom_host;
        public int custom_port = 993;
    }

    [Serializable]
    public sealed class EmailConnectResponseDto
    {
        public bool success;
        public string message;
        public int account_id;
    }

    [Serializable]
    public sealed class EmailAccountsResponseDto
    {
        public EmailAccountDto[] accounts;
    }

    [Serializable]
    public sealed class EmailAccountDto
    {
        public int id;
        public string email;
        public string server_key;
        public bool is_active;
        public string last_checked_at;
        public string last_error;
        public string created_at;
    }

    [Serializable]
    public sealed class EmailSearchRequestDto
    {
        public string[] keywords;
        public int days_back = 7;
        public string[] folders;
        public int max_emails = 50;
    }

    [Serializable]
    public sealed class EmailSearchResponseDto
    {
        public bool success;
        public string message;
        public int count;
        public EmailPreviewDto[] emails;
    }

    [Serializable]
    public sealed class EmailPreviewDto
    {
        public string uid;
        public string subject;
        public string from;
        public string date;
        public string date_str;
        public string text_preview;
        public string[] matched_keywords;
        public bool has_attachments;
        public string folder;
    }

    [Serializable]
    public sealed class EmailDetailEnvelopeDto
    {
        public bool success;
        public string message;
        public EmailDetailDto email;
    }

    [Serializable]
    public sealed class EmailDetailDto
    {
        public string uid;
        public string subject;
        public string from;
        public string to;
        public string date;
        public string date_str;
        public string text;
        public string html;
        public string text_preview;
        public int size;
        public EmailAttachmentDto[] attachments;
    }

    [Serializable]
    public sealed class EmailAttachmentDto
    {
        public string filename;
        public int size;
        public string content_type;
    }
}
