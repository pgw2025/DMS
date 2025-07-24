// DMS.Infrastructure.UnitTests/Services/BaseServiceTest.cs

using AutoMapper;
using AutoMapper.Internal;
using DMS.Application.Interfaces;
using DMS.Application.Services;
using DMS.Core.Interfaces;
using DMS.Core.Interfaces.Repositories;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using DMS.Infrastructure.Configurations;


namespace DMS.Infrastructure.UnitTests.Services;

public class BaseServiceTest
{
    // ServiceProvider 将是所有测试的服务容器
    protected readonly IServiceProvider ServiceProvider;

    public BaseServiceTest()
    {
        var services = new ServiceCollection();

        // --- 核心配置 ---
        services.AddAutoMapper(cfg =>
        {
            // 最终解决方案：根据异常信息的建议，设置此标记以忽略重复的Profile加载。
            // 注意：此属性位于 Internal() 方法下。
            cfg.Internal().AllowAdditiveTypeMapCreation = true;

            cfg.AddProfile(new DMS.Application.Profiles.MappingProfile());
            cfg.AddProfile(new DMS.Infrastructure.Profiles.MappingProfile());
        });

        // 2. 配置数据库上下文 (在测试中通常使用单例)
        services.AddSingleton<SqlSugarDbContext>(_ =>
        {
            var appSettings = new AppSettings { Database = { Database = "dms_test" } };
            return new SqlSugarDbContext(appSettings);
        });

        // --- 注册服务和仓储 ---
        // 使用 Transient 或 Scoped 取决于服务的生命周期需求，对于测试，Transient 通常更安全。
        
        // 注册仓储管理器
        services.AddTransient<IRepositoryManager, RepositoryManager>();
        services.AddTransient<IInitializeRepository, InitializeRepository>();

        // 注册应用服务
        services.AddTransient<IDeviceAppService, DeviceService>();
        // services.AddTransient<IVariableAppService, VariableAppService>(); // 如果需要测试 VariableService，取消此行注释
        // ... 在这里注册所有其他的应用服务 ...


        // --- 构建服务提供程序 ---
        ServiceProvider = services.BuildServiceProvider();

        // 验证 AutoMapper 配置 (可选，但强烈推荐)
        var mapper = ServiceProvider.GetService<IMapper>();
        mapper?.ConfigurationProvider.AssertConfigurationIsValid();
    }
}
