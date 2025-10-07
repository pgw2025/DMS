using DMS.Application.DTOs;
using DMS.Core.Events;
using DMS.Core.Models;

namespace DMS.Application.Interfaces
{
    public interface IAlarmService
    {
        /// <summary>
        /// 检查变量是否触发报警
        /// </summary>
        /// <param name="variable">变量DTO</param>
        /// <returns>是否触发报警</returns>
        bool CheckAlarm(Variable variable);
        
        /// <summary>
        /// 警报事件
        /// </summary>
        event EventHandler<AlarmEventArgs> OnAlarmTriggered;
    }
}