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
           
            return services;
        }
    }
}