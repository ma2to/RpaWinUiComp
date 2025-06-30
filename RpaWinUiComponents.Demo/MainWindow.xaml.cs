//RpaWinUiComponents.Demo/MainWindow.xaml.cs - Opravený
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RpaWinUiComponents.AdvancedWinUiDataGrid;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Configuration;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Events;

// Alias pre riešenie konfliktu ColumnDefinition
using DataGridColumnDefinition = RpaWinUiComponents.AdvancedWinUiDataGrid.Models.ColumnDefinition;
using ValidationRule = RpaWinUiComponents.AdvancedWinUiDataGrid.Models.ValidationRule;
using ThrottlingConfig = RpaWinUiComponents.AdvancedWinUiDataGrid.Models.ThrottlingConfig;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace RpaWinUiComponents.Demo
{
    /// <summary>
    /// Demo aplikácia pre AdvancedWinUiDataGrid komponent
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly ILogger<MainWindow> _logger;
        private readonly IServiceProvider _serviceProvider;
        private bool _isInitialized = false;

        public MainWindow()
        {
            this.InitializeComponent();

            // Setup logging and DI
            _serviceProvider = CreateServiceProvider();
            _logger = _serviceProvider.GetRequiredService<ILogger<MainWindow>>();

            // Configure AdvancedDataGrid services
            AdvancedWinUiDataGridControl.Configuration.ConfigureServices(_serviceProvider);

            this.Loaded += OnWindowLoaded;
            this.Closed += OnWindowClosed;

            _logger.LogInformation("Demo MainWindow created");
        }

        #region Window Events

        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await InitializeDataGridAsync();
                UpdateStatusBar();

                _logger.LogInformation("Demo application loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading demo application");
                await ShowErrorDialog("Chyba pri načítaní", ex.Message);
            }
        }

        private void OnWindowClosed(object sender, WindowEventArgs e)
        {
            try
            {
                MainDataGrid?.Dispose();
                _logger.LogInformation("Demo application closed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during application shutdown");
            }
        }

        #endregion

        #region DataGrid Initialization

        private async Task InitializeDataGridAsync()
        {
            try
            {
                var columns = CreateSampleColumns();
                var validationRules = CreateSampleValidationRules();
                var throttling = ThrottlingConfig.Default;

                await MainDataGrid.InitializeAsync(columns, validationRules, throttling, 50);

                // Subscribe to events
                MainDataGrid.ErrorOccurred += OnDataGridError;

                _isInitialized = true;
                StatusTextBlock.Text = "DataGrid inicializovaný - pripravený na použitie";

                _logger.LogInformation("DataGrid initialized with {ColumnCount} columns and {RuleCount} validation rules",
                    columns.Count, validationRules.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing DataGrid");
                throw;
            }
        }

        private List<DataGridColumnDefinition> CreateSampleColumns()
        {
            return new List<DataGridColumnDefinition>
            {
                new DataGridColumnDefinition("Meno", typeof(string)) { MinWidth = 100, MaxWidth = 200, Header = "Meno", ToolTip = "Celé meno osoby" },
                new DataGridColumnDefinition("Priezvisko", typeof(string)) { MinWidth = 100, MaxWidth = 200, Header = "Priezvisko" },
                new DataGridColumnDefinition("Vek", typeof(int)) { MinWidth = 60, MaxWidth = 100, Header = "Vek" },
                new DataGridColumnDefinition("Email", typeof(string)) { MinWidth = 150, MaxWidth = 300, Header = "Email" },
                new DataGridColumnDefinition("Plat", typeof(decimal)) { MinWidth = 100, MaxWidth = 150, Header = "Plat (€)" },
                new DataGridColumnDefinition("DatumNastupu", typeof(DateTime)) { MinWidth = 120, MaxWidth = 180, Header = "Dátum nástupu" },
                new DataGridColumnDefinition("Aktívny", typeof(bool)) { MinWidth = 80, MaxWidth = 100, Header = "Aktívny" },
                new DataGridColumnDefinition("Poznámky", typeof(string)) { MinWidth = 200, MaxWidth = 400, Header = "Poznámky" }
            };
        }

        private List<ValidationRule> CreateSampleValidationRules()
        {
            return new List<ValidationRule>
            {
                // Meno - povinné
                AdvancedWinUiDataGridControl.Validation.Required("Meno", "Meno je povinné pole"),
                
                // Priezvisko - povinné
                AdvancedWinUiDataGridControl.Validation.Required("Priezvisko", "Priezvisko je povinné pole"),
                
                // Vek - rozsah
                AdvancedWinUiDataGridControl.Validation.Range("Vek", 18, 67, "Vek musí byť medzi 18 a 67 rokov"),
                AdvancedWinUiDataGridControl.Validation.Numeric("Vek", "Vek musí byť číslo"),
                
                // Email - formát
                AdvancedWinUiDataGridControl.Validation.Email("Email", "Neplatný formát emailu"),
                AdvancedWinUiDataGridControl.Validation.Required("Email", "Email je povinný"),
                
                // Plat - rozsah
                AdvancedWinUiDataGridControl.Validation.Range("Plat", 600, 50000, "Plat musí byť medzi 600€ a 50,000€"),
                AdvancedWinUiDataGridControl.Validation.Numeric("Plat", "Plat musí byť číslo"),
                
                // Podmienené validácie
                AdvancedWinUiDataGridControl.Validation.Conditional(
                    "Poznámky",
                    (value, row) => !string.IsNullOrEmpty(value?.ToString()),
                    row => row.GetValue<int>("Vek") > 50,
                    "Pre zamestnancov starších ako 50 rokov sú poznámky povinné",
                    "PoznámkyPreStaršíchZamestnancov"
                )
            };
        }

        #endregion

        #region Button Event Handlers

        private async void OnLoadSampleDataClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_isInitialized)
                {
                    await ShowErrorDialog("Chyba", "DataGrid nie je inicializovaný");
                    return;
                }

                StatusTextBlock.Text = "Načítavam ukážkové dáta...";
                LoadSampleDataButton.IsEnabled = false;

                var sampleData = CreateSampleData();
                await MainDataGrid.LoadDataAsync(sampleData);

                UpdateStatusBar();
                StatusTextBlock.Text = $"Načítaných {sampleData.Count} ukážkových záznamov";

                _logger.LogInformation("Sample data loaded: {RecordCount} records", sampleData.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sample data");
                await ShowErrorDialog("Chyba pri načítaní dát", ex.Message);
                StatusTextBlock.Text = "Chyba pri načítaní ukážkových dát";
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
                if (!_isInitialized) return;

                StatusTextBlock.Text = "Validujem všetky dáta...";
                ValidateAllButton.IsEnabled = false;

                var isValid = await MainDataGrid.ValidateAllRowsAsync();

                UpdateStatusBar();
                StatusTextBlock.Text = isValid ? "Všetky dáta sú validné ✅" : "Nájdené validačné chyby ❌";

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
                if (!_isInitialized) return;

                var result = await ShowConfirmDialog("Potvrdenie", "Naozaj chcete vymazať všetky dáta?");
                if (!result) return;

                StatusTextBlock.Text = "Mažem dáta...";
                ClearDataButton.IsEnabled = false;

                await MainDataGrid.ClearAllDataAsync();

                UpdateStatusBar();
                StatusTextBlock.Text = "Všetky dáta vymazané";

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
                if (!_isInitialized) return;

                StatusTextBlock.Text = "Exportujem dáta...";
                ExportDataButton.IsEnabled = false;

                var dataTable = await MainDataGrid.ExportToDataTableAsync();

                if (dataTable.Rows.Count == 0)
                {
                    await ShowInfoDialog("Export", "Žiadne dáta na export");
                    return;
                }

                // Convert to CSV
                var csv = ConvertDataTableToCsv(dataTable);

                // Save to file
                var savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                savePicker.FileTypeChoices.Add("CSV súbory", new List<string>() { ".csv" });
                savePicker.SuggestedFileName = $"DataGrid_Export_{DateTime.Now:yyyyMMdd_HHmmss}";

                // Set window handle for picker
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

                var file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    await FileIO.WriteTextAsync(file, csv);
                    StatusTextBlock.Text = $"Exportovaných {dataTable.Rows.Count} záznamov do {file.Name}";

                    _logger.LogInformation("Data exported to {FileName}: {RecordCount} records", file.Name, dataTable.Rows.Count);
                }
                else
                {
                    StatusTextBlock.Text = "Export zrušený";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting data");
                await ShowErrorDialog("Chyba pri exporte", ex.Message);
                StatusTextBlock.Text = "Chyba pri exporte dát";
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
                if (!_isInitialized) return;

                StatusTextBlock.Text = "Odstraňujem prázdne riadky...";
                RemoveEmptyRowsButton.IsEnabled = false;

                await MainDataGrid.RemoveEmptyRowsAsync();

                UpdateStatusBar();
                StatusTextBlock.Text = "Prázdne riadky odstránené";

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

        #endregion

        #region Settings Event Handlers

        private async void OnThrottlingChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!_isInitialized || ThrottlingComboBox.SelectedItem is not ComboBoxItem item)
                    return;

                var tag = item.Tag?.ToString();
                ThrottlingConfig newConfig = tag switch
                {
                    "Disabled" => ThrottlingConfig.Disabled,
                    "Fast" => ThrottlingConfig.Fast,
                    "Slow" => ThrottlingConfig.Slow,
                    _ => ThrottlingConfig.Default
                };

                // Would need to reinitialize with new throttling config
                // For demo purposes, just log the change
                _logger.LogInformation("Throttling changed to: {ThrottlingMode}", tag);
                StatusTextBlock.Text = $"Throttling nastavený na: {item.Content}";
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
                AdvancedWinUiDataGridControl.Configuration.SetDebugLogging(isEnabled);

                _logger.LogInformation("Debug logging {Status}", isEnabled ? "enabled" : "disabled");
                StatusTextBlock.Text = $"Debug logging {(isEnabled ? "zapnutý" : "vypnutý")}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing debug logging");
            }
        }

        #endregion

        #region DataGrid Event Handlers

        private async void OnDataGridError(object? sender, ComponentErrorEventArgs e)
        {
            try
            {
                _logger.LogError(e.Exception, "DataGrid error in operation: {Operation}", e.Operation);

                await ShowErrorDialog($"Chyba v DataGrid ({e.Operation})", e.Exception.Message);
                StatusTextBlock.Text = $"Chyba: {e.Operation}";
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
            var firstNames = new[] { "Ján", "Peter", "Mária", "Anna", "Michal", "Eva", "Tomáš", "Katarína", "Martin", "Zuzana" };
            var lastNames = new[] { "Novák", "Svoboda", "Dvořák", "Černý", "Procházka", "Krejčí", "Novotný", "Kratochvíl", "Fiala", "Mareš" };
            var domains = new[] { "gmail.com", "azet.sk", "centrum.sk", "post.sk", "outlook.com" };

            var data = new List<Dictionary<string, object?>>();

            for (int i = 0; i < 25; i++)
            {
                var firstName = firstNames[random.Next(firstNames.Length)];
                var lastName = lastNames[random.Next(lastNames.Length)];
                var age = random.Next(22, 65);
                var salary = random.Next(800, 8000);
                var startDate = DateTime.Now.AddDays(-random.Next(30, 3650));
                var isActive = random.Next(100) > 10; // 90% chance of being active

                // Simulate some validation errors
                var email = i % 7 == 0 ? "invalid-email" : $"{firstName.ToLower()}.{lastName.ToLower()}@{domains[random.Next(domains.Length)]}";
                if (i % 5 == 0) age = random.Next(15, 20); // Some underage
                if (i % 8 == 0) salary = random.Next(100, 500); // Some underpaid

                var notes = string.Empty;
                if (age > 50 && i % 3 == 0)
                {
                    notes = i % 2 == 0 ? "Skúsený zamestnanec" : ""; // Some missing notes for older employees
                }

                data.Add(new Dictionary<string, object?>
                {
                    ["Meno"] = firstName,
                    ["Priezvisko"] = lastName,
                    ["Vek"] = age,
                    ["Email"] = email,
                    ["Plat"] = salary,
                    ["DatumNastupu"] = startDate,
                    ["Aktívny"] = isActive,
                    ["Poznámky"] = notes
                });
            }

            return data;
        }

        private string ConvertDataTableToCsv(DataTable dataTable)
        {
            var csv = new System.Text.StringBuilder();

            // Headers
            var headers = dataTable.Columns.Cast<DataColumn>().Select(column => EscapeCsvValue(column.ColumnName));
            csv.AppendLine(string.Join(",", headers));

            // Data rows
            foreach (DataRow row in dataTable.Rows)
            {
                var fields = row.ItemArray.Select(field => EscapeCsvValue(field?.ToString() ?? ""));
                csv.AppendLine(string.Join(",", fields));
            }

            return csv.ToString();
        }

        private string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }

        private void UpdateStatusBar()
        {
            try
            {
                // This would need to be implemented based on the actual DataGrid ViewModel
                // For now, just show basic status
                StatusTextBlock.Text = "Pripravené";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status bar");
            }
        }

        private IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();

            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddDebug();
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            // Add AdvancedDataGrid services
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

        private async Task ShowInfoDialog(string title, string message)
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