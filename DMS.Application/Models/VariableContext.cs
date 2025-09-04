using DMS.Application.DTOs;
using DMS.Core.Models;

namespace DMS.Application.Models
{
    public class VariableContext
    {
        public VariableDto Data { get; set; }
        public bool IsHandled { get; set; }

        public VariableContext(VariableDto data)
        {
            Data = data;
            IsHandled = false; // 默认未处理
        }
    }
}