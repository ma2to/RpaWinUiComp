//Views/AdvancedDataGridControl.xaml.cs - Kompletne opravený
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Events;
using RpaWinUiComponents.AdvancedWinUiDataGrid.ViewModels;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Configuration;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Commands;

// Alias pre riešenie konfliktu ColumnDefinition
//using DataGridColumnDefinition = RpaWinUiComponents.AdvancedWinUiDataGrid.Models.ColumnDefinition;
using ValidationRule = RpaWinUiComponents.AdvancedWinUiDataGrid.Models.ValidationRule;
using ThrottlingConfig = RpaWinUiComponents.AdvancedWinUiDataGrid.Models.ThrottlingConfig;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Views
{
    /// <summary>
    /// Hlavný UserControl pre AdvancedWinUiDataGrid komponent
    /// </summary>
    public sealed partial class AdvancedDataGridControl : UserControl, IDisposable
    {
        private AdvancedDataGridViewModel? _viewModel;
        private readonly ILogger<AdvancedDataGridControl> _logger;
        private bool _disposed = false;
        private bool _isKeyboardShortcutsVisible = false;

        public AdvancedDataGridControl()
        {
            this.InitializeComponent();

            var loggerProvider = GetLoggerProvider();
            _logger = loggerProvider.CreateLogger<AdvancedDataGridControl>();

            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;

            _logger.LogDebug("AdvancedDataGridControl created");
        }

        #region Properties and Events

        public AdvancedDataGridViewModel? ViewModel
        {
            get => _viewModel;
            private set
            {
                if (_viewModel != null)
                {
                    _viewModel.ErrorOccurred -= OnViewModelError;
                    _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                }

                _viewModel = value;

                if (_viewModel != null)
                {
                    _viewModel.ErrorOccurred += OnViewModelError;
                    _viewModel.PropertyChanged += OnViewModelPropertyChanged;

                    // Sync keyboard shortcuts visibility
                    _isKeyboardShortcutsVisible = _viewModel.IsKeyboardShortcutsVisible;
                    UpdateKeyboardShortcutsVisibility();
                }

                this.DataContext = _viewModel;
            }
        }

        public event EventHandler<ComponentErrorEventArgs>? ErrorOccurred;

        #endregion

        #region Public API Methods

        /// <summary>
        /// Inicializuje komponent s konfiguráciou stĺpcov a validáciami
        /// </summary>
        public async Task InitializeAsync(
    List<DataGridColumnDefinition> columns,  // OPRAVA: Použiť alias
    List<ValidationRule>? validationRules = null,
    ThrottlingConfig? throttling = null,
    int initialRowCount = 100)
        {
            ThrowIfDisposed();

            try
            {
                _logger.LogInformation("Initializing AdvancedDataGrid with {ColumnCount} columns, {InitialRowCount} rows",
                    columns?.Count ?? 0, initialRowCount);

                if (_viewModel == null)
                {
                    _viewModel = CreateViewModel();
                    ViewModel = _viewModel;
                }

                await _viewModel.InitializeAsync(columns, validationRules ?? new List<ValidationRule>(), throttling, initialRowCount);

                _logger.LogInformation("AdvancedDataGrid initialized successfully with {InitialRowCount} rows", initialRowCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing AdvancedDataGrid");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "InitializeAsync"));
                throw;
            }
        }

        /// <summary>
        /// Načíta dáta z DataTable s automatickou validáciou
        /// </summary>
        public async Task LoadDataAsync(DataTable dataTable)
        {
            ThrowIfDisposed();

            try
            {
                if (_viewModel == null)
                    throw new InvalidOperationException("Component must be initialized first! Call InitializeAsync() before LoadDataAsync().");

                if (!_viewModel.IsInitialized)
                    throw new InvalidOperationException("Component not properly initialized! Call InitializeAsync() with validation rules first.");

                _logger.LogInformation("Loading data from DataTable with {RowCount} rows", dataTable?.Rows.Count ?? 0);
                await _viewModel.LoadDataAsync(dataTable);
                _logger.LogInformation("Data loaded successfully with applied validations");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading data from DataTable");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "LoadDataAsync"));
                throw;
            }
        }

        /// <summary>
        /// Načíta dáta zo zoznamu dictionary objektov
        /// </summary>
        public async Task LoadDataAsync(List<Dictionary<string, object?>> data)
        {
            ThrowIfDisposed();

            try
            {
                if (_viewModel == null)
                    throw new InvalidOperationException("Component must be initialized first!");

                var dataTable = ConvertToDataTable(data);
                await _viewModel.LoadDataAsync(dataTable);
                _logger.LogInformation("Data loaded from dictionary list successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading data from dictionary list");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "LoadDataAsync"));
                throw;
            }
        }

        /// <summary>
        /// Exportuje validné dáta do DataTable
        /// </summary>
        public async Task<DataTable> ExportToDataTableAsync()
        {
            ThrowIfDisposed();

            try
            {
                if (_viewModel == null)
                    return new DataTable();

                var result = await _viewModel.ExportDataAsync();
                _logger.LogInformation("Data exported to DataTable with {RowCount} rows", result.Rows.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting data");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "ExportToDataTableAsync"));
                return new DataTable();
            }
        }

        /// <summary>
        /// Validuje všetky riadky a vráti true ak sú všetky validné
        /// </summary>
        public async Task<bool> ValidateAllRowsAsync()
        {
            ThrowIfDisposed();

            try
            {
                if (_viewModel == null)
                    return false;

                var result = await _viewModel.ValidateAllRowsAsync();
                _logger.LogInformation("Validation completed, all valid: {AllValid}", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating all rows");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "ValidateAllRowsAsync"));
                return false;
            }
        }

        /// <summary>
        /// Vymaže všetky dáta zo všetkých buniek
        /// </summary>
        public async Task ClearAllDataAsync()
        {
            ThrowIfDisposed();

            try
            {
                if (_viewModel?.ClearAllDataCommand != null && _viewModel.ClearAllDataCommand.CanExecute(null))
                {
                    if (_viewModel.ClearAllDataCommand is AsyncRelayCommand asyncCommand)
                    {
                        await asyncCommand.ExecuteAsync();
                    }
                    else
                    {
                        _viewModel.ClearAllDataCommand.Execute(null);
                        // For non-async commands, we simulate completion
                        await Task.CompletedTask;
                    }
                    _logger.LogInformation("All data cleared");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing all data");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "ClearAllDataAsync"));
            }
        }

        /// <summary>
        /// Odstráni všetky prázdne riadky
        /// </summary>
        public async Task RemoveEmptyRowsAsync()
        {
            ThrowIfDisposed();

            try
            {
                if (_viewModel?.RemoveEmptyRowsCommand != null && _viewModel.RemoveEmptyRowsCommand.CanExecute(null))
                {
                    if (_viewModel.RemoveEmptyRowsCommand is AsyncRelayCommand asyncCommand)
                    {
                        await asyncCommand.ExecuteAsync();
                    }
                    else
                    {
                        _viewModel.RemoveEmptyRowsCommand.Execute(null);
                        // For non-async commands, we simulate completion
                        await Task.CompletedTask;
                    }
                    _logger.LogInformation("Empty rows removed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing empty rows");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "RemoveEmptyRowsAsync"));
            }
        }

        /// <summary>
        /// Reset komponentu do pôvodného stavu
        /// </summary>
        public void Reset()
        {
            if (_disposed) return;

            try
            {
                _logger.LogInformation("Resetting AdvancedDataGrid");
                _viewModel?.Reset();

                _isKeyboardShortcutsVisible = false;
                UpdateKeyboardShortcutsVisibility();

                _logger.LogInformation("AdvancedDataGrid reset completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting AdvancedDataGrid");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "Reset"));
            }
        }

        #endregion

        #region Event Handlers

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_disposed) return;

            try
            {
                _viewModel ??= CreateViewModel();
                ViewModel = _viewModel;
                SetupEventHandlers();

                UpdateKeyboardShortcutsVisibility();

                _logger.LogDebug("AdvancedDataGrid loaded");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during OnLoaded");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "OnLoaded"));
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_disposed) return;

            try
            {
                UnsubscribeAllEvents();
                _logger.LogDebug("AdvancedDataGrid unloaded");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during OnUnloaded");
            }
        }

        private void OnToggleKeyboardShortcuts_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _logger.LogDebug("Toggle keyboard shortcuts button clicked");

                _isKeyboardShortcutsVisible = !_isKeyboardShortcutsVisible;
                UpdateKeyboardShortcutsVisibility();

                if (_viewModel != null)
                {
                    _viewModel.IsKeyboardShortcutsVisible = _isKeyboardShortcutsVisible;
                }

                _logger.LogInformation("Keyboard shortcuts visibility toggled to: {IsVisible}", _isKeyboardShortcutsVisible);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling keyboard shortcuts");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "OnToggleKeyboardShortcuts_Click"));
            }
        }

        private void OnDeleteRowClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.CommandParameter is RpaWinUiComponents.AdvancedWinUiDataGrid.Models.DataGridRow row)
                {
                    _viewModel?.DeleteRowCommand?.Execute(row);
                    _logger.LogDebug("Delete row button clicked for row: {RowIndex}", row.RowIndex);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling delete row click");
                OnErrorOccurred(new ComponentErrorEventArgs(ex, "OnDeleteRowClick"));
            }
        }

        private void OnCellEditingLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is TextBox textBox && textBox.DataContext is RpaWinUiComponents.AdvancedWinUiDataGrid.Models.DataGridCell cell)
                {
                    cell.IsEditing = false;
                    _logger.LogTrace("Cell editing ended for: {ColumnName}", cell.ColumnName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling cell editing lost focus");
            }
        }

        private void OnCellEditingKeyDown(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                if (sender is TextBox textBox && textBox.DataContext is RpaWinUiComponents.AdvancedWinUiDataGrid.Models.DataGridCell cell)
                {
                    switch (e.Key)
                    {
                        case VirtualKey.Enter:
                            if (!e.KeyStatus.IsMenuKeyDown) // Shift+Enter pre nový riadok
                            {
                                cell.IsEditing = false;
                                _viewModel?.NavigationService.MoveToNextRow();
                                e.Handled = true;
                            }
                            break;
                        case VirtualKey.Escape:
                            cell.CancelEditing();
                            cell.IsEditing = false;
                            e.Handled = true;
                            break;
                        case VirtualKey.Tab:
                            cell.IsEditing = false;
                            if (e.KeyStatus.IsMenuKeyDown) // Shift+Tab
                                _viewModel?.NavigationService.MoveToPreviousCell();
                            else
                                _viewModel?.NavigationService.MoveToNextCell();
                            e.Handled = true;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling cell editing key down");
            }
        }

        private void OnMainControlKeyDown(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                var ctrlPressed = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

                if (ctrlPressed)
                {
                    switch (e.Key)
                    {
                        case VirtualKey.C:
                            if (_viewModel?.CopyCommand?.CanExecute(null) == true)
                            {
                                _viewModel.CopyCommand.Execute(null);
                                e.Handled = true;
                            }
                            break;
                        case VirtualKey.V:
                            if (_viewModel?.PasteCommand?.CanExecute(null) == true)
                            {
                                _viewModel.PasteCommand.Execute(null);
                                e.Handled = true;
                            }
                            break;
                    }
                }
                else
                {
                    switch (e.Key)
                    {
                        case VirtualKey.Tab:
                            if (e.KeyStatus.IsMenuKeyDown) // Shift+Tab
                                _viewModel?.NavigationService.MoveToPreviousCell();
                            else
                                _viewModel?.NavigationService.MoveToNextCell();
                            e.Handled = true;
                            break;
                        case VirtualKey.Enter:
                            _viewModel?.NavigationService.MoveToNextRow();
                            e.Handled = true;
                            break;
                        case VirtualKey.F2:
                            // Start editing current cell
                            var currentCell = _viewModel?.NavigationService.CurrentCell;
                            if (currentCell != null && !currentCell.IsReadOnly)
                            {
                                currentCell.IsEditing = true;
                                e.Handled = true;
                            }
                            break;
                        case VirtualKey.Delete:
                            // Clear current cell
                            var cellToDelete = _viewModel?.NavigationService.CurrentCell;
                            if (cellToDelete != null && !cellToDelete.IsReadOnly)
                            {
                                cellToDelete.Value = null;
                                e.Handled = true;
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling main control key down");
            }
        }

        private void OnViewModelError(object? sender, ComponentErrorEventArgs e)
        {
            OnErrorOccurred(e);
        }

        private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AdvancedDataGridViewModel.IsKeyboardShortcutsVisible))
            {
                // Sync keyboard shortcuts visibility
                _isKeyboardShortcutsVisible = _viewModel?.IsKeyboardShortcutsVisible ?? false;
                UpdateKeyboardShortcutsVisibility();
            }
        }

        #endregion

        #region Private Helper Methods

        private AdvancedDataGridViewModel CreateViewModel()
        {
            try
            {
                return DependencyInjectionConfig.GetService<AdvancedDataGridViewModel>()
                       ?? DependencyInjectionConfig.CreateViewModelWithoutDI();
            }
            catch
            {
                return DependencyInjectionConfig.CreateViewModelWithoutDI();
            }
        }

        private IDataGridLoggerProvider GetLoggerProvider()
        {
            try
            {
                return DependencyInjectionConfig.GetService<IDataGridLoggerProvider>()
                       ?? NullDataGridLoggerProvider.Instance;
            }
            catch
            {
                return NullDataGridLoggerProvider.Instance;
            }
        }

        private void SetupEventHandlers()
        {
            try
            {
                this.KeyDown += OnMainControlKeyDown;
                _logger.LogDebug("Event handlers set up");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up event handlers");
            }
        }

        private void UpdateKeyboardShortcutsVisibility()
        {
            try
            {
                // Ensure we're loaded before accessing named elements
                if (!this.IsLoaded)
                {
                    // Subscribe only once to avoid multiple subscriptions
                    this.Loaded -= OnDelayedUpdate;
                    this.Loaded += OnDelayedUpdate;
                    return;
                }

                // Use dispatcher to ensure UI thread access
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    try
                    {
                        // Check if elements exist before accessing them
                        var keyboardPanel = this.FindName("KeyboardShortcutsPanel") as Border;
                        if (keyboardPanel != null)
                        {
                            keyboardPanel.Visibility = _isKeyboardShortcutsVisible ? Visibility.Visible : Visibility.Collapsed;
                        }

                        var toggleIcon = this.FindName("ToggleIcon") as TextBlock;
                        if (toggleIcon != null)
                        {
                            toggleIcon.Text = _isKeyboardShortcutsVisible ? "▲" : "▼";
                        }

                        var toggleButton = this.FindName("ToggleKeyboardShortcutsButton") as Button;
                        if (toggleButton != null)
                        {
                            var backgroundColor = _isKeyboardShortcutsVisible
                                ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.DodgerBlue)
                                : new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.LightGray);

                            var foregroundColor = _isKeyboardShortcutsVisible
                                ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White)
                                : new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Black);

                            toggleButton.Background = backgroundColor;
                            toggleButton.Foreground = foregroundColor;
                        }

                        _logger.LogDebug("Keyboard shortcuts visibility updated: {IsVisible}", _isKeyboardShortcutsVisible);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in UI thread during keyboard shortcuts update");
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating keyboard shortcuts visibility");
            }
        }

        private void OnDelayedUpdate(object sender, RoutedEventArgs e)
        {
            this.Loaded -= OnDelayedUpdate;
            UpdateKeyboardShortcutsVisibility();
        }

        private void UnsubscribeAllEvents()
        {
            try
            {
                this.KeyDown -= OnMainControlKeyDown;
                this.Loaded -= OnLoaded;
                this.Loaded -= OnDelayedUpdate;
                this.Unloaded -= OnUnloaded;

                _logger.LogDebug("All events unsubscribed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsubscribing events");
            }
        }

        private DataTable ConvertToDataTable(List<Dictionary<string, object?>> data)
        {
            var dataTable = new DataTable();

            if (data?.Count > 0)
            {
                // Create columns from first dictionary
                foreach (var key in data[0].Keys)
                {
                    dataTable.Columns.Add(key, typeof(object));
                }

                // Add rows
                foreach (var row in data)
                {
                    var dataRow = dataTable.NewRow();
                    foreach (var kvp in row)
                    {
                        dataRow[kvp.Key] = kvp.Value ?? DBNull.Value;
                    }
                    dataTable.Rows.Add(dataRow);
                }
            }

            return dataTable;
        }

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                _logger?.LogDebug("Disposing AdvancedDataGridControl...");

                UnsubscribeAllEvents();

                if (_viewModel != null)
                {
                    _viewModel.ErrorOccurred -= OnViewModelError;
                    _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                    _viewModel.Dispose();
                    _viewModel = null;
                }

                this.DataContext = null;

                _disposed = true;
                _logger?.LogInformation("AdvancedDataGridControl disposed successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during disposal");
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AdvancedDataGridControl));
        }

        #endregion

        #region Error Handling

        protected void OnErrorOccurred(ComponentErrorEventArgs e)
        {
            ErrorOccurred?.Invoke(this, e);
        }

        #endregion
    }
}