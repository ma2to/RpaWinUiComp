//Services/Interfaces/IColumnService.cs - OPRAVENÝ
using System;
using System.Collections.Generic;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Events;
// ALIAS pre riešenie konfliktu
//using DataGridColumnDefinition = RpaWinUiComponents.AdvancedWinUiDataGrid.Models.ColumnDefinition;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Services.Interfaces
{
    public interface IColumnService
    {
        List<DataGridColumnDefinition> ProcessColumnDefinitions(List<DataGridColumnDefinition> columns);
        string GenerateUniqueColumnName(string baseName, List<string> existingNames);

        DataGridColumnDefinition CreateDeleteActionColumn();
        DataGridColumnDefinition CreateValidAlertsColumn();
        bool IsSpecialColumn(string columnName);

        List<DataGridColumnDefinition> ReorderSpecialColumns(List<DataGridColumnDefinition> columns);
        void ValidateColumnDefinitions(List<DataGridColumnDefinition> columns);

        event EventHandler<ComponentErrorEventArgs> ErrorOccurred;
    }
}