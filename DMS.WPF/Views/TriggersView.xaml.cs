using System.Windows.Controls;
using DMS.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DMS.WPF.Views
{
    /// <summary>
    /// TriggersView.xaml 的交互逻辑
    /// </summary>
    public partial class TriggersView : UserControl
    {
        public TriggersView()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetRequiredService<TriggersViewModel>();
        }
    }
}