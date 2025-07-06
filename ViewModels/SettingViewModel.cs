using CommunityToolkit.Mvvm.Input;
using PMSWPF.Config;
using PMSWPF.Data;
using PMSWPF.Helper;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMSWPF.ViewModels;

public partial class SettingViewModel : ViewModelBase
{
    private ConnectionSettings _connectionSettings;

    public SettingViewModel()
    {
        _connectionSettings = ConnectionSettings.Load();
        AvailableDbTypes = Enum.GetNames(typeof(SqlSugar.DbType)).ToList();
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