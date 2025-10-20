using System;
using DMS.Core.Models.Triggers;

namespace DMS.Application.DTOs
{
    public class TriggerVariableDto
    {
        public int Id { get; set; }

        public int TriggerDefinitionId { get; set; }

        public int VariableId { get; set; }
        
        // 从核心模型转换为DTO
        public static implicit operator TriggerVariableDto(TriggerVariable triggerVariable)
        {
            return new TriggerVariableDto
            {
                Id = triggerVariable.Id,
                TriggerDefinitionId = triggerVariable.TriggerDefinitionId,
                VariableId = triggerVariable.VariableId
            };
        }

        // 从DTO转换为核心模型
        public static implicit operator TriggerVariable(TriggerVariableDto dto)
        {
            return new TriggerVariable
            {
                Id = dto.Id,
                TriggerDefinitionId = dto.TriggerDefinitionId,
                VariableId = dto.VariableId
            };
        }
    }
}