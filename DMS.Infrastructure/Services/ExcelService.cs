using System.Data;
using System.Reflection;
using DMS.Core.Enums;
using DMS.Core.Interfaces.Services;
using DMS.Core.Models;
using DMS.Infrastructure.Helper;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace DMS.Infrastructure.Services;

public class ExcelService : IExcelService
{
    /// <summary>
    /// 将数据列表导出到 Excel 文件。
    /// </summary>
    /// <typeparam name="T">数据类型。</typeparam>
    /// <param name="data">要导出的数据列表。</param>
    /// <param name="filePath">Excel 文件的保存路径。</param>
    /// <param name="sheetName">工作表的名称。</param>
    public  void ExportToExcel<T>(IEnumerable<T> data, string filePath, string sheetName = "Sheet1") where T : class
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet(sheetName);

            // 获取T类型的属性
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // 创建表头
            IRow headerRow = sheet.CreateRow(0);
            for (int i = 0; i < properties.Length; i++)
            {
                headerRow.CreateCell(i).SetCellValue(properties[i].Name);
            }

            // 填充数据
            int rowIndex = 1;
            foreach (var item in data)
            {
                IRow dataRow = sheet.CreateRow(rowIndex++);
                for (int i = 0; i < properties.Length; i++)
                {
                    object value = properties[i].GetValue(item, null);
                    dataRow.CreateCell(i).SetCellValue(value?.ToString() ?? string.Empty);
                }
            }

