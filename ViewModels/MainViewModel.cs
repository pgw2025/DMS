using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using PMSWPF.Data.Entities;
using PMSWPF.Message;
using System.Collections.ObjectModel;

namespace PMSWPF.ViewModels
{
    partial class MainViewModel : ObservableRecipient, IRecipient<MyMessage>
    {
       
        public MainViewModel()
        { 
            IsActive = true;
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
