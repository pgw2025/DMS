namespace DMS.Core.Enums
{
    /// <summary>
    /// MQTT服务器属性类型枚举
    /// </summary>
    public enum MqttServerPropertyType
    {
        /// <summary>
        /// 服务器名称
        /// </summary>
        ServerName,

        /// <summary>
        /// 服务器URL
        /// </summary>
        ServerUrl,

        /// <summary>
        /// 端口
        /// </summary>
        Port,

        /// <summary>
        /// 是否连接
        /// </summary>
        IsConnect,

        /// <summary>
        /// 用户名
        /// </summary>
        Username,

        /// <summary>
        /// 密码
        /// </summary>
        Password,

        /// <summary>
        /// 是否激活
        /// </summary>
        IsActive,

        /// <summary>
        /// 订阅主题
        /// </summary>
        SubscribeTopic,

        /// <summary>
        /// 发布主题
        /// </summary>
        PublishTopic,

        /// <summary>
        /// 客户端ID
        /// </summary>
        ClientId,

        /// <summary>
        /// 消息格式
        /// </summary>
        MessageFormat,

        /// <summary>
        /// 消息头
        /// </summary>
        MessageHeader,

        /// <summary>
        /// 消息内容
        /// </summary>
        MessageContent,

        /// <summary>
        /// 消息尾
        /// </summary>
        MessageFooter,

        /// <summary>
        /// 所有属性
        /// </summary>
        All
    }
}