using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Interfaces
{
    public interface ITransaction
    {
        Task BeginTranAsync();
        Task CommitTranAsync();
        SqlSugarClient GetInstance();
        Task RollbackTranAsync();
    }
}
