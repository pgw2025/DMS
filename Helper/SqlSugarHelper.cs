using PMSWPF.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMSWPF.Helper
{
    public class SqlSugarHelper
    {
        private DbContext _db;

        public SqlSugarHelper() { 
            _db=new DbContext();
            
        }

        public void InitTables()
        {
            
        }

       
    }
}
