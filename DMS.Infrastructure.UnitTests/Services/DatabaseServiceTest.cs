using DMS.Core.Interfaces;
using DMS.Infrastructure.Configurations;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Services;

namespace DMS.Infrastructure.UnitTests.Services;

public class DatabaseServiceTest
{

    private IDatabaseService _databaseService;
    public DatabaseServiceTest()
    {
        AppSettings appSettings = new AppSettings();
        appSettings.Database.Database = "dms_test";
        SqlSugarDbContext dbContext=new SqlSugarDbContext(appSettings);
        _databaseService = new DatabaseService(dbContext);
    }
    [Fact]
    public void InitializeTables_Test()
    {
        _databaseService.InitializeTables();
        Assert.True(_databaseService.IsAnyTable("dbdevice"));
    }
    
    [Fact]
    public void InitializeTableIndex_Test()
    {
        _databaseService.InitializeTableIndex();
    }
}