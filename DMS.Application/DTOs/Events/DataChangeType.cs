namespace DMS.Application.DTOs.Events
{
    /// <summary>
    /// 数据变更类型枚举
    /// </summary>
    public enum DataChangeType
    {
        /// <summary>
        /// 添加
        /// </summary>
        Added,

        /// <summary>
        /// 更新
        /// </summary>
        Updated,

        /// <summary>
        /// 删除
        /// </summary>
        Deleted,

        /// <summary>
        /// 加载
        /// </summary>
        Loaded,

        /// <summary>
        /// 批量操作
        /// </summary>
        BatchOperation
    }
}