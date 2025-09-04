using DMS.Application.Interfaces;
using DMS.Infrastructure.Configuration;
using DMS.Infrastructure.Interfaces.Services;
using DMS.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DMS.Infrastructure.Extensions
{
    /// <summary>
    /// OPC UA服务扩展方法
    /// </summary>
    public static class OpcUaServiceExtensions
    {
        /// <summary>
        /// 添加OPC UA服务
        /// </summary>
        public static IServiceCollection AddOpcUaServices(this IServiceCollection services)
        {
            // 注册配置选项
            services.Configure<OpcUaServiceOptions>(
                options => {
                    // 可以从配置文件或其他来源加载配置
                });

            // 注册服务
            services.AddSingleton<IOpcUaServiceManager, OpcUaServiceManager>();
            
            // 注册后台服务
            services.AddHostedService<OptimizedOpcUaBackgroundService>();

            return services;
        }
    }
}