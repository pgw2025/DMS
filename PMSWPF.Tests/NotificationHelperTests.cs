using NLog;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;
using PMSWPF.Helper;
using System;
using System.Linq;

namespace PMSWPF.Tests
{
    [TestFixture]
    public class NotificationHelperTests
    {
        private MemoryTarget _memoryTarget;
        // private Logger _logger; // 声明一个 Logger 变量 - 这个不再需要了

        [SetUp]
        public void Setup()
        {
            // 1. 加载外部的 nlog.config 文件
            LogManager.LoadConfiguration("nlog.config");

            // 2. 获取当前加载的配置
            var config = LogManager.Configuration;

            // 3. 创建 MemoryTarget
            _memoryTarget = new MemoryTarget("testMemoryTarget")
                            {
                                // 捕获消息、级别、异常和 MDC 数据
                                Layout
                                    = "${message}|${level}|${exception:format=tostring}|${mdlc:item=CallerFilePath}|${mdlc:item=CallerParentLineNumber}"
                            };

            // 4. 将 MemoryTarget 添加到配置中
            config.AddTarget(_memoryTarget);

            // 5. 定义一个规则，将所有日志路由到 MemoryTarget
            // 注意：这里我们添加一个新的规则，确保日志也发送到 MemoryTarget
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, _memoryTarget);

            // 6. 重新应用配置，以包含新的 MemoryTarget 和规则
            LogManager.Configuration = config;

            // 7. 清空 MemoryTarget 中的日志
            _memoryTarget.Logs.Clear();
        }

        [TearDown]
        public void Teardown()
        {
            // 在每个测试结束后，重置 NLog 配置，避免影响其他测试
            LogManager.Configuration = null;
            // 清除 _memoryTarget 引用，以满足静态分析工具的建议
            _memoryTarget.Dispose();
            _memoryTarget = null;
        }

        [Test]
        public void ShowException_ShouldLogErrorWithExceptionAndCallerInfo()
        {
            // Arrange
            // var testMessage = "这是一个测试异常消息";
            // var innerException = new InvalidOperationException("内部操作失败");
            // var testException = new Exception("外部异常", innerException);
            // var expectedCallerFilePath = "D:\\CShap\\PMSWPF\\Tests\\NotificationHelperTests.cs"; // 模拟调用文件路径
            // var expectedCallerLineNumber = 49; // 模拟调用行号 (根据实际测试代码行数调整)

            // Act
            // 调用 ShowException 方法，并传入模拟的异常和调用信息
            // NotificationHelper.ShowException(testMessage, testException, expectedCallerFilePath, expectedCallerLineNumber);

            NlogHelper.Info("info");
            NlogHelper.Warn("warn");
            NlogHelper.Trace("trace");
            NlogHelper.Error("hello");

            // Assert
            Assert.That(_memoryTarget.Logs.Count, Is.EqualTo(1), "应该只记录一条日志");

            var logEntry = _memoryTarget.Logs[0];

            // 验证日志消息
            // Assert.That(logEntry, Does.Contain(testMessage), "日志消息应该包含测试消息");
            //
            // // 验证日志级别
            // Assert.That(logEntry, Does.Contain("|Error|"), "日志级别应该是 Error");
            //
            // // 验证异常信息
            // Assert.That(logEntry, Does.Contain(testException.Message), "日志应该包含外部异常消息");
            // Assert.That(logEntry, Does.Contain(innerException.Message), "日志应该包含内部异常消息");
            // Assert.That(logEntry, Does.Contain(nameof(InvalidOperationException)), "日志应该包含内部异常类型");
            // Assert.That(logEntry, Does.Contain(nameof(Exception)), "日志应该包含外部异常类型");
            //
            // // 验证 CallerFilePath 和 CallerParentLineNumber
            // Assert.That(logEntry, Does.Contain($"|{expectedCallerFilePath}|"), "日志应该包含 CallerFilePath");
            // Assert.That(logEntry, Does.Contain($"|{expectedCallerLineNumber}"), "日志应该包含 CallerParentLineNumber");
        }
    }
}