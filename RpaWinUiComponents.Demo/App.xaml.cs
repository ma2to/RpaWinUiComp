//RpaWinUiComponents.Demo/App.xaml.cs - Opravený
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using RpaWinUiComponents.AdvancedWinUiDataGrid;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Configuration;
using System;

namespace RpaWinUiComponents.Demo
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _mainWindow;
        private IHost? _host;

        /// <summary>
        /// Initializes the singleton application object. This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            // Initialize dependency injection and logging
            _host = CreateHostBuilder().Build();

            // Configure AdvancedDataGrid services
            AdvancedWinUiDataGridControl.Configuration.ConfigureServices(_host.Services);

            var loggerFactory = _host.Services.GetRequiredService<ILoggerFactory>();
            AdvancedWinUiDataGridControl.Configuration.ConfigureLogging(loggerFactory);
            AdvancedWinUiDataGridControl.Configuration.SetDebugLogging(true);
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            try
            {
                _mainWindow = new MainWindow();
                _mainWindow.Activate();

                var logger = _host?.Services.GetRequiredService<ILogger<App>>();
                logger?.LogInformation("RpaWinUiComponents Demo application started successfully");
            }
            catch (Exception ex)
            {
                // Log startup error
                System.Diagnostics.Debug.WriteLine($"Error starting application: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Creates and configures the application host
        /// </summary>
        private static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Add logging with detailed configuration
                    services.AddLogging(builder =>
                    {
                        builder.AddDebug();
                        builder.AddConsole();
                        builder.SetMinimumLevel(LogLevel.Debug);

                        // Configure different log levels for different namespaces
                        builder.AddFilter("Microsoft", LogLevel.Warning);
                        builder.AddFilter("System", LogLevel.Warning);
                        builder.AddFilter("RpaWinUiComponents", LogLevel.Debug);
                    });

                    // Register AdvancedDataGrid services
                    services.AddAdvancedWinUiDataGrid();

                    // Register demo-specific services if needed
                    // services.AddScoped<IDemoService, DemoService>();
                });
        }

        /// <summary>
        /// Gets the current application instance as App
        /// </summary>
        public new static App Current => (App)Application.Current;

        /// <summary>
        /// Gets the main window instance
        /// </summary>
        public Window? MainWindow => _mainWindow;

        /// <summary>
        /// Gets the service provider for dependency injection
        /// </summary>
        public IServiceProvider? Services => _host?.Services;
    }
}