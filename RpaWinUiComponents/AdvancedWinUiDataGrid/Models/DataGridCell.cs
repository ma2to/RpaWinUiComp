//Models/DataGridCell.cs - Opravený
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Models
{
    /// <summary>
    /// Model pre jednu bunku v DataGrid
    /// </summary>
    public class DataGridCell : INotifyPropertyChanged
    {
        private object? _value;
        private object? _originalValue;
        private bool _hasValidationError;
        private List<string> _validationErrors = new();
        private bool _isSelected;
        private bool _isEditing;
        private bool _hasFocus;
        private bool _hasUnsavedChanges;
        private bool _isReadOnly;

        public string ColumnName { get; init; } = string.Empty;
        public Type DataType { get; init; } = typeof(string);
        public int RowIndex { get; init; }
        public int ColumnIndex { get; init; }

        public object? Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public object? OriginalValue
        {
            get => _originalValue;
            set => SetProperty(ref _originalValue, value);
        }

        public bool HasValidationError
        {
            get => _hasValidationError;
            set => SetProperty(ref _hasValidationError, value);
        }

        public List<string> ValidationErrors
        {
            get => _validationErrors;
            set => SetProperty(ref _validationErrors, value ?? new List<string>());
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public bool HasFocus
        {
            get => _hasFocus;
            set => SetProperty(ref _hasFocus, value);
        }

        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            set => SetProperty(ref _hasUnsavedChanges, value);
        }

        public bool IsReadOnly
        {
            get => _isReadOnly;
            set => SetProperty(ref _isReadOnly, value);
        }

        /// <summary>
        /// Textová reprezentácia validačných chýb
        /// </summary>
        public string ValidationErrorsText => string.Join("; ", ValidationErrors);

        /// <summary>
        /// Či má bunka nejaké validačné chyby
        /// </summary>
        public bool HasValidationErrors => ValidationErrors.Count > 0;

        public DataGridCell()
        {
        }

        public DataGridCell(string columnName, Type dataType, int rowIndex, int columnIndex)
        {
            ColumnName = columnName;
            DataType = dataType;
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
        }

        /// <summary>
        /// Začne editáciu bunky
        /// </summary>
        public void StartEditing()
        {
            if (IsReadOnly) return;

            OriginalValue = Value;
            IsEditing = true;
            HasUnsavedChanges = false;
        }

        /// <summary>
        /// Ukončí editáciu a uloží zmeny
        /// </summary>
        public void CommitChanges()
        {
            OriginalValue = Value;
            IsEditing = false;
            HasUnsavedChanges = false;
        }

        /// <summary>
        /// Zruší editáciu a obnoví pôvodnú hodnotu
        /// </summary>
        public void CancelEditing()
        {
            if (HasUnsavedChanges && OriginalValue != Value)
            {
                Value = OriginalValue;
                ClearValidationErrors();
            }

            IsEditing = false;
            HasUnsavedChanges = false;
        }

        /// <summary>
        /// Nastaví hodnotu bez spustenia validácie
        /// </summary>
        public void SetValueWithoutValidation(object? value)
        {
            _value = value;
            _originalValue = value;
            _hasUnsavedChanges = false;
            OnPropertyChanged(nameof(Value));
        }

        /// <summary>
        /// Získa typovú hodnotu bunky
        /// </summary>
        public T GetValue<T>()
        {
            try
            {
                if (Value == null)
                    return default(T)!;

                if (Value is T directValue)
                    return directValue;

                return (T)Convert.ChangeType(Value, typeof(T))!;
            }
            catch
            {
                return default(T)!;
            }
        }

        /// <summary>
        /// Nastaví validačné chyby
        /// </summary>
        public void SetValidationErrors(IEnumerable<string> errors)
        {
            ValidationErrors = errors?.ToList() ?? new List<string>();
            HasValidationError = ValidationErrors.Count > 0;
            OnPropertyChanged(nameof(ValidationErrorsText));
            OnPropertyChanged(nameof(HasValidationErrors));
        }

        /// <summary>
        /// Vymaže všetky validačné chyby
        /// </summary>
        public void ClearValidationErrors()
        {
            SetValidationErrors(Enumerable.Empty<string>());
        }

        /// <summary>
        /// Pridá validačnú chybu
        /// </summary>
        public void AddValidationError(string error)
        {
            if (!string.IsNullOrWhiteSpace(error) && !ValidationErrors.Contains(error))
            {
                var errors = new List<string>(ValidationErrors) { error };
                SetValidationErrors(errors);
            }
        }

        private void OnValueChanged()
        {
            if (IsEditing)
            {
                HasUnsavedChanges = !AreValuesEqual(Value, OriginalValue);
            }
        }

        private bool AreValuesEqual(object? value1, object? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Pre string porovnanie s trim
            if (value1 is string str1 && value2 is string str2)
            {
                return string.Equals(str1?.Trim(), str2?.Trim(), StringComparison.Ordinal);
            }

            return value1.Equals(value2);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);

            if (propertyName == nameof(Value))
            {
                OnValueChanged();
            }

            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return $"[{RowIndex},{ColumnIndex}] {ColumnName}: {Value} (Errors: {ValidationErrors.Count})";
        }
    }
}