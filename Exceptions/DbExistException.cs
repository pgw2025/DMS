namespace PMSWPF.Excptions;

public class DbExistException : Exception
{
    public DbExistException()
    {
    }

    public DbExistException(string message) : base(message)
    {
    }

    public DbExistException(string message, Exception innerException) : base(message, innerException)
    {
    }
}