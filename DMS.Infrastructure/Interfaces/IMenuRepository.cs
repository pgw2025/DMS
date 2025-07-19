using DMS.Core.Models;
using DMS.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Interfaces
{
    public interface IMenuRepository
    {
        Task<int> DeleteAsync(MenuBean menu);
        Task<int> DeleteAsync(MenuBean menu, ITransaction db);
        Task<List<MenuBean>> GetMenuTreesAsync();
        Task<int> AddAsync(MenuBean menu);
        Task<int> AddAsync(MenuBean menu, ITransaction db);
        Task<int> AddVarTableMenuAsync(Device dbDevice, int parentMenuId, ITransaction db);
        Task<int> AddAsync(Device device, ITransaction db);
        Task<int> UpdateAsync(MenuBean menu);
        
        Task<MenuBean?> GetMenuByDataIdAsync(int dataId, MenuType menuType);
        Task<MenuBean> GetMainMenuByNameAsync(string name);
    }
}