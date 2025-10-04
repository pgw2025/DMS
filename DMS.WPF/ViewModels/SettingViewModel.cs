using CommunityToolkit.Mvvm.Input;
using DMS.Application.Configurations;
using DMS.WPF.Helper;
using DMS.WPF.Interfaces;

namespace DMS.WPF.ViewModels;

public partial class SettingViewModel : ViewModelBase
{
    private AppSettings _settings;
    private readonly INotificationService _notificationService;

    public SettingViewModel(AppSettings appSettings,INotificationService notificationService)
    {
        _settings = appSettings;
        _notificationService = notificationService;
        AvailableDbTypes = Enum.GetNames(typeof(SqlSugar.DbType)).ToList();
        Themes = new List<string> { "浅色", "深色", "跟随系统" };
    }

    public List<string> Themes { get; }

    public string SelectedTheme
    {
        get => _settings.Theme;
        set
        {
            if (_settings.Theme != value)
            { 
                _settings.Theme = value;
                OnPropertyChanged();
                _settings.Save();
                ThemeHelper.ApplyTheme(value);
            }
        }
    }

    public List<string> AvailableDbTypes { get; set; }

    public string SelectedDbType
    {
        get => _settings.Db.DbType;
        set
        {
            if (_settings.Db.DbType != value)
            {
                _settings.Db.DbType = value;
                OnPropertyChanged();
                _settings.Save();
            }
        }
    }

    public string Server
    {
        get => _settings.Db.Server;
        set
        {
            if (_settings.Db.Server != value)
            {
                _settings.Db.Server = value;
                OnPropertyChanged();
                _settings.Save();
            }
        }
    }

    public int Port
    {
        get => _settings.Db.Port;
        set
        {
            if (_settings.Db.Port != value)
            {
                _settings.Db.Port = value;
                OnPropertyChanged();
                _settings.Save();
            }
        }
    }

    public string UserId
    {
        get => _settings.Db.UserId;
        set
        {
            if (_settings.Db.UserId != value)
            {
                _settings.Db.UserId = value;
                OnPropertyChanged();
                _settings.Save();
            }
        }
    }
    
    public string Password
    {
        get => _settings.Db.Password;
        set
        {
            if (_settings.Db.Password != value)
            {
                _settings.Db.Password = value;
                OnPropertyChanged();
                _settings.Save();
            }
        }
    }
    
    public string DbName
    {
        get => _settings.Db.DbName;
        set
        {
            if (_settings.Db.DbName != value)
            {
                _settings.Db.DbName = value;
                OnPropertyChanged();
                _settings.Save();
            }
        }
    }

    public bool MinimizeToTrayOnClose
    {
        get => _settings.MinimizeToTrayOnClose;
        set
        {
            if (_settings.MinimizeToTrayOnClose != value)
            {
                _settings.MinimizeToTrayOnClose = value;
                OnPropertyChanged(nameof(MinimizeToTrayOnClose));
                _settings.Save();
            }
        }
    }
    
    public int VariablePollingInterval
    {
        get => _settings.VariableImportTemplate.PollingInterval;
        set
        {
            if (_settings.VariableImportTemplate.PollingInterval != value)
            {
                _settings.VariableImportTemplate.PollingInterval = value;
                OnPropertyChanged();
                _settings.Save();
            }
        }
    }
    
    public bool VariableIsActive
    {
        get => _settings.VariableImportTemplate.IsActive;
        set
        {
            if (_settings.VariableImportTemplate.IsActive != value)
            {
                _settings.VariableImportTemplate.IsActive = value;
                OnPropertyChanged();
                _settings.Save();
            }
        }
    }
    
    public bool VariableIsHistoryEnabled
    {
        get => _settings.VariableImportTemplate.IsHistoryEnabled;
        set
        {
            if (_settings.VariableImportTemplate.IsHistoryEnabled != value)
            {
                _settings.VariableImportTemplate.IsHistoryEnabled = value;
                OnPropertyChanged();
                _settings.Save();
            }
        }
    }
    
    public double VariableHistoryDeadband
    {
        get => _settings.VariableImportTemplate.HistoryDeadband;
        set
        {
            if (_settings.VariableImportTemplate.HistoryDeadband != value)
            {
                _settings.VariableImportTemplate.HistoryDeadband = value;
                OnPropertyChanged();
                _settings.Save();
            }
        }
    }



    [RelayCommand]
    private async Task TestConnection()
    {
        try
        {
            // 使用当前配置测试数据库连接
            using var db = new SqlSugar.SqlSugarScope(new SqlSugar.ConnectionConfig()
            {
                DbType = (SqlSugar.DbType)Enum.Parse(typeof(SqlSugar.DbType), _settings.Db.DbType),
                ConnectionString = _settings.ToConnectionString(),
                IsAutoCloseConnection = true
            });
            
            await db.Ado.ExecuteCommandAsync("SELECT 1");
            _notificationService.ShowSuccess("连接成功！");
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"连接失败：{ex.Message}", ex);
        }
    }
}
