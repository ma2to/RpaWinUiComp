//Models/ValidationRule.cs
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Models
{
    /// <summary>
    /// Validačné pravidlo pre bunky v DataGrid
    /// </summary>
    public class ValidationRule
    {
        /// <summary>
        /// Názov stĺpca na ktorý sa pravidlo aplikuje
        /// </summary>
        public string ColumnName { get; set; } = string.Empty;

        /// <summary>
        /// Validačná funkcia - prvý parameter je hodnota bunky, druhý je celý riadok
        /// </summary>
        public Func<object?, DataGridRow, bool> ValidationFunction { get; set; } = (_, _) => true;

        /// <summary>
        /// Chybová správa pri neúspešnej validácii
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Podmienka kedy sa má validácia aplikovať (default: vždy)
        /// </summary>
        public Func<DataGridRow, bool> ApplyCondition { get; set; } = _ => true;

        /// <summary>
        /// Priorita pravidla (vyššie číslo = vyššia priorita)
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Unique identifikátor pravidla
        /// </summary>
        public string RuleName { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Timeout pre async validácie (default: 5 sekúnd)
        /// </summary>
        public TimeSpan ValidationTimeout { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Či je validácia async
        /// </summary>
        public bool IsAsync { get; set; } = false;

        /// <summary>
        /// Async validačná funkcia (ak IsAsync = true)
        /// </summary>
        public Func<object?, DataGridRow, CancellationToken, Task<bool>>? AsyncValidationFunction { get; set; }

        public ValidationRule()
        {
        }

        public ValidationRule(string columnName, Func<object?, DataGridRow, bool> validationFunction, string errorMessage)
        {
            ColumnName = columnName;
            ValidationFunction = validationFunction;
            ErrorMessage = errorMessage;
            RuleName = $"{columnName}_{Guid.NewGuid().ToString("N")[..8]}";
        }

        /// <summary>
        /// Kontroluje či sa má validácia aplikovať na daný riadok
        /// </summary>
        public bool ShouldApply(DataGridRow row)
        {
            try
            {
                return ApplyCondition?.Invoke(row) ?? true;
            }
            catch
            {
                return true; // V prípade chyby aplikuj validáciu
            }
        }

        /// <summary>
        /// Synchronne validuje hodnotu
        /// </summary>
        public bool Validate(object? value, DataGridRow row)
        {
            try
            {
                if (!ShouldApply(row))
                    return true;

                return ValidationFunction?.Invoke(value, row) ?? true;
            }
            catch
            {
                return false; // V prípade chyby považuj za nevalidné
            }
        }

        /// <summary>
        /// Async validácia hodnoty
        /// </summary>
        public async Task<bool> ValidateAsync(object? value, DataGridRow row, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ShouldApply(row))
                    return true;

                if (IsAsync && AsyncValidationFunction != null)
                {
                    using var timeoutCts = new CancellationTokenSource(ValidationTimeout);
                    using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

                    return await AsyncValidationFunction(value, row, combinedCts.Token);
                }

                return Validate(value, row);
            }
            catch (OperationCanceledException)
            {
                return false; // Timeout alebo zrušenie = nevalidné
            }
            catch
            {
                return false; // Akákoľvek chyba = nevalidné
            }
        }

        public override string ToString()
        {
            return $"{RuleName}: {ColumnName} - {ErrorMessage}";
        }
    }
}