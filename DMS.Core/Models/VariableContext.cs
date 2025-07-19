using DMS.Core.Models;

namespace DMS.Core.Models
{
    public class VariableContext
    {
        public Variable Data { get; set; }
        public bool IsHandled { get; set; }

        public VariableContext(Variable data)
        {
            Data = data;
            IsHandled = false; // 默认未处理
        }
    }
}