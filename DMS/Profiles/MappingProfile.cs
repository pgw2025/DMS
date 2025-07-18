using System;
using AutoMapper;
using DMS.Data.Entities;
using DMS.Models;

namespace DMS.Profiles;

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
        CreateMap<DbDevice, Device>().ReverseMap();

        // --- 变量表映射 (List中的元素) ---
        CreateMap<DbVariableTable, VariableTable>().ReverseMap();
        
        CreateMap<DbVariable, Variable>().ReverseMap();
        // --- MQTT 和 变量数据 映射 ---
        CreateMap<DbMqtt, Mqtt>().ReverseMap();
        CreateMap<DbVariableMqtt, VariableMqtt>().ReverseMap();
        
        CreateMap<DbMenu, MenuBean>().ReverseMap();
        
    }
}
