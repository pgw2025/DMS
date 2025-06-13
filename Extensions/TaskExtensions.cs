namespace PMSWPF.Extensions;

public static class TaskExtensions
{

    public  static async Task Await(this Task task,Action<Exception> onError=null,Action onComplete=null)
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
    
}