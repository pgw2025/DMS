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
            CreateMap<Device, DeviceItem>()
                .ReverseMap();
            CreateMap<Variable, VariableItem>()
                .ForMember(dest => dest.MqttAliases, opt => opt.Ignore())
                .ReverseMap();
            CreateMap<VariableTable, VariableTableItem>()
                .ReverseMap();
            CreateMap<OpcUaNode, OpcUaNodeItem>()
                .ReverseMap();
            CreateMap<VariableItem, VariableItem>();
            CreateMap<MqttAlias, MqttAliasItem>()
                .ReverseMap();


            CreateMap<MenuBean, MenuItem>()
                .ReverseMap();



            CreateMap<MqttServer, MqttServerItem>()
                .ForMember(dest => dest.VariableAliases, opt => opt.Ignore())
                .ReverseMap();
            CreateMap<UserDto, UserItem>().ReverseMap();
            CreateMap<VariableHistoryDto, VariableHistoryItem>().ReverseMap();
            CreateMap<Variable, VariableItem>()
                .ReverseMap();
            CreateMap<NlogDto, NlogItem>().ReverseMap();
        }
    }
}
