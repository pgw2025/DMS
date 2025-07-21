using DMS.Core.Models;

namespace DMS.Core.Interfaces;

public interface IMenuRepository : IBaseRepository<MenuBean>
{
    // 可以添加特定于菜单的查询方法，例如获取所有菜单项
}