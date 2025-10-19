using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.Core.Models.Triggers;
using DMS.Core.Interfaces.Repositories;

namespace DMS.Core.Interfaces.Repositories.Triggers
{
    /// <summary>
    /// 触发器仓储接口 (定义对 TriggerMenu 实体的数据访问方法)
    /// </summary>
    public interface ITriggerRepository : IBaseRepository<Trigger>
    {
    }
}