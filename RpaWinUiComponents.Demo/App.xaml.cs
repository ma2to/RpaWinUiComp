using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using System;

namespace RpaWinUiComponents.Demo
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        private Window? _mainWindow;
        private IHost? _host;

        /// <summary>
        /// Initializes the singleton application object.
        /// </summary>
        public App()
        {
            // BEZ InitializeComponent - vyrieši CS1061 chybu

            // Initialize dependency injection and logging
            _host = CreateHostBuilder().Build();
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

                var logger = _host?.Services.GetService<ILogger<App>>();
                logger?.LogInformation("RpaWinUiComponents Demo application started successfully");
            }
            catch (Exception ex)
            {
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
                    services.AddLogging(builder =>
                    {
                        builder.AddDebug();
                        builder.AddConsole();
                        builder.SetMinimumLevel(LogLevel.Debug);
                    });
                });
        }

        public new static App Current => (App)Application.Current;
        public Window? MainWindow => _mainWindow;
        public IServiceProvider? Services => _host?.Services;
    }
}