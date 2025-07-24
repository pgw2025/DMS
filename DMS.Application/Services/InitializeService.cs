using DMS.Core.Interfaces.Repositories;

namespace DMS.Application.Services;

public class InitializeService
{
    private readonly IInitializeRepository _repository;

    public InitializeService(IInitializeRepository repository )
    {
        _repository = repository;
    }

    public void InitializeTables()
    {
        _repository.InitializeTables();
    }

    public void InitializeTableIndex()
    {
        _repository.InitializeTableIndex();
    }

    public void InitializeMenus()
    {
        _repository.InitializeMenus();
    }

}