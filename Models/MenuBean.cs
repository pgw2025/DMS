using PMSWPF.Enums;

namespace PMSWPF.Models;

public class MenuBean
{
    
    public  int Id { get; set; }
    public string Icon { get; set; }
    public string Name { get; set; }
    public int ParentId { get; set; }
    public int DataId { get; set; }
    public MenuType Type { get; set; }
    public Object? Data { get; set; }
    public List<MenuBean> Items { get; set; }
}