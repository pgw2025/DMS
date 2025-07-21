using Bogus;
using DMS.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Infrastructure.UnitTests
{
    public static class FakerHelper
    {
        public static DbDevice FakeDbDevice()
        {
            var dbDevice = new Faker<DbDevice>()
                  .RuleFor(d => d.Name, f => f.Commerce.ProductName())
                  .RuleFor(d => d.Description, f => f.Commerce.ProductDescription())
                  .RuleFor(d => d.IpAddress, f => f.Internet.Ip())
                  .RuleFor(d => d.OpcUaServerUrl, f => f.Internet.Url())
                  .Generate();
            dbDevice.Port = 102;
            dbDevice.Protocol = ProtocolType.S7;
            dbDevice.Slot = 1;
            dbDevice.Rack = 0;
            dbDevice.CpuType = "S7-1200";
            dbDevice.DeviceType = Core.Enums.DeviceType.SiemensPLC;
            
            return dbDevice;
        }

        // public static DbVariableTable FakeDbVariableTable()
        // {
        //     var dbVarTable = new Faker<DbVariableTable>()
        //           .RuleFor(d => d.Name, f => f.Commerce.ProductName())
        //           .RuleFor(d => d.Description, f => f.Commerce.ProductDescription())
        //           .Generate();
        //     dbVarTable.ProtocolType = Core.Enums.ProtocolType.S7;
        //     dbVarTable.IsActive=true;
        //     return dbVarTable;
        // }

        // public static DbVariable FakeDbVariable()
        // {
        //     var dbVariable = new Faker<DbVariable>()
        //           .RuleFor(d => d.Name, f => f.Commerce.ProductName())
        //           .RuleFor(d => d.Description, f => f.Commerce.ProductDescription())
        //           .RuleFor(d => d.S7Address, f => f.Internet.DomainWord())
        //           .RuleFor(d => d.DataValue, f => f.Commerce.Price())
        //           .Generate();
        //     dbVariable.ProtocolType = Core.Enums.ProtocolType.S7;
        //     dbVariable.IsActive = true;
        //     dbVariable.SignalType=Core.Enums.SignalType.RunSignal;
        //     dbVariable.UpdateTime=DateTime.Now;
        //     dbVariable.DataType = "String";
        //     return dbVariable;
        // }

    }
}
