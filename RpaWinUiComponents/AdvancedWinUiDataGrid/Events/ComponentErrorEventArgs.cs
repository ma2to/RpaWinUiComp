//Events/ComponentErrorEventArgs.cs
using System;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Events
{
    public class ComponentErrorEventArgs : EventArgs
    {
        public Exception Exception { get; }
        public string Operation { get; }
        public string? AdditionalInfo { get; }
        public DateTime Timestamp { get; } = DateTime.Now;

        public ComponentErrorEventArgs(Exception exception, string operation, string? additionalInfo = null)
        {
            Exception = exception;
            Operation = operation;
            AdditionalInfo = additionalInfo;
        }

        public override string ToString()
        {
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] {Operation}: {Exception.Message}" +
                   (string.IsNullOrEmpty(AdditionalInfo) ? "" : $" - {AdditionalInfo}");
        }
    }
}