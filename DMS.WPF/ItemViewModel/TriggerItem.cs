using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Application.DTOs;
using DMS.Core.Models.Triggers;

namespace DMS.WPF.ItemViewModel
{
    /// <summary>
    /// 触发器项视图模型
    /// </summary>
    public partial class TriggerItem : ObservableObject
    {
        /// <summary>
        /// 触发器唯一标识符
        /// </summary>
        [ObservableProperty]
        private int _id;

        /// <summary>
        /// 触发器名称
        /// </summary>
        [ObservableProperty]
        private string _name = string.Empty;

        /// <summary>
        /// 触发器描述
        /// </summary>
        [ObservableProperty]
        private string _description = string.Empty;

        /// <summary>
        /// 触发器是否处于激活状态
        /// </summary>
        [ObservableProperty]
        private bool _isActive;

        /// <summary>
        /// 动作类型
        /// </summary>
        [ObservableProperty]
        private ActionType _action;

        /// <summary>
        /// 动作配置 JSON 字符串
        /// </summary>
        [ObservableProperty]
        private string _actionConfigurationJson = string.Empty;

        /// <summary>
        /// 抑制持续时间
        /// </summary>
        [ObservableProperty]
        private TimeSpan? _suppressionDuration;

        /// <summary>
        /// 上次触发的时间
        /// </summary>
        [ObservableProperty]
        private DateTime? _lastTriggeredAt;

        /// <summary>
        /// 创建时间
        /// </summary>
        [ObservableProperty]
        private DateTime _createdAt;

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [ObservableProperty]
        private DateTime _updatedAt;

        /// <summary>
        /// 关联的变量 ID 列表
        /// </summary>
        public ObservableCollection<int> VariableIds { get; } = new ObservableCollection<int>();
    }
}