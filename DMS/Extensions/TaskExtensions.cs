namespace DMS.Extensions;

/// <summary>
/// 任务扩展类，提供异步任务的扩展方法。
/// </summary>
public static class TaskExtensions
{
    /// <summary>
    /// 等待一个没有返回值的 Task 完成，并提供错误处理和完成时的回调。
    /// </summary>
    /// <param name="task">要等待的 Task。</param>
    /// <param name="onError">发生异常时的回调函数。</param>
    /// <param name="onComplete">任务成功完成时的回调函数。</param>
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

    /// <summary>
    /// 等待一个有返回值的 Task 完成，并提供错误处理和完成时的回调。
    /// </summary>
    /// <typeparam name="T">Task 的返回结果类型。</typeparam>
    /// <param name="task">要等待的 Task。</param>
    /// <param name="onError">发生异常时的回调函数。</param>
    /// <param name="onComplete">任务成功完成时的回调函数，接收任务的返回结果。</param>
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