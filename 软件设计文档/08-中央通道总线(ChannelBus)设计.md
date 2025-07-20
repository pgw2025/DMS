# 软件开发文档 - 08. 中央通道总线 (ChannelBus) 设计

本文档详细阐述了 `ChannelBus` 核心服务的设计与实现。该服务旨在为整个应用程序提供一个统一的、解耦的、高性能的内存消息通道管理机制。

## 1. 设计理念

在复杂的应用程序中，不同的后台服务（生产者）需要与不同的处理器（消费者）进行通信。直接依赖或传递队列实例会导致紧耦合。`ChannelBus` 的设计目标是：

*   **完全解耦**：生产者和消费者只依赖于 `IChannelBus` 接口和约定的通道名称，彼此完全独立。
*   **集中管理**：所有 `System.Threading.Channels.Channel<T>` 实例由一个中央单例服务统一创建和分发，避免混乱。
*   **高性能**：充分利用 `Channel<T>` 带来的高吞吐量和低延迟的异步通信能力。
*   **多通道支持**：可以根据不同的业务场景（如数据处理、日志记录、事件通知）创建任意多个独立的命名通道。

## 2. 接口定义 (`DMS.WPF`)

接口被定义在WPF项目中，因为它是一个应用级的、协调性的核心服务。

```csharp
// 文件: DMS.WPF/Services/IChannelBus.cs
using System.Threading.Channels;

namespace DMS.WPF.Services;

/// <summary>
/// 定义了一个中央通道总线，用于在应用程序的不同部分之间创建和分发高性能的、解耦的内存消息通道。
/// </summary>
public interface IChannelBus
{
    /// <summary>
    /// 获取指定名称和类型的通道的写入器。
    /// 如果具有该名称的通道不存在，则会自动创建。
    /// </summary>
    /// <typeparam name="T">通道中流动的数据类型。</typeparam>
    /// <param name="channelName">通道的唯一标识名称，例如 "DataProcessingQueue"。</param>
    /// <returns>一个用于向通道写入数据的 ChannelWriter<T>。</returns>
    ChannelWriter<T> GetWriter<T>(string channelName);

    /// <summary>
    /// 获取指定名称和类型的通道的读取器。
    /// 如果具有该名称的通道不存在，则会自动创建。
    /// </summary>
    /// <typeparam name="T">通道中流动的数据类型。</typeparam>
    /// <param name="channelName">通道的唯一标识名称，例如 "DataProcessingQueue"。</param>
    /// <returns>一个用于从通道读取数据的 ChannelReader<T>。</returns>
    ChannelReader<T> GetReader<T>(string channelName);
}
```

## 3. 实现 (`DMS.WPF`)

`ChannelBusService` 使用 `ConcurrentDictionary` 来确保线程安全地管理所有通道实例。

```csharp
// 文件: DMS.WPF/Services/ChannelBusService.cs
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace DMS.WPF.Services;

/// <summary>
/// IChannelBus的单例实现，管理应用程序中所有命名的通道。
/// </summary>
public class ChannelBusService : IChannelBus
{
    private readonly ConcurrentDictionary<string, object> _channels;

    public ChannelBusService()
    {
        _channels = new ConcurrentDictionary<string, object>();
    }

    public ChannelWriter<T> GetWriter<T>(string channelName)
    {
        // GetOrAdd 是一个原子操作，能防止多个线程同时创建同一个通道的竞态条件。
        var channel = (Channel<T>)_channels.GetOrAdd(
            channelName,
            _ => Channel.CreateUnbounded<T>() // 如果通道不存在，则创建新的无界通道
        );
        return channel.Writer;
    }

    public ChannelReader<T> GetReader<T>(string channelName)
    {
        // 同样使用 GetOrAdd 来确保获取到的是同一个通道实例。
        var channel = (Channel<T>)_channels.GetOrAdd(
            channelName,
            _ => Channel.CreateUnbounded<T>()
        );
        return channel.Reader;
    }
}
```

## 4. 依赖注入 (`App.xaml.cs`)

`IChannelBus` 必须作为单例注册在DI容器中，以保证整个应用程序共享同一个总线实例和其中的所有通道。

```csharp
// 文件: DMS.WPF/App.xaml.cs
private void ConfigureServices(IServiceCollection services)
{
    // ... 其他服务注册

    // 注册中央通道总线为单例
    services.AddSingleton<IChannelBus, ChannelBusService>();

    // ...
}
```

## 5. 使用示例

### 生产者 (Producer)

```csharp
public class MyDataProducer
{
    private readonly ChannelWriter<string> _writer;

    public MyDataProducer(IChannelBus channelBus)
    {
        // 从总线获取写入器
        _writer = channelBus.GetWriter<string>("MyDataQueue");
    }

    public async Task ProduceDataAsync(string data)
    {
        await _writer.WriteAsync(data);
    }
}
```

### 消费者 (Consumer)

```csharp
public class MyDataConsumer
{
    private readonly ChannelReader<string> _reader;

    public MyDataConsumer(IChannelBus channelBus)
    {
        // 从总线获取读取器
        _reader = channelBus.GetReader<string>("MyDataQueue");
    }

    public async Task StartConsumingAsync(CancellationToken token)
    {
        await foreach (var data in _reader.ReadAllAsync(token))
        {
            // 处理数据...
        }
    }
}
```
