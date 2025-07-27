using DMS.WPF.ViewModels.Dialogs;
using System.Threading.Tasks;

namespace DMS.WPF.Services
{
    public interface IDialogService
    {
        Task<TResult> ShowDialogAsync<TResult>(DialogViewModelBase<TResult> viewModel);
    }
}