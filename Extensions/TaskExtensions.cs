namespace PMSWPF.Extensions;

public static class TaskExtensions
{
    public static async Task Await(this Task task, Action<Exception> onError = null, Action onComplete = null)
    {
        try
        {
            await task;
            onComplete?.Invoke();
        }
        catch (Exception e)
        {
            onError?.Invoke(e);
        }
    }
    public static async Task Await<T>(this Task<T> task, Action<Exception> onError = null, Action<T> onComplete = null)
    {
        try
        {
            T res= await task;
            onComplete?.Invoke(res);
        }
        catch (Exception e)
        {
            onError?.Invoke(e);
        }
    }
}