using PMSWPF.Enums;

namespace PMSWPF.Models;

public class VariableTable
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ProtocolType ProtocolType { get; set; }
    public List<DataVariable> DataVariables { get; set; }
}