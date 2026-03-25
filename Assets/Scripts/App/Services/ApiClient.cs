using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SubMonitor.App.Config;
using SubMonitor.App.Core;
using SubMonitor.App.Session;
using UnityEngine;
using UnityEngine.Networking;

namespace SubMonitor.App.Services
{
    public sealed class ApiClient
    {
        private readonly SessionStore _sessionStore;

        public ApiClient(SessionStore sessionStore)
        {
            _sessionStore = sessionStore;
        }

        public Task<ApiResult<TResponse>> GetAsync<TResponse>(string path, bool authorize = true)
        {
            return SendAsync(path, UnityWebRequest.kHttpVerbGET, null, authorize, JsonHelper.FromJson<TResponse>);
        }

        public Task<ApiResult<bool>> GetBooleanAsync(string path, bool authorize = true)
        {
            return SendAsync(path, UnityWebRequest.kHttpVerbGET, null, authorize, ParseBoolean);
        }

        public Task<ApiResult<TResponse[]>> GetArrayAsync<TResponse>(string path, bool authorize = true)
        {
            return SendAsync(path, UnityWebRequest.kHttpVerbGET, null, authorize, JsonHelper.FromJsonArray<TResponse>);
        }

        public Task<ApiResult<TResponse>> PostJsonAsync<TRequest, TResponse>(string path, TRequest payload, bool authorize = true)
        {
            string json = JsonHelper.ToJson(payload);
            return SendAsync(path, UnityWebRequest.kHttpVerbPOST, json, authorize, JsonHelper.FromJson<TResponse>);
        }

        public Task<ApiResult<TResponse>> PutJsonAsync<TRequest, TResponse>(string path, TRequest payload, bool authorize = true)
        {
            string json = JsonHelper.ToJson(payload);
            return SendAsync(path, UnityWebRequest.kHttpVerbPUT, json, authorize, JsonHelper.FromJson<TResponse>);
        }

        public async Task<ApiResult<TResponse>> PostFormAsync<TResponse>(
            string path,
            IDictionary<string, string> formFields,
            bool authorize = false)
        {
            var form = new WWWForm();
            foreach (KeyValuePair<string, string> entry in formFields)
            {
                form.AddField(entry.Key, entry.Value ?? string.Empty);
            }

            using (var request = UnityWebRequest.Post(ApiConfig.BuildUrl(path), form))
            {
                request.timeout = ApiConfig.RequestTimeoutSeconds;
                if (authorize)
                {
                    ApplyAuthorizationHeader(request);
                }

                await request.SendAsync();
                return ParseResponse(request, JsonHelper.FromJson<TResponse>);
            }
        }

        public async Task<ApiResult> DeleteAsync(string path, bool authorize = true)
        {
            using (var request = UnityWebRequest.Delete(ApiConfig.BuildUrl(path)))
            {
                request.timeout = ApiConfig.RequestTimeoutSeconds;
                if (authorize)
                {
                    ApplyAuthorizationHeader(request);
                }

                await request.SendAsync();

                if (IsSuccessStatusCode(request.responseCode))
                {
                    return ApiResult.Success(request.responseCode);
                }

                return ApiResult.Failure(
                    MapFailureKind(request),
                    BuildErrorMessage(request),
                    request.responseCode,
                    JsonHelper.ExtractValidationMessages(request.downloadHandler != null ? request.downloadHandler.text : string.Empty));
            }
        }

        private async Task<ApiResult<TResponse>> SendAsync<TResponse>(
            string path,
            string method,
            string jsonBody,
            bool authorize,
            Func<string, TResponse> parser)
        {
            using (var request = new UnityWebRequest(ApiConfig.BuildUrl(path), method))
            {
                request.timeout = ApiConfig.RequestTimeoutSeconds;
                request.downloadHandler = new DownloadHandlerBuffer();

                if (!string.IsNullOrWhiteSpace(jsonBody))
                {
                    byte[] payloadBytes = Encoding.UTF8.GetBytes(jsonBody);
                    request.uploadHandler = new UploadHandlerRaw(payloadBytes);
                    request.SetRequestHeader("Content-Type", "application/json");
                }

                request.SetRequestHeader("Accept", "application/json");

                if (authorize)
                {
                    ApplyAuthorizationHeader(request);
                }

                await request.SendAsync();
                return ParseResponse(request, parser);
            }
        }

