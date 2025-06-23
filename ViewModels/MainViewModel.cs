using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using PMSWPF.Data.Entities;
using PMSWPF.Message;
using System.Collections.ObjectModel;
using PMSWPF.Services;

namespace PMSWPF.ViewModels
{
   public partial class MainViewModel : ObservableRecipient, IRecipient<MyMessage>
    {
        private readonly NavgatorServices _navgatorServices;

        [ObservableProperty]
       private ViewModelBase currentViewModel;
        public MainViewModel(NavgatorServices navgatorServices)
        {
            _navgatorServices = navgatorServices;
            _navgatorServices.OnViewModelChanged += () =>
            {
                CurrentViewModel = _navgatorServices.CurrentViewModel;
            };
            IsActive = true;
            CurrentViewModel = new HomeViewModel();
        }

        public void NavgateTo<T>() where T : ViewModelBase
        {
            _navgatorServices.NavigateTo<T>();
        }

        string text = "Hello Count:";

        [ObservableProperty]
        string message;

        public void Receive(MyMessage message)
        {
            Message = text + message.Count;
        }
    }
}
