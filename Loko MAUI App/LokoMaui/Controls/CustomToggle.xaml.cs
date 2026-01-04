using loko.Helpers.Extensions;
using loko.Models;
using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace loko.Controls
{
    public partial class CustomToggle
    {
        private static readonly Color DefaultSwitchOffColor =
            (Color)Application.Current.Resources["SilverChaliceColor"];

        private static readonly Color DefaultSwitchOnColor =
            (Color)Application.Current.Resources["GreenTealColor"];

        private static readonly Color DefaultSwitchingOnColor =
            (Color)Application.Current.Resources["SilverChaliceColorTransparent40"];

        private static readonly Color DefaultSwitchingOffColor =
            (Color)Application.Current.Resources["GreenTealColorTransparent40"];

        public static readonly BindableProperty SwitchedOnProperty =
            BindableProperty.Create(nameof(SwitchedOn), typeof(bool), typeof(CustomToggle),
                false, propertyChanged: OnSwitchedChanged);

        public static readonly BindableProperty SwitchingOnProperty =
            BindableProperty.Create(nameof(SwitchingOn), typeof(bool), typeof(CustomToggle),
                false, propertyChanged: OnSwitchingOnChanged, defaultBindingMode: BindingMode.TwoWay);

        public static readonly BindableProperty SwitchingOffProperty =
            BindableProperty.Create(nameof(SwitchingOff), typeof(bool), typeof(CustomToggle),
                false, propertyChanged: OnSwitchingOffChanged, defaultBindingMode: BindingMode.TwoWay);

        public static readonly BindableProperty SwitchingOnCommandProperty =
            BindableProperty.Create(nameof(SwitchingOnCommand), typeof(ICommand),
                typeof(CustomToggle));

        public static readonly BindableProperty SwitchingOffCommandProperty =
            BindableProperty.Create(nameof(SwitchingOffCommand), typeof(ICommand),
                typeof(CustomToggle));

        public CustomToggle()
        {
            InitializeComponent();
        }

        public bool SwitchedOn
        {
            get => (bool)GetValue(SwitchedOnProperty);
            set => SetValue(SwitchedOnProperty, value);
        }

        public bool SwitchingOn
        {
            get => (bool)GetValue(SwitchingOnProperty);
            set => SetValue(SwitchingOnProperty, value);
        }

        public bool SwitchingOff
        {
            get => (bool)GetValue(SwitchingOffProperty);
            set => SetValue(SwitchingOffProperty, value);
        }

        public ICommand SwitchingOnCommand
        {
            get => (ICommand)GetValue(SwitchingOnCommandProperty);
            set => SetValue(SwitchingOnCommandProperty, value);
        }

        public ICommand SwitchingOffCommand
        {
            get => (ICommand)GetValue(SwitchingOffCommandProperty);
            set => SetValue(SwitchingOffCommandProperty, value);
        }

        private static void OnSwitchedChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!(bindable is CustomToggle toggle))
                return;

            if (newValue is bool switched) toggle.SetToggleStatus(switched);
        }

        private static void OnSwitchingOnChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!(bindable is CustomToggle toggle))
                return;

            if (!(newValue is bool switchingOn))
                return;

            if (switchingOn)
                toggle.ToggleArea.ColorTo(DefaultSwitchOffColor, DefaultSwitchingOnColor, color =>
                    toggle.ToggleArea.BackgroundColor = color, 100, Easing.Linear);
            else if (!toggle.SwitchedOn)
                toggle.ToggleArea.ColorTo(DefaultSwitchingOnColor, DefaultSwitchOffColor, color =>
                    toggle.ToggleArea.BackgroundColor = color, 100, Easing.Linear);
        }

        private static void OnSwitchingOffChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!(bindable is CustomToggle toggle))
                return;

            if (!(newValue is bool switchingOff))
                return;

            if (switchingOff)
                toggle.ToggleArea.ColorTo(DefaultSwitchOnColor, DefaultSwitchingOffColor, color =>
                    toggle.ToggleArea.BackgroundColor = color, 100, Easing.Linear);
            else if (toggle.SwitchedOn)
                toggle.ToggleArea.ColorTo(DefaultSwitchingOffColor, DefaultSwitchOnColor, color =>
                    toggle.ToggleArea.BackgroundColor = color, 100, Easing.Linear);
        }

        private void SetToggleStatus(bool switchedOn)
        {
            switch (switchedOn)
            {
                case true:
                    ToggleArea.ColorTo(DefaultSwitchingOnColor, DefaultSwitchOnColor, color =>
                        ToggleArea.BackgroundColor = color, 250, Easing.Linear);
                    ToggleStick.TranslateTo(22, 0, 250, Easing.SinOut);
                    break;
                case false:
                    ToggleArea.ColorTo(DefaultSwitchingOffColor, DefaultSwitchOffColor, color =>
                        ToggleArea.BackgroundColor = color, 250, Easing.Linear);
                    ToggleStick.TranslateTo(0, 0, 250, Easing.SinOut);
                    break;
            }
        }

        private void OnToggleTapped(object sender, EventArgs e)
        {
            if (SwitchingOn || SwitchingOff)
                return;

            if (!(BindingContext is BLEDevice availableDevice))
                return;

            if (SwitchedOn)
                SwitchingOffCommand?.Execute(availableDevice);
            else
                SwitchingOnCommand?.Execute(availableDevice);
        }
    }
}