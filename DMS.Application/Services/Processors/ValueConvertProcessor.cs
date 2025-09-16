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
    public async Task ProcessAsync(VariableContext context)
    {
        var oldValue = context.Data.DataValue;
        ConvertS7ValueToStringAndNumeric(context.Data, context.NewValue);
        context.Data.UpdatedAt = DateTime.Now;
        // 如何值没有变化则中断处理
        if (context.Data.DataValue==oldValue)
        {
            context.IsHandled = true;
        }
    }
    /// <summary>
    /// 将从 S7 读取的对象值转换为字符串表示和数值表示
    /// </summary>
    /// <param name="variable">关联的变量 DTO</param>
    /// <param name="value">从 S7 读取的原始对象值</param>
    /// <returns>(字符串表示, 数值表示)</returns>
    private void ConvertS7ValueToStringAndNumeric(VariableDto variable, object value)
    {
        if (value == null)
            return ;

        // 首先根据 value 的实际运行时类型进行匹配和转换
        string directConversion = null;
        double numericValue = 0.0;
        bool numericParsed = false;

        switch (value)
        {
            case double d:
                directConversion = d.ToString("G17", CultureInfo.InvariantCulture);
                numericValue = d;
                numericParsed = true;
                break;
            case float f:
                directConversion = f.ToString("G9", CultureInfo.InvariantCulture);
                numericValue = f;
                numericParsed = true;
                break;
            case int i:
                directConversion = i.ToString(CultureInfo.InvariantCulture);
                numericValue = i;
                numericParsed = true;
                break;
            case uint ui:
                directConversion = ui.ToString(CultureInfo.InvariantCulture);
                numericValue = ui;
                numericParsed = true;
                break;
            case short s:
                directConversion = s.ToString(CultureInfo.InvariantCulture);
                numericValue = s;
                numericParsed = true;
                break;
            case ushort us:
                directConversion = us.ToString(CultureInfo.InvariantCulture);
                numericValue = us;
                numericParsed = true;
                break;
            case byte b:
                directConversion = b.ToString(CultureInfo.InvariantCulture);
                numericValue = b;
                numericParsed = true;
                break;
            case sbyte sb:
                directConversion = sb.ToString(CultureInfo.InvariantCulture);
                numericValue = sb;
                numericParsed = true;
                break;
            case long l:
                directConversion = l.ToString(CultureInfo.InvariantCulture);
                numericValue = l;
                numericParsed = true;
                break;
            case ulong ul:
                directConversion = ul.ToString(CultureInfo.InvariantCulture);
                numericValue = ul;
                numericParsed = true;
                break;
            case bool boolValue:
                directConversion = boolValue.ToString().ToLowerInvariant();
                numericValue = boolValue ? 1.0 : 0.0;
                numericParsed = true;
                break;
            case string str:
                directConversion = str;
                // 尝试从字符串解析数值
                if (double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedFromStr))
                {
                    numericValue = parsedFromStr;
                    numericParsed = true;
                }
                break;
            default:
                // 对于未预期的类型，记录日志
                _logger.LogWarning($"变量 {variable.Name} 读取到未预期的数据类型: {value.GetType().Name}, 值: {value}");
                directConversion = value.ToString() ?? string.Empty;
                // 尝试从 ToString() 结果解析数值
                if (double.TryParse(directConversion, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedFromObj))
                {
                    numericValue = parsedFromObj;
                    numericParsed = true;
                }
                break;
        }

        // 如果直接转换成功，直接返回

        // 如果直接转换未能解析数值，并且变量有明确的 DataType，可以尝试更精细的解析
        // (这部分逻辑在上面的 switch 中已经处理了大部分情况，这里作为后备)
        // 在这个实现中，我们主要依赖于 value 的实际类型进行转换，因为这通常更可靠。
        // 如果需要，可以根据 variable.DataType 添加额外的解析逻辑。

        // 返回最终结果
        variable.DataValue = directConversion ?? value.ToString() ?? string.Empty;
        variable.NumericValue = numericValue;
    }
    
    
}