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
      Assert.NotEqual(addVarTable.Id, 0);
    }
}