using DMS.Core.Models;
using DMS.Core.Enums;
using SqlSugar;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.Core.Interfaces
{
    public interface IMenuRepository
    {
        Task<int> DeleteAsync(MenuBean menu);
        Task<int> DeleteAsync(MenuBean menu, SqlSugarClient db);
        Task<List<MenuBean>> GetMenuTreesAsync();
        Task<int> AddAsync(MenuBean menu);
        Task<int> AddAsync(MenuBean menu, SqlSugarClient db);
        Task<int> AddVarTableMenuAsync(Device dbDevice, int parentMenuId, SqlSugarClient db);
        Task<int> AddAsync(Device device, SqlSugarClient db);
        Task<int> UpdateAsync(MenuBean menu);
        
        Task<MenuBean?> GetMenuByDataIdAsync(int dataId, MenuType menuType);
        Task<MenuBean> GetMainMenuByNameAsync(string name);
    }
}