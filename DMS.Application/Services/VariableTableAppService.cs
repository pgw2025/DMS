using AutoMapper;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Core.Interfaces;
using DMS.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.Core.Enums;

namespace DMS.Application.Services
{
    /// <summary>
    /// 变量表应用服务，负责处理变量表相关的业务逻辑。
    /// 实现 <see cref="IVariableTableAppService"/> 接口。
    /// </summary>
    public class VariableTableAppService : IVariableTableAppService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMapper _mapper;

        /// <summary>
        /// 构造函数，注入仓储管理器和AutoMapper。
        /// </summary>
        /// <param name="repositoryManager">仓储管理器实例。</param>
        /// <param name="mapper">AutoMapper 实例。</param>
        public VariableTableAppService(IRepositoryManager repositoryManager, IMapper mapper)
        {
            _repositoryManager = repositoryManager;
            _mapper = mapper;
        }

        /// <summary>
        /// 异步根据ID获取变量表。
        /// </summary>
        /// <param name="id">变量表ID。</param>
        /// <returns>变量表数据传输对象。</returns>
        public async Task<VariableTableDto> GetVariableTableByIdAsync(int id)
        {
            var variableTable = await _repositoryManager.VariableTables.GetByIdAsync(id);
            return _mapper.Map<VariableTableDto>(variableTable);
        }

        /// <summary>
        /// 异步获取所有变量表。
        /// </summary>
        /// <returns>变量表数据传输对象列表。</returns>
        public async Task<List<VariableTableDto>> GetAllVariableTablesAsync()
        {
            var variableTables = await _repositoryManager.VariableTables.GetAllAsync();
            return _mapper.Map<List<VariableTableDto>>(variableTables);
        }

        /// <summary>
        /// 异步创建变量表，并可选择性地创建关联菜单。
        /// </summary>
        /// <param name="createDto">包含变量表和菜单信息的创建数据传输对象。</param>
        /// <returns>创建后的变量表数据传输对象。</returns>
        /// <exception cref="ApplicationException">如果添加变量表失败或找不到设备菜单。</exception>
        public async Task<CreateVariableTableWithMenuDto> CreateVariableTableAsync(CreateVariableTableWithMenuDto createDto)
        {
            await _repositoryManager.BeginTranAsync();
            try
            {
                var variableTable = _mapper.Map<VariableTable>(createDto.VariableTable);
                variableTable.DeviceId = createDto.DeviceId;
                
                var createdVariableTable = await _repositoryManager.VariableTables.AddAsync(variableTable);
                if (createdVariableTable.Id == 0)
                {
                    throw new ApplicationException($"添加变量表失败，设备ID:{createDto.DeviceId},请检查。");
                }
                
                _mapper.Map(createdVariableTable, createDto.VariableTable);

                if (createDto.Menu!=null)
                {
                    // 获取设备菜单，作为变量表菜单的父级
                    var deviceMenu
                        = await _repositoryManager.Menus.GetMenuByTargetIdAsync(
                            MenuType.DeviceMenu, createDto.DeviceId);
                    if (deviceMenu == null)
                    {
                        throw new ApplicationException($"添加变量表菜单时，找不到设备ID:{createDto.DeviceId},请检查。");
                    }

                    // 映射菜单实体并设置关联信息
                    var menu = _mapper.Map<MenuBean>(createDto.Menu);
                    menu.ParentId = deviceMenu.Id;
                    menu.TargetViewKey = "VariableTableMenu";
                    menu.TargetId = createdVariableTable.Id;
                    menu.MenuType = MenuType.VariableTableMenu;
                   var addMenu= await _repositoryManager.Menus.AddAsync(menu);
                   _mapper.Map(addMenu, createDto.Menu);
                }
                


                await _repositoryManager.CommitAsync();

                return createDto;
            }
            catch
            {
                await _repositoryManager.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// 异步更新变量表。
        /// </summary>
        /// <param name="variableTableDto">要更新的变量表数据传输对象。</param>
        /// <returns>受影响的行数。</returns>
        /// <exception cref="ApplicationException">如果找不到变量表。</exception>
        public async Task<int> UpdateVariableTableAsync(VariableTableDto variableTableDto)
        {
            try
            {
                await _repositoryManager.BeginTranAsync();
                var variableTable = await _repositoryManager.VariableTables.GetByIdAsync(variableTableDto.Id);
                if (variableTable == null)
                {
                    throw new ApplicationException($"VariableTable with ID {variableTableDto.Id} not found.");
                }

                _mapper.Map(variableTableDto, variableTable);
                int res = await _repositoryManager.VariableTables.UpdateAsync(variableTable);
                await _repositoryManager.CommitAsync();
                return res;
            }
            catch (Exception ex)
            {
                await _repositoryManager.RollbackAsync();
                // 可以在此记录日志
                throw new ApplicationException($"更新变量表时发生错误，操作已回滚,错误信息:{ex.Message}", ex);
            }
        }

        /// <summary>
        /// 异步根据ID删除变量表，包括其关联的变量、MQTT别名和菜单（事务性操作）。
        /// </summary>
        /// <param name="id">要删除的变量表ID。</param>
        /// <returns>如果删除成功则为 true，否则为 false。</returns>
        /// <exception cref="InvalidOperationException">如果删除变量表失败。</exception>
        /// <exception cref="ApplicationException">如果删除变量表时发生其他错误。</exception>
        public async Task<bool> DeleteVariableTableAsync(int id)
        {
            try
            {
                await _repositoryManager.BeginTranAsync();
                var delRes = await _repositoryManager.VariableTables.DeleteByIdAsync(id);
                if (delRes == 0)
                {
                    throw new InvalidOperationException($"删除变量表失败：变量表ID:{id}，请检查变量表Id是否存在");
                }

                // 删除关联的变量
                await _repositoryManager.Variables.DeleteByIdAsync(id);

                // 删除关联的MQTT别名
                // await _repositoryManager.VariableMqttAlias.DeleteByVariableTableIdAsync(id);

                // 删除关联的菜单树
                await _repositoryManager.Menus.DeleteMenuTreeByTargetIdAsync(MenuType.VariableTableMenu, id);

                await _repositoryManager.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _repositoryManager.RollbackAsync();
                // 可以在此记录日志
                throw new ApplicationException($"删除变量表时发生错误，操作已回滚,错误信息:{ex.Message}", ex);
            }
        }
    }
}