using DMS.Application.Interfaces;
using DMS.Core.Interfaces;
using DMS.Core.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Services;

public class DataProcessingService : IDataProcessingService
{
    private readonly ILogger<DataProcessingService> _logger;
    private readonly IRepositoryManager _repositoryManager;

    public DataProcessingService(ILogger<DataProcessingService> logger, IRepositoryManager repositoryManager)
    {
        _logger = logger;
        _repositoryManager = repositoryManager;
    }

    public async Task EnqueueAsync(Variable variable)
    {
        _logger.LogInformation($"Processing variable: {variable.Name}, Value: {variable.DataValue}");

        // 这里可以添加将变量数据保存到数据库的逻辑
        // 例如：保存到 VariableHistory 表
        var history = new VariableHistory
        {
            VariableId = variable.Id,
            Value = variable.DataValue.ToString(),
            Timestamp = System.DateTime.Now
        };
        await _repositoryManager.VariableHistories.AddAsync(history);
    }
}