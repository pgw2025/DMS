using AutoMapper;
using DMS.Core.Models;
using DMS.Infrastructure.Configurations;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Profiles;
using DMS.Infrastructure.Repositories;
using DMS.Infrastructure.Services;

namespace DMS.Infrastructure.UnitTests.Services;

public class VariableTableServieTest
{
    private readonly VariableTableRepository _variableTableRepository;
    private readonly VariableTableService _variableTableService;
    private readonly IMapper _mapper;

    public VariableTableServieTest()
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
        _variableTableRepository= new VariableTableRepository(_mapper,dbContext);
        _variableTableService = new VariableTableService(_variableTableRepository);
    }

    [Fact]
    public async Task AddAsync_Test()
    {
        // var dbDevice = FakerHelper
        // var addDevice= await _variableTableService.AddAsync(_mapper.Map<Device>(dbDevice));
        // Assert.NotEqual(0, addDevice.Id);
    }
    
    [Fact]
    public async Task TakeAsync_Test()
    {
        var device= await _variableTableService.TakeAsync(2);
        Assert.Equal(2,device.Count);
    }  
    [Fact]
    public async Task UpdateAsync_Test()
    {
        var devices= await _variableTableService.TakeAsync(1);
        // devices[0].IpAddress = "127.0.0.1";
        var res= await _variableTableService.UpdateAsync(devices[0]);
        Assert.Equal(1,res);
    } 
    [Fact]
    public async Task DeleteAsync_Test()
    {
        var devices= await _variableTableService.TakeAsync(1);
        var res= await _variableTableService.DeleteAsync(devices[0]);
        Assert.Equal(1,res);
    }
}