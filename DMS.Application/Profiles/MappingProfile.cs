using AutoMapper;
using DMS.Core.Models;
using DMS.Application.DTOs;

namespace DMS.Application.Profiles;

/// <summary>
/// 配置AutoMapper的映射规则。
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Device 映射
        CreateMap<CreateDeviceDto, Device>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.VariableTables, opt => opt.Ignore());

        CreateMap<UpdateDeviceDto, Device>()
            // 1. 首先，忽略那些永远不应从DTO更新的属性
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Description, opt => opt.Ignore())
            .ForMember(dest => dest.VariableTables, opt => opt.Ignore())
            .ForMember(dest => dest.CpuType, opt => opt.Ignore())
            .ForMember(dest => dest.DeviceType, opt => opt.Ignore())

            // 2. 然后，为每个可空属性单独设置条件
            .ForMember(dest => dest.Name, opt => opt.Condition(src => src.Name != null))
            .ForMember(dest => dest.Protocol, opt => opt.Condition(src => src.Protocol.HasValue))
            .ForMember(dest => dest.IpAddress, opt => opt.Condition(src => src.IpAddress != null))
            .ForMember(dest => dest.Port, opt => opt.Condition(src => src.Port.HasValue))
            .ForMember(dest => dest.Rack, opt => opt.Condition(src => src.Rack.HasValue))
            .ForMember(dest => dest.Slot, opt => opt.Condition(src => src.Slot.HasValue))
            .ForMember(dest => dest.OpcUaServerUrl, opt => opt.Condition(src => src.OpcUaServerUrl != null))
            .ForMember(dest => dest.IsActive, opt => opt.Condition(src => src.IsActive.HasValue));

        CreateMap<Device, DeviceDto>()
            .ForMember(dest => dest.Protocol, opt => opt.MapFrom(src => src.Protocol.ToString()))
            .ForMember(dest => dest.Status, opt => opt.Ignore());

        // VariableTable 映射
        CreateMap<VariableTable, VariableTableDto>().ReverseMap();

        // Variable 映射
        CreateMap<Variable, VariableDto>()
            .ForMember(dest => dest.DataType, opt => opt.MapFrom(src => src.DataType.ToString()))
            .ForMember(dest => dest.CSharpDataType, opt => opt.MapFrom(src => src.CSharpDataType))
            .ForMember(dest => dest.Address, opt => opt.Ignore());

        // MqttServer 映射
        CreateMap<MqttServer, MqttServerDto>().ReverseMap();

        // VariableMqttAlias 映射
        CreateMap<VariableMqttAlias, VariableMqttAliasDto>()
            .ForMember(dest => dest.MqttServerName, opt => opt.Ignore())
            .ReverseMap();

        // VariableHistory 映射
        CreateMap<VariableHistory, VariableHistoryDto>().ReverseMap();

        // MenuBean 映射
        CreateMap<MenuBean, MenuBeanDto>().ReverseMap();

        // User 映射
        CreateMap<User, UserDto>().ReverseMap();
    }
}
