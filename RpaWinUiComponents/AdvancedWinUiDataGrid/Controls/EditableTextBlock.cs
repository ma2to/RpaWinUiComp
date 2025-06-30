//Controls/EditableTextBlock.cs - Custom WinUI 3 Control
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.System;

namespace RpaWinUiComponents.AdvancedWinUiDataGrid.Controls
{
    /// <summary>
    /// Custom control ktorý umožňuje prepínanie medzi TextBlock a TextBox pre in-place editing
    /// </summary>
    public sealed class EditableTextBlock : Control
    {
        private TextBlock? _textBlock;
        private TextBox? _textBox;
        private Border? _rootBorder;

        #region Dependency Properties

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(EditableTextBlock),
                new PropertyMetadata(string.Empty, OnTextChanged));

        public static readonly DependencyProperty IsEditingProperty =
            DependencyProperty.Register(
                nameof(IsEditing),
                typeof(bool),
                typeof(EditableTextBlock),
                new PropertyMetadata(false, OnIsEditingChanged));

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(
                nameof(IsReadOnly),
                typeof(bool),
                typeof(EditableTextBlock),
                new PropertyMetadata(false));

        public static readonly DependencyProperty HasValidationErrorProperty =
            DependencyProperty.Register(
                nameof(HasValidationError),
                typeof(bool),
                typeof(EditableTextBlock),
                new PropertyMetadata(false, OnValidationErrorChanged));

        public static readonly DependencyProperty ValidationErrorsTextProperty =
            DependencyProperty.Register(
                nameof(ValidationErrorsText),
                typeof(string),
                typeof(EditableTextBlock),
                new PropertyMetadata(string.Empty));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public bool IsEditing
        {
            get => (bool)GetValue(IsEditingProperty);
            set => SetValue(IsEditingProperty, value);
        }

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public bool HasValidationError
        {
            get => (bool)GetValue(HasValidationErrorProperty);
            set => SetValue(HasValidationErrorProperty, value);
        }

        public string ValidationErrorsText
        {
            get => (string)GetValue(ValidationErrorsTextProperty);
            set => SetValue(ValidationErrorsTextProperty, value);
        }

        #endregion

        #region Events

        public event EventHandler<string>? TextChanged;
        public event EventHandler? EditingStarted;
        public event EventHandler? EditingCompleted;
        public event EventHandler? EditingCancelled;

        #endregion

        public EditableTextBlock()
        {
            DefaultStyleKey = typeof(EditableTextBlock);

            // Handle pointer events for starting edit mode
            PointerPressed += OnPointerPressed;
            DoubleTapped += OnDoubleTapped;
            KeyDown += OnKeyDown;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Get template parts
            _textBlock = GetTemplateChild("PART_TextBlock") as TextBlock;
            _textBox = GetTemplateChild("PART_TextBox") as TextBox;
            _rootBorder = GetTemplateChild("PART_RootBorder") as Border;

            // Subscribe to TextBox events
            if (_textBox != null)
            {
                _textBox.LostFocus += OnTextBoxLostFocus;
                _textBox.KeyDown += OnTextBoxKeyDown;
                _textBox.TextChanged += OnTextBoxTextChanged;
            }

            UpdateVisualState();
            UpdateValidationVisualState();
        }

