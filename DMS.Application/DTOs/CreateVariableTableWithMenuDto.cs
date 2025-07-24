
using DMS.Core.Models;

namespace DMS.Application.DTOs
{
    public class CreateVariableTableWithMenuDto
    {
        public VariableTableDto VariableTable { get; set; }
        public int DeviceId { get; set; }
        public MenuBeanDto Menu { get; set; }
    }
}
