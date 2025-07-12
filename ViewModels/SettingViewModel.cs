using CommunityToolkit.Mvvm.Input;
using PMSWPF.Config;
using PMSWPF.Data;
using PMSWPF.Helper;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PMSWPF.Services;

namespace PMSWPF.ViewModels;

public partial class SettingViewModel : ViewModelBase
{
    private ConnectionSettings _connectionSettings;
    private readonly S7BackgroundService _s7BackgroundService;
    private readonly MqttBackgroundService _mqttBackgroundService;
    private readonly OpcUaBackgroundService _opcUaBackgroundService;

    public SettingViewModel(S7BackgroundService s7BackgroundService, MqttBackgroundService mqttBackgroundService, OpcUaBackgroundService opcUaBackgroundService)
    {
        _connectionSettings = ConnectionSettings.Load();
        AvailableDbTypes = Enum.GetNames(typeof(SqlSugar.DbType)).ToList();
        _s7BackgroundService = s7BackgroundService;
        _mqttBackgroundService = mqttBackgroundService;
        _opcUaBackgroundService = opcUaBackgroundService;
        Themes = new List<string> { "浅色", "深色", "跟随系统" };
    }

    public List<string> Themes { get; }

    public string SelectedTheme
    {
        get => _connectionSettings.Theme;
        set
        {
            if (_connectionSettings.Theme != value)
            { 
                _connectionSettings.Theme = value;
                OnPropertyChanged();
                _connectionSettings.Save();
                ThemeHelper.ApplyTheme(value);
            }
        }
    }

    public List<string> AvailableDbTypes { get; set; }

    public string SelectedDbType
    {
        get => _connectionSettings.DbType;
        set
        {
            if (_connectionSettings.DbType != value)
            {
                _connectionSettings.DbType = value;
                OnPropertyChanged();
                _connectionSettings.Save();
            }
        }
    }

    public string Server
    {
        get => _connectionSettings.Server;
        set
        {
            if (_connectionSettings.Server != value)
            {
                _connectionSettings.Server = value;
                OnPropertyChanged();
                _connectionSettings.Save();
            }
        }
    }

    public int Port
    {
        get => _connectionSettings.Port;
        set
        {
            if (_connectionSettings.Port != value)
            {
                _connectionSettings.Port = value;
                OnPropertyChanged();
                _connectionSettings.Save();
            }
        }
    }

    public string UserId
    {
        get => _connectionSettings.UserId;
        set
        {
            if (_connectionSettings.UserId != value)
            {
                _connectionSettings.UserId = value;
                OnPropertyChanged();
                _connectionSettings.Save();
            }
        }
    }

    public string Password
    {
        get => _connectionSettings.Password;
        set
        {
            if (_connectionSettings.Password != value)
            {
                _connectionSettings.Password = value;
                OnPropertyChanged();
                _connectionSettings.Save();
            }
        }
    }

    public string Database
    {
        get => _connectionSettings.Database;
        set
        {
            if (_connectionSettings.Database != value)
            {
                _connectionSettings.Database = value;
                OnPropertyChanged();
                _connectionSettings.Save();
            }
        }
    }

    public bool EnableS7Service
    {
        get => _connectionSettings.EnableS7Service;
        set
        {
            if (_connectionSettings.EnableS7Service != value)
            {
                _connectionSettings.EnableS7Service = value;
                OnPropertyChanged();
                _connectionSettings.Save();
                if (value)
                {
                    _s7BackgroundService.StartService();
                }
                else
                {
                    _s7BackgroundService.StopService();
                }
            }
        }
    }

    public bool EnableMqttService
    {
        get => _connectionSettings.EnableMqttService;
        set
        {
            if (_connectionSettings.EnableMqttService != value)
            {
                _connectionSettings.EnableMqttService = value;
                OnPropertyChanged();
                _connectionSettings.Save();
                if (value)
                {
                    _mqttBackgroundService.StartService();
                }
                else
                {
                    _mqttBackgroundService.StopService();
                }
            }
        }
    }

    public bool EnableOpcUaService
    {
        get => _connectionSettings.EnableOpcUaService;
        set
        {
            if (_connectionSettings.EnableOpcUaService != value)
            {
                _connectionSettings.EnableOpcUaService = value;
                OnPropertyChanged();
                _connectionSettings.Save();
                if (value)
                {
                    _opcUaBackgroundService.StartService();
                }
                else
                {
                    _opcUaBackgroundService.StopService();
                }
            }
        }
    }

    public bool MinimizeToTrayOnClose
    {
        get => _connectionSettings.MinimizeToTrayOnClose;
        set
        {
            if (_connectionSettings.MinimizeToTrayOnClose != value)
            {
                _connectionSettings.MinimizeToTrayOnClose = value;
                OnPropertyChanged(nameof(MinimizeToTrayOnClose));
                _connectionSettings.Save();
            }
        }
    }

    [RelayCommand]
    private async Task TestConnection()
    {
        try
        {
            using (var db = DbContext.GetInstance())
            {
                await db.Ado.OpenAsync();
                NotificationHelper.ShowSuccess("连接成功！");
            }
        }
        catch (Exception ex)
        {
            NotificationHelper.ShowError($"连接失败：{ex.Message}", ex);
        }
    }
}
