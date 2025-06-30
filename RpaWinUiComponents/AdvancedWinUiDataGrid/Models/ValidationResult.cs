//Models/ValidationResult.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Models
{
    /// <summary>
    /// Výsledok validácie bunky alebo riadku
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; init; }
        public List<string> ErrorMessages { get; init; } = new();
        public string ColumnName { get; init; } = string.Empty;
        public int RowIndex { get; init; }
        public TimeSpan ValidationDuration { get; init; }
        public bool WasAsync { get; init; }

        public ValidationResult(bool isValid = true)
        {
            IsValid = isValid;
        }

        public static ValidationResult Success(string columnName = "", int rowIndex = -1)
        {
            return new ValidationResult(true)
            {
                ColumnName = columnName,
                RowIndex = rowIndex
            };
        }

        public static ValidationResult Failure(string errorMessage, string columnName = "", int rowIndex = -1)
        {
            return new ValidationResult(false)
            {
                ErrorMessages = new List<string> { errorMessage ?? string.Empty },
                ColumnName = columnName,
                RowIndex = rowIndex
            };
        }

        public static ValidationResult Failure(IEnumerable<string> errorMessages, string columnName = "", int rowIndex = -1)
        {
            return new ValidationResult(false)
            {
                ErrorMessages = errorMessages?.ToList() ?? new List<string>(),
                ColumnName = columnName,
                RowIndex = rowIndex
            };
        }

        public override string ToString()
        {
            if (IsValid)
                return $"✅ Valid - {ColumnName}[{RowIndex}]";

            return $"❌ Invalid - {ColumnName}[{RowIndex}]: {string.Join(", ", ErrorMessages)}";
        }
    }
}