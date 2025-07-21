using AutoMapper;
using DMS.Core.Interfaces;
using DMS.Core.Models;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;

namespace DMS.Application.Services;

/// <summary>
/// 实现菜单管理的应用服务。
/// </summary>
public class MenuService : IMenuService
{
    private readonly IRepositoryManager _repoManager;
    private readonly IMapper _mapper;

    public MenuService(IRepositoryManager repoManager, IMapper mapper)
    {
        _repoManager = repoManager;
        _mapper = mapper;
    }

    public async Task<MenuBeanDto> GetMenuByIdAsync(int id)
    {
        var menu = await _repoManager.Menus.GetByIdAsync(id);
        return _mapper.Map<MenuBeanDto>(menu);
    }

    public async Task<List<MenuBeanDto>> GetAllMenusAsync()
    {
        var menus = await _repoManager.Menus.GetAllAsync();
        return _mapper.Map<List<MenuBeanDto>>(menus);
    }

    public async Task<int> CreateMenuAsync(MenuBeanDto menuDto)
    {
        try
        {
            _repoManager.BeginTranAsync();
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

    public async Task UpdateMenuAsync(MenuBeanDto menuDto)
    {
        try
        {
            _repoManager.BeginTranAsync();
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

    public async Task DeleteMenuAsync(int id)
    {
        try
        {
            _repoManager.BeginTranAsync();
            await _repoManager.Menus.DeleteAsync(id);
            await _repoManager.CommitAsync();
        }
        catch (Exception ex)
        {
            await _repoManager.RollbackAsync();
            throw new ApplicationException("删除菜单时发生错误，操作已回滚。", ex);
        }
    }
}