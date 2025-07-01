//Services/Implementation/DataService.cs - KOMPLETNÝ OPRAVENÝ
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Events;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Models;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Services.Interfaces;
// ALIAS pre riešenie konfliktu
//using DataGridColumnDefinition = RpaWinUiComponents.AdvancedWinUiDataGrid.Models.ColumnDefinition;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Services.Implementation
{
    public class DataService : IDataService
    {
        private readonly ILogger<DataService> _logger;
        private List<DataGridRow> _rows = new();
        private List<DataGridColumnDefinition> _columns = new();
        private bool _isInitialized = false;

        public event EventHandler<DataChangeEventArgs>? DataChanged;
        public event EventHandler<ComponentErrorEventArgs>? ErrorOccurred;

        public DataService(ILogger<DataService> logger)
        {
            _logger = logger;
        }

        public async Task InitializeAsync(List<DataGridColumnDefinition> columns, int initialRowCount = 100)
        {
            try
            {
                _columns = columns ?? throw new ArgumentNullException(nameof(columns));
                _rows.Clear();

                initialRowCount = Math.Max(1, Math.Min(initialRowCount, 10000)); // Safety limits

                _logger.LogInformation("Initializing DataService with {ColumnCount} columns and {InitialRowCount} rows",
                    _columns.Count, initialRowCount);

                // Create initial empty rows
                await Task.Run(() =>
                {
                    for (int i = 0; i < initialRowCount; i++)
                    {
                        var row = CreateEmptyRow(i);
                        _rows.Add(row);
                    }
                });

                _isInitialized = true;

                OnDataChanged(new DataChangeEventArgs
                {
                    ChangeType = DataChangeType.Initialize,
                    AffectedRowCount = initialRowCount
                });

                _logger.LogInformation("DataService initialized successfully with {RowCount} empty rows", _rows.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing DataService");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "InitializeAsync"));
                throw;
            }
        }

        public async Task LoadDataAsync(DataTable dataTable)
        {
            try
            {
                if (!_isInitialized)
                    throw new InvalidOperationException("DataService must be initialized first");

                _logger.LogInformation("Loading data from DataTable with {RowCount} rows", dataTable?.Rows.Count ?? 0);

                var newRows = new List<DataGridRow>();

                if (dataTable != null)
                {
                    await Task.Run(() =>
                    {
                        int rowIndex = 0;
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            var gridRow = new DataGridRow(rowIndex);

                            foreach (var column in _columns)
                            {
                                var cell = new DataGridCell(column.Name, column.DataType, rowIndex, _columns.IndexOf(column))
                                {
                                    IsReadOnly = column.IsReadOnly
                                };

                                if (dataTable.Columns.Contains(column.Name))
                                {
                                    var value = dataRow[column.Name];
                                    cell.SetValueWithoutValidation(value == DBNull.Value ? null : value);
                                }

                                gridRow.AddCell(column.Name, cell);
                            }

                            gridRow.UpdateEmptyStatus();
                            newRows.Add(gridRow);
                            rowIndex++;
                        }
                    });
                }

                // Replace current rows with loaded data
                _rows.Clear();
                _rows.AddRange(newRows);

                OnDataChanged(new DataChangeEventArgs
                {
                    ChangeType = DataChangeType.LoadData,
                    AffectedRowCount = newRows.Count
                });

                _logger.LogInformation("Successfully loaded {RowCount} rows from DataTable", _rows.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading data from DataTable");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "LoadDataAsync"));
                throw;
            }
        }

        public async Task LoadDataAsync(List<Dictionary<string, object?>> data)
        {
            try
            {
                if (!_isInitialized)
                    throw new InvalidOperationException("DataService must be initialized first");

                _logger.LogInformation("Loading data from dictionary list with {RowCount} rows", data?.Count ?? 0);

                var newRows = new List<DataGridRow>();

                if (data != null)
                {
                    await Task.Run(() =>
                    {
                        int rowIndex = 0;
                        foreach (var dataRow in data)
                        {
                            var gridRow = new DataGridRow(rowIndex);

                            foreach (var column in _columns)
                            {
                                var cell = new DataGridCell(column.Name, column.DataType, rowIndex, _columns.IndexOf(column))
                                {
                                    IsReadOnly = column.IsReadOnly
                                };

                                if (dataRow.ContainsKey(column.Name))
                                {
                                    cell.SetValueWithoutValidation(dataRow[column.Name]);
                                }

                                gridRow.AddCell(column.Name, cell);
                            }

                            gridRow.UpdateEmptyStatus();
                            newRows.Add(gridRow);
                            rowIndex++;
                        }
                    });
                }

                // Replace current rows with loaded data
                _rows.Clear();
                _rows.AddRange(newRows);

                OnDataChanged(new DataChangeEventArgs
                {
                    ChangeType = DataChangeType.LoadData,
                    AffectedRowCount = newRows.Count
                });

                _logger.LogInformation("Successfully loaded {RowCount} rows from dictionary list", _rows.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading data from dictionary list");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "LoadDataAsync"));
                throw;
            }
        }

        public async Task<DataTable> ExportToDataTableAsync()
        {
            try
            {
                _logger.LogDebug("Exporting data to DataTable");

                var dataTable = new DataTable();

                // Add columns (exclude special columns)
                foreach (var column in _columns.Where(c => !IsSpecialColumn(c.Name)))
                {
                    var dataType = Nullable.GetUnderlyingType(column.DataType) ?? column.DataType;
                    dataTable.Columns.Add(column.Name, dataType);
                }

                // Add rows (only non-empty rows)
                await Task.Run(() =>
                {
                    foreach (var row in _rows.Where(r => !r.IsEmpty))
                    {
                        var dataRow = dataTable.NewRow();
                        foreach (var column in _columns.Where(c => !IsSpecialColumn(c.Name)))
                        {
                            var value = row.GetValue<object>(column.Name);
                            dataRow[column.Name] = value ?? DBNull.Value;
                        }
                        dataTable.Rows.Add(dataRow);
                    }
                });

                _logger.LogInformation("Exported {RowCount} rows to DataTable", dataTable.Rows.Count);
                return dataTable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting data to DataTable");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "ExportToDataTableAsync"));
                return new DataTable();
            }
        }

        public async Task ClearAllDataAsync()
        {
            try
            {
                _logger.LogInformation("Clearing all data");

                await Task.Run(() =>
                {
                    foreach (var row in _rows)
                    {
                        foreach (var cell in row.Cells.Values.Where(c => !IsSpecialColumn(c.ColumnName)))
                        {
                            cell.Value = null;
                            cell.ClearValidationErrors();
                        }
                        row.UpdateEmptyStatus();
                        row.UpdateValidationStatus();
                    }
                });

                OnDataChanged(new DataChangeEventArgs
                {
                    ChangeType = DataChangeType.ClearData,
                    AffectedRowCount = _rows.Count
                });

                _logger.LogInformation("Successfully cleared all data");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing all data");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "ClearAllDataAsync"));
                throw;
            }
        }

        public async Task RemoveEmptyRowsAsync()
        {
            try
            {
                _logger.LogDebug("Removing empty rows");

                var result = await Task.Run(() =>
                {
                    var dataRows = _rows.Where(r => !r.IsEmpty).ToList();
                    var removedCount = _rows.Count - dataRows.Count;

                    // Keep data rows and recreate indexes
                    for (int i = 0; i < dataRows.Count; i++)
                    {
                        // Update row index for consistency
                        var updatedRow = new DataGridRow(i);
                        foreach (var cell in dataRows[i].Cells.Values)
                        {
                            var newCell = new DataGridCell(cell.ColumnName, cell.DataType, i, cell.ColumnIndex)
                            {
                                Value = cell.Value,
                                OriginalValue = cell.OriginalValue,
                                IsReadOnly = cell.IsReadOnly
                            };
                            newCell.SetValidationErrors(cell.ValidationErrors);
                            updatedRow.AddCell(cell.ColumnName, newCell);
                        }
                        dataRows[i] = updatedRow;
                    }

                    return new { DataRows = dataRows, RemovedCount = removedCount };
                });

                _rows.Clear();
                _rows.AddRange(result.DataRows);

                OnDataChanged(new DataChangeEventArgs
                {
                    ChangeType = DataChangeType.RemoveEmptyRows,
                    AffectedRowCount = result.RemovedCount
                });

                _logger.LogInformation("Removed {RemovedCount} empty rows, {DataRowCount} data rows remain",
                    result.RemovedCount, result.DataRows.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing empty rows");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "RemoveEmptyRowsAsync"));
                throw;
            }
        }

        public async Task RemoveRowsByConditionAsync(string columnName, Func<object?, bool> condition)
        {
            try
            {
                _logger.LogDebug("Removing rows by condition for column: {ColumnName}", columnName);

                var result = await Task.Run(() =>
                {
                    var rowsToRemove = new List<DataGridRow>();

                    foreach (var row in _rows.ToList())
                    {
                        if (columnName == "HasValidationErrors")
                        {
                            if (condition(row.HasValidationErrors))
                                rowsToRemove.Add(row);
                        }
                        else
                        {
                            var cell = row.GetCell(columnName);
                            if (cell != null && condition(cell.Value))
                                rowsToRemove.Add(row);
                        }
                    }

                    return rowsToRemove;
                });

                foreach (var row in result)
                {
                    _rows.Remove(row);
                }

                OnDataChanged(new DataChangeEventArgs
                {
                    ChangeType = DataChangeType.RemoveRows,
                    AffectedRowCount = result.Count
                });

                _logger.LogInformation("Removed {RowCount} rows by condition for column: {ColumnName}", result.Count, columnName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing rows by condition for column: {ColumnName}", columnName);
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "RemoveRowsByConditionAsync"));
                throw;
            }
        }

        public async Task<int> RemoveRowsByValidationAsync(List<ValidationRule> customRules)
        {
            try
            {
                _logger.LogDebug("Removing rows by custom validation with {RuleCount} rules", customRules?.Count ?? 0);

                if (customRules == null || customRules.Count == 0)
                    return 0;

                var result = await Task.Run(() =>
                {
                    var rowsToRemove = new List<DataGridRow>();

                    foreach (var row in _rows.Where(r => !r.IsEmpty).ToList())
                    {
                        foreach (var rule in customRules)
                        {
                            var cell = row.GetCell(rule.ColumnName);
                            if (cell != null && !rule.Validate(cell.Value, row))
                            {
                                rowsToRemove.Add(row);
                                break; // One failed rule is enough to remove the row
                            }
                        }
                    }

                    return rowsToRemove;
                });

                foreach (var row in result)
                {
                    _rows.Remove(row);
                }

                OnDataChanged(new DataChangeEventArgs
                {
                    ChangeType = DataChangeType.RemoveRows,
                    AffectedRowCount = result.Count
                });

                _logger.LogInformation("Removed {RowCount} rows by custom validation", result.Count);
                return result.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing rows by custom validation");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "RemoveRowsByValidationAsync"));
                return 0;
            }
        }

        public DataGridRow CreateEmptyRow()
        {
            var rowIndex = _rows.Count;
            return CreateEmptyRow(rowIndex);
        }

        public List<DataGridRow> GetRows()
        {
            return new List<DataGridRow>(_rows);
        }

        public int GetRowCount()
        {
            return _rows.Count;
        }

        private DataGridRow CreateEmptyRow(int rowIndex)
        {
            var row = new DataGridRow(rowIndex);

            foreach (var column in _columns)
            {
                var cell = new DataGridCell(column.Name, column.DataType, rowIndex, _columns.IndexOf(column))
                {
                    IsReadOnly = column.IsReadOnly,
                    Value = null
                };

                row.AddCell(column.Name, cell);
            }

            row.UpdateEmptyStatus();
            return row;
        }

        private static bool IsSpecialColumn(string columnName)
        {
            return columnName == "DeleteAction" || columnName == "ValidAlerts";
        }

        protected virtual void OnDataChanged(DataChangeEventArgs e)
        {
            DataChanged?.Invoke(this, e);
        }

        protected virtual void OnErrorOccurred(ComponentErrorEventArgs e)
        {
            ErrorOccurred?.Invoke(this, e);
        }
    }
}