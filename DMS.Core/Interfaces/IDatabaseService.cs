namespace DMS.Core.Interfaces
{
    public interface IDatabaseService
    {
        void InitializeDataBase();
        Task InitializeMenu();
    }
}