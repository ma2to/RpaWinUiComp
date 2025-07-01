using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using DataGridColumnDefinition = RpaWinUiComponents.AdvancedWinUiDataGrid.Models.ColumnDefinition;
using ValidationRule = RpaWinUiComponents.AdvancedWinUiDataGrid.Models.ValidationRule;
using ThrottlingConfig = RpaWinUiComponents.AdvancedWinUiDataGrid.Models.ThrottlingConfig;

namespace RpaWinUiComponents.Demo
{
    public sealed partial class MainWindow : Window
    {
        private readonly ILogger<MainWindow> _logger;
        private readonly IServiceProvider _serviceProvider;
        private bool _isInitialized = false;
        private RpaWinUiComponents.AdvancedWinUiDataGrid.AdvancedWinUiDataGridControl? _dataGridControl;

        public MainWindow()
        {
            this.InitializeComponent();

            _serviceProvider = CreateServiceProvider();
            _logger = _serviceProvider.GetRequiredService<ILogger<MainWindow>>();

            _ = InitializeAsync();

            this.Closed += OnWindowClosed;
            _logger.LogInformation("Demo MainWindow created");
        }

        #region Window Events

        private void OnWindowClosed(object sender, WindowEventArgs e)
        {
            try
            {
                _dataGridControl?.Dispose();
                _logger.LogInformation("Demo application closed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during application shutdown");
            }
        }

        #endregion

        #region Initialization

        private async Task InitializeAsync()
        {
            try
            {
                await Task.Delay(100);

                _dataGridControl = new RpaWinUiComponents.AdvancedWinUiDataGrid.AdvancedWinUiDataGridControl();

                if (DataGridPlaceholder.Parent is Grid parentGrid)
                {
                    var index = parentGrid.Children.IndexOf(DataGridPlaceholder);
                    parentGrid.Children.RemoveAt(index);
                    parentGrid.Children.Insert(index, _dataGridControl);
                }

                var columns = CreateSampleColumns();
                var validationRules = CreateSampleValidationRules();
                var throttling = ThrottlingConfig.Default;

                await _dataGridControl.InitializeAsync(columns, validationRules, throttling, 50);
                _dataGridControl.ErrorOccurred += OnDataGridError;

                _isInitialized = true;
                UpdateStatusText("DataGrid inicializovaný - pripravený na použitie");

                _logger.LogInformation("DataGrid initialized with {ColumnCount} columns and {RuleCount} validation rules",
                    columns.Count, validationRules.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing DataGrid");
                UpdateStatusText("Chyba pri inicializácii DataGrid");
                await ShowErrorDialog("Chyba inicializácie", ex.Message);
            }
        }

        private List<DataGridColumnDefinition> CreateSampleColumns()
        {
            return new List<DataGridColumnDefinition>
            {
                new("Meno", typeof(string)) { MinWidth = 100, MaxWidth = 200, Header = "Meno" },
                new("Priezvisko", typeof(string)) { MinWidth = 100, MaxWidth = 200, Header = "Priezvisko" },
                new("Vek", typeof(int)) { MinWidth = 60, MaxWidth = 100, Header = "Vek" },
                new("Email", typeof(string)) { MinWidth = 150, MaxWidth = 300, Header = "Email" },
                new("Plat", typeof(decimal)) { MinWidth = 100, MaxWidth = 150, Header = "Plat (€)" }
            };
        }

        private List<ValidationRule> CreateSampleValidationRules()
        {
            return new List<ValidationRule>
            {
                new("Meno", (value, row) => !string.IsNullOrWhiteSpace(value?.ToString()), "Meno je povinné")
                {
                    RuleName = "Meno_Required"
                },
                new("Vek", (value, row) => {
                    if (value == null || string.IsNullOrWhiteSpace(value.ToString())) return true;
                    if (int.TryParse(value.ToString(), out int age))
                        return age >= 18 && age <= 67;
                    return false;
                }, "Vek musí byť medzi 18 a 67 rokov")
                {
                    RuleName = "Vek_Range"
                }
            };
        }

        #endregion

        #region Button Event Handlers

        private async void OnLoadSampleDataClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_isInitialized || _dataGridControl == null)
                {
                    await ShowErrorDialog("Chyba", "DataGrid nie je inicializovaný");
                    return;
                }

                UpdateStatusText("Načítavam ukážkové dáta...");
                LoadSampleDataButton.IsEnabled = false;

                var sampleData = CreateSampleData();
                await _dataGridControl.LoadDataAsync(sampleData);

                UpdateStatusText($"Načítaných {sampleData.Count} ukážkových záznamov");
                _logger.LogInformation("Sample data loaded: {RecordCount} records", sampleData.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sample data");
                await ShowErrorDialog("Chyba pri načítaní dát", ex.Message);
                UpdateStatusText("Chyba pri načítaní ukážkových dát");
            }
            finally
            {
                LoadSampleDataButton.IsEnabled = true;
            }
        }

