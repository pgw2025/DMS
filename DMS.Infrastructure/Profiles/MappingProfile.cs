using System;
using AutoMapper;
using DMS.Infrastructure.Entities;
using DMS.Core.Models;

namespace DMS.Infrastructure.Profiles;

/// <summary>
/// AutoMapper 的配置类，用于定义对象之间的映射规则。
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // --- 用户映射 ---


        // --- 设备映射 (包含List的父对象) ---
        // AutoMapper 会自动使用上面的规则来处理 VariableTables 属性
        CreateMap<DbDevice, Core.Models.Device>()
            .ReverseMap();

        // --- 变量表映射 (List中的元素) ---
        CreateMap<DbVariableTable, VariableTable>()
            .ReverseMap();
        
        CreateMap<DbVariable, Variable>().ReverseMap();
        // --- MQTT 和 变量数据 映射 ---
        CreateMap<DbMqttServer, MqttServer>()
            .ForMember(dest => dest.VariableAliases, opt => opt.Ignore())
            .ReverseMap();
        CreateMap<DbVariableMqttAlias, VariableMqttAlias>()
            .ReverseMap();
        
        CreateMap<DbMenu, MenuBean>().ReverseMap();
        
        CreateMap<DbNlog, Nlog>().ReverseMap();
        
        CreateMap<DbVariableHistory, VariableHistory>().ReverseMap();
    }
}
