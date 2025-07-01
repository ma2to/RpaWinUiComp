//Commands/AsyncRelayCommand.cs - Rozšírený
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Commands
{
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _executeAsync;
        private readonly Func<bool>? _canExecute;
        private readonly Action<Exception>? _errorHandler;
        private bool _isExecuting = false;

        public AsyncRelayCommand(Func<Task> executeAsync, Func<bool>? canExecute = null, Action<Exception>? errorHandler = null)
        {
            _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            _canExecute = canExecute;
            _errorHandler = errorHandler;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return !_isExecuting && (_canExecute?.Invoke() ?? true);
        }

        public async void Execute(object? parameter)
        {
            await ExecuteAsync();
        }

        /// <summary>
        /// Explicit async execution method
        /// </summary>
        public async Task ExecuteAsync()
        {
            if (_isExecuting) return;

            try
            {
                _isExecuting = true;
                RaiseCanExecuteChanged();
                await _executeAsync();
            }
            catch (Exception ex)
            {
                if (_errorHandler != null)
                {
                    _errorHandler(ex);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"AsyncRelayCommand error: {ex.Message}");
                }
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class AsyncRelayCommand<T> : ICommand
    {
        private readonly Func<T?, Task> _executeAsync;
        private readonly Func<T?, bool>? _canExecute;
        private readonly Action<Exception>? _errorHandler;
        private bool _isExecuting = false;

        public AsyncRelayCommand(Func<T?, Task> executeAsync, Func<T?, bool>? canExecute = null, Action<Exception>? errorHandler = null)
        {
            _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            _canExecute = canExecute;
            _errorHandler = errorHandler;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            T? typedParameter = parameter is T tp ? tp : default(T);
            return !_isExecuting && (_canExecute?.Invoke(typedParameter) ?? true);
        }

        public async void Execute(object? parameter)
        {
            T? typedParameter = parameter is T tp ? tp : default(T);
            await ExecuteAsync(typedParameter);
        }

        /// <summary>
        /// Explicit async execution method
        /// </summary>
        public async Task ExecuteAsync(T? parameter = default(T))
        {
            if (_isExecuting) return;

            try
            {
                _isExecuting = true;
                RaiseCanExecuteChanged();
                await _executeAsync(parameter);
            }
            catch (Exception ex)
            {
                if (_errorHandler != null)
                {
                    _errorHandler(ex);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"AsyncRelayCommand<T> error: {ex.Message}");
                }
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}