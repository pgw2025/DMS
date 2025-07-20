using DMS.Infrastructure.Data;
using DMS.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMS.Config;

namespace DMS.Infrastructure.UnitTests.Repository_Test
{
    public class BaseRepositoryTests
    {

        protected readonly SqlSugarDbContext _sqlSugarDbContext;
        
        public BaseRepositoryTests()
        {
            // Load real connection settings
            var connectionSettings = new Config.AppSettings()
            {
                 Database = new DatabaseSettings()
                            {
                                Database = "dms_test"
                            }
            };
            _sqlSugarDbContext = new SqlSugarDbContext(connectionSettings);
            _sqlSugarDbContext.GetInstance().DbMaintenance.CreateDatabase();
            _sqlSugarDbContext.GetInstance().CodeFirst.InitTables<DbDevice>();
        }
    }
}
