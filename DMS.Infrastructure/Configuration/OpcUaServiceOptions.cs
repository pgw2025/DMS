namespace DMS.Infrastructure.Configuration
{
    /// <summary>
    /// OPC UA服务配置
    /// </summary>
    public class OpcUaServiceOptions
    {
        /// <summary>
        /// 配置节名称
        /// </summary>
        public const string SectionName = "OpcUaService";

        /// <summary>
        /// 最大并发连接数
        /// </summary>
        public int MaxConcurrentConnections { get; set; } = 10;

        /// <summary>
        /// 重连延迟（毫秒）
        /// </summary>
        public int ReconnectDelayMs { get; set; } = 5000;

        /// <summary>
        /// 订阅发布间隔（毫秒）
        /// </summary>
        public int SubscriptionPublishingIntervalMs { get; set; } = 1000;

        /// <summary>
        /// 订阅采样间隔（毫秒）
        /// </summary>
        public int SubscriptionSamplingIntervalMs { get; set; } = 1000;

        /// <summary>
        /// 连接超时时间（毫秒）
        /// </summary>
        public int ConnectionTimeoutMs { get; set; } = 30000;

        /// <summary>
        /// 是否自动接受不受信任的证书
        /// </summary>
        public bool AutoAcceptUntrustedCertificates { get; set; } = true;
    }
}