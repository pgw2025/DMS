using DMS.Config;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;
using DMS.Infrastructure.Repositories;

namespace DMS.Infrastructure.UnitTests.Repositories_Test;

public class DeviceRepository_Test
{
    private readonly DeviceRepository _deviceRepository;

    public DeviceRepository_Test()
    {
        AppSettings appSettings = new AppSettings();
        appSettings.Database.Database = "dms_test";
        var dbContext = new SqlSugarDbContext(appSettings);
        dbContext.GetInstance()
                 .DbMaintenance.CreateDatabase();
        dbContext.GetInstance()
                 .CodeFirst.InitTables<DbDevice>();
        // dbContext.GetInstance()
        //          .DbMaintenance.CreateIndex("Devices", new[] { "name" }, true);
        _deviceRepository = new DeviceRepository(dbContext);
    }

    [Fact]
    public async Task AddAsync_Test()
    {
        var dbDevice = FakerHelper.FakeDbDevice();
        var resDevice = await _deviceRepository.AddAsync(dbDevice);
        var res = await _deviceRepository.GetByIdAsync(resDevice.Id);

        Assert.NotNull(res);
    }
    
    [Fact]
    public async Task UpdateAsync_Test()
    {
        var dbDevice = FakerHelper.FakeDbDevice();
        var resDevice = await _deviceRepository.AddAsync(dbDevice);
        
        var res2 = await _deviceRepository.GetByIdAsync(resDevice.Id);
        res2.Name = "HaHa";
        var res = await _deviceRepository.UpdateAsync(res2);

        Assert.Equal(res, 1);
    }
}