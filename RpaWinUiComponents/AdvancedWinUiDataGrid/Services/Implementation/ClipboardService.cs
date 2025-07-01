//Services/Implementation/ClipboardService.cs - OPRAVENÝ
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Windows.ApplicationModel.DataTransfer;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Events;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Models;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Services.Interfaces;
// ALIAS pre riešenie konfliktu
//using DataGridColumnDefinition = RpaWinUiComponents.AdvancedWinUiDataGrid.Models.ColumnDefinition;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Services.Implementation
{
    public class ClipboardService : IClipboardService
    {
        private readonly ILogger<ClipboardService> _logger;

        public event EventHandler<ComponentErrorEventArgs>? ErrorOccurred;

        public ClipboardService(ILogger<ClipboardService> logger)
        {
            _logger = logger;
        }

        public async Task<string?> GetClipboardTextAsync()
        {
            try
            {
                var dataPackageView = Clipboard.GetContent();
                if (dataPackageView.Contains(StandardDataFormats.Text))
                {
                    var result = await dataPackageView.GetTextAsync();
                    _logger.LogDebug("Retrieved clipboard text, length: {Length}", result?.Length ?? 0);
                    return result;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting clipboard text");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "GetClipboardTextAsync"));
                return null;
            }
        }

        public async Task SetClipboardTextAsync(string text)
        {
            try
            {
                var dataPackage = new DataPackage();
                dataPackage.SetText(text);
                dataPackage.RequestedOperation = DataPackageOperation.Copy;

                Clipboard.SetContent(dataPackage);
                _logger.LogDebug("Set clipboard text, length: {Length}", text?.Length ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting clipboard text");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "SetClipboardTextAsync"));
            }
        }

        public async Task<bool> HasClipboardTextAsync()
        {
            try
            {
                var dataPackageView = Clipboard.GetContent();
                var result = dataPackageView.Contains(StandardDataFormats.Text);
                _logger.LogDebug("Clipboard contains text: {HasText}", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking clipboard text");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "HasClipboardTextAsync"));
                return false;
            }
        }

        public string ConvertToExcelFormat(string[,] data)
        {
            try
            {
                if (data == null || data.Length == 0)
                {
                    _logger.LogWarning("ConvertToExcelFormat called with null or empty data");
                    return string.Empty;
                }

                var sb = new StringBuilder();
                int rows = data.GetLength(0);
                int cols = data.GetLength(1);

                _logger.LogDebug("Converting {Rows}x{Cols} data to Excel format", rows, cols);

                for (int i = 0; i < rows; i++)
                {
                    var rowData = new string[cols];
                    for (int j = 0; j < cols; j++)
                    {
                        rowData[j] = data[i, j] ?? "";
                    }

                    if (i > 0)
                        sb.AppendLine();

                    sb.Append(string.Join("\t", rowData));
                }

                var result = sb.ToString();
                _logger.LogDebug("Excel format conversion completed, result length: {Length}", result.Length);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting data to Excel format");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "ConvertToExcelFormat"));
                return string.Empty;
            }
        }

        public string[,] ParseFromExcelFormat(string clipboardData)
        {
            try
            {
                if (string.IsNullOrEmpty(clipboardData))
                {
                    _logger.LogWarning("ParseFromExcelFormat called with null or empty data");
                    return new string[0, 0];
                }

                _logger.LogDebug("Parsing clipboard data, length: {Length}", clipboardData.Length);

                var normalizedData = clipboardData.Replace("\r\n", "\n").Replace("\r", "\n");
                var lines = normalizedData.Split(new[] { '\n' }, StringSplitOptions.None);

                var lastNonEmptyLine = lines.Length - 1;
                while (lastNonEmptyLine >= 0 && string.IsNullOrEmpty(lines[lastNonEmptyLine]))
                {
                    lastNonEmptyLine--;
                }

                if (lastNonEmptyLine < 0)
                    return new string[0, 0];

                var actualLines = lines.Take(lastNonEmptyLine + 1).ToArray();

                if (actualLines.Length == 0)
                    return new string[0, 0];

                var maxCols = actualLines.Max(line => line.Split('\t').Length);

                if (actualLines.Length == 1 && !actualLines[0].Contains('\t'))
                {
                    var result = new string[1, 1];
                    result[0, 0] = actualLines[0];
                    _logger.LogDebug("Parsed single cell data");
                    return result;
                }

                var resultArray = new string[actualLines.Length, maxCols];

                for (int i = 0; i < actualLines.Length; i++)
                {
                    var cells = actualLines[i].Split('\t');
                    for (int j = 0; j < maxCols; j++)
                    {
                        resultArray[i, j] = j < cells.Length ? (cells[j] ?? "") : "";
                    }
                }

                _logger.LogDebug("Parsed clipboard data to {Rows}x{Cols} array", actualLines.Length, maxCols);
                return resultArray;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing clipboard data from Excel format");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "ParseFromExcelFormat"));
                var fallbackResult = new string[1, 1];
                fallbackResult[0, 0] = clipboardData ?? "";
                return fallbackResult;
            }
        }

        public async Task CopySelectedCellsAsync(IEnumerable<DataGridCell> selectedCells)
        {
            try
            {
                var cells = selectedCells.ToList();
                if (cells.Count == 0)
                {
                    _logger.LogDebug("No cells selected for copy operation");
                    return;
                }

                // Create data map from selected cells
                var dataMap = new Dictionary<(int row, int col), string>();
                var minRow = int.MaxValue;
                var maxRow = int.MinValue;
                var minCol = int.MaxValue;
                var maxCol = int.MinValue;

                foreach (var cell in cells)
                {
                    var value = cell.Value?.ToString() ?? "";
                    dataMap[(cell.RowIndex, cell.ColumnIndex)] = value;

                    minRow = Math.Min(minRow, cell.RowIndex);
                    maxRow = Math.Max(maxRow, cell.RowIndex);
                    minCol = Math.Min(minCol, cell.ColumnIndex);
                    maxCol = Math.Max(maxCol, cell.ColumnIndex);
                }

                // Convert to 2D array
                var rows = maxRow - minRow + 1;
                var cols = maxCol - minCol + 1;
                var data = new string[rows, cols];

                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        var actualRow = minRow + r;
                        var actualCol = minCol + c;
                        data[r, c] = dataMap.TryGetValue((actualRow, actualCol), out var value) ? value : "";
                    }
                }

                var excelFormat = ConvertToExcelFormat(data);
                await SetClipboardTextAsync(excelFormat);

                _logger.LogDebug("Copied {CellCount} cells to clipboard", cells.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error copying selected cells");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "CopySelectedCellsAsync"));
            }
        }

        public async Task<bool> PasteToPositionAsync(int startRowIndex, int startColumnIndex, List<DataGridRow> rows, List<DataGridColumnDefinition> columns)
        {
            try
            {
                var clipboardData = await GetClipboardTextAsync();
                if (string.IsNullOrEmpty(clipboardData))
                {
                    _logger.LogDebug("No clipboard data available for paste");
                    return false;
                }

                var parsedData = ParseFromExcelFormat(clipboardData);
                var dataRows = parsedData.GetLength(0);
                var dataCols = parsedData.GetLength(1);

                if (dataRows == 0 || dataCols == 0)
                {
                    _logger.LogDebug("No valid data parsed from clipboard");
                    return false;
                }

                var editableColumns = columns.Where(c => !IsSpecialColumn(c.Name)).ToList();

                // Ensure we have enough rows
                var neededRows = startRowIndex + dataRows;
                while (rows.Count < neededRows)
                {
                    var newRow = CreateEmptyRow(rows.Count, editableColumns);
                    rows.Add(newRow);
                }

                // Paste data
                var pastedCells = 0;
                for (int r = 0; r < dataRows; r++)
                {
                    var targetRowIndex = startRowIndex + r;
                    if (targetRowIndex >= rows.Count) break;

                    for (int c = 0; c < dataCols; c++)
                    {
                        var targetColumnIndex = startColumnIndex + c;
                        if (targetColumnIndex >= editableColumns.Count) break;

                        var columnName = editableColumns[targetColumnIndex].Name;
                        var targetRow = rows[targetRowIndex];
                        var cell = targetRow.GetCell(columnName);

                        if (cell != null && !cell.IsReadOnly)
                        {
                            cell.Value = parsedData[r, c];
                            pastedCells++;
                        }
                    }
                }

                _logger.LogDebug("Pasted {PastedCells} cells from clipboard", pastedCells);
                return pastedCells > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pasting to position");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "PasteToPositionAsync"));
                return false;
            }
        }

        private DataGridRow CreateEmptyRow(int rowIndex, List<DataGridColumnDefinition> columns)
        {
            var row = new DataGridRow(rowIndex);

            foreach (var column in columns)
            {
                var cell = new DataGridCell(column.Name, column.DataType, rowIndex, columns.IndexOf(column))
                {
                    IsReadOnly = column.IsReadOnly
                };
                row.AddCell(column.Name, cell);
            }

            return row;
        }

        private static bool IsSpecialColumn(string columnName)
        {
            return columnName == "DeleteAction" || columnName == "ValidAlerts";
        }

        protected virtual void OnErrorOccurred(ComponentErrorEventArgs e)
        {
            ErrorOccurred?.Invoke(this, e);
        }
    }
}