//Helpers/ValidationHelper.cs
using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using RpaWinUiComponents.AdvancedWinUiDataGrid.Models;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Helpers
{
    /// <summary>
    /// Pomocné metódy pre tvorbu validačných pravidiel
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Vytvorí pravidlo pre povinné pole
        /// </summary>
        public static ValidationRule Required(string columnName, string? errorMessage = null)
        {
            return new ValidationRule(
                columnName,
                (value, row) => !string.IsNullOrWhiteSpace(value?.ToString()),
                errorMessage ?? $"{columnName} je povinné pole"
            )
            {
                RuleName = $"{columnName}_Required"
            };
        }

        /// <summary>
        /// Vytvorí pravidlo pre kontrolu dĺžky textu
        /// </summary>
        public static ValidationRule Length(string columnName, int minLength, int maxLength = int.MaxValue, string? errorMessage = null)
        {
            return new ValidationRule(
                columnName,
                (value, row) =>
                {
                    var text = value?.ToString() ?? "";
                    return text.Length >= minLength && text.Length <= maxLength;
                },
                errorMessage ?? $"{columnName} musí mať dĺžku medzi {minLength} a {maxLength} znakmi"
            )
            {
                RuleName = $"{columnName}_Length"
            };
        }

        /// <summary>
        /// Vytvorí pravidlo pre kontrolu číselného rozsahu
        /// </summary>
        public static ValidationRule Range(string columnName, double min, double max, string? errorMessage = null)
        {
            return new ValidationRule(
                columnName,
                (value, row) =>
                {
                    if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                        return true;

                    if (double.TryParse(value.ToString(), out double numValue))
                    {
                        return numValue >= min && numValue <= max;
                    }

                    return false;
                },
                errorMessage ?? $"{columnName} musí byť medzi {min} a {max}"
            )
            {
                RuleName = $"{columnName}_Range"
            };
        }

        /// <summary>
        /// Vytvorí pravidlo pre validáciu emailu
        /// </summary>
        public static ValidationRule Email(string columnName, string? errorMessage = null)
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            return new ValidationRule(
                columnName,
                (value, row) =>
                {
                    if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                        return true;

                    return emailRegex.IsMatch(value.ToString()!);
                },
                errorMessage ?? $"{columnName} musí mať platný formát emailu"
            )
            {
                RuleName = $"{columnName}_Email"
            };
        }

        /// <summary>
        /// Vytvorí pravidlo pre validáciu číselných hodnôt
        /// </summary>
        public static ValidationRule Numeric(string columnName, string? errorMessage = null)
        {
            return new ValidationRule(
                columnName,
                (value, row) =>
                {
                    if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                        return true;

                    return double.TryParse(value.ToString(), out _);
                },
                errorMessage ?? $"{columnName} musí byť číslo"
            )
            {
                RuleName = $"{columnName}_Numeric"
            };
        }

        /// <summary>
        /// Vytvorí podmienené validačné pravidlo
        /// </summary>
        public static ValidationRule Conditional(
            string columnName,
            Func<object?, DataGridRow, bool> validationFunction,
            Func<DataGridRow, bool> condition,
            string errorMessage,
            string? ruleName = null)
        {
            return new ValidationRule(columnName, validationFunction, errorMessage)
            {
                ApplyCondition = condition,
                RuleName = ruleName ?? $"{columnName}_Conditional_{Guid.NewGuid().ToString("N")[..8]}"
            };
        }

        /// <summary>
        /// Vytvorí async validačné pravidlo (napr. pre kontrolu v databáze)
        /// </summary>
        public static ValidationRule AsyncRule(
            string columnName,
            Func<object?, DataGridRow, CancellationToken, Task<bool>> asyncValidationFunction,
            string errorMessage,
            TimeSpan? timeout = null,
            string? ruleName = null)
        {
            return new ValidationRule(columnName, (_, _) => true, errorMessage)
            {
                IsAsync = true,
                AsyncValidationFunction = asyncValidationFunction,
                ValidationTimeout = timeout ?? TimeSpan.FromSeconds(5),
                RuleName = ruleName ?? $"{columnName}_Async_{Guid.NewGuid().ToString("N")[..8]}"
            };
        }

        /// <summary>
        /// Vytvorí pravidlo pre regex validáciu
        /// </summary>
        public static ValidationRule Regex(string columnName, string pattern, string? errorMessage = null, RegexOptions options = RegexOptions.None)
        {
            var regex = new System.Text.RegularExpressions.Regex(pattern, options | RegexOptions.Compiled);

            return new ValidationRule(
                columnName,
                (value, row) =>
                {
                    if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                        return true;

                    return regex.IsMatch(value.ToString()!);
                },
                errorMessage ?? $"{columnName} nemá správny formát"
            )
            {
                RuleName = $"{columnName}_Regex"
            };
        }

        /// <summary>
        /// Vytvorí pravidlo pre porovnanie s inou bunkou v rovnakom riadku
        /// </summary>
        public static ValidationRule Compare(
            string columnName,
            string compareColumnName,
            CompareOperation operation = CompareOperation.Equal,
            string? errorMessage = null)
        {
            return new ValidationRule(
                columnName,
                (value, row) =>
                {
                    var compareValue = row.GetValue<object>(compareColumnName);
                    return CompareValues(value, compareValue, operation);
                },
                errorMessage ?? $"{columnName} musí byť {GetOperationText(operation)} ako {compareColumnName}"
            )
            {
                RuleName = $"{columnName}_Compare_{compareColumnName}"
            };
        }

        private static bool CompareValues(object? value1, object? value2, CompareOperation operation)
        {
            if (value1 == null && value2 == null) return operation == CompareOperation.Equal;
            if (value1 == null || value2 == null) return operation == CompareOperation.NotEqual;

            try
            {
                var comparable1 = value1 as IComparable ?? value1.ToString();
                var comparable2 = value2 as IComparable ?? value2.ToString();

                var comparison = Comparer.Default.Compare(comparable1, comparable2);

                return operation switch
                {
                    CompareOperation.Equal => comparison == 0,
                    CompareOperation.NotEqual => comparison != 0,
                    CompareOperation.GreaterThan => comparison > 0,
                    CompareOperation.GreaterThanOrEqual => comparison >= 0,
                    CompareOperation.LessThan => comparison < 0,
                    CompareOperation.LessThanOrEqual => comparison <= 0,
                    _ => false
                };
            }
            catch
            {
                return false;
            }
        }

        private static string GetOperationText(CompareOperation operation)
        {
            return operation switch
            {
                CompareOperation.Equal => "rovnaké",
                CompareOperation.NotEqual => "rozdielne",
                CompareOperation.GreaterThan => "väčšie",
                CompareOperation.GreaterThanOrEqual => "väčšie alebo rovnaké",
                CompareOperation.LessThan => "menšie",
                CompareOperation.LessThanOrEqual => "menšie alebo rovnaké",
                _ => "porovnateľné"
            };
        }
    }

    /// <summary>
    /// Typy porovnania pre Compare validáciu
    /// </summary>
    public enum CompareOperation
    {
        Equal,
        NotEqual,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual
    }
}