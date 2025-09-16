namespace DMS.Application.Events
{
    /// <summary>
    /// 数据加载完成事件参数
    /// </summary>
    public class DataLoadCompletedEventArgs : System.EventArgs
    {

        /// <summary>
        /// 加载是否成功
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// 加载时间
        /// </summary>
        public DateTime LoadTime { get; }

        /// <summary>
        /// 错误信息（如果加载失败）
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="isSuccess">是否成功</param>
        /// <param name="errorMessage">错误信息</param>
        public DataLoadCompletedEventArgs(bool isSuccess, string errorMessage = null)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            LoadTime = DateTime.Now;
        }
    }
}