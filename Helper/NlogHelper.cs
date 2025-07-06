using System.Runtime.CompilerServices;
using NLog;

namespace PMSWPF.Helper;

public static class NlogHelper
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    
    public static void Error( string msg,Exception exception=null,[CallerFilePath] string callerFilePath = "",
                              [CallerMemberName] string callerMember = "",
                              [CallerLineNumber] int callerLineNumber = 0)
    {
        // 使用 using 语句确保 MappedDiagnosticsLogicalContext 在作用域结束时被清理
        // 这对于异步方法尤其重要，因为上下文会随着异步操作的流转而传递
        using (MappedDiagnosticsLogicalContext.SetScoped("CallerFilePath", callerFilePath))
        {
            using (MappedDiagnosticsLogicalContext.SetScoped("CallerLineNumber", callerLineNumber))
            {
                using (MappedDiagnosticsLogicalContext.SetScoped("CallerMember", callerMember))
                {
                    Logger.Error(exception,msg);
                } // 当 using 块结束时，"user-id" 和 "transaction-id" 会自动从上下文中移除
            } 
        }
    }
    public static void Info( string msg,[CallerFilePath] string callerFilePath = "",
                             [CallerMemberName] string callerMember = "",
                             [CallerLineNumber] int callerLineNumber = 0)
    {
        // 使用 using 语句确保 MappedDiagnosticsLogicalContext 在作用域结束时被清理
        // 这对于异步方法尤其重要，因为上下文会随着异步操作的流转而传递
        using (MappedDiagnosticsLogicalContext.SetScoped("CallerFilePath", callerFilePath))
        {
            using (MappedDiagnosticsLogicalContext.SetScoped("CallerLineNumber", callerLineNumber))
            {
                using (MappedDiagnosticsLogicalContext.SetScoped("CallerMember", callerMember))
                {
                    Logger.Info(msg);
                } // 当 using 块结束时，"user-id" 和 "transaction-id" 会自动从上下文中移除
            } 
        }
    }
    
    public static void Warn( string msg,[CallerFilePath] string callerFilePath = "",
                             [CallerMemberName] string callerMember = "",
                             [CallerLineNumber] int callerLineNumber = 0)
    {
        // 使用 using 语句确保 MappedDiagnosticsLogicalContext 在作用域结束时被清理
        // 这对于异步方法尤其重要，因为上下文会随着异步操作的流转而传递
        using (MappedDiagnosticsLogicalContext.SetScoped("CallerFilePath", callerFilePath))
        {
            using (MappedDiagnosticsLogicalContext.SetScoped("CallerLineNumber", callerLineNumber))
            {
                using (MappedDiagnosticsLogicalContext.SetScoped("CallerMember", callerMember))
                {
                    Logger.Warn(msg);
                } // 当 using 块结束时，"user-id" 和 "transaction-id" 会自动从上下文中移除
            } 
        }
    }  
    
    public static void Trace( string msg,[CallerFilePath] string callerFilePath = "",
                                [CallerMemberName] string callerMember = "",
                                [CallerLineNumber] int callerLineNumber = 0)
    {
        // 使用 using 语句确保 MappedDiagnosticsLogicalContext 在作用域结束时被清理
        // 这对于异步方法尤其重要，因为上下文会随着异步操作的流转而传递
        using (MappedDiagnosticsLogicalContext.SetScoped("CallerFilePath", callerFilePath))
        {
            using (MappedDiagnosticsLogicalContext.SetScoped("CallerLineNumber", callerLineNumber))
            {
                using (MappedDiagnosticsLogicalContext.SetScoped("CallerMember", callerMember))
                {
                    Logger.Trace(msg);
                } // 当 using 块结束时，"user-id" 和 "transaction-id" 会自动从上下文中移除
            } 
        }
    }
    
}