            workbook.Write(fs);
        }
    }

    /// <summary>
    /// 将 DataTable 导出到 Excel 文件。
    /// </summary>
    /// <param name="dataTable">要导出的 DataTable。</param>
    /// <param name="filePath">Excel 文件的保存路径。</param>
    /// <param name="sheetName">工作表的名称。</param>
    public  void ExportToExcel(DataTable dataTable, string filePath, string sheetName = "Sheet1")
    {
        if (dataTable == null)
        {
            throw new ArgumentNullException(nameof(dataTable));
        }

        using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet(sheetName);

            // 创建表头
            IRow headerRow = sheet.CreateRow(0);
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                headerRow.CreateCell(i).SetCellValue(dataTable.Columns[i].ColumnName);
            }

            // 填充数据
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                IRow dataRow = sheet.CreateRow(i + 1);
                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    dataRow.CreateCell(j).SetCellValue(dataTable.Rows[i][j].ToString());
                }
            }

            workbook.Write(fs);
        }
    }

    /// <summary>
    /// 从 Excel 文件导入数据到 DataTable。
    /// </summary>
    /// <param name="filePath">Excel 文件的路径。</param>
    /// <param name="sheetName">工作表的名称。</param>
    /// <param name="hasHeaderRow">是否包含表头行。</param>
    /// <returns>包含导入数据的 DataTable。</returns>
    public static DataTable ImportFromExcel(string filePath, string sheetName = "Sheet1", bool hasHeaderRow = true)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The specified file does not exist.", filePath);
        }

        var dt = new DataTable();
        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            IWorkbook workbook = new XSSFWorkbook(fs);
            ISheet sheet = workbook.GetSheet(sheetName) ?? workbook.GetSheetAt(0);

            if (sheet == null)
            {
                throw new Exception($"Sheet with name '{sheetName}' not found.");
            }

            IRow headerRow = hasHeaderRow ? sheet.GetRow(0) : null;
            int firstRow = hasHeaderRow ? 1 : 0;
            int cellCount = headerRow?.LastCellNum ?? sheet.GetRow(sheet.FirstRowNum).LastCellNum;

            // 创建列
            for (int i = 0; i < cellCount; i++)
            {
                string columnName = hasHeaderRow ? headerRow.GetCell(i)?.ToString() ?? $"Column{i + 1}" : $"Column{i + 1}";
                dt.Columns.Add(columnName);
            }

            // 填充数据
            for (int i = firstRow; i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row == null) continue;

                DataRow dataRow = dt.NewRow();
                for (int j = 0; j < cellCount; j++)
                {
                    ICell cell = row.GetCell(j);
                    dataRow[j] = cell?.ToString() ?? string.Empty;
                }
                dt.Rows.Add(dataRow);
            }
        }
        return dt;
    }

    /// <summary>
    /// 从 Excel 文件导入数据到对象列表。
    /// </summary>
    /// <typeparam name="T">要转换的目标类型。</typeparam>
    /// <param name="filePath">Excel 文件的路径。</param>
    /// <param name="sheetName">工作表的名称。</param>
    /// <returns>包含导入数据的对象列表。</returns>
    public  List<T> ImportFromExcel<T>(string filePath, string sheetName = "Sheet1") where T : class, new()
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The specified file does not exist.", filePath);
        }

        var list = new List<T>();
        PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            IWorkbook workbook = new XSSFWorkbook(fs);
            ISheet sheet = workbook.GetSheet(sheetName) ?? workbook.GetSheetAt(0);

            if (sheet == null)
            {
                throw new Exception($"Sheet with name '{sheetName}' not found.");
            }

            IRow headerRow = sheet.GetRow(0);
            if (headerRow == null)
            {
                throw new Exception("Header row not found.");
            }

            // 创建列名到属性的映射
            var columnMap = new Dictionary<int, PropertyInfo>();
            for (int i = 0; i < headerRow.LastCellNum; i++)
            {
                string columnName = headerRow.GetCell(i)?.ToString();
                if (!string.IsNullOrEmpty(columnName))
                {
                    var prop = properties.FirstOrDefault(p => p.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                    if (prop != null)
                    {
                        columnMap[i] = prop;
                    }
                }
            }

            // 读取数据行
            for (int i = 1; i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row == null) continue;

                var item = new T();
                foreach (var map in columnMap)
                {
                    ICell cell = row.GetCell(map.Key);
                    if (cell != null)
                    {
                        try
                        {
                            // 尝试进行类型转换
                            object value = Convert.ChangeType(cell.ToString(), map.Value.PropertyType);
                            map.Value.SetValue(item, value, null);
                        }
                        catch (Exception)
                        {
                            // 转换失败时可以记录日志或设置默认值
                        }
                    }
                }
                list.Add(item);
            }
        }
        return list;
    }

    /// <summary>
    ///  从博途的变量表中导如变量
    /// </summary>
    /// <param name="excelFilePath"></param>
    /// <returns></returns>
    /// <exception cref="AggregateException"></exception>
    public  List<Variable> ImprotFromTiaVariableTable(string excelFilePath)
    {
        // Act
        // _testFilePath = "C:\Users\Administrator\Desktop\浓度变量.xlsx";
        var dataTable = ImportFromExcel(excelFilePath);
        // 判断表头的名字
        if (dataTable.Columns[0].ColumnName != "Name" || dataTable.Columns[2].ColumnName != "Data Type" &&
            dataTable.Columns[3].ColumnName != "Logical Address")
            throw new AggregateException(
                "Excel表格式不正确：第一列的名字是：Name,第三列的名字是：Data Type,Data Type,第四列的名字是：Logical Address,请检查");


        List<Variable> variableDatas = new List<DMS.Core.Models.Variable>();
        foreach (DataRow dataRow in dataTable.Rows)
        {
            DMS.Core.Models.Variable variable = new DMS.Core.Models.Variable();
            variable.Name = dataRow["Name"].ToString();
            variable.DataType = (DMS.Core.Enums.DataType)Enum.Parse(typeof(DMS.Core.Enums.DataType), SiemensHelper.S7ToCSharpTypeString(dataRow["Data Type"].ToString()), true);
            variable.SignalType = SignalType.OtherASignal;
            var exS7Addr = dataRow["Logical Address"].ToString();
            if (exS7Addr.StartsWith("%"))
            {
                variable.S7Address = exS7Addr.Substring(1);
            }

            variable.OpcUaNodeId = "";
            variable.Protocol = ProtocolType.S7;
            variable.PollLevel = 30000; // ThirtySeconds
            variableDatas.Add(variable);
        }

        return variableDatas;
    }
}