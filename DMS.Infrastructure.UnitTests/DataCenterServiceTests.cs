using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Application.Services;
using DMS.Core.Interfaces;
using Moq;
using System.Collections.Concurrent;
using Xunit;

namespace DMS.Infrastructure.UnitTests
{
    public class DataCenterServiceTests
    {
        [Fact]
        public void DataCenterService_Should_Implement_All_Required_Methods()
        {
            // Arrange
            var mockRepositoryManager = new Mock<IRepositoryManager>();
            var mockMapper = new Mock<IMapper>();
            var mockDeviceAppService = new Mock<IDeviceAppService>();
            var mockVariableTableAppService = new Mock<IVariableTableAppService>();
            var mockVariableAppService = new Mock<IVariableAppService>();

            // Act
            var dataCenterService = new DataCenterService(
                mockRepositoryManager.Object,
                mockMapper.Object,
                mockDeviceAppService.Object,
                mockVariableTableAppService.Object,
                mockVariableAppService.Object);

            // Assert - Verify that service implements the interface
            Assert.IsAssignableFrom<IDataCenterService>(dataCenterService);
        }

        [Fact]
        public void DataCenterService_Should_Have_ConcurrentDictionary_Properties()
        {
            // Arrange
            var mockRepositoryManager = new Mock<IRepositoryManager>();
            var mockMapper = new Mock<IMapper>();
            var mockDeviceAppService = new Mock<IDeviceAppService>();
            var mockVariableTableAppService = new Mock<IVariableTableAppService>();
            var mockVariableAppService = new Mock<IVariableAppService>();

            // Act
            var dataCenterService = new DataCenterService(
                mockRepositoryManager.Object,
                mockMapper.Object,
                mockDeviceAppService.Object,
                mockVariableTableAppService.Object,
                mockVariableAppService.Object);

            // Assert
            Assert.NotNull(dataCenterService.Devices);
            Assert.NotNull(dataCenterService.VariableTables);
            Assert.NotNull(dataCenterService.Variables);
            Assert.IsType<ConcurrentDictionary<int, DeviceDto>>(dataCenterService.Devices);
            Assert.IsType<ConcurrentDictionary<int, VariableTableDto>>(dataCenterService.VariableTables);
            Assert.IsType<ConcurrentDictionary<int, VariableDto>>(dataCenterService.Variables);
        }

        [Fact]
        public void DataCenterService_AddDeviceToMemory_Should_Add_Device_To_Dictionary()
        {
            // Arrange
            var mockRepositoryManager = new Mock<IRepositoryManager>();
            var mockMapper = new Mock<IMapper>();
            var mockDeviceAppService = new Mock<IDeviceAppService>();
            var mockVariableTableAppService = new Mock<IVariableTableAppService>();
            var mockVariableAppService = new Mock<IVariableAppService>();
            var dataCenterService = new DataCenterService(
                mockRepositoryManager.Object,
                mockMapper.Object,
                mockDeviceAppService.Object,
                mockVariableTableAppService.Object,
                mockVariableAppService.Object);

            var deviceDto = new DeviceDto { Id = 1, Name = "Test Device" };

            // Act
            dataCenterService.AddDeviceToMemory(deviceDto);

            // Assert
            Assert.True(dataCenterService.Devices.ContainsKey(1));
            Assert.Equal(deviceDto, dataCenterService.Devices[1]);
        }

        [Fact]
        public void DataCenterService_UpdateDeviceInMemory_Should_Update_Device_In_Dictionary()
        {
            // Arrange
            var mockRepositoryManager = new Mock<IRepositoryManager>();
            var mockMapper = new Mock<IMapper>();
            var mockDeviceAppService = new Mock<IDeviceAppService>();
            var mockVariableTableAppService = new Mock<IVariableTableAppService>();
            var mockVariableAppService = new Mock<IVariableAppService>();
            var dataCenterService = new DataCenterService(
                mockRepositoryManager.Object,
                mockMapper.Object,
                mockDeviceAppService.Object,
                mockVariableTableAppService.Object,
                mockVariableAppService.Object);

            var deviceDto = new DeviceDto { Id = 1, Name = "Test Device" };
            var updatedDeviceDto = new DeviceDto { Id = 1, Name = "Updated Device" };

            // Act
            dataCenterService.AddDeviceToMemory(deviceDto);
            dataCenterService.UpdateDeviceInMemory(updatedDeviceDto);

            // Assert
            Assert.True(dataCenterService.Devices.ContainsKey(1));
            Assert.Equal(updatedDeviceDto, dataCenterService.Devices[1]);
            Assert.Equal("Updated Device", dataCenterService.Devices[1].Name);
        }

        [Fact]
        public void DataCenterService_RemoveDeviceFromMemory_Should_Remove_Device_From_Dictionary()
        {
            // Arrange
            var mockRepositoryManager = new Mock<IRepositoryManager>();
            var mockMapper = new Mock<IMapper>();
            var mockDeviceAppService = new Mock<IDeviceAppService>();
            var mockVariableTableAppService = new Mock<IVariableTableAppService>();
            var mockVariableAppService = new Mock<IVariableAppService>();
            var dataCenterService = new DataCenterService(
                mockRepositoryManager.Object,
                mockMapper.Object,
                mockDeviceAppService.Object,
                mockVariableTableAppService.Object,
                mockVariableAppService.Object);

            var deviceDto = new DeviceDto { Id = 1, Name = "Test Device" };

            // Act
            dataCenterService.AddDeviceToMemory(deviceDto);
            dataCenterService.RemoveDeviceFromMemory(1);

            // Assert
            Assert.False(dataCenterService.Devices.ContainsKey(1));
        }
    }
}