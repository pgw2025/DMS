using AutoMapper;
using DMS.Application.DTOs;
using DMS.Core.Models;
using DMS.Core.Models.Triggers;
using DMS.Infrastructure.Models;
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
                .ForMember(dest => dest.Triggers, opt => opt.Ignore())
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
            CreateMap<TriggerItem, TriggerItem>().ReverseMap();

            // 添加触发器相关映射
            CreateMap<TriggerItem, Trigger>()
                .ForMember(dest => dest.Variables, opt => opt.Ignore());
            CreateMap<Trigger, TriggerItem>()
                .ForMember(dest => dest.Variables, opt => opt.Ignore());
        }
    }
}
