using DMS.Application.DTOs;
using DMS.WPF.ItemViewModel;
using DMS.Application.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace DMS.WPF.Factories
{
    /// <summary>
    /// VariableItemViewModel工厂类
    /// 专门用于构建VariableItemViewModel实例
    /// </summary>
    public interface IVariableItemViewModelFactory
    {
        /// <summary>
        /// 从VariableDto创建VariableItemViewModel实例
        /// </summary>
        /// <param name="variableDto">变量数据传输对象</param>
        /// <returns>VariableItemViewModel实例</returns>
        VariableItem CreateNewVariableItemViewModel();
        
    }

    public class VariableItemViewModelFactory : IVariableItemViewModelFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly AppSettings _appSettings;

        public VariableItemViewModelFactory(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        /// <summary>
        /// 从VariableDto创建VariableItemViewModel实例
        /// </summary>
        /// <param name="variableDto">变量数据传输对象</param>
        /// <returns>VariableItemViewModel实例</returns>
        public VariableItem CreateNewVariableItemViewModel()
        {

            var viewModel = new VariableItem()
            {
               
                IsActive = _appSettings.VariableImportTemplate.IsActive,                
                IsHistoryEnabled = _appSettings.VariableImportTemplate.IsHistoryEnabled,                  
                HistoryDeadband = _appSettings.VariableImportTemplate.HistoryDeadband,                
                PollingInterval = _appSettings.VariableImportTemplate.PollingInterval,                
            };


            return viewModel;
        }

    }
}