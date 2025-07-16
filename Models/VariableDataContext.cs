using PMSWPF.Models;

namespace PMSWPF.Models
{
    public class VariableDataContext
    {
        public VariableData Data { get; set; }
        public bool IsHandled { get; set; }

        public VariableDataContext(VariableData data)
        {
            Data = data;
            IsHandled = false; // 默认未处理
        }
    }
}