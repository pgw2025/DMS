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
    public class VariableTableAppService : IVariableTableAppService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMapper _mapper;

        public VariableTableAppService(IRepositoryManager repositoryManager, IMapper mapper)
        {
            _repositoryManager = repositoryManager;
            _mapper = mapper;
        }

        public async Task<VariableTableDto> GetVariableTableByIdAsync(int id)
        {
            var variableTable = await _repositoryManager.VariableTables.GetByIdAsync(id);
            return _mapper.Map<VariableTableDto>(variableTable);
        }

        public async Task<List<VariableTableDto>> GetAllVariableTablesAsync()
        {
            var variableTables = await _repositoryManager.VariableTables.GetAllAsync();
            return _mapper.Map<List<VariableTableDto>>(variableTables);
        }

        public async Task<VariableTableDto> CreateVariableTableAsync(CreateVariableTableWithMenuDto createDto)
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

                if (createDto.Menu!=null)
                {
                    var deviceMenu
                        = await _repositoryManager.Menus.GetMenuByTargetIdAsync(
                            MenuType.DeviceMenu, createDto.DeviceId);
                    if (deviceMenu == null)
                    {
                        throw new ApplicationException($"添加变量表菜单时，找不到设备ID:{createDto.DeviceId},请检查。");
                    }

                    var menu = _mapper.Map<MenuBean>(createDto.Menu);
                    menu.ParentId = deviceMenu.Id;
                    menu.TargetId = createdVariableTable.Id;
                    menu.MenuType = MenuType.VariableTableMenu;
                    await _repositoryManager.Menus.AddAsync(menu);
                }
                


                await _repositoryManager.CommitAsync();

                return _mapper.Map<VariableTableDto>(createdVariableTable);
            }
            catch
            {
                await _repositoryManager.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateVariableTableAsync(VariableTableDto variableTableDto)
        {
            var variableTable = _mapper.Map<VariableTable>(variableTableDto);
            await _repositoryManager.VariableTables.UpdateAsync(variableTable);
        }

        public async Task DeleteVariableTableAsync(int id)
        {
            await _repositoryManager.VariableTables.DeleteByIdAsync(id);
        }
    }
}