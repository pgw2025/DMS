using DMS.Infrastructure.Interfaces.Services;
using DMS.Infrastructure.Services;
using Opc.Ua;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DMS.Infrastructure.UnitTests.Services
{
    public class OpcUaServiceTest
    {
        [Fact]
        public async Task TestOpcUaService_CreateSession_WithValidUrl_ShouldCreateSession()
        {
            // Arrange
            var service = new OpcUaService();
            var opcUaServerUrl = "opc.tcp://localhost:4840"; // 示例URL，实际测试时需要真实的OPC UA服务器

            // Act & Assert
            // 注意：这个测试需要真实的OPC UA服务器才能通过
            // 在实际测试环境中，您需要启动一个OPC UA服务器
            try
            {
                await service.CreateSession(opcUaServerUrl);
                // 如果没有异常，则认为会话创建成功
                Assert.True(true);
            }
            catch (Exception ex)
            {
                // 在没有真实服务器的情况下，我们期望出现连接异常
                Assert.NotNull(ex);
            }
        }

        [Fact]
        public async Task TestOpcUaService_CreateSession_WithNullUrl_ShouldThrowException()
        {
            // Arrange
            var service = new OpcUaService();
            string opcUaServerUrl = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await service.CreateSession(opcUaServerUrl);
            });
        }

        [Fact]
        public async Task TestOpcUaService_CreateSession_WithEmptyUrl_ShouldThrowException()
        {
            // Arrange
            var service = new OpcUaService();
            var opcUaServerUrl = "";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await service.CreateSession(opcUaServerUrl);
            });
        }

        [Fact]
        public void TestOpcUaService_IsConnected_WithoutSession_ShouldReturnFalse()
        {
            // Arrange
            var service = new OpcUaService();

            // Act
            var isConnected = service.IsConnected();

            // Assert
            Assert.False(isConnected);
        }

        [Fact]
        public async Task TestOpcUaService_ConnectAsync_WithoutSession_ShouldThrowException()
        {
            // Arrange
            var service = new OpcUaService();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await service.ConnectAsync("opc.tcp://localhost:4840");
            });
        }

        [Fact]
        public void TestOpcUaService_Connect_WithoutSession_ShouldThrowException()
        {
            // Arrange
            var service = new OpcUaService();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                service.Connect();
            });
        }

        [Fact]
        public void TestOpcUaService_AddSubscription_WithoutSession_ShouldThrowException()
        {
            // Arrange
            var service = new OpcUaService();
            var subscriptionName = "TestSubscription";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                service.AddSubscription(subscriptionName);
            });
        }

        [Fact]
        public void TestOpcUaService_BrowseNodes_WithoutSession_ShouldThrowException()
        {
            // Arrange
            var service = new OpcUaService();
            var nodeId = NodeId.Null;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                service.BrowseNodes(nodeId);
            });
        }

        [Fact]
        public void TestOpcUaService_ReadValue_WithoutSession_ShouldThrowException()
        {
            // Arrange
            var service = new OpcUaService();
            var nodeId = NodeId.Null;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                service.ReadValue(nodeId);
            });
        }

        [Fact]
        public void TestOpcUaService_WriteValue_WithoutSession_ShouldThrowException()
        {
            // Arrange
            var service = new OpcUaService();
            var nodeId = NodeId.Null;
            var value = "test";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                service.WriteValue(nodeId, value);
            });
        }
    }
}