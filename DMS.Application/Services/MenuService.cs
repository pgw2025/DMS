using AutoMapper;
using DMS.Core.Interfaces;
using DMS.Core.Models;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;

namespace DMS.Application.Services;

/// <summary>
/// 菜单应用服务，负责处理菜单相关的业务逻辑。
/// 实现 <see cref="IMenuService"/> 接口。
/// </summary>
public class MenuService : IMenuService
{
    private readonly IRepositoryManager _repoManager;
    private readonly IMapper _mapper;

    /// <summary>
    /// 构造函数，通过依赖注入获取仓储管理器和AutoMapper实例。
    /// </summary>
    /// <param name="repoManager">仓储管理器实例。</param>
    /// <param name="mapper">AutoMapper 实例。</param>
    public MenuService(IRepositoryManager repoManager, IMapper mapper)
    {
        _repoManager = repoManager;
        _mapper = mapper;
    }

    /// <summary>
    /// 异步根据ID获取菜单数据传输对象。
    /// </summary>
    /// <param name="id">菜单ID。</param>
    /// <returns>菜单数据传输对象。</returns>
    public async Task<MenuBeanDto> GetMenuByIdAsync(int id)
    {
        var menu = await _repoManager.Menus.GetByIdAsync(id);
        return _mapper.Map<MenuBeanDto>(menu);
    }

    /// <summary>
    /// 异步获取所有菜单数据传输对象列表。
    /// </summary>
    /// <returns>菜单数据传输对象列表。</returns>
    public async Task<List<MenuBeanDto>> GetAllMenusAsync()
    {
        var menus = await _repoManager.Menus.GetAllAsync();
        return _mapper.Map<List<MenuBeanDto>>(menus);
    }

    /// <summary>
    /// 异步创建一个新菜单（事务性操作）。
    /// </summary>
    /// <param name="menuDto">要创建的菜单数据传输对象。</param>
    /// <returns>新创建菜单的ID。</returns>
    /// <exception cref="ApplicationException">如果创建菜单时发生错误。</exception>
    public async Task<int> CreateMenuAsync(MenuBeanDto menuDto)
    {
        try
        {
            await _repoManager.BeginTranAsync();
            var menu = _mapper.Map<MenuBean>(menuDto);
            await _repoManager.Menus.AddAsync(menu);
            await _repoManager.CommitAsync();
            return menu.Id;
        }
        catch (Exception ex)
        {
            await _repoManager.RollbackAsync();
            throw new ApplicationException("创建菜单时发生错误，操作已回滚。", ex);
        }
    }

    /// <summary>
    /// 异步更新一个已存在的菜单（事务性操作）。
    /// </summary>
    /// <param name="menuDto">要更新的菜单数据传输对象。</param>
    /// <returns>表示异步操作的任务。</returns>
    /// <exception cref="ApplicationException">如果找不到菜单或更新菜单时发生错误。</exception>
    public async Task UpdateMenuAsync(MenuBeanDto menuDto)
    {
        try
        {
            await _repoManager.BeginTranAsync();
            var menu = await _repoManager.Menus.GetByIdAsync(menuDto.Id);
            if (menu == null)
            {
                throw new ApplicationException($"Menu with ID {menuDto.Id} not found.");
            }
            _mapper.Map(menuDto, menu);
            await _repoManager.Menus.UpdateAsync(menu);
            await _repoManager.CommitAsync();
        }
        catch (Exception ex)
        {
            await _repoManager.RollbackAsync();
            throw new ApplicationException("更新菜单时发生错误，操作已回滚。", ex);
        }
    }

    /// <summary>
    /// 异步删除一个菜单（事务性操作）。
    /// </summary>
    /// <param name="id">要删除菜单的ID。</param>
    /// <returns>表示异步操作的任务。</returns>
    /// <exception cref="ApplicationException">如果删除菜单时发生错误。</exception>
    public async Task DeleteMenuAsync(int id)
    {
        try
        {
            await _repoManager.BeginTranAsync();
            await _repoManager.Menus.DeleteByIdAsync(id);
            await _repoManager.CommitAsync();
        }
        catch (Exception ex)
        {
            await _repoManager.RollbackAsync();
            throw new ApplicationException("删除菜单时发生错误，操作已回滚。", ex);
        }
    }
}