using System.Data;
using System.Globalization;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Application.Models;
using DMS.Core.Enums;
using DMS.Core.Models;
using Microsoft.Extensions.Logging;

namespace DMS.Application.Services.Processors;

public class ValueConvertProcessor : IVariableProcessor
{
    private readonly ILogger<ValueConvertProcessor> _logger;

    public ValueConvertProcessor(ILogger<ValueConvertProcessor> logger)
    {
        _logger = logger;
    }

    public Task ProcessAsync(VariableContext context)
    {
        var oldValue = context.Data.DataValue;
        // 如果值没有变化则中断处理链
        if (context.Data.DataValue == context.NewValue)
        {
            context.IsHandled = true;
        }

        // 步骤 1: 将原始值转换为 DataValue 和 NumericValue
        context.Data.DataValue = context.NewValue;
        if (context.Data.DataType == DataType.Bool)
        {
            context.Data.NumericValue=context.NewValue=="True"?1:0;
            context.Data.DisplayValue = context.NewValue;
        }
        else
        {
            if (double.TryParse(context.Data.DataValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedFromStr))
            {
                context.Data.NumericValue = parsedFromStr;
                
            }
            // 步骤 2: 根据公式计算 DisplayValue
            CalculateDisplayValue(context.Data);
        }
        

       

        context.Data.UpdatedAt = DateTime.Now;

        
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// 根据转换公式计算用于UI显示的DisplayValue
    /// </summary>
    /// <param name="variable">需要处理的变量DTO</param>
    private void CalculateDisplayValue(Variable variable)
    {
        // 默认情况下，显示值等于原始数据值
        variable.DisplayValue = variable.DataValue;

        // 如果没有转换公式，则直接返回
        if (string.IsNullOrWhiteSpace(variable.ConversionFormula))
        {
            return;
        }

        try
        {
            // 将公式中的 'x' 替换为实际的数值
            // 使用 InvariantCulture 确保小数点是 '.'
            string expression = variable.ConversionFormula.ToLowerInvariant()
                                        .Replace("x", variable.NumericValue.ToString(CultureInfo.InvariantCulture));

            // 使用 DataTable.Compute 来安全地计算表达式
            var result = new DataTable().Compute(expression, null);

                        // 将计算结果格式化后赋给 DisplayValue
                        if (result is double || result is decimal || result is float)
                        {
                            variable.DisplayValue = string.Format("{0:F2}", result); // 默认格式化为两位小数，可根据需要调整
                            variable.NumericValue = Convert.ToDouble(result); // 更新NumericValue为计算后的值
                        }
                        else
                        {
                            variable.DisplayValue = result.ToString();
                            // 尝试将字符串结果解析回double，以更新NumericValue
                            if (double.TryParse(result.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedResult))
                            {
                                variable.NumericValue = parsedResult;
                            }
                        }        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"为变量 {variable.Name} (ID: {variable.Id}) 计算DisplayValue时出错。公式: '{variable.ConversionFormula}'");
            // 如果计算出错，DisplayValue 将保持为原始的 DataValue，保证程序健壮性
        }
    }

    /// <summary>
    /// 将从 S7 读取的对象值转换为字符串表示和数值表示
    /// </summary>
    /// <param name="variable">关联的变量 DTO</param>
    /// <param name="value">从 S7 读取的原始对象值</param>
    private void ConvertS7ValueToStringAndNumeric(Variable variable, string value)
    {
        if (value == null)
            return;

        string directConversion = null;
        double numericValue = 0.0;
        if (variable.DataType == DataType.Bool)
        {
            numericValue=value.ToString()=="True"?1.0:0.0;
            directConversion = value.ToString();
            return;
        }
        

        variable.DataValue = directConversion ?? value.ToString() ?? string.Empty;
        variable.NumericValue = numericValue;
    }
}