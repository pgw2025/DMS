using DMS.Application.Interfaces;
using DMS.Application.Services;
using DMS.Core.Interfaces.Repositories;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace DMS.Infrastructure.UnitTests.Services;

[TestSubject(typeof(InitializeService))]
public class InitializeServiceTest:BaseServiceTest
{
    private readonly IInitializeRepository _initializeRepository;

    public InitializeServiceTest()
    {
        _initializeRepository = ServiceProvider.GetRequiredService<IInitializeRepository>();
    }

    [Fact]
    public void InitializeTablesTest()
    {
        _initializeRepository.InitializeTables();
    }
}