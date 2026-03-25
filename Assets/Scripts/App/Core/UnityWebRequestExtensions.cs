using System.Threading.Tasks;
using UnityEngine.Networking;

namespace SubMonitor.App.Core
{
    public static class UnityWebRequestExtensions
    {
        public static Task<UnityWebRequest> SendAsync(this UnityWebRequest request)
        {
            var completionSource = new TaskCompletionSource<UnityWebRequest>();
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            operation.completed += _ => completionSource.TrySetResult(request);
            return completionSource.Task;
        }
    }
}
