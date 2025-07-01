// RpaWinUiComponents.Demo/App.xaml.cs - KOMPLETNÝ OPRAVENÝ
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using System;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Configuration;

namespace RpaWinUiComponents.Demo
{
    public sealed partial class App : Application
    {
        private Window? _mainWindow;
        private IHost? _host;

        public App()
        {
            // OPRAVA: InitializeComponent MUSÍ byť volané
            this.InitializeComponent();

            // Initialize dependency injection and logging
            _host = CreateHostBuilder().Build();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            try
            {
                _mainWindow = new MainWindow();
                _mainWindow.Activate();

                var logger = _host?.Services.GetService<ILogger<App>>();
                logger?.LogInformation("RpaWinUiComponents Demo application started successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting application: {ex}");
                throw;
            }
        }

        private static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddLogging(builder =>
                    {
                        builder.AddDebug();
                        builder.AddConsole();
                        builder.SetMinimumLevel(LogLevel.Debug);
                    });

                    // OPRAVA: Registrácia služieb pre AdvancedWinUiDataGrid
                    services.AddAdvancedWinUiDataGrid();
                });
        }

        public new static App Current => (App)Application.Current;
        public Window? MainWindow => _mainWindow;
        public IServiceProvider? Services => _host?.Services;
    }
}