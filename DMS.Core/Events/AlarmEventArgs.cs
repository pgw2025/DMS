using System;

namespace DMS.Core.Events
{
    public class AlarmEventArgs : EventArgs
    {
        public int VariableId { get; }
        public string VariableName { get; }
        public double CurrentValue { get; }
        public double ThresholdValue { get; }
        public string Message { get; }
        public DateTime Timestamp { get; }
        public string AlarmType { get; } // 可以是 "High", "Low", "Change" 等

        public AlarmEventArgs(int variableId, string variableName, double currentValue, 
                              double thresholdValue, string message, string alarmType)
        {
            VariableId = variableId;
            VariableName = variableName;
            CurrentValue = currentValue;
            ThresholdValue = thresholdValue;
            Message = message;
            Timestamp = DateTime.Now;
            AlarmType = alarmType;
        }
    }
}