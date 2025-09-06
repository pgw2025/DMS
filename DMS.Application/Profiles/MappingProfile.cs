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



        CreateMap<Device, DeviceDto>()
            .ReverseMap();
          
           

        // VariableTable 映射
        CreateMap<VariableTable, VariableTableDto>().ReverseMap();

        // Variable 映射
        CreateMap<Variable, VariableDto>()
            .ReverseMap();


        CreateMap<VariableDto, Variable>()
            .ReverseMap();
           

        // MqttServer 映射
        CreateMap<MqttServer, MqttServerDto>().ReverseMap();

        // VariableMqttAlias 映射
        CreateMap<VariableMqttAlias, VariableMqttAliasDto>()
            .ForMember(dest => dest.MqttServerName, opt => opt.MapFrom(src => src.MqttServer.ServerName))
            .ReverseMap();

        // VariableHistory 映射
        CreateMap<VariableHistory, VariableHistoryDto>().ReverseMap();

        // MenuBean 映射
        CreateMap<MenuBean, MenuBeanDto>().ReverseMap();

        // User 映射
        CreateMap<User, UserDto>().ReverseMap();
    }
}
