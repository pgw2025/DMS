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
        public async Task GetAllAsync_ShouldReturnListOfDbDevices()
        {

            // Act
            var result = await _deviceRepository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
        }
        [Fact]
        public async Task AddAsync_Test()
        {
            for (var i = 0; i < 10; i++)
            {
                var dbDevice = FakerHelper.FakeDbDevice();
                //await _sqlSugarDbContext.GetInstance().Insertable(dbDevice).ExecuteCommandAsync();

                // Act
                var result = await _deviceRepository.AddAsync(dbDevice);
            }

            // Assert
            //Assert.NotNull(result);
            //Assert.Contains(result, d => d.Name == testDevice.Name);

            // Clean up after the test
            //await _sqlSugarDbContext.GetInstance().Deleteable<DbDevice>().ExecuteCommandAsync();
        }
    }
}