using System.Diagnostics;
using AutoMapper;
using DMS.Infrastructure.Entities;
using DMS.Core.Enums;
using DMS.Core.Helper;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Interfaces;

namespace DMS.Infrastructure.Repositories;

/// <summary>
/// Mqtt仓储类，用于操作DbMqtt实体
/// </summary>
public class MqttRepository : BaseRepository<DbMqtt, Mqtt>
{
    private readonly MenuRepository _menuRepository;

    public MqttRepository(MenuRepository menuRepository, IMapper mapper, ITransaction transaction)
        : base(mapper, transaction)
    {
        _menuRepository = menuRepository;
    }

    /// <summary>
    /// 根据ID获取Mqtt配置
    /// </summary>
    /// <param name="id">主键ID</param>
    /// <returns></returns>
    public async Task<Mqtt> GetByIdAsync(int id)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await Db.Queryable<DbMqtt>()
                             .In(id)
                             .SingleAsync();
        stopwatch.Stop();
        NlogHelper.Info($"根据ID '{id}' 获取Mqtt配置耗时：{stopwatch.ElapsedMilliseconds}ms");
        return _mapper.Map<Mqtt>(result);
    }

    /// <summary>
    /// 获取所有Mqtt配置
    /// </summary>
    /// <returns></returns>
    public async Task<List<Mqtt>> GetAllAsync()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await Db.Queryable<DbMqtt>()
                             .Includes(m => m.VariableMqtts, vm => vm.Variable)
                             .Includes(m => m.VariableMqtts, vm => vm.Mqtt)
                             .ToListAsync();
        stopwatch.Stop();
        NlogHelper.Info($"获取所有Mqtt配置耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result.Select(m => _mapper.Map<Mqtt>(m))
                     .ToList();
    }

    /// <summary>
    /// 新增Mqtt配置
    /// </summary>
    /// <param name="mqtt">Mqtt实体</param>
    /// <returns></returns>
    public async Task<int> AddAsync(Mqtt mqtt)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        await Db.BeginTranAsync();
        try
        {
            var result = await Db.Insertable(_mapper.Map<DbMqtt>(mqtt))
                                 .ExecuteReturnIdentityAsync();
            var mqttMenu = await _menuRepository.GetMainMenuByNameAsync("Mqtt服务器");
            // AddAsync menu entry
            var menu = new MenuBean()
                       {
                           Name = mqtt.Name,
                           // Icon = SegoeFluentIcons.Wifi.Glyph,
                           Type = MenuType.MqttMenu,
                           DataId = result,
                           ParentId = mqttMenu.Id,
                       };
            await _menuRepository.AddAsync(menu, Db);
            await Db.CommitTranAsync();
            stopwatch.Stop();
            NlogHelper.Info($"新增Mqtt配置 '{mqtt.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
        catch (Exception ex)
        {
            await Db.RollbackTranAsync();
            NlogHelper.Error($"添加MQTT配置 {{mqtt.Name}} 失败", ex);
            throw;
        }
    }

    /// <summary>
    /// 更新Mqtt配置
    /// </summary>
    /// <param name="mqtt">Mqtt实体</param>
    /// <returns></returns>
    public async Task<int> UpdateAsync(Mqtt mqtt)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        await Db.BeginTranAsync();
        try
        {
            var result = await Db.Updateable(_mapper.Map<DbMqtt>(mqtt))
                                 .ExecuteCommandAsync();
            // Update menu entry
            var menu = await _menuRepository.GetMenuByDataIdAsync(mqtt.Id, MenuType.MqttMenu);
            if (menu != null)
            {
                menu.Name = mqtt.Name;
                await _menuRepository.UpdateAsync(menu, Db);
            }

            await Db.CommitTranAsync();
            stopwatch.Stop();
            NlogHelper.Info($"更新Mqtt配置 '{mqtt.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
        catch (Exception ex)
        {
            await Db.RollbackTranAsync();
            NlogHelper.Error($"更新MQTT配置 {{mqtt.Name}} 失败", ex);
            throw;
        }
    }

    /// <summary>
    /// 根据ID删除Mqtt配置
    /// </summary>
    /// <param name="mqtt">Mqtt实体</param>
    /// <returns></returns>
    public async Task<int> DeleteAsync(Mqtt mqtt)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        await Db.BeginTranAsync();
        try
        {
            var result = await Db.Deleteable<DbMqtt>()
                                 .In(mqtt.Id)
                                 .ExecuteCommandAsync();
            // DeleteAsync menu entry
            var menu = await _menuRepository.GetMenuByDataIdAsync(mqtt.Id, MenuType.MqttMenu);
            if (menu != null)
            {
                await _menuRepository.DeleteAsync(menu, Db);
            }

            await Db.CommitTranAsync();
            stopwatch.Stop();
            NlogHelper.Info($"删除Mqtt配置ID '{mqtt.Id}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
        catch (Exception ex)
        {
            await Db.RollbackTranAsync();
            NlogHelper.Error($"删除MQTT配置 {{mqtt.Name}} 失败", ex);
            throw;
        }
    }
}