        #region Event Handlers

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!IsReadOnly && !IsEditing)
            {
                // Single click to focus, but don't start editing yet
                Focus(FocusState.Programmatic);
            }
        }

        private void OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (!IsReadOnly && !IsEditing)
            {
                StartEditing();
                e.Handled = true;
            }
        }

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (!IsReadOnly && !IsEditing)
            {
                switch (e.Key)
                {
                    case VirtualKey.F2:
                    case VirtualKey.Enter:
                    case VirtualKey.Space:
                        StartEditing();
                        e.Handled = true;
                        break;
                }
            }
        }

        private void OnTextBoxKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (_textBox == null) return;

            switch (e.Key)
            {
                case VirtualKey.Enter:
                    if (!Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift)
                        .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
                    {
                        CommitEdit();
                        e.Handled = true;
                    }
                    break;
                case VirtualKey.Escape:
                    CancelEdit();
                    e.Handled = true;
                    break;
                case VirtualKey.Tab:
                    CommitEdit();
                    // Don't handle Tab - let it navigate normally
                    break;
            }
        }

        private void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (IsEditing)
            {
                CommitEdit();
            }
        }

        private void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_textBox != null && IsEditing)
            {
                // Update Text property in real-time during editing
                SetValue(TextProperty, _textBox.Text);
            }
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EditableTextBlock control)
            {
                control.UpdateTextDisplays();
                control.TextChanged?.Invoke(control, e.NewValue?.ToString() ?? string.Empty);
            }
        }

        private static void OnIsEditingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EditableTextBlock control)
            {
                control.UpdateVisualState();

                if ((bool)e.NewValue)
                {
                    control.OnEditingStarted();
                }
                else
                {
                    control.OnEditingCompleted();
                }
            }
        }

        private static void OnValidationErrorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EditableTextBlock control)
            {
                control.UpdateValidationVisualState();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Programaticky začne editáciu
        /// </summary>
        public void StartEditing()
        {
            if (IsReadOnly || IsEditing) return;

            IsEditing = true;
        }

        /// <summary>
        /// Potvrdí zmeny a ukončí editáciu
        /// </summary>
        public void CommitEdit()
        {
            if (!IsEditing) return;

            if (_textBox != null)
            {
                Text = _textBox.Text;
            }

            IsEditing = false;
        }

        /// <summary>
        /// Zruší zmeny a ukončí editáciu
        /// </summary>
        public void CancelEdit()
        {
            if (!IsEditing) return;

            // Restore original text
            if (_textBox != null)
            {
                _textBox.Text = Text;
            }

            IsEditing = false;
            EditingCancelled?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Private Methods

        private void UpdateTextDisplays()
        {
            if (_textBlock != null)
            {
                _textBlock.Text = Text ?? string.Empty;
            }

            if (_textBox != null && !IsEditing)
            {
                _textBox.Text = Text ?? string.Empty;
            }
        }

        private void UpdateVisualState()
        {
            if (_textBlock != null)
            {
                _textBlock.Visibility = IsEditing ? Visibility.Collapsed : Visibility.Visible;
            }

            if (_textBox != null)
            {
                _textBox.Visibility = IsEditing ? Visibility.Visible : Visibility.Collapsed;

                if (IsEditing)
                {
                    _textBox.Text = Text ?? string.Empty;

                    // Focus and select all text when starting edit
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        _textBox.Focus(FocusState.Programmatic);
                        _textBox.SelectAll();
                    });
                }
            }

            // Update visual states
            VisualStateManager.GoToState(this, IsEditing ? "Editing" : "Normal", true);
            VisualStateManager.GoToState(this, IsReadOnly ? "ReadOnly" : "Editable", true);
        }

        private void UpdateValidationVisualState()
        {
            if (_rootBorder != null)
            {
                if (HasValidationError)
                {
                    _rootBorder.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Red);
                    _rootBorder.BorderThickness = new Thickness(2);
                    _rootBorder.Background = new SolidColorBrush(Microsoft.UI.Colors.MistyRose);

                    // Set tooltip for validation errors
                    if (!string.IsNullOrEmpty(ValidationErrorsText))
                    {
                        ToolTipService.SetToolTip(this, ValidationErrorsText);
                    }
                }
                else
                {
                    _rootBorder.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Gray);
                    _rootBorder.BorderThickness = new Thickness(1);
                    _rootBorder.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);

                    ToolTipService.SetToolTip(this, null);
                }
            }

            VisualStateManager.GoToState(this, HasValidationError ? "ValidationError" : "Valid", true);
        }

        private void OnEditingStarted()
        {
            EditingStarted?.Invoke(this, EventArgs.Empty);
        }

        private void OnEditingCompleted()
        {
            EditingCompleted?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}