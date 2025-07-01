// AdvancedWinUiDataGrid/AdvancedWinUiDataGridControl.cs - KOMPLETNÝ OPRAVENÝ
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Events;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Models;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Views;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ThrottlingConfig = RpaWinUiComponents.AdvancedWinUiDataGrid.Models.ThrottlingConfig;
using ValidationRule = RpaWinUiComponents.AdvancedWinUiDataGrid.Models.ValidationRule;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid
{
    /// <summary>
    /// Hlavný wrapper komponent pre AdvancedWinUiDataGrid s konfigurovateľnou validáciou
    /// KOMPLETNE OPRAVENÁ VERZIA - definuje všetky metódy ktoré sa používajú v MainWindow
    /// </summary>
    public class AdvancedWinUiDataGridControl : UserControl, IDisposable
    {
        private readonly AdvancedDataGridControl _internalView;
        private bool _disposed = false;
        private bool _isInitialized = false;

        public AdvancedWinUiDataGridControl()
        {
            _internalView = new AdvancedDataGridControl();
            Content = _internalView;

            // Pripojenie error eventov
            _internalView.ErrorOccurred += OnInternalError;
        }

        #region Events

        /// <summary>
        /// Event ktorý sa spustí pri chybe v komponente
        /// </summary>
        public event EventHandler<ComponentErrorEventArgs>? ErrorOccurred;

        #endregion

        #region Static Configuration Methods

        /// <summary>
        /// Konfiguruje dependency injection pre AdvancedWinUiDataGrid
        /// </summary>
        public static class Configuration
        {
            /// <summary>
            /// Konfiguruje služby pre AdvancedWinUiDataGrid
            /// </summary>
            public static void ConfigureServices(IServiceProvider serviceProvider)
            {
                RpaWinUiComponents.AdvancedWinUiDataGrid.Configuration.DependencyInjectionConfig.ConfigureServices(serviceProvider);
            }

            /// <summary>
            /// Konfiguruje logging pre AdvancedWinUiDataGrid
            /// </summary>
            public static void ConfigureLogging(ILoggerFactory loggerFactory)
            {
                RpaWinUiComponents.AdvancedWinUiDataGrid.Configuration.LoggerFactory.Configure(loggerFactory);
            }

            /// <summary>
            /// Zapne/vypne debug logging
            /// </summary>
            public static void SetDebugLogging(bool enabled)
            {
                RpaWinUiComponents.AdvancedWinUiDataGrid.Helpers.DebugHelper.IsDebugEnabled = enabled;
            }
        }

        #endregion

        #region Inicializácia a Konfigurácia

        /// <summary>
        /// Inicializuje komponent s konfiguráciou stĺpcov a validáciami
        /// </summary>
        /// <param name="columns">Definície stĺpcov</param>
        /// <param name="validationRules">Validačné pravidlá (voliteľné)</param>
        /// <param name="throttling">Throttling konfigurácia (voliteľné)</param>
        /// <param name="initialRowCount">Počiatočný počet riadkov</param>
        public async Task InitializeAsync(
            List<DataGridColumnDefinition> columns,
            List<ValidationRule>? validationRules = null,
            ThrottlingConfig? throttling = null,
            int initialRowCount = 100)
        {
            try
            {
                await _internalView.InitializeAsync(columns, validationRules, throttling, initialRowCount);
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "InitializeAsync"));
                throw;
            }
        }

        /// <summary>
        /// Resetuje komponent do pôvodného stavu
        /// </summary>
        public void Reset()
        {
            try
            {
                _internalView.Reset();
                _isInitialized = false;
            }
            catch (Exception ex)
            {
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "Reset"));
            }
        }

        #endregion

        #region Načítanie Dát

        /// <summary>
        /// Načíta dáta z DataTable s automatickou validáciou
        /// </summary>
        public async Task LoadDataAsync(DataTable dataTable)
        {
            try
            {
                if (!_isInitialized)
                    throw new InvalidOperationException("Component must be initialized first");

                await _internalView.LoadDataAsync(dataTable);
            }
            catch (Exception ex)
            {
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "LoadDataAsync"));
                throw;
            }
        }

        /// <summary>
        /// Načíta dáta zo zoznamu dictionary objektov
        /// </summary>
        public async Task LoadDataAsync(List<Dictionary<string, object?>> data)
        {
            try
            {
                if (!_isInitialized)
                    throw new InvalidOperationException("Component must be initialized first");

                await _internalView.LoadDataAsync(data);
            }
            catch (Exception ex)
            {
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "LoadDataAsync"));
                throw;
            }
        }

        #endregion

        #region Export Dát

        /// <summary>
        /// Exportuje validné dáta do DataTable
        /// </summary>
        public async Task<DataTable> ExportToDataTableAsync()
        {
            try
            {
                if (!_isInitialized)
                    return new DataTable();

                return await _internalView.ExportToDataTableAsync();
            }
            catch (Exception ex)
            {
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "ExportToDataTableAsync"));
                return new DataTable();
            }
        }

        #endregion

        #region Validácia

        /// <summary>
        /// Validuje všetky riadky a vráti true ak sú všetky validné
        /// </summary>
        public async Task<bool> ValidateAllRowsAsync()
        {
            try
            {
                if (!_isInitialized)
                    return false;

                return await _internalView.ValidateAllRowsAsync();
            }
            catch (Exception ex)
            {
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "ValidateAllRowsAsync"));
                return false;
            }
        }

        #endregion

        #region Manipulácia s Riadkami

        /// <summary>
        /// Vymaže všetky dáta zo všetkých buniek
        /// </summary>
        public async Task ClearAllDataAsync()
        {
            try
            {
                if (!_isInitialized)
                    return;

                await _internalView.ClearAllDataAsync();
            }
            catch (Exception ex)
            {
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "ClearAllDataAsync"));
                throw;
            }
        }

        /// <summary>
        /// Odstráni všetky prázdne riadky
        /// </summary>
        public async Task RemoveEmptyRowsAsync()
        {
            try
            {
                if (!_isInitialized)
                    return;

                await _internalView.RemoveEmptyRowsAsync();
            }
            catch (Exception ex)
            {
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "RemoveEmptyRowsAsync"));
                throw;
            }
        }

        /// <summary>
        /// Odstráni riadky ktoré spĺňajú zadanú podmienku
        /// </summary>
        public async Task RemoveRowsByConditionAsync(string columnName, Func<object?, bool> condition)
        {
            try
            {
                if (!_isInitialized)
                    return;

                if (_internalView.ViewModel != null)
                {
                    await _internalView.ViewModel.RemoveRowsByConditionAsync(columnName, condition);
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "RemoveRowsByConditionAsync"));
                throw;
            }
        }

        /// <summary>
        /// Odstráni riadky ktoré nevyhovujú vlastným validačným pravidlám
        /// </summary>
        public async Task<int> RemoveRowsByValidationAsync(List<ValidationRule> customRules)
        {
            try
            {
                if (!_isInitialized)
                    return 0;

                if (_internalView.ViewModel != null)
                {
                    return await _internalView.ViewModel.RemoveRowsByValidationAsync(customRules);
                }
                return 0;
            }
            catch (Exception ex)
            {
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "RemoveRowsByValidationAsync"));
                return 0;
            }
        }

        #endregion

        #region Public API Models

        /// <summary>
        /// Wrapper pre prístup k riadku v validačných funkciách
        /// </summary>
        public class GridDataRow
        {
            private readonly DataGridRow _internal;

            internal GridDataRow(DataGridRow internalModel)
            {
                _internal = internalModel;
            }

            /// <summary>
            /// Získa hodnotu z bunky podľa názvu stĺpca
            /// </summary>
            public object? GetValue(string columnName) => _internal.GetCell(columnName)?.Value;

            /// <summary>
            /// Získa typovú hodnotu z bunky podľa názvu stĺpca
            /// </summary>
            public T? GetValue<T>(string columnName) => _internal.GetValue<T>(columnName);
        }

        /// <summary>
        /// Informácie o chybe v komponente
        /// </summary>
        public class ComponentError : EventArgs
        {
            public Exception Exception { get; set; }
            public string Operation { get; set; }
            public string AdditionalInfo { get; set; }
            public DateTime Timestamp { get; set; } = DateTime.Now;

            public ComponentError(Exception exception, string operation, string additionalInfo = "")
            {
                Exception = exception;
                Operation = operation;
                AdditionalInfo = additionalInfo;
            }

            public override string ToString()
            {
                return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] {Operation}: {Exception.Message}" +
                       (string.IsNullOrEmpty(AdditionalInfo) ? "" : $" - {AdditionalInfo}");
            }
        }

        #endregion

        #region Static Validation Helpers

        /// <summary>
        /// Pomocné metódy pre tvorbu validačných pravidiel
        /// </summary>
        public static class Validation
        {
            /// <summary>
            /// Vytvorí pravidlo pre povinné pole
            /// </summary>
            public static ValidationRule Required(string columnName, string? errorMessage = null)
            {
                return new ValidationRule(
                    columnName,
                    (value, row) => !string.IsNullOrWhiteSpace(value?.ToString()),
                    errorMessage ?? $"{columnName} je povinné pole"
                )
                {
                    RuleName = $"{columnName}_Required"
                };
            }

            /// <summary>
            /// Vytvorí pravidlo pre kontrolu dĺžky textu
            /// </summary>
            public static ValidationRule Length(string columnName, int minLength, int maxLength = int.MaxValue, string? errorMessage = null)
            {
                return new ValidationRule(
                    columnName,
                    (value, row) =>
                    {
                        var text = value?.ToString() ?? "";
                        return text.Length >= minLength && text.Length <= maxLength;
                    },
                    errorMessage ?? $"{columnName} musí mať dĺžku medzi {minLength} a {maxLength} znakmi"
                )
                {
                    RuleName = $"{columnName}_Length"
                };
            }

            /// <summary>
            /// Vytvorí pravidlo pre kontrolu číselného rozsahu
            /// </summary>
            public static ValidationRule Range(string columnName, double min, double max, string? errorMessage = null)
            {
                return new ValidationRule(
                    columnName,
                    (value, row) =>
                    {
                        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                            return true;

                        if (double.TryParse(value.ToString(), out double numValue))
                        {
                            return numValue >= min && numValue <= max;
                        }

                        return false;
                    },
                    errorMessage ?? $"{columnName} musí byť medzi {min} a {max}"
                )
                {
                    RuleName = $"{columnName}_Range"
                };
            }

            /// <summary>
            /// Vytvorí podmienené validačné pravidlo
            /// </summary>
            public static ValidationRule Conditional(string columnName,
                Func<object?, GridDataRow, bool> validationFunction,
                Func<GridDataRow, bool> condition,
                string errorMessage,
                string? ruleName = null)
            {
                return new ValidationRule(
                    columnName,
                    (value, row) => validationFunction(value, new GridDataRow(row)),
                    errorMessage
                )
                {
                    ApplyCondition = row => condition(new GridDataRow(row)),
                    RuleName = ruleName ?? $"{columnName}_Conditional_{Guid.NewGuid().ToString("N")[..8]}"
                };
            }

            /// <summary>
            /// Vytvorí pravidlo pre validáciu číselných hodnôt
            /// </summary>
            public static ValidationRule Numeric(string columnName, string? errorMessage = null)
            {
                return new ValidationRule(
                    columnName,
                    (value, row) =>
                    {
                        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                            return true;

                        return double.TryParse(value.ToString(), out _);
                    },
                    errorMessage ?? $"{columnName} musí byť číslo"
                )
                {
                    RuleName = $"{columnName}_Numeric"
                };
            }

            /// <summary>
            /// Vytvorí pravidlo pre validáciu emailu
            /// </summary>
            public static ValidationRule Email(string columnName, string? errorMessage = null)
            {
                return new ValidationRule(
                    columnName,
                    (value, row) =>
                    {
                        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                            return true;

                        var email = value.ToString();
                        return email?.Contains("@") == true && email.Contains(".") && email.Length > 5;
                    },
                    errorMessage ?? $"{columnName} musí mať platný formát emailu"
                )
                {
                    RuleName = $"{columnName}_Email"
                };
            }
        }

        #endregion

        #region Private Event Handlers

        private void OnInternalError(object? sender, ComponentErrorEventArgs e)
        {
            OnErrorOccurred(e);
        }

        private void OnErrorOccurred(ComponentErrorEventArgs error)
        {
            ErrorOccurred?.Invoke(this, error);
        }

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                // Unsubscribe od eventov
                if (_internalView != null)
                {
                    _internalView.ErrorOccurred -= OnInternalError;
                    _internalView.Dispose();
                }

                _disposed = true;
            }
            catch (Exception ex)
            {
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "Dispose"));
            }
        }

        #endregion
    }
}