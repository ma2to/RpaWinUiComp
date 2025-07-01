//Services/Implementation/ColumnService.cs - OPRAVENÝ
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Events;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Services.Interfaces;
// ALIAS pre riešenie konfliktu
//using DataGridColumnDefinition = RpaWinUiComponents.AdvancedWinUiDataGrid.Models.ColumnDefinition;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Services.Implementation
{
    public class ColumnService : IColumnService
    {
        private readonly ILogger<ColumnService> _logger;

        public event EventHandler<ComponentErrorEventArgs>? ErrorOccurred;

        public ColumnService(ILogger<ColumnService> logger)
        {
            _logger = logger;
        }

        public List<DataGridColumnDefinition> ProcessColumnDefinitions(List<DataGridColumnDefinition> columns)
        {
            try
            {
                _logger.LogDebug("Processing {Count} column definitions", columns?.Count ?? 0);

                var processedColumns = new List<DataGridColumnDefinition>();
                var existingNames = new List<string>();

                foreach (var column in columns ?? new List<DataGridColumnDefinition>())
                {
                    if (!column.IsValid(out var errorMessage))
                    {
                        _logger.LogWarning("Invalid column definition: {ErrorMessage}", errorMessage);
                        continue;
                    }

                    var uniqueName = GenerateUniqueColumnName(column.Name, existingNames);
                    var processedColumn = column.Clone();
                    processedColumn.Name = uniqueName;

                    processedColumns.Add(processedColumn);
                    existingNames.Add(uniqueName);
                }

                _logger.LogInformation("Successfully processed {Count} column definitions", processedColumns.Count);
                return processedColumns;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing column definitions");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "ProcessColumnDefinitions"));
                return columns ?? new List<DataGridColumnDefinition>();
            }
        }

        public string GenerateUniqueColumnName(string baseName, List<string> existingNames)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(baseName))
                    baseName = "Column";

                var uniqueName = baseName;
                var counter = 1;

                while (existingNames.Contains(uniqueName, StringComparer.OrdinalIgnoreCase))
                {
                    uniqueName = $"{baseName}_{counter}";
                    counter++;
                }

                _logger.LogTrace("Generated unique column name: {UniqueName} from base: {BaseName}", uniqueName, baseName);
                return uniqueName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating unique column name for base: {BaseName}", baseName);
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "GenerateUniqueColumnName"));
                return baseName ?? "Column";
            }
        }

        public DataGridColumnDefinition CreateDeleteActionColumn()
        {
            _logger.LogDebug("Creating DeleteAction column");
            return new DataGridColumnDefinition
            {
                Name = "DeleteAction",
                DataType = typeof(object),
                MinWidth = 50,
                MaxWidth = 50,
                Width = 50,
                AllowResize = false,
                AllowSort = false,
                IsReadOnly = true,
                Header = "Akcie"
            };
        }

        public DataGridColumnDefinition CreateValidAlertsColumn()
        {
            _logger.LogDebug("Creating ValidAlerts column");
            return new DataGridColumnDefinition
            {
                Name = "ValidAlerts",
                DataType = typeof(string),
                MinWidth = 150,
                MaxWidth = 400,
                Width = 250,
                AllowResize = true,
                AllowSort = false,
                IsReadOnly = true,
                Header = "Validačné chyby"
            };
        }

        public bool IsSpecialColumn(string columnName)
        {
            var isSpecial = columnName == "DeleteAction" || columnName == "ValidAlerts";
            _logger.LogTrace("Column {ColumnName} is special: {IsSpecial}", columnName, isSpecial);
            return isSpecial;
        }

        public List<DataGridColumnDefinition> ReorderSpecialColumns(List<DataGridColumnDefinition> columns)
        {
            try
            {
                var result = new List<DataGridColumnDefinition>();

                var normalColumns = columns.Where(c => !IsSpecialColumn(c.Name)).ToList();
                result.AddRange(normalColumns);

                var deleteActionColumn = columns.FirstOrDefault(c => c.Name == "DeleteAction");
                if (deleteActionColumn != null)
                {
                    result.Add(deleteActionColumn);
                }

                var validAlertsColumn = columns.FirstOrDefault(c => c.Name == "ValidAlerts");
                if (validAlertsColumn != null)
                {
                    result.Add(validAlertsColumn);
                }
                else
                {
                    result.Add(CreateValidAlertsColumn());
                }

                _logger.LogDebug("Reordered columns: {NormalCount} normal + {SpecialCount} special",
                    normalColumns.Count, result.Count - normalColumns.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering special columns");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "ReorderSpecialColumns"));
                return columns;
            }
        }

        public void ValidateColumnDefinitions(List<DataGridColumnDefinition> columns)
        {
            try
            {
                var errors = new List<string>();

                if (columns == null || columns.Count == 0)
                {
                    errors.Add("Zoznam stĺpcov nemôže byť prázdny");
                }
                else
                {
                    var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    foreach (var column in columns)
                    {
                        if (!column.IsValid(out var columnError))
                        {
                            errors.Add($"Stĺpec '{column.Name}': {columnError}");
                        }

                        if (names.Contains(column.Name))
                        {
                            errors.Add($"Duplicitný názov stĺpca: '{column.Name}'");
                        }
                        else
                        {
                            names.Add(column.Name);
                        }
                    }
                }

                if (errors.Count > 0)
                {
                    var errorMessage = string.Join("; ", errors);
                    _logger.LogError("Column validation failed: {Errors}", errorMessage);
                    throw new ArgumentException($"Chyby v definíciách stĺpcov: {errorMessage}");
                }

                _logger.LogDebug("Column definitions validation passed for {Count} columns", columns.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating column definitions");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "ValidateColumnDefinitions"));
                throw;
            }
        }

        protected virtual void OnErrorOccurred(ComponentErrorEventArgs e)
        {
            ErrorOccurred?.Invoke(this, e);
        }
    }
}