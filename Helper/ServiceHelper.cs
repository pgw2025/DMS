using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using PMSWPF.Enums;

namespace PMSWPF.Helper;

public static class ServiceHelper
{
    // 定义不同轮询级别的间隔时间。
    public static Dictionary<PollLevelType, TimeSpan> PollingIntervals = new Dictionary<PollLevelType, TimeSpan>
                                                                         {
                                                                             {
                                                                                 PollLevelType.TenMilliseconds,
                                                                                 TimeSpan.FromMilliseconds(
                                                                                     (int)PollLevelType.TenMilliseconds)
                                                                             },
                                                                             {
                                                                                 PollLevelType.HundredMilliseconds,
                                                                                 TimeSpan.FromMilliseconds(
                                                                                     (int)PollLevelType
                                                                                         .HundredMilliseconds)
                                                                             },
                                                                             {
                                                                                 PollLevelType.FiveHundredMilliseconds,
                                                                                 TimeSpan.FromMilliseconds(
                                                                                     (int)PollLevelType
                                                                                         .FiveHundredMilliseconds)
                                                                             },
                                                                             {
                                                                                 PollLevelType.OneSecond,
                                                                                 TimeSpan.FromMilliseconds(
                                                                                     (int)PollLevelType.OneSecond)
                                                                             },
                                                                             {
                                                                                 PollLevelType.FiveSeconds,
                                                                                 TimeSpan.FromMilliseconds(
                                                                                     (int)PollLevelType.FiveSeconds)
                                                                             },
                                                                             {
                                                                                 PollLevelType.TenSeconds,
                                                                                 TimeSpan.FromMilliseconds(
                                                                                     (int)PollLevelType.TenSeconds)
                                                                             },
                                                                             {
                                                                                 PollLevelType.TwentySeconds,
                                                                                 TimeSpan.FromMilliseconds(
                                                                                     (int)PollLevelType.TwentySeconds)
                                                                             },
                                                                             {
                                                                                 PollLevelType.ThirtySeconds,
                                                                                 TimeSpan.FromMilliseconds(
                                                                                     (int)PollLevelType.ThirtySeconds)
                                                                             },
                                                                             {
                                                                                 PollLevelType.OneMinute,
                                                                                 TimeSpan.FromMilliseconds(
                                                                                     (int)PollLevelType.OneMinute)
                                                                             },
                                                                             {
                                                                                 PollLevelType.ThreeMinutes,
                                                                                 TimeSpan.FromMilliseconds(
                                                                                     (int)PollLevelType.ThreeMinutes)
                                                                             },
                                                                             {
                                                                                 PollLevelType.FiveMinutes,
                                                                                 TimeSpan.FromMilliseconds(
                                                                                     (int)PollLevelType.FiveMinutes)
                                                                             },
                                                                             {
                                                                                 PollLevelType.TenMinutes,
                                                                                 TimeSpan.FromMilliseconds(
                                                                                     (int)PollLevelType.TenMinutes)
                                                                             },
                                                                             {
                                                                                 PollLevelType.ThirtyMinutes,
                                                                                 TimeSpan.FromMilliseconds(
                                                                                     (int)PollLevelType.ThirtyMinutes)
                                                                             }
                                                                         };
    /// <summary>
    /// 创建并配置 OPC UA 会话。
    /// </summary>
    /// <param name="endpointUrl">OPC UA 服务器的终结点 URL。</param>
    /// <param name="stoppingToken"></param>
    /// <returns>创建的 Session 对象，如果失败则返回 null。</returns>
    public static async Task<Session> CreateOpcUaSessionAsync(string endpointUrl, CancellationToken stoppingToken = default)
    {
            // 1. 创建应用程序配置
            var application = new ApplicationInstance
                              {
                                  ApplicationName = "OpcUADemoClient",
                                  ApplicationType = ApplicationType.Client,
                                  ConfigSectionName = "Opc.Ua.Client"
                              };

            var config = new ApplicationConfiguration()
                         {
                             ApplicationName = application.ApplicationName,
                             ApplicationUri = $"urn:{System.Net.Dns.GetHostName()}:OpcUADemoClient",
                             ApplicationType = application.ApplicationType,
                             SecurityConfiguration = new SecurityConfiguration
                                                     {
                                                         ApplicationCertificate = new CertificateIdentifier
                                                             {
                                                                 StoreType = "Directory",
                                                                 StorePath
                                                                     = "%CommonApplicationData%/OPC Foundation/CertificateStores/MachineDefault",
                                                                 SubjectName = application.ApplicationName
                                                             },
                                                         TrustedIssuerCertificates = new CertificateTrustList
                                                             {
                                                                 StoreType = "Directory",
                                                                 StorePath
                                                                     = "%CommonApplicationData%/OPC Foundation/CertificateStores/UA Certificate Authorities"
                                                             },
                                                         TrustedPeerCertificates = new CertificateTrustList
                                                             {
                                                                 StoreType = "Directory",
                                                                 StorePath
                                                                     = "%CommonApplicationData%/OPC Foundation/CertificateStores/UA Applications"
                                                             },
                                                         RejectedCertificateStore = new CertificateTrustList
                                                             {
                                                                 StoreType = "Directory",
                                                                 StorePath
                                                                     = "%CommonApplicationData%/OPC Foundation/CertificateStores/RejectedCertificates"
                                                             },
                                                         AutoAcceptUntrustedCertificates
                                                             = true // 自动接受不受信任的证书 (仅用于测试)
                                                     },
                             TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                             ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 },
                             TraceConfiguration = new TraceConfiguration
                                                  {
                                                      OutputFilePath = "./Logs/OpcUaClient.log",
                                                      DeleteOnLoad = true,
                                                      TraceMasks = Utils.TraceMasks.Error |
                                                                   Utils.TraceMasks.Security
                                                  }
                         };
            application.ApplicationConfiguration = config;

            // 验证并检查证书
            await config.Validate(ApplicationType.Client);
            

            // 2. 查找并选择端点 (将 useSecurity 设置为 false 以进行诊断)
            var selectedEndpoint = CoreClientUtils.SelectEndpoint(config, endpointUrl, false);

            var session = await Session.Create(
                config,
                new ConfiguredEndpoint(null, selectedEndpoint, EndpointConfiguration.Create(config)),
                false,
                "PMSWPF OPC UA Session",
                60000,
                new UserIdentity(new AnonymousIdentityToken()),
                null,stoppingToken);
            return session;
    }
}