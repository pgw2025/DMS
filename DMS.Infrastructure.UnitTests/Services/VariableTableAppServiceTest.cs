using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Application.Services;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace DMS.Infrastructure.UnitTests.Services;

[TestSubject(typeof(VariableTableAppService))]
public class VariableTableAppServiceTest : BaseServiceTest
{
    private readonly IVariableTableAppService _variableTableAppService;

    public VariableTableAppServiceTest()
    {
        _variableTableAppService = ServiceProvider.GetRequiredService<IVariableTableAppService>();
    }

    [Fact]
    public async Task CreateVariableTableAsyncTest()
    {
        var dto = new CreateVariableTableWithMenuDto()
                  {
                      VariableTable = FakerHelper.FakeVariableTableDto(),
                      Menu = FakerHelper.FakeCreateMenuDto(),
                      DeviceId = 5
                  };
      var addVarTable= await  _variableTableAppService.CreateVariableTableAsync(dto);
      Assert.NotEqual(addVarTable.VariableTable.Id, 0);
    }

    [Fact]
    public async Task DeleteVariableTableAsyncTest()
    {
        // Arrange: Create a variable table first
        var createDto = new CreateVariableTableWithMenuDto()
        {
            VariableTable = FakerHelper.FakeVariableTableDto(),
            Menu = FakerHelper.FakeCreateMenuDto(),
            DeviceId = 5 // Assuming a device with ID 5 exists for testing
        };
        var createdVariableTable = await _variableTableAppService.CreateVariableTableAsync(createDto);
        Assert.NotEqual(createdVariableTable.VariableTable.Id, 0);

        // Act: Delete the created variable table
        var isDeleted = await _variableTableAppService.DeleteVariableTableAsync(createdVariableTable.VariableTable.Id);

        // Assert: Verify deletion was successful
        Assert.True(isDeleted);

        // Optionally, try to retrieve the deleted variable table to confirm it's gone
        var deletedTable = await _variableTableAppService.GetVariableTableByIdAsync(createdVariableTable.VariableTable.Id);
        Assert.Null(deletedTable);
    }

    [Fact]
    public async Task UpdateVariableTableAsyncTest()
    {
        // Arrange: Create a variable table first
        var createDto = new CreateVariableTableWithMenuDto()
        {
            VariableTable = FakerHelper.FakeVariableTableDto(),
            Menu = FakerHelper.FakeCreateMenuDto(),
            DeviceId = 5 // Assuming a device with ID 5 exists for testing
        };
        var createdVariableTable = await _variableTableAppService.CreateVariableTableAsync(createDto);
        Assert.NotEqual(createdVariableTable.VariableTable.Id, 0);

        // Modify some properties of the DTO
        createdVariableTable.VariableTable.Name = "Updated Variable Table Name";
        createdVariableTable.VariableTable.Description = "This is an updated description.";

        // Act: Update the variable table
        var affectedRows = await _variableTableAppService.UpdateVariableTableAsync(createdVariableTable.VariableTable);

        // Assert: Verify update was successful
        Assert.Equal(1, affectedRows);

        // Retrieve the updated variable table to confirm changes
        var updatedTable = await _variableTableAppService.GetVariableTableByIdAsync(createdVariableTable.VariableTable.Id);
        Assert.NotNull(updatedTable);
        Assert.Equal("Updated Variable Table Name", updatedTable.Name);
        Assert.Equal("This is an updated description.", updatedTable.Description);
    }
}