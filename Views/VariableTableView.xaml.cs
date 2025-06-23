using System.Windows.Controls;
using PMSWPF.ViewModels;

namespace PMSWPF.Views;

public partial class VariableTableView : UserControl
{
    public VariableTableView(VariableTableViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}