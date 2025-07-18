using System.Threading.Tasks;
using DMS.Models;

namespace DMS.Services.Processors
{
    /// <summary>
    /// 负责将变量数据发布到MQTT的处理器。
    /// </summary>
    public class MqttPublishProcessor : IVariableProcessor
    {
        private readonly MqttBackgroundService _mqttBackgroundService;

        public MqttPublishProcessor(MqttBackgroundService mqttBackgroundService)
        {
            _mqttBackgroundService = mqttBackgroundService;
        }

        /// <summary>
        /// 处理单个变量上下文，如果有关联的MQTT配置，则将其推送到发送队列。
        /// </summary>
        /// <param name="context">包含变量及其元数据的上下文对象。</param>
        public async Task ProcessAsync(VariableContext context)
        {
            var variable = context.Data;
            if (variable?.VariableMqtts == null || variable.VariableMqtts.Count == 0)
            {
                return; // 没有关联的MQTT配置，直接返回
            }

            // 遍历所有关联的MQTT配置，并将其推入发送队列
            foreach (var variableMqtt in variable.VariableMqtts)
            {
                // 确保VariableMqtt对象中包含了最新的Variable数据
                variableMqtt.Variable = variable;
                await _mqttBackgroundService.SendVariableAsync(variableMqtt);
            }
        }
    }
}
