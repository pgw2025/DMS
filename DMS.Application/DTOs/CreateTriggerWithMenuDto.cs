using DMS.Core.Models;
using DMS.Core.Models.Triggers;

namespace DMS.Application.DTOs
{
    /// <summary>
    /// 创建触发器及其关联菜单的数据传输对象
    /// </summary>
    public class CreateTriggerWithMenuDto
    {
        /// <summary>
        /// 触发器信息
        /// </summary>
        public Trigger Trigger { get; set; }
        
        /// <summary>
        /// 菜单项信息
        /// </summary>
        public MenuBean TriggerMenu { get; set; }
    }
}