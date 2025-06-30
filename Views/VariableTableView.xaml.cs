using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using PMSWPF.ViewModels;

namespace PMSWPF.Views;

public partial class VariableTableView : UserControl
{
    public VariableTableView()
    {
        InitializeComponent();
        DataContext = App.Current.Services.GetRequiredService<VariableTableViewModel>();
    }
}