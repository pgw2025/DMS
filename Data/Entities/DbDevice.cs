namespace PMSWPF.Data.Entities;

public class DbDevice
{
    public int  Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public bool IsRuning { get; set; }
    
}