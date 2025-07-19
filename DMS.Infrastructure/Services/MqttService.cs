using AutoMapper;
using DMS.Core.Enums;
using DMS.Core.Helper;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;
using DMS.Infrastructure.Repositories;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Services
{
    public class MqttService
    {
        private readonly MqttRepository _mqttRepository;
        private readonly MenuRepository _menuRepository;
        private readonly IMapper _mapper;
        private readonly SqlSugarDbContext _dbContext;
        private SqlSugarClient Db => _dbContext.GetInstance();

        public MqttService(MqttRepository mqttRepository, MenuRepository menuRepository, IMapper mapper, SqlSugarDbContext dbContext)
        {
            _mqttRepository = mqttRepository;
            _menuRepository = menuRepository;
            _mapper = mapper;
            _dbContext = dbContext;
        }

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
                    Type = MenuType.MqttMenu,
                    DataId = result,
                    ParentId = mqttMenu.Id,
                };
                await _menuRepository.AddAsync(_mapper.Map<DbMenu>(menu));
                await Db.CommitTranAsync();
                stopwatch.Stop();
                NlogHelper.Info($"新增Mqtt配置 '{mqtt.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
                return result;
            }
            catch (Exception ex)
            {
                await Db.RollbackTranAsync();
                NlogHelper.Error($"添加MQTT配置 {mqtt.Name} 失败", ex);
                throw;
            }
        }

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
                    await _menuRepository.UpdateAsync(_mapper.Map<DbMenu>(menu));
                }

                await Db.CommitTranAsync();
                stopwatch.Stop();
                NlogHelper.Info($"更新Mqtt配置 '{mqtt.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
                return result;
            }
            catch (Exception ex)
            {
                await Db.RollbackTranAsync();
                NlogHelper.Error($"更新MQTT配置 {mqtt.Name} 失败", ex);
                throw;
            }
        }

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
                    await _menuRepository.DeleteAsync(_mapper.Map<DbMenu>(menu));
                }

                await Db.CommitTranAsync();
                stopwatch.Stop();
                NlogHelper.Info($"删除Mqtt配置ID '{mqtt.Id}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
                return result;
            }
            catch (Exception ex)
            {
                await Db.RollbackTranAsync();
                NlogHelper.Error($"删除MQTT配置 {mqtt.Name} 失败", ex);
                throw;
            }
        }
    }
}
