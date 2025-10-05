using AutoMapper;
using DMS.Application.DTOs;
using DMS.Core.Models;
using DMS.Infrastructure.Models;
using DMS.WPF.ViewModels.Items;

namespace DMS.WPF.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<DeviceDto, DeviceItemViewModel>()
                .ReverseMap();
            CreateMap<Variable, VariableItemViewModel>()
                .ReverseMap();
            CreateMap<OpcUaNode, OpcUaNodeItemViewModel>()
                .ReverseMap();
            CreateMap<VariableItemViewModel, VariableItemViewModel>();
            CreateMap<VariableMqttAliasDto, VariableMqttAliasItemViewModel>().ReverseMap();
            CreateMap<VariableMqttAlias, VariableMqttAliasItemViewModel>().ReverseMap();
                

            CreateMap<MenuBeanDto, MenuItemViewModel>()
                .ReverseMap();
                
            CreateMap<MqttServerDto, MqttServerItemViewModel>().ReverseMap();
            CreateMap<MqttServerItemViewModel, MqttServerItemViewModel>().ReverseMap();

            CreateMap<MqttServer, MqttServerItemViewModel>().ReverseMap();
            CreateMap<UserDto, UserItemViewModel>().ReverseMap();
            CreateMap<VariableHistoryDto, VariableHistoryItemViewModel>().ReverseMap();
            CreateMap<VariableDto, VariableItemViewModel>()
                .ReverseMap();
            CreateMap<VariableMqttAliasDto, VariableMqttAliasItemViewModel>().ReverseMap();
            CreateMap<VariableTableDto, VariableTableItemViewModel>().ReverseMap();
            CreateMap<NlogDto, NlogItemViewModel>().ReverseMap();
        }
    }
}