//Events/DataChangeEventArgs.cs
using System;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Events
{
    public class DataChangeEventArgs : EventArgs
    {
        public DataChangeType ChangeType { get; init; }
        public object? ChangedData { get; init; }
        public string? ColumnName { get; init; }
        public int RowIndex { get; init; } = -1;
        public int AffectedRowCount { get; init; }
        public TimeSpan OperationDuration { get; init; }
    }

    public enum DataChangeType
    {
        Initialize,
        LoadData,
        ClearData,
        CellValueChanged,
        RemoveRows,
        RemoveEmptyRows,
        AddRows,
        RowValidationChanged
    }
}