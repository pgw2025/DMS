namespace PMSWPF.Helper;

/// <summary>
/// 西门子帮助类
/// </summary>
public static class SiemensHelper
{
    /// <summary>
    /// 将S7数据类型字符串转换为C#数据类型字符串
    /// </summary>
    /// <param name="s7Type">S7数据类型字符串</param>
    /// <returns>对应的C#数据类型字符串</returns>
    public static string S7ToCSharpTypeString(string s7Type)
    {
        switch (s7Type.ToUpper())
        {
            case "BOOL":
                return "bool";
            case "BYTE":
                return "byte";
            case "WORD":
                return "ushort";
            case "DWORD":
                return "uint";
            case "INT":
                return "short";
            case "DINT":
                return "int";
            case "REAL":
                return "float";
            case "LREAL":
                return "double";
            case "CHAR":
                return "char";
            case "STRING":
                return "string";
            case "TIMER":
            case "TIME":
                return "TimeSpan";
            case "COUNTER":
                return "ushort";
            case "DATE":
                return "DateTime";
            case "TIME_OF_DAY":
            case "TOD":
                return "DateTime";
            case "DATE_AND_TIME":
            case "DT":
                return "DateTime";
            default:
                return "object";
        }
    }
}