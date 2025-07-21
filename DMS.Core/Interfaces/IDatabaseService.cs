namespace DMS.Core.Interfaces
{
    public interface IDatabaseService
    {
        void InitializeTables();
        void InitializeTableIndex();
        void InitializeMenus();

        bool IsAnyTable(string tableName);
    }
}