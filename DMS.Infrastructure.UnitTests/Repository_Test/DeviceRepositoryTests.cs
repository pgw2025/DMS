using Xunit;
using Moq;
using AutoMapper;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;
using DMS.Infrastructure.Repositories;
using SqlSugar;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMS.Infrastructure.UnitTests.Repository_Test
{
    public class DeviceRepositoryTests:BaseRepositoryTests
    {
        private readonly DeviceRepository _deviceRepository;

        public DeviceRepositoryTests() : base()
        {
   
            _deviceRepository = new DeviceRepository(_sqlSugarDbContext);
        }

        [Fact]
        public async Task GetAllAsync_Test()
        {
            // Act
            var result = await _deviceRepository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateByIdAsync_Test()
        {
            var device = await _deviceRepository.GetByIdAsync(33);
            device.Name = "张飞";
            // Act
            var result = await _deviceRepository.UpdateAsync(device);

            // Assert
            //Assert.NotNull(result);
            Assert.Equal(result, 1);
        }


        [Fact]
        public async Task DeleteAsync_Test()
        {
            var device = await _deviceRepository.GetByIdAsync(33);
            // Act
            var result = await _deviceRepository.DeleteAsync(device);

            // Assert
            //Assert.NotNull(result);
            Assert.Equal(result, 1);
        }

        [Fact]
        public async Task AddAsync_Test()
        {
            try
            {
                //await _sqlSugarDbContext.BeginTranAsync();
                for (var i = 0; i < 10; i++)
                {
                    var dbDevice = FakerHelper.FakeDbDevice();
                    //await _sqlSugarDbContext.GetInstance().Insertable(dbDevice).ExecuteCommandAsync();

                    // Act
                    var result = await _deviceRepository.AddAsync(dbDevice);
                }
                throw new Exception("模拟错误。。。");
                //await _sqlSugarDbContext.CommitTranAsync();
            }
            catch (Exception e)
            {
                //await _sqlSugarDbContext.RollbackTranAsync();
                Console.WriteLine($"添加设备时发生了错误：{e}");
            }
           

            // Assert
            //Assert.NotNull(result);
            //Assert.Contains(result, d => d.Name == testDevice.Name);

            // Clean up after the test
            //await _sqlSugarDbContext.GetInstance().Deleteable<DbDevice>().ExecuteCommandAsync();
        }
    }
}