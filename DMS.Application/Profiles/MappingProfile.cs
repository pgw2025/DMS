using AutoMapper;
using DMS.Core.Models;
using DMS.Application.DTOs;
using DMS.Core.Enums;

namespace DMS.Application.Profiles;

/// <summary>
/// 配置AutoMapper的映射规则。
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Device 映射
        CreateMap<CreateDeviceDto, Device>();
        CreateMap<UpdateDeviceDto, Device>();
        CreateMap<Device, DeviceDto>()
            .ForMember(dest => dest.Protocol, opt => opt.MapFrom(src => src.Protocol.ToString()));

        // VariableTable 映射
        CreateMap<VariableTable, VariableTableDto>().ReverseMap();

        // Variable 映射
        CreateMap<Variable, VariableDto>()
            .ForMember(dest => dest.DataType, opt => opt.MapFrom(src => src.DataType.ToString()))
            .ForMember(dest => dest.CSharpDataType, opt => opt.MapFrom(src => src.CSharpDataType));

        // MqttServer 映射
        CreateMap<MqttServer, MqttServerDto>().ReverseMap();

        // VariableMqttAlias 映射
        CreateMap<VariableMqttAlias, VariableMqttAliasDto>().ReverseMap();

        // VariableTable 映射
        CreateMap<VariableTable, VariableTableDto>().ReverseMap();

        // VariableHistory 映射
        CreateMap<VariableHistory, VariableHistoryDto>().ReverseMap();

        // MenuBean 映射
        CreateMap<MenuBean, MenuBeanDto>().ReverseMap();

        // User 映射
        CreateMap<User, UserDto>().ReverseMap();
    }
}