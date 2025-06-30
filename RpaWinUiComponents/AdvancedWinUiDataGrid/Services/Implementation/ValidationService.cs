//Services/Implementation/ValidationService.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Events;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Models;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Services.Interfaces;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Services.Implementation
{
    public class ValidationService : IValidationService
    {
        private readonly ILogger<ValidationService> _logger;
        private readonly ConcurrentDictionary<string, List<ValidationRule>> _validationRules = new();
        private readonly SemaphoreSlim _validationSemaphore = new(5, 5);

        public event EventHandler<ValidationCompletedEventArgs>? ValidationCompleted;
        public event EventHandler<ComponentErrorEventArgs>? ValidationErrorOccurred;

        public ValidationService(ILogger<ValidationService> logger)
        {
            _logger = logger;
        }

        public async Task<ValidationResult> ValidateCellAsync(DataGridCell cell, DataGridRow row, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (row.IsEmpty)
                {
                    cell.ClearValidationErrors();
                    return ValidationResult.Success(cell.ColumnName, cell.RowIndex);
                }

                if (!_validationRules.ContainsKey(cell.ColumnName))
                {
                    cell.ClearValidationErrors();
                    return ValidationResult.Success(cell.ColumnName, cell.RowIndex);
                }

                await _validationSemaphore.WaitAsync(cancellationToken);

                try
                {
                    var rules = _validationRules[cell.ColumnName]
                        .Where(r => r.ShouldApply(row))
                        .OrderByDescending(r => r.Priority)
                        .ToList();

                    var errorMessages = new List<string>();
                    var hasAsyncValidation = false;

                    foreach (var rule in rules)
                    {
                        try
                        {
                            bool isValid;

                            if (rule.IsAsync)
                            {
                                hasAsyncValidation = true;
                                isValid = await rule.ValidateAsync(cell.Value, row, cancellationToken);
                            }
                            else
                            {
                                isValid = rule.Validate(cell.Value, row);
                            }

                            if (!isValid)
                            {
                                errorMessages.Add(rule.ErrorMessage);
                                _logger.LogDebug("Validation failed: {RuleName} for {ColumnName}[{RowIndex}]",
                                    rule.RuleName, cell.ColumnName, cell.RowIndex);
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Validation rule {RuleName} failed with exception", rule.RuleName);
                            errorMessages.Add($"Chyba validácie: {rule.ErrorMessage}");
                        }
                    }

                    cell.SetValidationErrors(errorMessages);

                    var result = new ValidationResult(errorMessages.Count == 0)
                    {
                        ErrorMessages = errorMessages,
                        ColumnName = cell.ColumnName,
                        RowIndex = cell.RowIndex,
                        ValidationDuration = stopwatch.Elapsed,
                        WasAsync = hasAsyncValidation
                    };

                    return result;
                }
                finally
                {
                    _validationSemaphore.Release();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Validation cancelled for {ColumnName}[{RowIndex}]", cell.ColumnName, cell.RowIndex);
                return ValidationResult.Failure("Validácia bola zrušená", cell.ColumnName, cell.RowIndex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating cell {ColumnName}[{RowIndex}]", cell.ColumnName, cell.RowIndex);
                OnValidationErrorOccurred(new ComponentErrorEventArgs(ex, "ValidateCellAsync"));
                return ValidationResult.Failure($"Chyba pri validácii: {ex.Message}", cell.ColumnName, cell.RowIndex);
            }
        }

        public async Task<List<ValidationResult>> ValidateRowAsync(DataGridRow row, CancellationToken cancellationToken = default)
        {
            var results = new List<ValidationResult>();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (row.IsEmpty)
                {
                    foreach (var cell in row.Cells.Values.Where(c => !IsSpecialColumn(c.ColumnName)))
                    {
                        cell.ClearValidationErrors();
                    }
                    row.UpdateValidationStatus();
                    return results;
                }

                var cellsToValidate = row.Cells.Values
                    .Where(c => !IsSpecialColumn(c.ColumnName) && _validationRules.ContainsKey(c.ColumnName))
                    .ToList();

                var validationTasks = cellsToValidate.Select(cell => ValidateCellAsync(cell, row, cancellationToken));
                var cellResults = await Task.WhenAll(validationTasks);

                results.AddRange(cellResults);
                row.UpdateValidationStatus();

                OnValidationCompleted(new ValidationCompletedEventArgs
                {
                    Row = row,
                    Results = results,
                    TotalDuration = stopwatch.Elapsed,
                    AsyncValidationCount = results.Count(r => r.WasAsync)
                });

                _logger.LogDebug("Row validation completed: {ValidCount} valid, {InvalidCount} invalid",
                    results.Count(r => r.IsValid), results.Count(r => !r.IsValid));

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating row {RowIndex}", row.RowIndex);
                OnValidationErrorOccurred(new ComponentErrorEventArgs(ex, "ValidateRowAsync"));
                return results;
            }
        }

        public async Task<List<ValidationResult>> ValidateAllRowsAsync(IEnumerable<DataGridRow> rows, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            var allResults = new List<ValidationResult>();
            var dataRows = rows.Where(r => !r.IsEmpty).ToList();

            if (dataRows.Count == 0)
            {
                _logger.LogInformation("No non-empty rows to validate");
                return allResults;
            }

            try
            {
                _logger.LogInformation("Validating {RowCount} non-empty rows", dataRows.Count);

                const int batchSize = 10;
                var totalRows = dataRows.Count;
                var processedRows = 0;

                for (int i = 0; i < dataRows.Count; i += batchSize)
                {
                    var batch = dataRows.Skip(i).Take(batchSize).ToList();

                    var batchTasks = batch.Select(row => ValidateRowAsync(row, cancellationToken));
                    var batchResults = await Task.WhenAll(batchTasks);

                    foreach (var rowResults in batchResults)
                    {
                        allResults.AddRange(rowResults);
                    }

                    processedRows += batch.Count;
                    var progressPercentage = (double)processedRows / totalRows * 100;
                    progress?.Report(progressPercentage);

                    _logger.LogDebug("Validated batch: {ProcessedRows}/{TotalRows} rows ({Progress:F1}%)",
                        processedRows, totalRows, progressPercentage);

                    cancellationToken.ThrowIfCancellationRequested();
                }

                var validCount = allResults.Count(r => r.IsValid);
                var invalidCount = allResults.Count(r => !r.IsValid);

                _logger.LogInformation("Validation completed: {ValidCount} valid, {InvalidCount} invalid results",
                    validCount, invalidCount);

                return allResults;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Validation of all rows was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating all rows");
                OnValidationErrorOccurred(new ComponentErrorEventArgs(ex, "ValidateAllRowsAsync"));
                return allResults;
            }
        }

        public void AddValidationRule(ValidationRule rule)
        {
            try
            {
                if (rule == null)
                    throw new ArgumentNullException(nameof(rule));

                if (string.IsNullOrWhiteSpace(rule.ColumnName))
                    throw new ArgumentException("ColumnName cannot be null or empty", nameof(rule));

                _validationRules.AddOrUpdate(
                    rule.ColumnName,
                    new List<ValidationRule> { rule },
                    (key, existingRules) =>
                    {
                        existingRules.RemoveAll(r => r.RuleName == rule.RuleName);
                        existingRules.Add(rule);
                        return existingRules;
                    });

                _logger.LogDebug("Added validation rule '{RuleName}' for column '{ColumnName}'",
                    rule.RuleName, rule.ColumnName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding validation rule");
                OnValidationErrorOccurred(new ComponentErrorEventArgs(ex, "AddValidationRule"));
            }
        }

        public void RemoveValidationRule(string columnName, string ruleName)
        {
            try
            {
                if (_validationRules.TryGetValue(columnName, out var rules))
                {
                    var removedCount = rules.RemoveAll(r => r.RuleName == ruleName);
                    _logger.LogDebug("Removed {RemovedCount} validation rule(s) '{RuleName}' from column '{ColumnName}'",
                        removedCount, ruleName, columnName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing validation rule '{RuleName}' from column '{ColumnName}'",
                    ruleName, columnName);
                OnValidationErrorOccurred(new ComponentErrorEventArgs(ex, "RemoveValidationRule"));
            }
        }

        public void ClearValidationRules(string? columnName = null)
        {
            try
            {
                if (columnName == null)
                {
                    var totalRules = _validationRules.Values.Sum(rules => rules.Count);
                    _validationRules.Clear();
                    _logger.LogInformation("Cleared all {TotalRules} validation rules", totalRules);
                }
                else if (_validationRules.TryGetValue(columnName, out var rules))
                {
                    var ruleCount = rules.Count;
                    rules.Clear();
                    _logger.LogDebug("Cleared {RuleCount} validation rules from column '{ColumnName}'",
                        ruleCount, columnName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing validation rules for column: {ColumnName}", columnName ?? "ALL");
                OnValidationErrorOccurred(new ComponentErrorEventArgs(ex, "ClearValidationRules"));
            }
        }

        public List<ValidationRule> GetValidationRules(string columnName)
        {
            try
            {
                return _validationRules.TryGetValue(columnName, out var rules)
                    ? new List<ValidationRule>(rules)
                    : new List<ValidationRule>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting validation rules for column: {ColumnName}", columnName);
                OnValidationErrorOccurred(new ComponentErrorEventArgs(ex, "GetValidationRules"));
                return new List<ValidationRule>();
            }
        }

        public bool HasValidationRules(string columnName)
        {
            return _validationRules.TryGetValue(columnName, out var rules) && rules.Count > 0;
        }

        public int GetTotalRuleCount()
        {
            return _validationRules.Values.Sum(rules => rules.Count);
        }

        private static bool IsSpecialColumn(string columnName)
        {
            return columnName == "DeleteAction" || columnName == "ValidAlerts";
        }

        protected virtual void OnValidationCompleted(ValidationCompletedEventArgs e)
        {
            ValidationCompleted?.Invoke(this, e);
        }

        protected virtual void OnValidationErrorOccurred(ComponentErrorEventArgs e)
        {
            ValidationErrorOccurred?.Invoke(this, e);
        }
    }
}