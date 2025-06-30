using PMSWPF.Enums;
using PMSWPF.ViewModels;

namespace PMSWPF.Models;

public class MenuBean
{
    
    public  int Id { get; set; }
    public string Icon { get; set; }
    public string Name { get; set; }
    public int ParentId { get; set; }
    public int DataId { get; set; }
    public MenuType Type { get; set; }
    
    public ViewModelBase ViewModel { get; set; }
    public Object? Data { get; set; }
    public List<MenuBean> Items { get; set; }
}