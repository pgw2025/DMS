using System.Threading.Tasks;
using AutoMapper;
using DMS.Application.Interfaces;
using DMS.Application.Models;
using DMS.Core.Interfaces.Services;
using DMS.Core.Models;

namespace DMS.Application.Services.Processors;

/// <summary>
/// 负责将变量数据发布到MQTT的处理器。
/// </summary>
public class MqttPublishProcessor : IVariableProcessor
{
    private readonly IMapper _mapper;
    private readonly IMqttServiceManager _mqttServiceManager;

    public MqttPublishProcessor(IMapper mapper, IMqttServiceManager mqttServiceManager)
    {
        _mapper = mapper;
        _mqttServiceManager = mqttServiceManager;
    }

    /// <summary>
    /// 处理单个变量上下文，如果有关联的MQTT配置，则将其推送到发送队列。
    /// </summary>
    /// <param name="context">包含变量及其元数据的上下文对象。</param>
    public async Task ProcessAsync(VariableContext context)
    {
        var variable = context.Data;
        if (variable?.MqttAliases == null || variable.MqttAliases.Count == 0)
        {
            return; // 没有关联的MQTT配置，直接返回
        }
        
        // 遍历所有关联的MQTT配置，并将其推入发送队列
        foreach (var variableMqttAliasDto in variable.MqttAliases)
        {
            if (!variableMqttAliasDto.MqttServer.IsActive)
            {
                continue;
            }
            
            // 发布变量数据到MQTT服务器
           var variableMqttAlias = _mapper.Map<VariableMqttAlias>(variableMqttAliasDto);
           variableMqttAlias.Variable.DataValue=variable.DataValue;
            await _mqttServiceManager.PublishVariableDataAsync(variableMqttAlias);
        }
    }
}