        private ApiResult<TResponse> ParseResponse<TResponse>(UnityWebRequest request, Func<string, TResponse> parser)
        {
            if (IsSuccessStatusCode(request.responseCode))
            {
                string responseText = request.downloadHandler != null ? request.downloadHandler.text : string.Empty;
                if (string.IsNullOrWhiteSpace(responseText))
                {
                    return ApiResult<TResponse>.Failure(ApiFailureKind.Empty, "Сервер вернул пустой ответ.", request.responseCode);
                }

                try
                {
                    TResponse data = parser(responseText);
                    if (data == null)
                    {
                        return ApiResult<TResponse>.Failure(ApiFailureKind.Empty, "Не удалось прочитать ответ сервера.", request.responseCode);
                    }

                    return ApiResult<TResponse>.Success(data, request.responseCode);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                    return ApiResult<TResponse>.Failure(
                        ApiFailureKind.Unknown,
                        "Сервер ответил в неожиданном формате.",
                        request.responseCode);
                }
            }

            return ApiResult<TResponse>.Failure(
                MapFailureKind(request),
                BuildErrorMessage(request),
                request.responseCode,
                JsonHelper.ExtractValidationMessages(request.downloadHandler != null ? request.downloadHandler.text : string.Empty));
        }

        private void ApplyAuthorizationHeader(UnityWebRequest request)
        {
            string token = _sessionStore.GetAccessToken();
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.SetRequestHeader("Authorization", "Bearer " + token);
            }
        }

        private static bool IsSuccessStatusCode(long responseCode)
        {
            return responseCode >= 200 && responseCode < 300;
        }

        private static ApiFailureKind MapFailureKind(UnityWebRequest request)
        {
            string error = request.error ?? string.Empty;
            if (request.result == UnityWebRequest.Result.ConnectionError || error.IndexOf("resolve", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return error.IndexOf("timed out", StringComparison.OrdinalIgnoreCase) >= 0
                    ? ApiFailureKind.Timeout
                    : ApiFailureKind.NetworkUnavailable;
            }

            if (request.responseCode == 401)
            {
                return ApiFailureKind.Unauthorized;
            }

            if (request.responseCode == 403)
            {
                return ApiFailureKind.Forbidden;
            }

            if (request.responseCode == 404)
            {
                return ApiFailureKind.NotFound;
            }

            if (request.responseCode == 400 || request.responseCode == 409 || request.responseCode == 422)
            {
                return ApiFailureKind.Validation;
            }

            if (request.responseCode >= 500)
            {
                return ApiFailureKind.ServerError;
            }

            return ApiFailureKind.Unknown;
        }

        private static string BuildErrorMessage(UnityWebRequest request)
        {
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                if ((request.error ?? string.Empty).IndexOf("timed out", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "Сервер не ответил вовремя. Попробуйте еще раз.";
                }

                return "Не удалось подключиться к backend API.";
            }

            string responseText = request.downloadHandler != null ? request.downloadHandler.text : string.Empty;
            string extracted = JsonHelper.ExtractErrorMessage(responseText);
            if (!string.IsNullOrWhiteSpace(extracted))
            {
                return extracted;
            }

            if (request.responseCode == 401)
            {
                return "Сессия истекла. Войдите снова.";
            }

            if (request.responseCode == 403)
            {
                return "У вас нет доступа к этой операции.";
            }

            return string.IsNullOrWhiteSpace(request.error) ? "Запрос завершился с ошибкой." : request.error;
        }

        private static bool ParseBoolean(string responseText)
        {
            return string.Equals(responseText.Trim(), "true", StringComparison.OrdinalIgnoreCase);
        }
    }
}
