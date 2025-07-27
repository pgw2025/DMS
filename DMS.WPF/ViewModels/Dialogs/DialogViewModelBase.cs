using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace DMS.WPF.ViewModels.Dialogs
{
    public abstract partial class DialogViewModelBase<TResult> : ObservableObject
    {
        [ObservableProperty]
        private string _title;
        
        [ObservableProperty]
        private string _primaryButContent;

        public event Func<TResult, Task> CloseRequested;

        [RelayCommand]
        protected virtual async Task Close(TResult result)
        { 
            if (CloseRequested != null)
            {
                await CloseRequested(result);
            }
        }
    }
}