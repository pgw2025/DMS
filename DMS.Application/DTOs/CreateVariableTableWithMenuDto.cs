
using DMS.Core.Models;

namespace DMS.Application.DTOs
{
    public class CreateVariableTableWithMenuDto
    {
        public VariableTable VariableTable { get; set; }
        public int DeviceId { get; set; }
        public MenuBean Menu { get; set; }
    }
}
