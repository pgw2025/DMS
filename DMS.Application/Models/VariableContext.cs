using DMS.Application.DTOs;
using DMS.Core.Models;

namespace DMS.Application.Models
{
    public class VariableContext
    {
        public VariableDto Data { get; set; }
        
        public string NewValue { get; set; }
        public bool IsHandled { get; set; }

        public VariableContext(VariableDto data, string newValue="")
        {
            Data = data;
            IsHandled = false; // 默认未处理
            NewValue = newValue;
        }
    }
}