namespace PMSWPF.Excptions;

public class DbExistException: Exception
{
    public DbExistException() : base() { }

    public DbExistException(string message) : base(message) { }

    public DbExistException(string message, System.Exception innerException) : base(message, innerException) { }
}