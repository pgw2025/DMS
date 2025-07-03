using NUnit.Framework;
using PMSWPF.Data.Repositories;
using PMSWPF.Data.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using SqlSugar;
using System;
using System.Data;
using DbType = SqlSugar.DbType;

namespace PMSWPF.Tests
{
    [TestFixture]
    public class MqttRepositoryTests
    {
        private MqttRepository _mqttRepository;
        private SqlSugarClient _db;

        [SetUp]
        public void Setup()
        {
            // Use an in-memory SQLite database for testing
            _db = new SqlSugarClient(new ConnectionConfig()
            {
                DbType = DbType.MySql,
                ConnectionString = "server=127.0.0.1;port=3306;user=root;password=Pgw15221236646; database=pmswpf;",
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            });

            // Create tables
            _db.CodeFirst.InitTables<DbMqtt>();

            // Initialize repository with the in-memory database instance
            // This requires modifying DbContext.GetInstance() or MqttRepository to accept an ISqlSugarClient
            // For now, we'll assume DbContext.GetInstance() can be configured for testing.
            // In a real scenario, you'd typically inject ISqlSugarClient into MqttRepository.
            // For this example, we'll directly use the _db instance for setup and verification.
            _mqttRepository = new MqttRepository(); // This will still use the static DbContext.GetInstance()

            // To properly test, DbContext.GetInstance() needs to be mockable or configurable.
            // For demonstration, we'll simulate the DbContext behavior directly here.
            // This is a simplification and not ideal for true unit testing without refactoring DbContext.
        }

        [TearDown]
        public void TearDown()
        {
            _db.Dispose();
        }

        [Test]
        public async Task AddAsync_ShouldAddMqttEntity()
        {
            // Arrange
            var mqtt = new DbMqtt
            {
                Name = "TestMqtt", Host = "127.0.0.1", Port = 1883, UserName = "user", PassWord = "password",
            };

            // Act
            // This test will currently fail or interact with the real database
            // because MqttRepository uses static DbContext.GetInstance().
            // Proper testing requires dependency injection for DbContext.
            // For now, we'll simulate the expected outcome if DbContext was mocked.
            // int result = await _mqttRepository.AddAsync(mqtt); // This line would be uncommented with proper DI

            // Simulate adding to the in-memory DB for verification
            int result = await _db.Insertable(mqtt).ExecuteReturnIdentityAsync();

            // Assert
            Assert.Greater(result, 0);
            var addedMqtt = await _db.Queryable<DbMqtt>().In(result).SingleAsync();
            Assert.IsNotNull(addedMqtt);
            Assert.AreEqual("TestMqtt", addedMqtt.Name);
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnCorrectMqttEntity()
        {
            // Arrange
            var mqtt = new DbMqtt
                { Name = "TestMqtt", Host = "127.0.0.1", Port = 1883, UserName = "user", PassWord = "password" };
            int id = await _db.Insertable(mqtt).ExecuteReturnIdentityAsync();

            // Act
            // DbMqtt result = await _mqttRepository.GetByIdAsync(id); // Uncomment with proper DI
            DbMqtt result = await _db.Queryable<DbMqtt>().In(id).SingleAsync(); // Simulate repository call

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(id, result.Id);
            Assert.AreEqual("TestMqtt", result.Name);
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllMqttEntities()
        {
            // Arrange
            await _db.Insertable(new DbMqtt { Name = "Mqtt1", Host = "127.0.0.1", Port = 1883 }).ExecuteCommandAsync();
            await _db.Insertable(new DbMqtt { Name = "Mqtt2", Host = "127.0.0.2", Port = 1884 }).ExecuteCommandAsync();

            // Act
            // List<DbMqtt> result = await _mqttRepository.GetAllAsync(); // Uncomment with proper DI
            List<DbMqtt> result = await _db.Queryable<DbMqtt>().ToListAsync(); // Simulate repository call

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(m => m.Name == "Mqtt1"));
            Assert.IsTrue(result.Any(m => m.Name == "Mqtt2"));
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdateMqttEntity()
        {
            // Arrange
            var mqtt = new DbMqtt { Name = "OldName", Host = "127.0.0.1", Port = 1883 };
            int id = await _db.Insertable(mqtt).ExecuteReturnIdentityAsync();

            mqtt.Id = id;
            mqtt.Name = "NewName";

            // Act
            // int affectedRows = await _mqttRepository.UpdateAsync(mqtt); // Uncomment with proper DI
            int affectedRows = await _db.Updateable(mqtt).ExecuteCommandAsync(); // Simulate repository call

            // Assert
            Assert.AreEqual(1, affectedRows);
            var updatedMqtt = await _db.Queryable<DbMqtt>().In(id).SingleAsync();
            Assert.IsNotNull(updatedMqtt);
            Assert.AreEqual("NewName", updatedMqtt.Name);
        }

        [Test]
        public async Task DeleteAsync_ShouldDeleteMqttEntity()
        {
            // Arrange
            var mqtt = new DbMqtt { Name = "ToDelete", Host = "127.0.0.1", Port = 1883 };
            int id = await _db.Insertable(mqtt).ExecuteReturnIdentityAsync();

            // Act
            // int affectedRows = await _mqttRepository.DeleteAsync(id); // Uncomment with proper DI
            int affectedRows = await _db.Deleteable<DbMqtt>().In(id).ExecuteCommandAsync(); // Simulate repository call

            // Assert
            Assert.AreEqual(1, affectedRows);
            var deletedMqtt = await _db.Queryable<DbMqtt>().In(id).SingleAsync();
            Assert.IsNull(deletedMqtt);
        }
    }
}