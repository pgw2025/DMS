using Microsoft.Win32;
using System.IO;
using System.Windows;
using DMS.WPF.ViewModels.Dialogs;
using iNKORE.UI.WPF.Modern.Controls;

namespace DMS.Views.Dialogs;

public partial class ImportExcelDialog : ContentDialog
{
    public ImportExcelDialog(ImportExcelDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void Dialog_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
    }

    private void Dialog_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                string extension = Path.GetExtension(files[0]);
                if (extension.Equals(".xls", StringComparison.OrdinalIgnoreCase) || 
                    extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    if (DataContext is ImportExcelDialogViewModel viewModel)
                    {
                        viewModel.FilePath = files[0];
                    }
                }
                else
                {
                    // MessageBox.Show("Please drop a valid Excel file (.xls or .xlsx).", "Invalid File Type", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    private void SelectFile_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Excel Files (*.xlsx)|*.xlsx|All files (*.*)|*.*"
        };
        if (openFileDialog.ShowDialog() == true)
        {
            if (DataContext is ImportExcelDialogViewModel viewModel)
            {
                viewModel.FilePath = openFileDialog.FileName;
            }
        }
    }
}