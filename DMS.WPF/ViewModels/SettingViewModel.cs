using CommunityToolkit.Mvvm.Input;
using DMS.WPF.Helper;

namespace DMS.WPF.ViewModels;

public partial class SettingViewModel : ViewModelBase
{
    // private AppSettings _settings;

    public SettingViewModel()
    {
        // _settings = AppSettings.Load();
        // AvailableDbTypes = Enum.GetNames(typeof(SqlSugar.DbType)).ToList();
        // Themes = new List<string> { "浅色", "深色", "跟随系统" };
        // this.transaction = transaction;
    }

    public List<string> Themes { get; }

    // public string SelectedTheme
    // {
    //     get => _settings.Theme;
    //     set
    //     {
    //         if (_settings.Theme != value)
    //         { 
    //             _settings.Theme = value;
    //             OnPropertyChanged();
    //             _settings.Save();
    //             ThemeHelper.ApplyTheme(value);
    //         }
    //     }
    // }

    public List<string> AvailableDbTypes { get; set; }

    // public string SelectedDbType
    // {
    //     get => _settings.Database.DbType;
    //     set
    //     {
    //         if (_settings.Database.DbType != value)
    //         {
    //             _settings.Database.DbType = value;
    //             OnPropertyChanged();
    //             _settings.Save();
    //         }
    //     }
    // }

    // public string Server
    // {
    //     get => _settings.Database.Server;
    //     set
    //     {
    //         if (_settings.Database.Server != value)
    //         {
    //             _settings.Database.Server = value;
    //             OnPropertyChanged();
    //             _settings.Save();
    //         }
    //     }
    // }

    // public int Port
    // {
    //     get => _settings.Database.Port;
    //     set
    //     {
    //         if (_settings.Database.Port != value)
    //         {
    //             _settings.Database.Port = value;
    //             OnPropertyChanged();
    //             _settings.Save();
    //         }
    //     }
    // }

    // public string UserId
    // {
    //     get => _settings.Database.UserId;
    //     set
    //     {
    //         if (_settings.Database.UserId != value)
    //         {
    //             _settings.Database.UserId = value;
    //             OnPropertyChanged();
    //             _settings.Save();
    //         }
    //     }
    // }
    //
    // public string Password
    // {
    //     get => _settings.Database.Password;
    //     set
    //     {
    //         if (_settings.Database.Password != value)
    //         {
    //             _settings.Database.Password = value;
    //             OnPropertyChanged();
    //             _settings.Save();
    //         }
    //     }
    // }
    //
    // public string Database
    // {
    //     get => _settings.Database.Database;
    //     set
    //     {
    //         if (_settings.Database.Database != value)
    //         {
    //             _settings.Database.Database = value;
    //             OnPropertyChanged();
    //             _settings.Save();
    //         }
    //     }
    // }

    // public bool MinimizeToTrayOnClose
    // {
    //     get => _settings.MinimizeToTrayOnClose;
    //     set
    //     {
    //         if (_settings.MinimizeToTrayOnClose != value)
    //         {
    //             _settings.MinimizeToTrayOnClose = value;
    //             OnPropertyChanged(nameof(MinimizeToTrayOnClose));
    //             _settings.Save();
    //         }
    //     }
    // }

    [RelayCommand]
    private async Task TestConnection()
    {
        // try
        // {
        //     using (var db = transaction.GetInstance())
        //     {
        //         await db.Ado.OpenAsync();
        //         NotificationHelper.ShowSuccess("连接成功！");
        //     }
        // }
        // catch (Exception ex)
        // {
        //     NotificationHelper.ShowError($"连接失败：{ex.Message}", ex);
        // }
    }
}
