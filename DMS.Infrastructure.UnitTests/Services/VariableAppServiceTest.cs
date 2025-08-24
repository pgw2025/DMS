using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Application.Services;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace DMS.Infrastructure.UnitTests.Services;

[TestSubject(typeof(VariableAppService))]
public class VariableAppServiceTest : BaseServiceTest
{
    private readonly IVariableAppService _variableAppService;

    public VariableAppServiceTest()
    {
        _variableAppService = ServiceProvider.GetRequiredService<IVariableAppService>();
    }

    [Fact]
    public async Task CreateVariableAsyncTest()
    {
        // Arrange
        var dto = FakerHelper.FakeVariableDto();
        dto.VariableTableId = 1; // Assuming a variable table with ID 1 exists for testing

        // Act
        var createdId = await _variableAppService.CreateVariableAsync(dto);

        // Assert
        //Assert.NotEqual(0, createdId);
    }

    [Fact]
    public async Task UpdateVariableAsyncTest()
    {
        // Arrange: Create a variable first
        var createDto = FakerHelper.FakeVariableDto();
        createDto.VariableTableId = 1; // Assuming a variable table with ID 1 exists for testing
        var createdId = await _variableAppService.CreateVariableAsync(createDto);
        //Assert.NotEqual(0, createdId);

        //// Retrieve the created variable to update
        //var variableToUpdate = await _variableAppService.GetVariableByIdAsync(createdId);
        //Assert.NotNull(variableToUpdate);

        //// Modify some properties
        //variableToUpdate.Name = "Updated Variable Name";
        //variableToUpdate.Description = "Updated Description";

        //// Act
        //var affectedRows = await _variableAppService.UpdateVariableAsync(variableToUpdate);

        //// Assert
        //Assert.Equal(1, affectedRows);
        //var updatedVariable = await _variableAppService.GetVariableByIdAsync(createdId);
        //Assert.NotNull(updatedVariable);
        //Assert.Equal("Updated Variable Name", updatedVariable.Name);
        //Assert.Equal("Updated Description", updatedVariable.Description);
    }

    [Fact]
    public async Task DeleteVariableAsyncTest()
    {
        // Arrange: Create a variable first
        //var createDto = FakerHelper.FakeVariableDto();
        //createDto.VariableTableId = 1; // Assuming a variable table with ID 1 exists for testing
        //var createdId = await _variableAppService.CreateVariableAsync(createDto);
        //Assert.NotEqual(0, createdId);

        //// Act
        //var isDeleted = await _variableAppService.DeleteVariableAsync(createdId);

        //// Assert
        //Assert.True(isDeleted);
        //var deletedVariable = await _variableAppService.GetVariableByIdAsync(createdId);
        //Assert.Null(deletedVariable);
    }
}