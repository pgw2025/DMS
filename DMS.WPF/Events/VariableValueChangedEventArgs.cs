using System;

namespace DMS.WPF.Events;

/// <summary>
/// 变量值改变事件参数
/// </summary>
public class VariableValueChangedEventArgs : EventArgs
{
    /// <summary>
    /// 变量ID
    /// </summary>
    public int VariableId { get; }

    /// <summary>
    /// 变量名称
    /// </summary>
    public string VariableName { get; }

    /// <summary>
    /// 旧值
    /// </summary>
    public string OldValue { get; }

    /// <summary>
    /// 新值
    /// </summary>
    public string NewValue { get; }

    /// <summary>
    /// 值改变时间
    /// </summary>
    public DateTime ChangeTime { get; }

    /// <summary>
    /// 初始化VariableValueChangedEventArgs类的新实例
    /// </summary>
    /// <param name="variableId">变量ID</param>
    /// <param name="variableName">变量名称</param>
    /// <param name="oldValue">旧值</param>
    /// <param name="newValue">新值</param>
    public VariableValueChangedEventArgs(int variableId, string variableName, string oldValue, string newValue)
    {
        VariableId = variableId;
        VariableName = variableName;
        OldValue = oldValue;
        NewValue = newValue;
        ChangeTime = DateTime.Now;
    }
}