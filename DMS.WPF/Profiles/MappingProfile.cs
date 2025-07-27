using AutoMapper;
using DMS.Application.DTOs;
using DMS.WPF.ViewModels.Items;

namespace DMS.WPF.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<DeviceDto, DeviceItemViewModel>().ConstructUsing(src => new DeviceItemViewModel(src));
            CreateMap<DeviceItemViewModel, DeviceDto>();
            CreateMap<MenuBeanDto, MenuBeanItemViewModel>()
                .ForMember(dest => dest.Children, opt => opt.Ignore())
                .ConstructUsing(src => new MenuBeanItemViewModel(src, null)); // 假设 NavigationService 可以通过依赖注入获取或在ViewModel中处理
            CreateMap<MqttServerDto, MqttServerItemViewModel>().ConstructUsing(src => new MqttServerItemViewModel(src));
            CreateMap<UserDto, UserItemViewModel>().ConstructUsing(src => new UserItemViewModel(src));
            CreateMap<VariableHistoryDto, VariableHistoryItemViewModel>().ConstructUsing(src => new VariableHistoryItemViewModel(src));
            CreateMap<VariableDto, VariableItemViewModel>().ConstructUsing(src => new VariableItemViewModel(src));
            CreateMap<VariableMqttAliasDto, VariableMqttAliasItemViewModel>().ConstructUsing(src => new VariableMqttAliasItemViewModel(src));
            CreateMap<VariableTableDto, VariableTableItemViewModel>().ConstructUsing(src => new VariableTableItemViewModel(src));
        }
    }
}