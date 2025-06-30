//Services/Interfaces/IValidationService.cs
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Models;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Events;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Services.Interfaces
{
    public interface IValidationService
    {
        Task<ValidationResult> ValidateCellAsync(DataGridCell cell, DataGridRow row, CancellationToken cancellationToken = default);
        Task<List<ValidationResult>> ValidateRowAsync(DataGridRow row, CancellationToken cancellationToken = default);
        Task<List<ValidationResult>> ValidateAllRowsAsync(IEnumerable<DataGridRow> rows, IProgress<double>? progress = null, CancellationToken cancellationToken = default);

        void AddValidationRule(ValidationRule rule);
        void RemoveValidationRule(string columnName, string ruleName);
        void ClearValidationRules(string? columnName = null);

        List<ValidationRule> GetValidationRules(string columnName);
        bool HasValidationRules(string columnName);
        int GetTotalRuleCount();

        event EventHandler<ValidationCompletedEventArgs> ValidationCompleted;
        event EventHandler<ComponentErrorEventArgs> ValidationErrorOccurred;
    }
}