using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ValidationRule = RpaWinUiComponents.AdvancedWinUiDataGrid.Models.ValidationRule;
using ThrottlingConfig = RpaWinUiComponents.AdvancedWinUiDataGrid.Models.ThrottlingConfig;
using DataGridColumnDefinition = RpaWinUiComponents.AdvancedWinUiDataGrid.Models.ColumnDefinition;

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

            // Spustenie inicializácie na pozadí
            _ = InitializeAsync();

            this.Closed += OnWindowClosed;
            _logger.LogInformation("Demo MainWindow created");
        }

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

        private async Task InitializeAsync()
        {
            try
            {
                UpdateStatusText("Inicializuje sa komponent...");
                await Task.Delay(500);

                _dataGridControl = new RpaWinUiComponents.AdvancedWinUiDataGrid.AdvancedWinUiDataGridControl();

                UpdateStatusText("Nastavujú sa stĺpce...");
                await Task.Delay(300);

                // Nahradenie placeholder s DataGrid
                if (DataGridPlaceholder.Parent is Grid parentGrid)
                {
                    var index = parentGrid.Children.IndexOf(DataGridPlaceholder);
                    parentGrid.Children.RemoveAt(index);
                    parentGrid.Children.Insert(index, _dataGridControl);
                }

                UpdateStatusText("Vytvárajú sa validačné pravidlá...");
                await Task.Delay(300);

                var columns = CreateSampleColumns();
                var validationRules = CreateSampleValidationRules();
                var throttling = ThrottlingConfig.Default;

                UpdateStatusText("Inicializuje sa DataGrid...");
                await _dataGridControl.InitializeAsync(columns, validationRules, throttling, 20);

                _dataGridControl.ErrorOccurred += OnDataGridError;

                // Skrytie loading panela
                if (LoadingPanel != null)
                    LoadingPanel.Visibility = Visibility.Collapsed;
                if (DataGridPlaceholder != null)
                    DataGridPlaceholder.Visibility = Visibility.Visible;

                _isInitialized = true;

                // Aktualizácia UI
                if (InitStatusText != null)
                {
                    InitStatusText.Text = " - ✅ Pripravené";
                    InitStatusText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green);
                }

                UpdateStatusText("DataGrid inicializovaný - pripravený na použitie");

                _logger.LogInformation("DataGrid initialized successfully");

                // Auto-load sample data
                await Task.Delay(1000);
                await LoadSampleDataInternal();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during DataGrid initialization");
                if (LoadingPanel != null)
                    LoadingPanel.Visibility = Visibility.Collapsed;
                if (InitStatusText != null)
                {
                    InitStatusText.Text = " - ❌ Chyba";
                    InitStatusText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
                }
                UpdateStatusText("Chyba pri inicializácii DataGrid");
                await ShowErrorDialog("Chyba inicializácie", ex.Message);
            }
        }

        private List<DataGridColumnDefinition> CreateSampleColumns()
        {
            return new List<DataGridColumnDefinition>
            {
                new("Meno", typeof(string)) { MinWidth = 120, MaxWidth = 200, Header = "👤 Meno" },
                new("Priezvisko", typeof(string)) { MinWidth = 120, MaxWidth = 200, Header = "👥 Priezvisko" },
                new("Email", typeof(string)) { MinWidth = 200, MaxWidth = 300, Header = "📧 Email" },
                new("Pozicia", typeof(string)) { MinWidth = 150, MaxWidth = 250, Header = "💼 Pozícia" },
                new("Plat", typeof(decimal)) { MinWidth = 100, MaxWidth = 150, Header = "💰 Plat (€)" },
                new("Vek", typeof(int)) { MinWidth = 60, MaxWidth = 100, Header = "🎂 Vek" }
            };
        }

        private List<ValidationRule> CreateSampleValidationRules()
        {
            return new List<ValidationRule>
            {
                new("Meno", (value, row) => !string.IsNullOrWhiteSpace(value?.ToString()),
                    "❌ Meno je povinné pole") { RuleName = "Meno_Required", Priority = 100 },

                new("Priezvisko", (value, row) => !string.IsNullOrWhiteSpace(value?.ToString()),
                    "❌ Priezvisko je povinné pole") { RuleName = "Priezvisko_Required", Priority = 100 },

                new("Email", (value, row) => {
                    var email = value?.ToString()?.Trim();
                    if (string.IsNullOrEmpty(email)) return true;
                    return email.Contains("@") && email.Contains(".") && email.Length > 5;
                }, "❌ Neplatný formát emailu") { RuleName = "Email_Format", Priority = 85 },

                new("Vek", (value, row) => {
                    if (value == null || string.IsNullOrWhiteSpace(value.ToString())) return true;
                    if (int.TryParse(value.ToString(), out int age))
                        return age >= 18 && age <= 67;
                    return false;
                }, "❌ Vek musí byť medzi 18 a 67 rokov") { RuleName = "Vek_Range", Priority = 80 },

                new("Plat", (value, row) => {
                    if (value == null || string.IsNullOrWhiteSpace(value.ToString())) return true;
                    if (decimal.TryParse(value.ToString(), out decimal plat))
                        return plat >= 500 && plat <= 10000;
                    return false;
                }, "❌ Plat musí byť medzi 500-10000 EUR") { RuleName = "Plat_Range", Priority = 80 }
            };
        }

        private async void OnLoadSampleDataClick(object sender, RoutedEventArgs e)
        {
            await LoadSampleDataInternal();
        }

        private async Task LoadSampleDataInternal()
        {
            try
            {
                if (!_isInitialized || _dataGridControl == null) return;

                UpdateStatusText("Načítavam ukážkové dáta...");
                if (LoadSampleDataButton != null)
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
            }
            finally
            {
                if (LoadSampleDataButton != null)
                    LoadSampleDataButton.IsEnabled = true;
            }
        }

        private async void OnValidateAllClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_isInitialized || _dataGridControl == null) return;

                UpdateStatusText("Validujem všetky dáta...");
                if (ValidateAllButton != null)
                    ValidateAllButton.IsEnabled = false;

                var isValid = await _dataGridControl.ValidateAllRowsAsync();
                var statusText = isValid ? "✅ Všetky dáta sú validné" : "❌ Nájdené validačné chyby";
                UpdateStatusText(statusText);

                _logger.LogInformation("Validation completed: {IsValid}", isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during validation");
                await ShowErrorDialog("Chyba pri validácii", ex.Message);
            }
            finally
            {
                if (ValidateAllButton != null)
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
                if (ClearDataButton != null)
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
                if (ClearDataButton != null)
                    ClearDataButton.IsEnabled = true;
            }
        }

        private async void OnExportDataClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_isInitialized || _dataGridControl == null) return;

                UpdateStatusText("Exportujem dáta...");
                if (ExportDataButton != null)
                    ExportDataButton.IsEnabled = false;

                var dataTable = await _dataGridControl.ExportToDataTableAsync();
                UpdateStatusText($"Exportovaných {dataTable.Rows.Count} záznamov");

                await ShowInfoDialog("Export dokončený",
                    $"Úspešne exportovaných {dataTable.Rows.Count} validných záznamov.");

                _logger.LogInformation("Data exported: {RecordCount} records", dataTable.Rows.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting data");
                await ShowErrorDialog("Chyba pri exporte", ex.Message);
            }
            finally
            {
                if (ExportDataButton != null)
                    ExportDataButton.IsEnabled = true;
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

        private List<Dictionary<string, object?>> CreateSampleData()
        {
            var random = new Random();
            var data = new List<Dictionary<string, object?>>();

            var mena = new[] { "Ján", "Peter", "Mária", "Anna", "Michal" };
            var priezviska = new[] { "Novák", "Svoboda", "Dvořák", "Černý", "Procházka" };
            var pozicie = new[] { "Developer", "Tester", "Analyst", "Manager" };

            // Validné záznamy
            for (int i = 0; i < 5; i++)
            {
                data.Add(new Dictionary<string, object?>
                {
                    ["Meno"] = mena[i],
                    ["Priezvisko"] = priezviska[i],
                    ["Email"] = $"{mena[i].ToLower()}.{priezviska[i].ToLower()}@company.sk",
                    ["Pozicia"] = pozicie[random.Next(pozicie.Length)],
                    ["Plat"] = random.Next(1000, 5000),
                    ["Vek"] = random.Next(25, 60)
                });
            }

            // Nevalidné záznamy pre testovanie
            data.AddRange(new[]
            {
                new Dictionary<string, object?>
                {
                    ["Meno"] = "", // Chýba meno
                    ["Priezvisko"] = "Test",
                    ["Email"] = "test@company.sk",
                    ["Pozicia"] = "Developer",
                    ["Plat"] = 1500m,
                    ["Vek"] = 30
                },
                new Dictionary<string, object?>
                {
                    ["Meno"] = "Test",
                    ["Priezvisko"] = "User",
                    ["Email"] = "invalid-email", // Nevalidný email
                    ["Pozicia"] = "Tester",
                    ["Plat"] = 1200m,
                    ["Vek"] = 28
                },
                new Dictionary<string, object?>
                {
                    ["Meno"] = "Mladý",
                    ["Priezvisko"] = "Student",
                    ["Email"] = "student@company.sk",
                    ["Pozicia"] = "Developer",
                    ["Plat"] = 800m,
                    ["Vek"] = 16 // Príliš mladý
                }
            });

            return data;
        }

        private void UpdateStatusText(string message)
        {
            try
            {
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    if (StatusTextBlock != null)
                    {
                        StatusTextBlock.Text = message;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating status text: {Message}", message);
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
    }
}