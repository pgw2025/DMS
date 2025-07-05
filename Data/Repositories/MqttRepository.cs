using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NLog;
using PMSWPF.Data.Entities;
using PMSWPF.Models;
using PMSWPF.Extensions;
using SqlSugar;
using PMSWPF.Helper;
using iNKORE.UI.WPF.Modern.Common.IconKeys;
using PMSWPF.Enums;

namespace PMSWPF.Data.Repositories;

/// <summary>
/// Mqtt仓储类，用于操作DbMqtt实体
/// </summary>
public class MqttRepository
{
    private readonly MenuRepository _menuRepository;
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    public MqttRepository()
    {
        _menuRepository = new MenuRepository();
    }

    /// <summary>
    /// 根据ID获取Mqtt配置
    /// </summary>
    /// <param name="id">主键ID</param>
    /// <returns></returns>
    public async Task<Mqtt> GetById(int id)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var _db = DbContext.GetInstance())
        {
            var result = await _db.Queryable<DbMqtt>()
                                  .In(id)
                                  .SingleAsync();
            stopwatch.Stop();
            Logger.Info($"根据ID '{id}' 获取Mqtt配置耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result.CopyTo<Mqtt>();
        }
    }

    /// <summary>
    /// 获取所有Mqtt配置
    /// </summary>
    /// <returns></returns>
    public async Task<List<Mqtt>> GetAll()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var _db = DbContext.GetInstance())
        {
            var result = await _db.Queryable<DbMqtt>().Includes(m=>m.VariableDatas)
                                  .ToListAsync();
            stopwatch.Stop();
            Logger.Info($"获取所有Mqtt配置耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result.Select(m => m.CopyTo<Mqtt>())
                         .ToList();
        }
    }

    /// <summary>
    /// 新增Mqtt配置
    /// </summary>
    /// <param name="mqtt">Mqtt实体</param>
    /// <returns></returns>
    public async Task<int> Add(Mqtt mqtt)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using var db = DbContext.GetInstance();
        await db.BeginTranAsync();
        try
        {
            var result = await db.Insertable(mqtt.CopyTo<DbMqtt>())
                                 .ExecuteReturnIdentityAsync();
            var mqttMenu = await _menuRepository.GetMainMenuByName("Mqtt服务器");
            // Add menu entry
            var menu = new MenuBean()
                       {
                           Name = mqtt.Name,
                           Icon = SegoeFluentIcons.Wifi.Glyph,
                           Type = MenuType.MqttMenu,
                           DataId = result,
                           ParentId = mqttMenu.Id,
                       };
            await _menuRepository.Add(menu, db);
            await db.CommitTranAsync();
            stopwatch.Stop();
            Logger.Info($"新增Mqtt配置 '{mqtt.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
        catch (Exception ex)
        {
            await db.RollbackTranAsync();
            Logger.Error(ex, $"添加MQTT配置 {{mqtt.Name}} 失败");
            throw;
        }
    }

    /// <summary>
    /// 更新Mqtt配置
    /// </summary>
    /// <param name="mqtt">Mqtt实体</param>
    /// <returns></returns>
    public async Task<int> Edit(Mqtt mqtt)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var db = DbContext.GetInstance())
        {
            await db.BeginTranAsync();
            try
            {
                var result = await db.Updateable(mqtt.CopyTo<DbMqtt>())
                                     .ExecuteCommandAsync();
                // Update menu entry
                var menu = await _menuRepository.GetMenuByDataId(mqtt.Id, MenuType.MqttMenu);
                if (menu != null)
                {
                    menu.Name = mqtt.Name;
                    await _menuRepository.Edit(menu, db);
                }

                await db.CommitTranAsync();
                stopwatch.Stop();
                Logger.Info($"更新Mqtt配置 '{mqtt.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
                return result;
            }
            catch (Exception ex)
            {
                await db.RollbackTranAsync();
                Logger.Error(ex, $"更新MQTT配置 {{mqtt.Name}} 失败");
                throw;
            }
        }
    }

    /// <summary>
    /// 根据ID删除Mqtt配置
    /// </summary>
    /// <param name="mqtt">Mqtt实体</param>
    /// <returns></returns>
    public async Task<int> Delete(Mqtt mqtt)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var db = DbContext.GetInstance())
        {
            await db.BeginTranAsync();
            try
            {
                var result = await db.Deleteable<DbMqtt>()
                                     .In(mqtt.Id)
                                     .ExecuteCommandAsync();
                // Delete menu entry
                var menu = await _menuRepository.GetMenuByDataId(mqtt.Id, MenuType.MqttMenu);
                await _menuRepository.DeleteMenu(menu, db);
                await db.CommitTranAsync();
                stopwatch.Stop();
                Logger.Info($"删除Mqtt配置ID '{mqtt.Id}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
                return result;
            }
            catch (Exception ex)
            {
                await db.RollbackTranAsync();
                Logger.Error(ex, $"删除MQTT配置 {{mqtt.Name}} 失败");
                throw;
            }
        }
    }
}