        private async void OnValidateAllClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_isInitialized || _dataGridControl == null) return;

                UpdateStatusText("Validujem všetky dáta...");
                ValidateAllButton.IsEnabled = false;

                var isValid = await _dataGridControl.ValidateAllRowsAsync();
                UpdateStatusText(isValid ? "Všetky dáta sú validné ✅" : "Nájdené validačné chyby ❌");

                _logger.LogInformation("Validation completed: {IsValid}", isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during validation");
                await ShowErrorDialog("Chyba pri validácii", ex.Message);
            }
            finally
            {
                ValidateAllButton.IsEnabled = true;
            }
        }

        private async void OnClearDataClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_isInitialized || _dataGridControl == null) return;

                var result = await ShowConfirmDialog("Potvrdenie", "Naozaj chcete vymazať všetky dáta?");
                if (!result) return;

                UpdateStatusText("Mažem dáta...");
                ClearDataButton.IsEnabled = false;

                await _dataGridControl.ClearAllDataAsync();
                UpdateStatusText("Všetky dáta vymazané");

                _logger.LogInformation("All data cleared");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing data");
                await ShowErrorDialog("Chyba pri mazaní dát", ex.Message);
            }
            finally
            {
                ClearDataButton.IsEnabled = true;
            }
        }

        private async void OnExportDataClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_isInitialized || _dataGridControl == null) return;

                UpdateStatusText("Exportujem dáta...");
                ExportDataButton.IsEnabled = false;

                var dataTable = await _dataGridControl.ExportToDataTableAsync();
                UpdateStatusText($"Exportovaných {dataTable.Rows.Count} záznamov");

                _logger.LogInformation("Data exported: {RecordCount} records", dataTable.Rows.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting data");
                await ShowErrorDialog("Chyba pri exporte", ex.Message);
            }
            finally
            {
                ExportDataButton.IsEnabled = true;
            }
        }

        private async void OnRemoveEmptyRowsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_isInitialized || _dataGridControl == null) return;

                UpdateStatusText("Odstraňujem prázdne riadky...");
                RemoveEmptyRowsButton.IsEnabled = false;

                await _dataGridControl.RemoveEmptyRowsAsync();
                UpdateStatusText("Prázdne riadky odstránené");

                _logger.LogInformation("Empty rows removed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing empty rows");
                await ShowErrorDialog("Chyba pri odstraňovaní riadkov", ex.Message);
            }
            finally
            {
                RemoveEmptyRowsButton.IsEnabled = true;
            }
        }

        private void OnThrottlingChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (ThrottlingComboBox.SelectedItem is ComboBoxItem item)
                {
                    var tag = item.Tag?.ToString();
                    _logger.LogInformation("Throttling changed to: {ThrottlingMode}", tag);
                    UpdateStatusText($"Throttling nastavený na: {item.Content}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing throttling settings");
            }
        }

        private void OnDebugLoggingChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                var isEnabled = DebugLoggingCheckBox.IsChecked == true;
                RpaWinUiComponents.AdvancedWinUiDataGrid.AdvancedWinUiDataGridControl.Configuration.SetDebugLogging(isEnabled);

                _logger.LogInformation("Debug logging {Status}", isEnabled ? "enabled" : "disabled");
                UpdateStatusText($"Debug logging {(isEnabled ? "zapnutý" : "vypnutý")}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing debug logging");
            }
        }

        private async void OnDataGridError(object? sender, RpaWinUiComponents.AdvancedWinUiDataGrid.Events.ComponentErrorEventArgs e)
        {
            try
            {
                _logger.LogError(e.Exception, "DataGrid error in operation: {Operation}", e.Operation);
                await ShowErrorDialog($"Chyba v DataGrid ({e.Operation})", e.Exception.Message);
                UpdateStatusText($"Chyba: {e.Operation}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling DataGrid error");
            }
        }

        #endregion

        #region Helper Methods

        private List<Dictionary<string, object?>> CreateSampleData()
        {
            var random = new Random();
            var firstNames = new[] { "Ján", "Peter", "Mária", "Anna", "Michal" };
            var lastNames = new[] { "Novák", "Svoboda", "Dvořák", "Černý", "Procházka" };

            var data = new List<Dictionary<string, object?>>();

            for (int i = 0; i < 10; i++)
            {
                data.Add(new Dictionary<string, object?>
                {
                    ["Meno"] = firstNames[random.Next(firstNames.Length)],
                    ["Priezvisko"] = lastNames[random.Next(lastNames.Length)],
                    ["Vek"] = random.Next(22, 65),
                    ["Email"] = $"test{i}@example.com",
                    ["Plat"] = random.Next(800, 8000)
                });
            }

            return data;
        }

        private void UpdateStatusText(string message)
        {
            try
            {
                StatusTextBlock.Text = message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status text: {Message}", message);
            }
        }

        private IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddLogging(builder =>
            {
                builder.AddDebug();
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            services.AddAdvancedWinUiDataGrid();
            return services.BuildServiceProvider();
        }

        #endregion

        #region Dialog Helpers

        private async Task ShowErrorDialog(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }

        private async Task<bool> ShowConfirmDialog(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                PrimaryButtonText = "Áno",
                SecondaryButtonText = "Nie",
                DefaultButton = ContentDialogButton.Secondary,
                XamlRoot = this.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }

        #endregion
    }
}
