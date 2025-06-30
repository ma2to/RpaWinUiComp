//Configuration/LoggerFactory.cs
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Configuration
{
    internal static class LoggerFactory
    {
        private static ILoggerFactory? _loggerFactory;

        public static void Configure(ILoggerFactory? loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public static ILogger<T> CreateLogger<T>()
        {
            return _loggerFactory?.CreateLogger<T>() ?? NullLogger<T>.Instance;
        }

        public static ILogger CreateLogger(string categoryName)
        {
            return _loggerFactory?.CreateLogger(categoryName) ?? NullLogger.Instance;
        }
    }
}