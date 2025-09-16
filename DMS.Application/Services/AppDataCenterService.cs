using AutoMapper;
using DMS.Application.DTOs;
using DMS.Core.Models;
using DMS.Application.Interfaces;
using DMS.Core.Interfaces;
using DMS.Core.Models;
using DMS.Core.Enums;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using DMS.Application.Events;
using DMS.Application.Interfaces.Management;
using DMS.Core.Events;

namespace DMS.Application.Services;

/// <summary>
/// 数据中心服务，负责管理所有的数据，包括设备、变量表、变量、菜单和日志。
/// 实现 <see cref="IAppDataCenterService"/> 接口。
/// </summary>
public class AppDataCenterService : IAppDataCenterService
{
    private readonly IMapper _mapper;
    private readonly IRepositoryManager _repositoryManager;


    #region 事件定义

    

    /// <summary>
    /// 当设备数据发生变化时触发
    /// </summary>
    public event EventHandler<DeviceChangedEventArgs> DeviceChanged;

    /// <summary>
    /// 当变量表数据发生变化时触发
    /// </summary>
    public event EventHandler<VariableTableChangedEventArgs> VariableTableChanged;

    /// <summary>
    /// 当变量数据发生变化时触发
    /// </summary>
    public event EventHandler<VariableChangedEventArgs> VariableChanged;

    /// <summary>
    /// 当菜单数据发生变化时触发
    /// </summary>
    public event EventHandler<MenuChangedEventArgs> MenuChanged;

    /// <summary>
    /// 当MQTT服务器数据发生变化时触发
    /// </summary>
    public event EventHandler<MqttServerChangedEventArgs> MqttServerChanged;

    /// <summary>
    /// 当日志数据发生变化时触发
    /// </summary>
    public event EventHandler<NlogChangedEventArgs> NlogChanged;

    /// <summary>
    /// 当变量值发生变化时触发
    /// </summary>
    public event EventHandler<VariableValueChangedEventArgs> VariableValueChanged;

    #endregion

 
    public AppDataCenterService(
        IRepositoryManager repositoryManager,
        IMapper mapper,
        IDataLoaderService dataLoaderService,
        IDeviceManagementService deviceManagementService,
        IVariableTableManagementService variableTableManagementService,
        IVariableManagementService variableManagementService,
        IMenuManagementService menuManagementService,
        IMqttManagementService mqttManagementService,
        ILogManagementService logManagementService
        )
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
        DataLoaderService = dataLoaderService;
        
        // 初始化管理服务
        DeviceManagementService = deviceManagementService;
        VariableTableManagementService = variableTableManagementService;
        VariableManagementService = variableManagementService;
        MenuManagementService = menuManagementService;
        MqttManagementService = mqttManagementService;
        LogManagementService = logManagementService;
    }

    public ILogManagementService LogManagementService { get; set; }

    public IMqttManagementService MqttManagementService { get; set; }

    public IMenuManagementService MenuManagementService { get; set; }

    public IVariableManagementService VariableManagementService { get; set; }

    public IVariableTableManagementService VariableTableManagementService { get; set; }

    public IDeviceManagementService DeviceManagementService { get; set; }

    public IDataLoaderService DataLoaderService { get; set; }
}