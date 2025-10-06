using AutoMapper;
using DMS.Application.DTOs;
using DMS.Core.Models;
using DMS.Infrastructure.Models;
using DMS.WPF.ItemViewModel;
using DMS.WPF.ItemViewModel;

namespace DMS.WPF.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<DeviceDto, DeviceItem>()
                .ReverseMap();
            CreateMap<Variable, VariableItem>()
                .ReverseMap();
            CreateMap<OpcUaNode, OpcUaNodeItem>()
                .ReverseMap();
            CreateMap<VariableItem, VariableItem>();
            CreateMap<MqttAlias, MqttAliasItem>().ReverseMap();
                

            CreateMap<MenuBeanDto, MenuItem>()
                .ReverseMap();
                
            CreateMap<MqttServerDto, MqttServerItem>().ReverseMap();
            CreateMap<MqttServerItem, MqttServerItem>().ReverseMap();

            CreateMap<MqttServer, MqttServerItem>().ReverseMap();
            CreateMap<UserDto, UserItem>().ReverseMap();
            CreateMap<VariableHistoryDto, VariableHistoryItem>().ReverseMap();
            CreateMap<VariableDto, VariableItem>()
                .ReverseMap();
            CreateMap<VariableTableDto, VariableTableItem>().ReverseMap();
            CreateMap<NlogDto, NlogItem>().ReverseMap();
        }
    }
}