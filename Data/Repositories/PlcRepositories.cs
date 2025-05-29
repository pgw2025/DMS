using PMSWPF.Data.Entities;
using SqlSugar;

namespace PMSWPF.Data.Repositories
{
    internal class PlcRepositories
    {
        private SqlSugarClient _db;

        public PlcRepositories()
        {

            _db = DbContext.GetInstance();
            var tabExist = _db.DbMaintenance.IsAnyTable(nameof(PLC), false);
            if (tabExist)
            {
                _db.CodeFirst.InitTables<PLC>();
            }
        }
    }
}
