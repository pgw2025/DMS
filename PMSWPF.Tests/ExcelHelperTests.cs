using NUnit.Framework;
using PMSWPF.Helper;
using System.Collections.Generic;
using System.Data;
using System.IO;
using PMSWPF.Enums;
using PMSWPF.Models;

namespace PMSWPF.Tests
{
    [TestFixture]
    public class ExcelHelperTests
    {
        private string _testFilePath;

        [SetUp]
        public void Setup()
        {
            // Create a temporary file for testing
            // _testFilePath = Path.Combine(Path.GetTempPath(), "test.xlsx");
            _testFilePath = "e:/test.xlsx";
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up the temporary file
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }

        [Test]
        public void ExportToExcel_WithListOfObjects_CreatesFile()
        {
            // Arrange
            var data = new List<TestData>
                       {
                           new TestData { Id = 1, Name = "Test1" },
                           new TestData { Id = 2, Name = "Test2" }
                       };

            // Act
            ExcelHelper.ExportToExcel(data, _testFilePath);

            // Assert
            Assert.IsTrue(File.Exists(_testFilePath));
        }

        [Test]
        public void ExportToExcel_WithDataTable_CreatesFile()
        {
            // Arrange
            var dataTable = new DataTable();
            dataTable.Columns.Add("Id", typeof(int));
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Rows.Add(1, "Test1");
            dataTable.Rows.Add(2, "Test2");

            // Act
            ExcelHelper.ExportToExcel(dataTable, _testFilePath);

            // Assert
            Assert.IsTrue(File.Exists(_testFilePath));
        }

        [Test]
        public void ImportFromExcel_ToDataTable_ReturnsCorrectData()
        {
            // Arrange
            var dataTable = new DataTable();
            dataTable.Columns.Add("Id", typeof(int));
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Rows.Add(1, "Test1");
            dataTable.Rows.Add(2, "Test2");
            ExcelHelper.ExportToExcel(dataTable, _testFilePath);

            // Act
            var result = ExcelHelper.ImportFromExcel(_testFilePath);

            // Assert
            Assert.AreEqual(2, result.Rows.Count);
            Assert.AreEqual("1", result.Rows[0]["Id"]);
            Assert.AreEqual("Test1", result.Rows[0]["Name"]);
        }

        [Test]
        public void ImportVarDataFormExcel()
        {
            // Act
            _testFilePath = "C:\\Users\\Administrator\\Desktop\\浓度变量.xlsx";
            var dataTable = ExcelHelper.ImportFromExcel(_testFilePath);
            // 判断表头的名字
            if (dataTable.Columns[0].ColumnName != "Name" || dataTable.Columns[2].ColumnName != "Data Type" &&
                dataTable.Columns[3].ColumnName != "Logical Address")
                throw new AggregateException(
                    "Excel表格式不正确：第一列的名字是：Name,第三列的名字是：Data Type,Data Type,第四列的名字是：Logical Address,请检查");
            
            
            List<Variable> variables = new List<Variable>();
            foreach (DataRow dataRow in dataTable.Rows)
            {
                Variable variable = new Variable();
                variable.Name=dataRow["Name"].ToString();
                variable.DataType=SiemensHelper.S7ToCSharpTypeString(dataRow["Data Type"].ToString()) ;
                var exS7Addr=dataRow["Logical Address"].ToString();
                if (exS7Addr.StartsWith("%"))
                {
                    variable.S7Address = exS7Addr.Substring(1);
                }

                variable.ProtocolType = ProtocolType.S7;
                variable.SignalType = SignalType.OtherASignal;
                variables.Add(variable);
            }
            Assert.Greater(variables.Count, 0);
            // Assert
            //     Assert.AreEqual(2, result.Rows.Count);FDFD
            //     Assert.AreEqual("1", result.Rows[0]["Id"]);
            //     Assert.AreEqual("Test1", result.Rows[0]["Name"]);
        }

        [Test]
        public void ImportFromExcel_ToListOfObjects_ReturnsCorrectData()
        {
            // Arrange
            var data = new List<TestData>
                       {
                           new TestData { Id = 1, Name = "Test1" },
                           new TestData { Id = 2, Name = "Test2" }
                       };
            ExcelHelper.ExportToExcel(data, _testFilePath);

            // Act
            var result = ExcelHelper.ImportFromExcel<TestData>(_testFilePath);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(1, result[0].Id);
            Assert.AreEqual("Test1", result[0].Name);
        }

        private class TestData
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}