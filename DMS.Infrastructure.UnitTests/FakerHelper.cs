using Bogus;
using DMS.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMS.Application.DTOs;
using DMS.Core.Enums;
using DMS.Core.Models;

namespace DMS.Infrastructure.UnitTests
{
    public static class FakerHelper
    {
        public static Device FakeDevice()
        {
            var dbDevice = new Faker<Device>()
                           .RuleFor(d => d.Name, f => f.Commerce.ProductName())
                           .RuleFor(d => d.Description, f => f.Commerce.ProductDescription())
                           .RuleFor(d => d.IpAddress, f => f.Internet.Ip())
                           .RuleFor(d => d.OpcUaServerUrl, f => f.Internet.Url())
                           .Generate();
            dbDevice.Port = 102;
            dbDevice.Protocol = ProtocolType.S7;
            dbDevice.Slot = 1;
            dbDevice.Rack = 0;
            dbDevice.CpuType = CpuType.S71200;
            dbDevice.DeviceType = Core.Enums.DeviceType.SiemensPLC;

            return dbDevice;
        }

        public static VariableTable FakeVariableTable()
        {
            var varTable = new Faker<VariableTable>()
                           .RuleFor(d => d.Name, f => f.Commerce.ProductName())
                           .RuleFor(d => d.Description, f => f.Commerce.ProductDescription())
                           .Generate();
            varTable.IsActive = true;
            return varTable;
        }


        public static VariableTableDto FakeVariableTableDto()
        {
            var varTable = new Faker<VariableTableDto>()
                           .RuleFor(d => d.Name, f => f.Commerce.ProductName())
                           .RuleFor(d => d.Description, f => f.Commerce.ProductDescription())
                           .Generate();
            varTable.IsActive = true;
            return varTable;
        }

        public static Variable FakeVariable()
        {
            var dbVariable = new Faker<Variable>()
                             .RuleFor(d => d.Name, f => f.Commerce.ProductName())
                             .RuleFor(d => d.S7Address, f => $"DB1.DBD{f.Random.Int(0, 1000)}")
                             .RuleFor(d => d.OpcUaNodeId, f => $"ns=2;s=My.DbDevice.Variable{f.Random.Int(1, 100)}")
                             .RuleFor(d => d.ConversionFormula, f => "x * 1.0")
                             .RuleFor(d => d.UpdatedBy, f => f.Name.FullName())
                             .Generate();

            // dbVariable.DataType = 1; 
            // dbVariable.PollLevel = 1; 
            dbVariable.IsActive = true;
            dbVariable.VariableTableId = 1;
            dbVariable.IsHistoryEnabled = true;
            dbVariable.HistoryDeadband = 0.1;
            dbVariable.IsAlarmEnabled = false;
            dbVariable.AlarmMinValue = 0;
            dbVariable.AlarmMaxValue = 100;
            dbVariable.AlarmDeadband = 1;
            dbVariable.Protocol = 0;
            dbVariable.DataType = 0;
            dbVariable.CreatedAt = DateTime.Now;
            dbVariable.UpdatedAt = DateTime.Now;
            dbVariable.IsModified = false;

            return dbVariable;
        }

        public static DbMenu FakeDbMenu()
        {
            var dbMenu = new Faker<DbMenu>()
                         .RuleFor(d => d.Header, f => f.Commerce.Department())
                         .RuleFor(d => d.Icon, f => f.Random.Word())
                         .RuleFor(d => d.DisplayOrder, f => f.Random.Number(1, 10))
                         .Generate();
            dbMenu.ParentId = 0;
            dbMenu.Childrens = new List<DbMenu>();
            return dbMenu;
        }

