using AutoMapper;
using DMS.Core.Models;
using DMS.Infrastructure.Configurations;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Profiles;
using DMS.Infrastructure.Repositories;
using DMS.Infrastructure.Services;
using JetBrains.Annotations;

namespace DMS.Infrastructure.UnitTests.Services;

[TestSubject(typeof(DeviceService))]
public class DeviceServiceTest
{
    private readonly DeviceRepository _deviceRepository;
    private readonly DeviceService _deviceService;
    private readonly IMapper _mapper;

    public DeviceServiceTest()
    {
        // 1. 创建 MapperConfiguration
        var mappingConfig = new MapperConfiguration(mc =>

        {
            // 添加你的所有 Profile
            mc.AddProfile(new MappingProfile());
            // 如果有其他 Profile，也可以在这里添加
            // mc.AddProfile(new AnotherProfile());
        });

        // 2. 验证映射配置是否有效 (可选，但在开发环境中推荐)
        mappingConfig.AssertConfigurationIsValid();
        // 3. 创建 IMapper 实例
        _mapper = mappingConfig.CreateMapper();


        AppSettings appSettings = new AppSettings();
        appSettings.Database.Database = "dms_test";
        SqlSugarDbContext dbContext = new SqlSugarDbContext(appSettings);
       _deviceRepository= new DeviceRepository(_mapper,dbContext);
        _deviceService = new DeviceService(_deviceRepository);
    }

    [Fact]
    public async Task AddAsync_Test()
    {
        var dbDevice = FakerHelper.FakeDbDevice();
       var addDevice= await _deviceService.AddAsync(_mapper.Map<Device>(dbDevice));
       Assert.NotEqual(0, addDevice.Id);
    }
    
    [Fact]
    public async Task TakeAsync_Test()
    {
        var device= await _deviceService.TakeAsync(2);
        Assert.Equal(2,device.Count);
    }
}