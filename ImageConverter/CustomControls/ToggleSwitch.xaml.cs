using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace ImageConverter.CustomControls
{
    /// <summary>
    /// Interaction logic for ToggleSwitch.xaml
    /// </summary>
    public partial class ToggleSwitch : UserControl
    {
        /// <summary>
        /// Event fired when the state of the toggle switch changes
        /// </summary>
        /// <param name="e"></param>
        public event EventHandler StateChanged;

        private ThicknessAnimation pillAnimation;
        private DoubleAnimation toggledBackgroundAnimation;
        private Storyboard storyboard;

        private readonly Thickness untoggledPillThickness = new Thickness(1, 2, 0, 2);
        private readonly Thickness toggledPillThickness = new Thickness(58, 2, 0, 2);

        private readonly double hiddenToggledBackgroundWidth = 56.00;
        private readonly double shownToggledBackgroundWidth = 100.00;

        private readonly double untoggledShadowDir = 0.00;
        private readonly double toggledShadowDir = 180.00;

        private readonly Duration animationDuration = new Duration(TimeSpan.FromMilliseconds(280));
        private readonly float animationAcceleration = 0.9f;

        private bool _isToggled = false;
        public bool IsToggled
        {
            get
            {
                return _isToggled;
            }
            set
            {
                _isToggled = value;
                UpdateSwitch();
            }
        }

        public ToggleSwitch()
        {
            InitializeComponent();
            if (_isToggled == false)
            {
                Pill.Margin = untoggledPillThickness;
                ShadowEffect.Direction = untoggledShadowDir;
                ToggledBackground.Width = hiddenToggledBackgroundWidth;
            }
        }

        private void Pill_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsToggled = !IsToggled;
            OnStateChanged(EventArgs.Empty);
        }

        private void UpdateSwitch()
        {
            storyboard = new Storyboard();

            //If the new status is toggled, move the pill from the untoggled to the toggled state
            if (_isToggled)
            {
                pillAnimation = new ThicknessAnimation()
                {
                    From = untoggledPillThickness,
                    To = toggledPillThickness,
                    Duration = animationDuration,
                    AccelerationRatio = animationAcceleration,
                };

                toggledBackgroundAnimation = new DoubleAnimation()
                {
                    From = hiddenToggledBackgroundWidth,
                    To = shownToggledBackgroundWidth,
                    Duration = animationDuration,
                    AccelerationRatio = animationAcceleration,
                };
            }
            //If the new status is UNtoggled, move the pill from the toggled to the UNtoggled state
            else
            {
                pillAnimation = new ThicknessAnimation()
                {
                    From = toggledPillThickness,
                    To = untoggledPillThickness,
                    Duration = animationDuration,
                    AccelerationRatio = animationAcceleration,
                };

                toggledBackgroundAnimation = new DoubleAnimation()
                {
                    From = shownToggledBackgroundWidth,
                    To = hiddenToggledBackgroundWidth,
                    Duration = animationDuration,
                    AccelerationRatio = animationAcceleration,
                };
            }
            storyboard.Children.Add(pillAnimation);
            Storyboard.SetTargetProperty(pillAnimation, new PropertyPath("Margin"));
            Storyboard.SetTarget(pillAnimation, Pill);

            storyboard.Children.Add(toggledBackgroundAnimation);
            Storyboard.SetTargetProperty(toggledBackgroundAnimation, new PropertyPath("Width"));
            Storyboard.SetTarget(toggledBackgroundAnimation, ToggledBackground);

            storyboard.Completed += delegate
            {
                if (_isToggled)
                    ShadowEffect.Direction = toggledShadowDir;
                else
                    ShadowEffect.Direction = untoggledShadowDir;
            };
            storyboard.Begin();
        }

        /// <summary>
        /// Event fired when the state of the toggle switch changes
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnStateChanged(EventArgs e)
        {
            StateChanged?.Invoke(this, e);
        }
    }
}
