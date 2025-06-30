//Services/Interfaces/IExportService.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Models;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Events;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Services.Interfaces
{
    public interface IExportService
    {
        Task<DataTable> ExportToDataTableAsync(List<DataGridRow> rows, List<ColumnDefinition> columns, bool includeValidAlerts = false);
        Task<string> ExportToCsvAsync(List<DataGridRow> rows, List<ColumnDefinition> columns, bool includeValidAlerts = false);
        Task<byte[]> ExportToExcelAsync(List<DataGridRow> rows, List<ColumnDefinition> columns, bool includeValidAlerts = false);

        Task<List<Dictionary<string, object?>>> ExportToDictionariesAsync(List<DataGridRow> rows, List<ColumnDefinition> columns);

        event EventHandler<ComponentErrorEventArgs> ErrorOccurred;
    }
}