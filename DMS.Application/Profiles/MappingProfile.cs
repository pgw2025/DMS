using AutoMapper;
using DMS.Application.DTOs;
using DMS.Core.Models;
using DMS.Core.Models.Triggers;

namespace DMS.Application.Profiles;

/// <summary>
/// 配置AutoMapper的映射规则�?/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {

        // Variable 映射



        // VariableHistory 映射
        CreateMap<VariableHistory, VariableHistoryDto>()
            .ForMember(dest => dest.VariableName, opt => opt.MapFrom(src => src.Variable.Name))
            .ReverseMap();



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
