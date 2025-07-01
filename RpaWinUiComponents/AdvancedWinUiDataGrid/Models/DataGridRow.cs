//Models/DataGridRow.cs - Opravený
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Models
{
    /// <summary>
    /// Model pre jeden riadok v DataGrid
    /// </summary>
    public class DataGridRow : INotifyPropertyChanged
    {
        private Dictionary<string, DataGridCell> _cells = new();
        private bool _hasValidationErrors;
        private bool _isEmpty = true;
        private bool _isSelected;
        private bool _isEvenRow;

        public int RowIndex { get; init; }
        public string RowId { get; init; } = Guid.NewGuid().ToString();

        public Dictionary<string, DataGridCell> Cells
        {
            get => _cells;
            set => SetProperty(ref _cells, value ?? new Dictionary<string, DataGridCell>());
        }

        public bool HasValidationErrors
        {
            get => _hasValidationErrors;
            set => SetProperty(ref _hasValidationErrors, value);
        }

        public bool IsEmpty
        {
            get => _isEmpty;
            set => SetProperty(ref _isEmpty, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool IsEvenRow
        {
            get => _isEvenRow;
            set => SetProperty(ref _isEvenRow, value);
        }

        /// <summary>
        /// Textová reprezentácia všetkých validačných chýb v riadku
        /// </summary>
        public string ValidationErrorsText
        {
            get
            {
                var errors = new List<string>();
                foreach (var cell in Cells.Values.Where(c => c.HasValidationError && !IsSpecialColumn(c.ColumnName)))
                {
                    errors.Add($"{cell.ColumnName}: {cell.ValidationErrorsText}");
                }
                return string.Join("; ", errors);
            }
        }

        /// <summary>
        /// Počet validačných chýb v riadku
        /// </summary>
        public int ValidationErrorCount => Cells.Values.Count(c => c.HasValidationError && !IsSpecialColumn(c.ColumnName));

        public DataGridRow()
        {
        }

        public DataGridRow(int rowIndex)
        {
            RowIndex = rowIndex;
        }

        /// <summary>
        /// Získa bunku podľa názvu stĺpca
        /// </summary>
        public DataGridCell? GetCell(string columnName)
        {
            return Cells.TryGetValue(columnName, out var cell) ? cell : null;
        }

        /// <summary>
        /// Získa typovú hodnotu z bunky podľa názvu stĺpca
        /// </summary>
        public T GetValue<T>(string columnName)
        {
            var cell = GetCell(columnName);
            if (cell == null)
                return default(T)!;

            return cell.GetValue<T>();
        }

        /// <summary>
        /// Nastaví hodnotu bunky
        /// </summary>
        public void SetValue(string columnName, object? value)
        {
            if (Cells.TryGetValue(columnName, out var cell))
            {
                cell.Value = value;
                UpdateEmptyStatus();
            }
        }

        /// <summary>
        /// Pridá bunku do riadku
        /// </summary>
        public void AddCell(string columnName, DataGridCell cell)
        {
            Cells[columnName] = cell;

            // Subscribe na zmeny v bunke
            cell.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(DataGridCell.Value))
                {
                    UpdateEmptyStatus();
                }
                else if (e.PropertyName == nameof(DataGridCell.HasValidationError))
                {
                    UpdateValidationStatus();
                }
            };

            UpdateEmptyStatus();
            UpdateValidationStatus();
        }

        /// <summary>
        /// Odstráni bunku z riadku
        /// </summary>
        public void RemoveCell(string columnName)
        {
            if (Cells.Remove(columnName))
            {
                UpdateEmptyStatus();
                UpdateValidationStatus();
            }
        }

        /// <summary>
        /// Vymaže všetky hodnoty v riadku (okrem špeciálnych stĺpcov)
        /// </summary>
        public void ClearValues()
        {
            foreach (var cell in Cells.Values.Where(c => !IsSpecialColumn(c.ColumnName)))
            {
                cell.Value = null;
                cell.ClearValidationErrors();
            }
            UpdateEmptyStatus();
            UpdateValidationStatus();
        }

        /// <summary>
        /// Aktualizuje stav prázdnosti riadku
        /// </summary>
        public void UpdateEmptyStatus()
        {
            try
            {
                var dataCells = Cells.Values.Where(c => !IsSpecialColumn(c.ColumnName));

                IsEmpty = dataCells.All(c =>
                    c.Value == null ||
                    string.IsNullOrWhiteSpace(c.Value?.ToString())
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ UpdateEmptyStatus error: {ex.Message}");
                IsEmpty = false;
            }
        }

        /// <summary>
        /// Aktualizuje stav validačných chýb v riadku
        /// </summary>
        public void UpdateValidationStatus()
        {
            HasValidationErrors = Cells.Values.Any(c => c.HasValidationError && !IsSpecialColumn(c.ColumnName));
            OnPropertyChanged(nameof(ValidationErrorsText));
            OnPropertyChanged(nameof(ValidationErrorCount));

            // Aktualizuj ValidAlerts stĺpec ak existuje
            var validAlertsCell = GetCell("ValidAlerts");
            if (validAlertsCell != null)
            {
                validAlertsCell.SetValueWithoutValidation(ValidationErrorsText);
            }
        }

        /// <summary>
        /// Začne editáciu konkrétnej bunky
        /// </summary>
        public void StartCellEditing(string columnName)
        {
            var cell = GetCell(columnName);
            if (cell != null && !cell.IsReadOnly)
            {
                // Ukončí editáciu ostatných buniek v riadku
                foreach (var otherCell in Cells.Values.Where(c => c != cell))
                {
                    if (otherCell.IsEditing)
                    {
                        otherCell.CommitChanges();
                    }
                }

                cell.StartEditing();
            }
        }

        /// <summary>
        /// Ukončí editáciu všetkých buniek v riadku
        /// </summary>
        public void CommitAllChanges()
        {
            foreach (var cell in Cells.Values.Where(c => c.IsEditing))
            {
                cell.CommitChanges();
            }
        }

        /// <summary>
        /// Zruší editáciu všetkých buniek v riadku
        /// </summary>
        public void CancelAllEditing()
        {
            foreach (var cell in Cells.Values.Where(c => c.IsEditing))
            {
                cell.CancelEditing();
            }
        }

        /// <summary>
        /// Kontroluje či je stĺpec špeciálny
        /// </summary>
        private static bool IsSpecialColumn(string columnName)
        {
            return columnName == "DeleteAction" || columnName == "ValidAlerts";
        }

        /// <summary>
        /// Exportuje riadok ako dictionary (bez špeciálnych stĺpcov)
        /// </summary>
        public Dictionary<string, object?> ExportToDictionary()
        {
            var result = new Dictionary<string, object?>();

            foreach (var cell in Cells.Values.Where(c => !IsSpecialColumn(c.ColumnName)))
            {
                result[cell.ColumnName] = cell.Value;
            }

            return result;
        }

        /// <summary>
        /// Importuje dáta z dictionary
        /// </summary>
        public void ImportFromDictionary(Dictionary<string, object?> data)
        {
            foreach (var kvp in data)
            {
                SetValue(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// Klonuje riadok
        /// </summary>
        public DataGridRow Clone()
        {
            var newRow = new DataGridRow(RowIndex)
            {
                IsEvenRow = IsEvenRow
            };

            foreach (var cell in Cells.Values)
            {
                var newCell = new DataGridCell(cell.ColumnName, cell.DataType, cell.RowIndex, cell.ColumnIndex)
                {
                    Value = cell.Value,
                    OriginalValue = cell.OriginalValue,
                    IsReadOnly = cell.IsReadOnly
                };
                newCell.SetValidationErrors(cell.ValidationErrors);

                newRow.AddCell(cell.ColumnName, newCell);
            }

            return newRow;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            var cellCount = Cells.Count;
            var errorCount = ValidationErrorCount;
            return $"Row {RowIndex}: {cellCount} cells, {errorCount} errors, Empty: {IsEmpty}";
        }
    }
}