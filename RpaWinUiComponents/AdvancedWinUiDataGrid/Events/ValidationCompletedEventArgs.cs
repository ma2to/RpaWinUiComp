//Events/ValidationCompletedEventArgs.cs
using System;
using System.Collections.Generic;
using System.Linq;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Models;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Events
{
    public class ValidationCompletedEventArgs : EventArgs
    {
        public DataGridRow? Row { get; init; }
        public DataGridCell? Cell { get; init; }
        public List<ValidationResult> Results { get; init; } = new();
        public bool IsValid => Results.All(r => r.IsValid);
        public TimeSpan TotalDuration { get; init; }
        public int AsyncValidationCount { get; init; }
    }
}