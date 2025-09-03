using DMS.WPF.ViewModels.Dialogs;

namespace DMS.WPF.Interfaces
{
    public interface IDialogService
    {
        Task<TResult> ShowDialogAsync<TResult>(DialogViewModelBase<TResult> viewModel);
    }
}