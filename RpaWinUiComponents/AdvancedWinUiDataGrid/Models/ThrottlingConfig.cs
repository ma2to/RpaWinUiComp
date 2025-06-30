//Models/ThrottlingConfig.cs
using System;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Models
{
    /// <summary>
    /// Konfigurácia pre throttling real-time validácie
    /// </summary>
    public class ThrottlingConfig
    {
        /// <summary>
        /// Delay pre real-time validáciu počas písania (default: 300ms)
        /// </summary>
        public int TypingDelayMs { get; set; } = 300;

        /// <summary>
        /// Delay pre validáciu po paste operácii (default: 100ms)
        /// </summary>
        public int PasteDelayMs { get; set; } = 100;

        /// <summary>
        /// Delay pre batch validáciu všetkých riadkov (default: 200ms)
        /// </summary>
        public int BatchValidationDelayMs { get; set; } = 200;

        /// <summary>
        /// Maximálny počet súčasných validácií (default: 5)
        /// </summary>
        public int MaxConcurrentValidations { get; set; } = 5;

        /// <summary>
        /// Či je throttling povolený (default: true)
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Timeout pre jednu validáciu (default: 30 sekúnd)
        /// </summary>
        public TimeSpan ValidationTimeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Minimálny delay medzi validáciami tej istej bunky (default: 50ms)
        /// </summary>
        public int MinValidationIntervalMs { get; set; } = 50;

        /// <summary>
        /// Default konfigurácia
        /// </summary>
        public static ThrottlingConfig Default => new();

        /// <summary>
        /// Rýchla konfigurácia pre jednoduché validácie
        /// </summary>
        public static ThrottlingConfig Fast => new()
        {
            TypingDelayMs = 150,
            PasteDelayMs = 50,
            BatchValidationDelayMs = 100,
            MaxConcurrentValidations = 10,
            MinValidationIntervalMs = 25
        };

        /// <summary>
        /// Pomalá konfigurácia pre zložité validácie
        /// </summary>
        public static ThrottlingConfig Slow => new()
        {
            TypingDelayMs = 500,
            PasteDelayMs = 200,
            BatchValidationDelayMs = 400,
            MaxConcurrentValidations = 3,
            MinValidationIntervalMs = 100
        };

        /// <summary>
        /// Vypnutý throttling - okamžitá validácia
        /// </summary>
        public static ThrottlingConfig Disabled => new()
        {
            IsEnabled = false,
            TypingDelayMs = 0,
            PasteDelayMs = 0,
            BatchValidationDelayMs = 0,
            MinValidationIntervalMs = 0
        };

        /// <summary>
        /// Vytvorí custom konfiguráciu
        /// </summary>
        public static ThrottlingConfig Custom(int typingDelayMs, int maxConcurrentValidations = 5)
        {
            return new ThrottlingConfig
            {
                TypingDelayMs = typingDelayMs,
                PasteDelayMs = Math.Max(10, typingDelayMs / 3),
                BatchValidationDelayMs = typingDelayMs * 2,
                MaxConcurrentValidations = maxConcurrentValidations,
                MinValidationIntervalMs = Math.Max(10, typingDelayMs / 6)
            };
        }

        /// <summary>
        /// Validácia konfigurácie
        /// </summary>
        public bool IsValidConfig(out string? errorMessage)
        {
            errorMessage = null;

            if (TypingDelayMs < 0)
            {
                errorMessage = "TypingDelayMs musí byť >= 0";
                return false;
            }

            if (PasteDelayMs < 0)
            {
                errorMessage = "PasteDelayMs musí byť >= 0";
                return false;
            }

            if (MaxConcurrentValidations < 1)
            {
                errorMessage = "MaxConcurrentValidations musí byť >= 1";
                return false;
            }

            if (ValidationTimeout <= TimeSpan.Zero)
            {
                errorMessage = "ValidationTimeout musí byť kladný";
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            return $"Throttling: {TypingDelayMs}ms typing, {MaxConcurrentValidations} concurrent, Enabled: {IsEnabled}";
        }
    }
}