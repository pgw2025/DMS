using System.Collections.ObjectModel;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Application.DTOs;
using DMS.Application.DTOs.Events;
using DMS.Application.Interfaces;
using DMS.WPF.Interfaces;
using DMS.WPF.ViewModels.Items;

namespace DMS.WPF.Services;

/// <summary>
/// 日志数据服务类，负责管理日志相关的数据和操作。
/// </summary>
public class LogDataService : ILogDataService
{
    private readonly IMapper _mapper;
    private readonly IDataStorageService _dataStorageService;
    private readonly IAppDataCenterService _appDataCenterService;



    /// <summary>
    /// LogDataService类的构造函数。
    /// </summary>
    /// <param name="mapper">AutoMapper 实例。</param>
    /// <param name="appDataCenterService">数据服务中心实例。</param>
    public LogDataService(IMapper mapper,IDataStorageService dataStorageService, IAppDataCenterService appDataCenterService)
    {
        _mapper = mapper;
        _dataStorageService = dataStorageService;
        _appDataCenterService = appDataCenterService;
    }

    public void LoadAllLog()
    {
        // 加载日志数据
        _dataStorageService.Nlogs = _mapper.Map<ObservableCollection<NlogItemViewModel>>(_appDataCenterService.Nlogs.Values);
    }

    /// <summary>
    /// 处理日志变更事件。
    /// </summary>
    public void OnNlogChanged(object sender, NlogChangedEventArgs e)
    {
        // 在UI线程上更新日志
        App.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            switch (e.ChangeType)
            {
                case DataChangeType.Added:
                    _dataStorageService.Nlogs.Add(_mapper.Map<NlogItemViewModel>(e.Nlog));
                    break;
                case DataChangeType.Updated:
                    var existingLog = _dataStorageService.Nlogs.FirstOrDefault(l => l.Id == e.Nlog.Id);
                    if (existingLog != null)
                    {
                        _mapper.Map(e.Nlog, existingLog);
                    }
                    break;
                case DataChangeType.Deleted:
                    var logToRemove = _dataStorageService.Nlogs.FirstOrDefault(l => l.Id == e.Nlog.Id);
                    if (logToRemove != null)
                    {
                        _dataStorageService.Nlogs.Remove(logToRemove);
                    }
                    break;
            }
        }));
    }
}