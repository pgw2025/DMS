using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Application.Services;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace DMS.Infrastructure.UnitTests.Services;

[TestSubject(typeof(DeviceService))]
public class DeviceServiceTest : BaseServiceTest // 继承基类
{
    private readonly IDeviceAppService _deviceService;

    public DeviceServiceTest() : base()
    {
        // 从 IoC 容器中解析出需要测试的服务
        // 使用 GetRequiredService 可以确保如果服务未注册，测试会立即失败，这通常是我们想要的。
        _deviceService = ServiceProvider.GetRequiredService<IDeviceAppService>();
    }

    [Fact]
    public async Task CreateDeviceWithDetailsAsyncTest()
    {
        // Arrange
        var dto = new CreateDeviceWithDetailsDto
        {
            Device = FakerHelper.FakeCreateDeviceDto(),
            VariableTable = FakerHelper.FakeVariableTableDto(),
            DeviceMenu = FakerHelper.FakeCreateMenuDto(),
            VariableTableMenu = FakerHelper.FakeCreateMenuDto()
            
            // ... 填充其他需要的数据
        };

        // Act
        var addedDeviceId = await _deviceService.CreateDeviceWithDetailsAsync(dto);

        // Assert
        Assert.NotEqual(0, addedDeviceId);
    }

    [Fact]
    public async Task DeleteDeviceAsyncTest()
    {
        // Act
        var isSuccess = await _deviceService.DeleteDeviceByIdAsync(4);

        // Assert
        Assert.Equal(isSuccess,true);
    }
}