//Services/Implementation/ExportService.cs - OPRAVENÝ
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Events;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Models;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Services.Interfaces;
// ALIAS pre riešenie konfliktu
//using DataGridColumnDefinition = RpaWinUiComponents.AdvancedWinUiDataGrid.Models.ColumnDefinition;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Services.Implementation
{
    public class ExportService : IExportService
    {
        private readonly ILogger<ExportService> _logger;

        public event EventHandler<ComponentErrorEventArgs>? ErrorOccurred;

        public ExportService(ILogger<ExportService> logger)
        {
            _logger = logger;
        }

        public async Task<DataTable> ExportToDataTableAsync(List<DataGridRow> rows, List<DataGridColumnDefinition> columns, bool includeValidAlerts = false)
        {
            try
            {
                _logger.LogDebug("Exporting {RowCount} rows with {ColumnCount} columns to DataTable, includeValidAlerts: {IncludeValidAlerts}",
                    rows?.Count ?? 0, columns?.Count ?? 0, includeValidAlerts);

                var dataTable = new DataTable();

                if (rows == null || columns == null)
                {
                    _logger.LogWarning("Cannot export null rows or columns");
                    return dataTable;
                }

                var exportColumns = GetExportColumns(columns, includeValidAlerts);

                // Create DataTable columns
                await Task.Run(() =>
                {
                    foreach (var column in exportColumns)
                    {
                        var dataType = Nullable.GetUnderlyingType(column.DataType) ?? column.DataType;
                        dataTable.Columns.Add(column.Name, dataType);
                        _logger.LogTrace("Added column to DataTable: {ColumnName} ({DataType})", column.Name, dataType.Name);
                    }
                });

                // Add data rows (only non-empty rows)
                var dataRows = rows.Where(r => !r.IsEmpty).ToList();

                await Task.Run(() =>
                {
                    foreach (var row in dataRows)
                    {
                        var dataRow = dataTable.NewRow();

                        foreach (var column in exportColumns)
                        {
                            var value = row.GetValue<object>(column.Name);
                            dataRow[column.Name] = value ?? DBNull.Value;
                        }

                        dataTable.Rows.Add(dataRow);
                    }
                });

                _logger.LogInformation("Successfully exported {RowCount} rows with {ColumnCount} columns to DataTable",
                    dataTable.Rows.Count, dataTable.Columns.Count);
                return dataTable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting to DataTable");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "ExportToDataTableAsync"));
                return new DataTable();
            }
        }

        public async Task<string> ExportToCsvAsync(List<DataGridRow> rows, List<DataGridColumnDefinition> columns, bool includeValidAlerts = false)
        {
            try
            {
                _logger.LogDebug("Exporting {RowCount} rows to CSV format, includeValidAlerts: {IncludeValidAlerts}",
                    rows?.Count ?? 0, includeValidAlerts);

                if (rows == null || columns == null)
                {
                    _logger.LogWarning("Cannot export null rows or columns to CSV");
                    return string.Empty;
                }

                var result = await Task.Run(() =>
                {
                    var sb = new StringBuilder();
                    var exportColumns = GetExportColumns(columns, includeValidAlerts);

                    // Add header row
                    var headers = exportColumns.Select(c => EscapeCsvValue(c.Header ?? c.Name));
                    sb.AppendLine(string.Join(",", headers));

                    // Add data rows (only non-empty rows)
                    var dataRows = rows.Where(r => !r.IsEmpty).ToList();
                    foreach (var row in dataRows)
                    {
                        var values = exportColumns.Select(c =>
                        {
                            var value = row.GetValue<object>(c.Name);
                            return EscapeCsvValue(value?.ToString() ?? "");
                        });
                        sb.AppendLine(string.Join(",", values));
                    }

                    return sb.ToString();
                });

                _logger.LogInformation("Successfully exported to CSV, length: {Length}", result.Length);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting to CSV");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "ExportToCsvAsync"));
                return string.Empty;
            }
        }

        public async Task<byte[]> ExportToExcelAsync(List<DataGridRow> rows, List<DataGridColumnDefinition> columns, bool includeValidAlerts = false)
        {
            try
            {
                _logger.LogDebug("Exporting {RowCount} rows to Excel format, includeValidAlerts: {IncludeValidAlerts}",
                    rows?.Count ?? 0, includeValidAlerts);

                // For this implementation, we'll export as CSV and convert to bytes
                // In a real implementation, you might want to use a library like ClosedXML or EPPlus
                var csv = await ExportToCsvAsync(rows, columns, includeValidAlerts);
                var result = Encoding.UTF8.GetBytes(csv);

                _logger.LogInformation("Successfully exported to Excel format, bytes: {ByteCount}", result.Length);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting to Excel");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "ExportToExcelAsync"));
                return Array.Empty<byte>();
            }
        }

        public async Task<List<Dictionary<string, object?>>> ExportToDictionariesAsync(List<DataGridRow> rows, List<DataGridColumnDefinition> columns)
        {
            try
            {
                _logger.LogDebug("Exporting {RowCount} rows to dictionary list", rows?.Count ?? 0);

                if (rows == null || columns == null)
                {
                    _logger.LogWarning("Cannot export null rows or columns to dictionaries");
                    return new List<Dictionary<string, object?>>();
                }

                var result = await Task.Run(() =>
                {
                    var dictionaries = new List<Dictionary<string, object?>>();
                    var exportColumns = GetExportColumns(columns, false); // Exclude ValidAlerts by default

                    var dataRows = rows.Where(r => !r.IsEmpty).ToList();
                    foreach (var row in dataRows)
                    {
                        var dict = new Dictionary<string, object?>();
                        foreach (var column in exportColumns)
                        {
                            dict[column.Name] = row.GetValue<object>(column.Name);
                        }
                        dictionaries.Add(dict);
                    }

                    return dictionaries;
                });

                _logger.LogInformation("Successfully exported {RowCount} rows to dictionary list", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting to dictionaries");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "ExportToDictionariesAsync"));
                return new List<Dictionary<string, object?>>();
            }
        }

        private List<DataGridColumnDefinition> GetExportColumns(List<DataGridColumnDefinition> originalColumns, bool includeValidAlerts)
        {
            var exportColumns = new List<DataGridColumnDefinition>();

            // Add normal columns (exclude DeleteAction and ValidAlerts initially)
            var normalColumns = originalColumns
                .Where(c => c.Name != "DeleteAction" && c.Name != "ValidAlerts")
                .ToList();

            exportColumns.AddRange(normalColumns);

            _logger.LogDebug("Added {NormalColumnCount} normal columns to export", normalColumns.Count);

            // Add ValidAlerts column at the end if requested
            if (includeValidAlerts)
            {
                var validAlertsColumn = originalColumns.FirstOrDefault(c => c.Name == "ValidAlerts");
                if (validAlertsColumn != null)
                {
                    exportColumns.Add(validAlertsColumn);
                    _logger.LogDebug("Added ValidAlerts column to export (at end)");
                }
                else
                {
                    _logger.LogWarning("ValidAlerts column requested but not found in original columns");
                }
            }

            _logger.LogInformation("Export columns prepared: {ColumnCount} total, includeValidAlerts: {IncludeValidAlerts}",
                exportColumns.Count, includeValidAlerts);

            return exportColumns;
        }

        private string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            // If value contains comma, quote, newline, or carriage return, escape it
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                // Escape quotes by doubling them and wrap in quotes
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }

        protected virtual void OnErrorOccurred(ComponentErrorEventArgs e)
        {
            ErrorOccurred?.Invoke(this, e);
        }
    }
}