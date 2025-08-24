using DMS.WPF.ViewModels.Dialogs;
using DMS.WPF.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using DMS.WPF.Views;
using iNKORE.UI.WPF.Modern.Controls;

namespace DMS.WPF.Services
{
    public class DialogService : IDialogService
    {
        private readonly IServiceProvider _serviceProvider;
        private static readonly Dictionary<Type, Type> _viewModelViewMap = new Dictionary<Type, Type>
        {
            { typeof(DeviceDialogViewModel), typeof(DeviceDialog) },
            { typeof(ConfirmDialogViewModel), typeof(ConfirmDialog) },
            { typeof(VariableTableDialogViewModel), typeof(VariableTableDialog) },
            { typeof(ImportExcelDialogViewModel), typeof(ImportExcelDialog) },
            { typeof(VariableDialogViewModel), typeof(VariableDialog) },
            // { typeof(MqttDialogViewModel), typeof(MqttDialog) }, // Add other mappings here
            // ... other dialogs
        };

        public DialogService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<TResult> ShowDialogAsync<TResult>(DialogViewModelBase<TResult> viewModel)
        {
            if (_viewModelViewMap.TryGetValue(viewModel.GetType(), out var viewType))
            {
                var tcs = new TaskCompletionSource<TResult>();

                var dialog = (ContentDialog)_serviceProvider.GetService(viewType);
                if (dialog == null)
                {
                    // If not registered in DI, create an instance directly
                    dialog = (ContentDialog)Activator.CreateInstance(viewType);
                }

                dialog.DataContext = viewModel;

                Func<TResult, Task> closeHandler = null;
                closeHandler = async (result) =>
                {
                    viewModel.CloseRequested -= closeHandler;
                    dialog.Hide();
                    tcs.SetResult(result);
                };

                viewModel.CloseRequested += closeHandler;

                _ = await dialog.ShowAsync();

                return await tcs.Task;
            }
            else
            {
                throw new ArgumentException($"No view registered for view model {viewModel.GetType().Name}");
            }
        }
    }
}