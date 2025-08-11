using DMS.Core.Models;

namespace DMS.Infrastructure.Services;

public interface IExcelService
{
    /// <summary>
    ///  从博途的变量表中导如变量
    /// </summary>
    /// <param name="excelFilePath"></param>
    /// <returns></returns>
    /// <exception cref="AggregateException"></exception>
    List<Variable> ImprotFromTiaVariableTable(string excelFilePath);
}