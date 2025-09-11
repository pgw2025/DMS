using System.Threading.Tasks;
using DMS.Application.Interfaces;
using DMS.Application.Models;
using DMS.Core.Models;
using Microsoft.Extensions.Logging;

namespace DMS.Application.Services.Processors;

public class UpdateDbVariableProcessor : IVariableProcessor
{
    private readonly ILogger<UpdateDbVariableProcessor> _logger;

    public UpdateDbVariableProcessor(ILogger<UpdateDbVariableProcessor> logger)
    {
        _logger = logger;
    }

    public async Task ProcessAsync(VariableContext context)
    {
        try
        {
           
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"更新数据库变量 {context.Data.Name} 失败: {ex.Message}");
        }
    }
}