using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Core.Enums
{
    /// <summary>
    /// 定义了C#中常用的数据类型。
    /// </summary>
    public enum CSharpDataType
    {
        // 基本数值类型
        Bool,
        Byte,
        SByte,
        Short,
        UShort,
        Int,
        UInt,
        Long,
        ULong,
        Float,
        Double,
        Decimal,
        
        // 字符和字符串类型
        Char,
        String,
        
        // 时间相关类型
        DateTime,
        TimeSpan,
        DateTimeOffset,
        
        // 其他常用类型
        Guid,
        Object,
        ByteArray,
        
        // 可空类型标识
        Nullable,
        
        // 未知类型
        Unknown
    }
}
