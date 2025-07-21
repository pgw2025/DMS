using SqlSugar;

namespace DMS.Infrastructure.Entities;

public class DbMqttServer
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }
    public string ServerName { get; set; }
    public string BrokerAddress { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public bool IsActive { get; set; }
    public string SubscribeTopic { get; set; }
    public string PublishTopic { get; set; }
    public string ClientId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConnectedAt { get; set; }
    public long ConnectionDuration { get; set; }
    public string MessageFormat { get; set; }
}