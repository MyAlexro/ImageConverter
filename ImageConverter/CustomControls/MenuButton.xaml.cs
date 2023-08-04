﻿using System.Windows.Controls;
using System.Windows.Input;


namespace ImageConverter
{
    /// <summary>
    /// Interaction logic for MenuButton.xaml
    /// </summary>
    public partial class MenuButton : UserControl
    {

        public MenuButton()
        {
            InitializeComponent();
            BackgroundRect1.Fill = ThemeManager.SolidColorBrushOfSelectedThemeMode();
            BackgroundRect2.Fill = ThemeManager.SolidColorBrushOfSelectedThemeMode();
            rect1.Fill = ThemeManager.SolidColorBrushOfSelectedThemeColor();
            rect2.Fill = ThemeManager.SolidColorBrushOfSelectedThemeColor();
            rect3.Fill = ThemeManager.SolidColorBrushOfSelectedThemeColor(); //applies the color of the selected theme
        }

        private void StackPanel_MouseEnter(object sender, MouseEventArgs e) //if the mouse goes over the button, the button gets darkes
        {
            rect1.Fill = ThemeManager.SelectedThemeHoveringColor();
            rect2.Fill = ThemeManager.SelectedThemeHoveringColor();
            rect3.Fill = ThemeManager.SelectedThemeHoveringColor();
        }

        private void StackPanel_MouseLeave(object sender, MouseEventArgs e) //if it leaves the button it returns to the same color
        {
            rect1.Fill = ThemeManager.SolidColorBrushOfSelectedThemeColor();
            rect2.Fill = ThemeManager.SolidColorBrushOfSelectedThemeColor();
            rect3.Fill = ThemeManager.SolidColorBrushOfSelectedThemeColor();
        }
    }
}
