using System.Data;
using System.Globalization;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Application.Models;
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

        // 步骤 1: 将原始值转换为 DataValue 和 NumericValue
        ConvertS7ValueToStringAndNumeric(context.Data, context.NewValue);

        // 步骤 2: 根据公式计算 DisplayValue
        CalculateDisplayValue(context.Data);

        context.Data.UpdatedAt = DateTime.Now;

        // 如果值没有变化则中断处理链
        if (context.Data.DataValue == oldValue)
        {
            context.IsHandled = true;
        }
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// 根据转换公式计算用于UI显示的DisplayValue
    /// </summary>
    /// <param name="variable">需要处理的变量DTO</param>
    private void CalculateDisplayValue(VariableDto variable)
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
    private void ConvertS7ValueToStringAndNumeric(VariableDto variable, object value)
    {
        if (value == null)
            return;

        string directConversion = null;
        double numericValue = 0.0;

        switch (value)
        {
            case double d:
                directConversion = d.ToString("G17", CultureInfo.InvariantCulture);
                numericValue = d;
                break;
            case float f:
                directConversion = f.ToString("G9", CultureInfo.InvariantCulture);
                numericValue = f;
                break;
            case int i:
                directConversion = i.ToString(CultureInfo.InvariantCulture);
                numericValue = i;
                break;
            case uint ui:
                directConversion = ui.ToString(CultureInfo.InvariantCulture);
                numericValue = ui;
                break;
            case short s:
                directConversion = s.ToString(CultureInfo.InvariantCulture);
                numericValue = s;
                break;
            case ushort us:
                directConversion = us.ToString(CultureInfo.InvariantCulture);
                numericValue = us;
                break;
            case byte b:
                directConversion = b.ToString(CultureInfo.InvariantCulture);
                numericValue = b;
                break;
            case sbyte sb:
                directConversion = sb.ToString(CultureInfo.InvariantCulture);
                numericValue = sb;
                break;
            case long l:
                directConversion = l.ToString(CultureInfo.InvariantCulture);
                numericValue = l;
                break;
            case ulong ul:
                directConversion = ul.ToString(CultureInfo.InvariantCulture);
                numericValue = ul;
                break;
            case bool boolValue:
                directConversion = boolValue.ToString().ToLowerInvariant();
                numericValue = boolValue ? 1.0 : 0.0;
                break;
            case string str:
                directConversion = str;
                if (double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedFromStr))
                {
                    numericValue = parsedFromStr;
                }
                break;
            default:
                _logger.LogWarning($"变量 {variable.Name} 读取到未预期的数据类型: {value.GetType().Name}, 值: {value}");
                directConversion = value.ToString() ?? string.Empty;
                if (double.TryParse(directConversion, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedFromObj))
                {
                    numericValue = parsedFromObj;
                }
                break;
        }

        variable.DataValue = directConversion ?? value.ToString() ?? string.Empty;
        variable.NumericValue = numericValue;
    }
}