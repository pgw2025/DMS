using AutoMapper;
using DMS.Core.Models;
using DMS.Application.DTOs;
using DMS.Core.Models.Triggers;

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
        CreateMap<VariableDto, VariableDto>()
            .ReverseMap();
           
        // MqttServer 映射
        CreateMap<MqttServer, MqttServerDto>().ReverseMap();
        CreateMap<MqttServerDto, MqttServerDto>().ReverseMap();

        // VariableHistory 映射
        CreateMap<VariableHistory, VariableHistoryDto>()
            .ForMember(dest => dest.VariableName, opt => opt.MapFrom(src => src.Variable.Name))
            .ReverseMap();

        // MenuBean 映射
        CreateMap<MenuBean, MenuBeanDto>().ReverseMap();

        // User 映射
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<Nlog, NlogDto>().ReverseMap();
        
        // 邮件相关映射
        CreateMap<EmailAccount, EmailAccountDto>().ReverseMap();
        CreateMap<EmailAccount, CreateEmailAccountRequest>().ReverseMap();
        
        CreateMap<EmailMessage, EmailMessageDto>().ReverseMap();
        CreateMap<EmailMessage, SendEmailRequest>()
            .ForMember(dest => dest.EmailAccountId, opt => opt.MapFrom(src => src.EmailAccountId))
            .ReverseMap();
            
        CreateMap<EmailTemplate, EmailTemplateDto>().ReverseMap();
        
        CreateMap<EmailLog, EmailLogDto>().ReverseMap();
        CreateMap<TriggerDefinition, TriggerDefinitionDto>()
            .ForMember(dest => dest.VariableIds, opt => opt.MapFrom(src => src.VariableIds))
            .ReverseMap()
            .ForMember(dest => dest.VariableIds, opt => opt.MapFrom(src => src.VariableIds));
    }
}
