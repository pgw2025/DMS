using DMS.Core.Models;

namespace DMS.Application.Models
{
    public class VariableContext
    {
        public Variable Data { get; set; }
        
        public string NewValue { get; set; }
        public bool IsHandled { get; set; }

        public VariableContext(Variable data, string newValue="")
        {
            Data = data;
            IsHandled = false; // 默认未处理
            NewValue = newValue;
        }
    }
}