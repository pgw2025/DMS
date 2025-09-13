namespace DMS.Core.Enums
{
    public enum AlarmRule
    {
        /// <summary>
        /// 超过上限报警
        /// </summary>
        AboveMax,
        
        /// <summary>
        /// 低于下限报警
        /// </summary>
        BelowMin,
        
        /// <summary>
        /// 超出范围报警（同时检查上限和下限）
        /// </summary>
        OutOfRange,
        
        /// <summary>
        /// 死区报警
        /// </summary>
        Deadband,
        
        /// <summary>
        /// 布尔值变化报警
        /// </summary>
        BooleanChange
    }
}