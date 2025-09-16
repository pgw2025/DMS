using System;
using DMS.Core.Enums;

namespace DMS.Application.DTOs.Events
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
        /// 父级菜单DTO
        /// </summary>
        public MenuBeanDto ParentMenu { get; }

        /// <summary>
        /// 变更时间
        /// </summary>
        public DateTime ChangeTime { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="changeType">变更类型</param>
        /// <param name="menu">菜单DTO</param>
        /// <param name="parentMenu">父级菜单DTO</param>
        public MenuChangedEventArgs(DataChangeType changeType, MenuBeanDto menu, MenuBeanDto parentMenu)
        {
            ChangeType = changeType;
            Menu = menu;
            ParentMenu = parentMenu;
            ChangeTime = DateTime.Now;
        }
    }
}