namespace DMS.Core.Interfaces.Repositories;

public interface IInitializeRepository
{
    void InitializeTables();
    void InitializeTableIndex();
    bool IsAnyTable(string tableName);
    bool IsAnyIndex(string indexName);
    void InitializeMenus();
}