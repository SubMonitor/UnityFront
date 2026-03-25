using System.Collections.Generic;

namespace SubMonitor.App.Core
{
    public enum ApiFailureKind
    {
        None,
        NetworkUnavailable,
        Unauthorized,
        Forbidden,
        Validation,
        NotFound,
        Timeout,
        ServerError,
        Empty,
        Unknown
    }

    public sealed class ApiResult
    {
        public bool IsSuccess { get; private set; }
        public ApiFailureKind FailureKind { get; private set; }
        public string ErrorMessage { get; private set; }
        public long StatusCode { get; private set; }
        public IReadOnlyList<string> ValidationErrors { get; private set; }

        public static ApiResult Success(long statusCode = 200)
        {
            return new ApiResult
            {
                IsSuccess = true,
                StatusCode = statusCode,
                FailureKind = ApiFailureKind.None,
                ValidationErrors = new List<string>()
            };
        }

        public static ApiResult Failure(
            ApiFailureKind failureKind,
            string errorMessage,
            long statusCode = 0,
            IReadOnlyList<string> validationErrors = null)
        {
            return new ApiResult
            {
                IsSuccess = false,
                FailureKind = failureKind,
                ErrorMessage = errorMessage,
                StatusCode = statusCode,
                ValidationErrors = validationErrors ?? new List<string>()
            };
        }
    }

    public sealed class ApiResult<T>
    {
        public bool IsSuccess { get; private set; }
        public T Data { get; private set; }
        public ApiFailureKind FailureKind { get; private set; }
        public string ErrorMessage { get; private set; }
        public long StatusCode { get; private set; }
        public IReadOnlyList<string> ValidationErrors { get; private set; }

        public static ApiResult<T> Success(T data, long statusCode = 200)
        {
            return new ApiResult<T>
            {
                IsSuccess = true,
                Data = data,
                StatusCode = statusCode,
                FailureKind = ApiFailureKind.None,
                ValidationErrors = new List<string>()
            };
        }

        public static ApiResult<T> Failure(
            ApiFailureKind failureKind,
            string errorMessage,
            long statusCode = 0,
            IReadOnlyList<string> validationErrors = null)
        {
            return new ApiResult<T>
            {
                IsSuccess = false,
                FailureKind = failureKind,
                ErrorMessage = errorMessage,
                StatusCode = statusCode,
                ValidationErrors = validationErrors ?? new List<string>()
            };
        }
    }
}
