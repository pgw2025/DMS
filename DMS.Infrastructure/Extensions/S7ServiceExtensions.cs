using DMS.Application.Interfaces;
using DMS.Infrastructure.Interfaces;
using DMS.Infrastructure.Interfaces.Services;
using DMS.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DMS.Infrastructure.Extensions
{
    /// <summary>
    /// S7服务扩展方法
    /// </summary>
    public static class S7ServiceExtensions
    {
        /// <summary>
        /// 添加S7服务
        /// </summary>
        public static IServiceCollection AddS7Services(this IServiceCollection services)
        {
            // 注册服务
            services.AddSingleton<IS7ServiceFactory, S7ServiceFactory>();
            services.AddSingleton<IS7ServiceManager, S7ServiceManager>();
            
            // 注册后台服务
            services.AddHostedService<S7BackgroundService>();
            
            // 注册优化的后台服务（可选）
            // services.AddHostedService<OptimizedS7BackgroundService>();

            return services;
        }
    }
}