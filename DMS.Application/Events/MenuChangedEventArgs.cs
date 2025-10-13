using DMS.Core.Enums;
using DMS.Core.Models;

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
        /// 菜单
        /// </summary>
        public MenuBean Menu { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="changeType">变更类型</param>
        /// <param name="menu">菜单</param>
        /// <param name="parentMenu">父级菜单</param>
        public MenuChangedEventArgs(DataChangeType changeType, MenuBean menu)
        {
            ChangeType = changeType;
            Menu = menu;
        }
    }
}