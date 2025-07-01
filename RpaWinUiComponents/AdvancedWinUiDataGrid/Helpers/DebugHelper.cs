//Helpers/DebugHelper.cs - Opravený
using System;
using Microsoft.Extensions.Logging;
// Alias pre riešenie konfliktu s LoggerFactory
using AppLoggerFactory = RpaWinUiComponents.AdvancedWinUiDataGrid.Configuration.LoggerFactory;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Helpers
{
    internal static class DebugHelper
    {
        private static bool _isDebugEnabled = true;
        private static readonly ILogger _logger = AppLoggerFactory.CreateLogger("DebugHelper");

        public static bool IsDebugEnabled
        {
            get => _isDebugEnabled;
            set => _isDebugEnabled = value;
        }

        public static void Log(string message, string category = "General")
        {
            if (!_isDebugEnabled) return;

            var formattedMessage = $"[{category}] {message}";

            // Log to both Debug output and logger
            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {formattedMessage}");
            _logger.LogDebug("{Message}", formattedMessage);
        }

        public static void LogError(Exception ex, string operation, string category = "Error")
        {
            if (!_isDebugEnabled) return;

            var errorMessage = $"[{category}] {operation}: {ex.Message}";

            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {errorMessage}");
            _logger.LogError(ex, "{Operation} failed", operation);

            if (ex.InnerException != null)
            {
                var innerMessage = $"[{category}] Inner: {ex.InnerException.Message}";
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {innerMessage}");
                _logger.LogError(ex.InnerException, "Inner exception for {Operation}", operation);
            }
        }

        public static void LogValidation(string columnName, string value, bool isValid, string errors = "")
        {
            if (!_isDebugEnabled) return;

            var status = isValid ? "✓ VALID" : "✗ INVALID";
            var errorInfo = isValid ? "" : $" | Errors: {errors}";
            var message = $"{status} | {columnName} = '{value}'{errorInfo}";

            Log(message, "Validation");

            if (isValid)
            {
                _logger.LogDebug("Validation passed for {ColumnName} = '{Value}'", columnName, value);
            }
            else
            {
                _logger.LogWarning("Validation failed for {ColumnName} = '{Value}': {Errors}", columnName, value, errors);
            }
        }

        public static void LogNavigation(int fromRow, int fromCol, int toRow, int toCol)
        {
            if (!_isDebugEnabled) return;

            var message = $"Navigation: [{fromRow},{fromCol}] → [{toRow},{toCol}]";
            Log(message, "Navigation");
            _logger.LogTrace("Cell navigation from [{FromRow},{FromCol}] to [{ToRow},{ToCol}]", fromRow, fromCol, toRow, toCol);
        }

        public static void LogDataOperation(string operation, int rowCount, int columnCount = 0)
        {
            if (!_isDebugEnabled) return;

            var info = columnCount > 0 ? $"{rowCount} rows, {columnCount} columns" : $"{rowCount} rows";
            var message = $"{operation}: {info}";
            Log(message, "Data");
            _logger.LogInformation("Data operation {Operation}: {RowCount} rows, {ColumnCount} columns", operation, rowCount, columnCount);
        }

        public static void LogClipboard(string operation, int rows = 0, int cols = 0)
        {
            if (!_isDebugEnabled) return;

            var size = rows > 0 ? $"{rows}×{cols}" : "unknown size";
            var message = $"{operation}: {size}";
            Log(message, "Clipboard");
            _logger.LogDebug("Clipboard operation {Operation} with size {Rows}×{Cols}", operation, rows, cols);
        }

        public static void LogComponent(string component, string message)
        {
            if (!_isDebugEnabled) return;

            Log(message, component);
            _logger.LogDebug("[{Component}] {Message}", component, message);
        }

        public static void EnableDebug()
        {
            _isDebugEnabled = true;
            Log("Debug logging enabled", "Debug");
            _logger.LogInformation("Debug logging enabled");
        }

        public static void DisableDebug()
        {
            Log("Debug logging disabled", "Debug");
            _logger.LogInformation("Debug logging disabled");
            _isDebugEnabled = false;
        }
    }
}