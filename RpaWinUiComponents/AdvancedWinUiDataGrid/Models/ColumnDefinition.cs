//Models/ColumnDefinition.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Models
{
    /// <summary>
    /// Definícia stĺpca pre AdvancedWinUiDataGrid
    /// </summary>
    public class ColumnDefinition : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private Type _dataType = typeof(string);
        private double _minWidth = 80;
        private double _maxWidth = 300;
        private double _width = 150;
        private bool _allowResize = true;
        private bool _allowSort = true;
        private bool _isReadOnly = false;
        private string? _header;
        private string? _toolTip;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value ?? string.Empty);
        }

        public Type DataType
        {
            get => _dataType;
            set => SetProperty(ref _dataType, value);
        }

        public double MinWidth
        {
            get => _minWidth;
            set => SetProperty(ref _minWidth, Math.Max(20, Math.Min(value, 2000)));
        }

        public double MaxWidth
        {
            get => _maxWidth;
            set => SetProperty(ref _maxWidth, Math.Max(50, Math.Min(value, 5000)));
        }

        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, Math.Max(MinWidth, Math.Min(value, MaxWidth)));
        }

        public bool AllowResize
        {
            get => _allowResize;
            set => SetProperty(ref _allowResize, value);
        }

        public bool AllowSort
        {
            get => _allowSort;
            set => SetProperty(ref _allowSort, value);
        }

        public bool IsReadOnly
        {
            get => _isReadOnly;
            set => SetProperty(ref _isReadOnly, value);
        }

        public string? Header
        {
            get => _header;
            set => SetProperty(ref _header, value);
        }

        public string? ToolTip
        {
            get => _toolTip;
            set => SetProperty(ref _toolTip, value);
        }

        /// <summary>
        /// Či je stĺpec špeciálny (DeleteAction, ValidAlerts)
        /// </summary>
        public bool IsSpecialColumn => Name == "DeleteAction" || Name == "ValidAlerts";

        /// <summary>
        /// Poradie stĺpca (špeciálne stĺpce majú vyššie číslo)
        /// </summary>
        public int SortOrder => IsSpecialColumn ? (Name == "ValidAlerts" ? 1000 : 999) : 0;

        public ColumnDefinition()
        {
        }

        public ColumnDefinition(string name, Type dataType)
        {
            Name = name;
            DataType = dataType;
            Header = name;
        }

        public ColumnDefinition(string name, Type dataType, double minWidth, double maxWidth)
            : this(name, dataType)
        {
            MinWidth = minWidth;
            MaxWidth = maxWidth;
            Width = Math.Min(maxWidth, Math.Max(minWidth, 150));
        }

        /// <summary>
        /// Vytvorí kópiu definície stĺpca
        /// </summary>
        public ColumnDefinition Clone()
        {
            return new ColumnDefinition
            {
                Name = Name,
                DataType = DataType,
                MinWidth = MinWidth,
                MaxWidth = MaxWidth,
                Width = Width,
                AllowResize = AllowResize,
                AllowSort = AllowSort,
                IsReadOnly = IsReadOnly,
                Header = Header,
                ToolTip = ToolTip
            };
        }

        /// <summary>
        /// Validuje definíciu stĺpca
        /// </summary>
        public bool IsValid(out string? errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(Name))
            {
                errorMessage = "Názov stĺpca je povinný";
                return false;
            }

            if (MinWidth <= 0 || MaxWidth <= 0)
            {
                errorMessage = "Šírka stĺpcov musí byť kladná";
                return false;
            }

            if (MinWidth > MaxWidth)
            {
                errorMessage = "Minimálna šírka nemôže byť väčšia ako maximálna";
                return false;
            }

            return true;
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
            return $"{Name} ({DataType.Name}) [{MinWidth}-{MaxWidth}]";
        }
    }
}