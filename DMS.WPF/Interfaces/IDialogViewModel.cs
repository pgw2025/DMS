namespace DMS.WPF.Interfaces
{
    /// <summary>
    /// 对话框视图模型接口
    /// </summary>
    public interface IDialogViewModel
    {
        /// <summary>
        /// 关闭请求事件
        /// </summary>
        event Action<bool?> CloseRequested;
    }
}