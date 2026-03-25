using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SubMonitor.App.Core
{
    public static class JsonHelper
    {
        [Serializable]
        private sealed class ArrayWrapper<T>
        {
            public T[] items;
        }

        public static string ToJson<T>(T data)
        {
            return JsonUtility.ToJson(data);
        }

        public static T FromJson<T>(string json)
        {
            return JsonUtility.FromJson<T>(json);
        }

        public static T[] FromJsonArray<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return Array.Empty<T>();
            }

            string wrappedJson = "{\"items\":" + json + "}";
            ArrayWrapper<T> wrapper = JsonUtility.FromJson<ArrayWrapper<T>>(wrappedJson);
            return wrapper != null && wrapper.items != null ? wrapper.items : Array.Empty<T>();
        }

        public static string ExtractErrorMessage(string responseText)
        {
            List<string> messages = ExtractValidationMessages(responseText);
            if (messages.Count > 0)
            {
                return string.Join("\n", messages.Distinct());
            }

            Match detailMatch = Regex.Match(responseText ?? string.Empty, "\"detail\"\\s*:\\s*\"(?<value>.*?)\"");
            if (detailMatch.Success)
            {
                return Unescape(detailMatch.Groups["value"].Value);
            }

            Match messageMatch = Regex.Match(responseText ?? string.Empty, "\"message\"\\s*:\\s*\"(?<value>.*?)\"");
            if (messageMatch.Success)
            {
                return Unescape(messageMatch.Groups["value"].Value);
            }

            return string.IsNullOrWhiteSpace(responseText) ? "Не удалось обработать ответ сервера." : responseText;
        }

        public static List<string> ExtractValidationMessages(string responseText)
        {
            var messages = new List<string>();
            if (string.IsNullOrWhiteSpace(responseText))
            {
                return messages;
            }

            MatchCollection messageMatches = Regex.Matches(responseText, "\"msg\"\\s*:\\s*\"(?<value>.*?)\"");
            foreach (Match match in messageMatches)
            {
                messages.Add(Unescape(match.Groups["value"].Value));
            }

            return messages;
        }

        private static string Unescape(string value)
        {
            string normalized = value.Replace("\\n", "\n");
            return Regex.Unescape(normalized);
        }
    }
}
