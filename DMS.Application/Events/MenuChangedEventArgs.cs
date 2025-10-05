using DMS.Application.DTOs;
using DMS.Core.Enums;

namespace DMS.Application.Events
{
    /// <summary>
    /// 菜单变更事件参数
    /// </summary>
    public class MenuChangedEventArgs : System.EventArgs
    {
        /// <summary>
        /// 变更类型
        /// </summary>
        public DataChangeType ChangeType { get; }

        /// <summary>
        /// 菜单DTO
        /// </summary>
        public MenuBeanDto Menu { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="changeType">变更类型</param>
        /// <param name="menu">菜单DTO</param>
        /// <param name="parentMenu">父级菜单DTO</param>
        public MenuChangedEventArgs(DataChangeType changeType, MenuBeanDto menu)
        {
            ChangeType = changeType;
            Menu = menu;
        }
    }
}