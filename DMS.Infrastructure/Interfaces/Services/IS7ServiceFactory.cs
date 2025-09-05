using DMS.Infrastructure.Interfaces.Services;

namespace DMS.Infrastructure.Interfaces.Services
{
    /// <summary>
    /// S7服务工厂接口，用于创建S7Service实例
    /// </summary>
    public interface IS7ServiceFactory
    {
        /// <summary>
        /// 创建S7服务实例
        /// </summary>
        /// <returns>S7服务实例</returns>
        IS7Service CreateService();
    }
}