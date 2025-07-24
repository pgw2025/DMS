
using DMS.Core.Enums;
using DMS.Core.Models;

namespace DMS.Core.Interfaces.Repositories
{
    public interface IMenuRepository:IBaseRepository<MenuBean>
    {
        Task<int> DeleteMenuTreeByIdAsync(int id);
        Task<int> DeleteMenuTreeByTargetIdAsync(MenuType menuType, int targetId);
        Task<MenuBean> GetMenuByTargetIdAsync(MenuType menuType, int targetId);
    }
}