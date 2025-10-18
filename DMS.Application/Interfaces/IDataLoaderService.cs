using System.Collections.Concurrent;
using DMS.Application.DTOs;
using DMS.Application.Events;

namespace DMS.Application.Interfaces;

/// <summary>
/// 定义数据加载服务接口，负责从数据源加载数据到内存中
/// </summary>
public interface IDataLoaderService
{
    /// <summary>
    /// 异步加载所有设备及其关联数据到内存中
    /// </summary>
    Task LoadAllDataToMemoryAsync();


}