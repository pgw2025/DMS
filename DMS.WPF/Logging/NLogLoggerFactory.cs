using Microsoft.Extensions.Logging;

namespace DMS.WPF.Logging;

/// <summary>
/// NLog ILoggerFactory实现，用于创建命名的NLogLogger实例
/// 这个工厂类允许通过类别名称创建不同的Logger实例，
/// 从而可以区分不同组件或模块的日志输出
/// </summary>
public class NLogLoggerFactory : ILoggerFactory
{
    /// <summary>
    /// 添加日志提供程序（NLog不使用此机制，保留为空实现）
    /// </summary>
    /// <param name="provider">日志提供程序</param>
    public void AddProvider(ILoggerProvider provider)
    {
        // NLog不使用providers机制，所以这里留空
    }

    /// <summary>
    /// 创建指定类别的Logger实例
    /// </summary>
    /// <param name="categoryName">日志类别名称（通常是类的全名）</param>
    /// <returns>ILogger实例</returns>
    public ILogger CreateLogger(string categoryName)
    {
        return new NLogLogger(categoryName);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        // 清理资源（如果需要）
    }
}