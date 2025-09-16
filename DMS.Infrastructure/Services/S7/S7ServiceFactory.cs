using DMS.Infrastructure.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace DMS.Infrastructure.Services.S7
{
    /// <summary>
    /// S7服务工厂实现，用于创建S7Service实例
    /// </summary>
    public class S7ServiceFactory : IS7ServiceFactory
    {
        private readonly ILogger<S7Service> _logger;

        public S7ServiceFactory(ILogger<S7Service> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 创建S7服务实例
        /// </summary>
        /// <returns>S7服务实例</returns>
        public IS7Service CreateService()
        {
            return new S7Service(_logger);
        }
    }
}