        public static DbMqttServer FakeDbMqttServer()
        {
            var dbMqttServer = new Faker<DbMqttServer>()
                               .RuleFor(d => d.ServerName, f => f.Company.CompanyName())
                               .RuleFor(d => d.BrokerAddress, f => f.Internet.Ip())
                               .RuleFor(d => d.Username, f => f.Internet.UserName())
                               .RuleFor(d => d.Password, f => f.Internet.Password())
                               .RuleFor(d => d.SubscribeTopic, f => "/topic/sub")
                               .RuleFor(d => d.PublishTopic, f => "/topic/pub")
                               .RuleFor(d => d.ClientId, f => Guid.NewGuid()
                                                                  .ToString())
                               .RuleFor(d => d.MessageFormat, f => "JSON")
                               .Generate();
            dbMqttServer.Port = 1883;
            dbMqttServer.IsActive = true;
            dbMqttServer.CreatedAt = DateTime.Now;
            return dbMqttServer;
        }

        // public static CreateDeviceDto FakeCreateDeviceDto()
        // {
        //     var deviceDto = new Faker<CreateDeviceDto>()
        //                     .RuleFor(d => d.Name, f => f.Commerce.ProductName())
        //                     .RuleFor(d => d.Description, f => f.Commerce.ProductDescription())
        //                     .RuleFor(d => d.IpAddress, f => f.Internet.Ip())
        //                     .RuleFor(d => d.OpcUaServerUrl, f => f.Internet.Url())
        //                     .Generate();
        //     deviceDto.Port = 102;
        //     deviceDto.Protocol = ProtocolType.S7;
        //     deviceDto.Slot = 1;
        //     deviceDto.Rack = 0;
        //     deviceDto.CpuType = "S7-1200";
        //     deviceDto.DeviceType = Core.Enums.DeviceType.SiemensPLC;
        //
        //     return deviceDto;
        // }

        public static MenuBeanDto FakeCreateMenuDto()
        {
            var menuDto = new Faker<MenuBeanDto>()
                          .RuleFor(m => m.Header, f => f.Commerce.ProductName())
                          .RuleFor(m => m.Icon, f => f.Random.Word())
                          .RuleFor(m => m.DisplayOrder, f => f.Random.Number(1, 100))
                          .RuleFor(m => m.ParentId, f => f.Random.Number(0, 10)) // 假设可以有父菜单
                          .Generate();
            return menuDto;
        }

        public static VariableDto FakeVariableDto()
        {
            var variableDto = new Faker<VariableDto>()
                              .RuleFor(v => v.Name, f => f.Commerce.ProductName())
                              .RuleFor(v => v.S7Address, f => $"DB1.DBD{f.Random.Int(0, 1000)}")
                              .RuleFor(v => v.SignalType, f => f.PickRandom<SignalType>())
                              .RuleFor(v => v.PollingInterval, f => f.Random.Int(10, 1800000))
                              .RuleFor(v => v.IsActive, f => f.Random.Bool())
                              .RuleFor(v => v.IsHistoryEnabled, f => f.Random.Bool())
                              .RuleFor(v => v.HistoryDeadband, f => f.Random.Double(0.0, 1.0))
                              .RuleFor(v => v.IsAlarmEnabled, f => f.Random.Bool())
                              .RuleFor(v => v.AlarmMinValue, f => f.Random.Double(0.0, 50.0))
                              .RuleFor(v => v.AlarmMaxValue, f => f.Random.Double(50.0, 100.0))
                              .RuleFor(v => v.AlarmDeadband, f => f.Random.Double(0.0, 1.0))
                              .RuleFor(v => v.Protocol, f => f.PickRandom<ProtocolType>())
                              .RuleFor(v => v.DataType, f => f.PickRandom(Enum.GetValues<DataType>()))
                              .RuleFor(v => v.OpcUaNodeId, f => $"ns=2;s=My.Variable{f.Random.Int(1, 100)}")
                              .RuleFor(v => v.ConversionFormula, f => "x * 1.0")
                              .RuleFor(v => v.UpdatedBy, f => f.Name.FullName())
                              .RuleFor(v => v.DataValue, f => f.Random.Double(0, 100).ToString())
                              .RuleFor(v => v.DisplayValue, f => f.Random.Word())
                              .RuleFor(v => v.Description, f => f.Lorem.Sentence())
                              .Generate();
            variableDto.VariableTableId = 1; // Default to 1 for testing purposes
            return variableDto;
        }
    }
}
