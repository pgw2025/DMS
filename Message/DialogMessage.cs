namespace PMSWPF.Message;

public class DialogMessage
{
    public bool IsCancel { get; set; }
    public bool IsConfirm { get; set; }
    public Object? Request { get; set; }
    public Object? Response { get; set; }
}