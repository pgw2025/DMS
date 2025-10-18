using DMS.Application.DTOs;
using DMS.Core.Models;
using System;
using DMS.Core.Models.Triggers;

namespace DMS.Application.Services.Triggers
{
    /// <summary>
    /// 触发上下文，封装了触发时所需的所有信息
    /// </summary>
    /// <param name="Trigger">被触发的触发器定义</param>
    /// <param name="CurrentValue">触发时变量的当前值</param>
    /// <param name="Variable">关联的变量信息</param>
    public record TriggerContext(TriggerDefinition Trigger, object CurrentValue, Variable Variable);
}