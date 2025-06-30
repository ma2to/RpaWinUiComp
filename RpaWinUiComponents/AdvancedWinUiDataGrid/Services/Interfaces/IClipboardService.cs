//Services/Interfaces/IClipboardService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Models;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Events;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Services.Interfaces
{
    public interface IClipboardService
    {
        Task<string?> GetClipboardTextAsync();
        Task SetClipboardTextAsync(string text);
        Task<bool> HasClipboardTextAsync();

        string ConvertToExcelFormat(string[,] data);
        string[,] ParseFromExcelFormat(string clipboardData);

        Task CopySelectedCellsAsync(IEnumerable<DataGridCell> selectedCells);
        Task<bool> PasteToPositionAsync(int startRowIndex, int startColumnIndex, List<DataGridRow> rows, List<ColumnDefinition> columns);

        event System.EventHandler<ComponentErrorEventArgs> ErrorOccurred;
    }
}