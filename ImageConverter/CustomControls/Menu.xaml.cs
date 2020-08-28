using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ImageConverter.Properties;
using ImageConverter.Classes;

namespace ImageConverter
{
    /// <summary>
    /// Code that makes the Menu work
    /// The rects have the name of the color at which they correspond
    /// </summary>
    public partial class Menu : UserControl
    {
        private Storyboard storyboard;
        private ThicknessAnimation thicknessAnimation;
        private FrameworkElement nameOfMenu;
        /// <summary>
        /// Position of the menu when it's closed
        /// </summary>
        private Thickness closedPos = new Thickness(-262, 0, 0, 0);
        /// <summary>
        /// Position of the menu when it's opened
        /// </summary>
        private Thickness openedPos = new Thickness(0, 0, 0, 0);
        /// <summary>
        /// Width of the border of the rect when it's selected or the mouse is over it
        /// </summary>
        private int selectedRectStrokeThickness = 3;
        /// <summary>
        /// Width of the border of the rect when it's not selected (normal state)
        /// </summary>
        private int unselectedRectStrokeThickness = 1;
        /// <summary>
        /// List of labels in the Options stackpanel
        /// </summary>
        List<Label> labels = new List<Label>();

        public Menu()
        {
            InitializeComponent();

            #region Apply theme mode and color
            MenuSP.Background = ThemeManager.SolidColorBrushOfSelectedThemeMode();
            SettingsLabel.Foreground = ThemeManager.SolidColorBrushOfSelectedThemeColor();
            labels = UtilityMethods.FindLabelsInStackPanel(MenuSP);
            //If the selected ThemeMode is DArkTheme the ThemeColor will be applied to the text of all the labels and textblocks
            if (Settings.Default.ThemeMode == "DarkTheme")
            {
                foreach (Label label in UtilityMethods.FindLabelsInStackPanel(MenuSP))
                {
                    label.Foreground = ThemeManager.SolidColorBrushOfSelectedThemeColor();
                }
            }
            #endregion
            #region Apply translations
            if (Settings.Default.Language == "it")
            {
                SettingsLabel.Content = LanguageManager.IT_SettingsLabelText;
                ThemeColorTextBlock.Text = LanguageManager.IT_ThemeColorTextBlockText;
                ThemeTextBlock.Text = LanguageManager.IT_ThemeTextBlockText;
                CreditsLabel.Content = LanguageManager.IT_CreditsLabelText;
                LanguageComboBox.SelectedIndex = Array.IndexOf(LanguageManager.languages, "it");
                LanguageOptionTextBlock.Text = LanguageManager.IT_LanguageLabelText;
                SaveBothImagesTextBlock.Text = LanguageManager.IT_SaveBothImagesTextBlockText;
            }
            else if (Settings.Default.Language == "en")
            {
                SettingsLabel.Content = LanguageManager.EN_SettingsLabelText;
                ThemeColorTextBlock.Text = LanguageManager.EN_ThemeColorTextBlockText;
                ThemeTextBlock.Text = LanguageManager.EN_ThemeTextBlockText;
                CreditsLabel.Content = LanguageManager.EN_CreditsLabelText;
                LanguageComboBox.SelectedIndex = Array.IndexOf(LanguageManager.languages, "en");
                LanguageOptionTextBlock.Text = LanguageManager.EN_LanguageLabelText;
                SaveBothImagesTextBlock.Text = LanguageManager.EN_SaveBothImagesTextBlockText;
            }
            #endregion

            SaveBothImagesToggleSwitch.IsToggled = Settings.Default.SaveBothImages;
        }

        /// <summary>
        /// Opens the menu by moving it into the visible window and load settings
        /// <para>Parameter: "menuElement" Menu element used in the MainWindow</para>
        /// </summary>
        /// <param name="menuElement"> Menu element used in the MainWindow</param>
        public void OpenMenu(FrameworkElement menuElement)
        {
            nameOfMenu = menuElement; //Name of the menu that you want to move
            storyboard = new Storyboard();
            thicknessAnimation = new ThicknessAnimation() //Set animation properties
            {
                From = closedPos,
                To = openedPos,
                DecelerationRatio = 0.5f,
                Duration = new Duration(TimeSpan.FromMilliseconds(600)),
            };

            foreach (Rectangle rect in ThemeColorsSP.Children) //Sets the width of the border of the rects in ThemeColorSP
            {
                if (Settings.Default.ThemeColor == rect.Name) //If the name of the rect is the same as the chosen theme color
                {
                    rect.StrokeThickness = selectedRectStrokeThickness; //Set the chosen-rect width border
                }
            }
            foreach (Rectangle rect in ThemeModesSP.Children) //Sets the width of the border of the rects in ThemeModeSP
            {
                if (Settings.Default.ThemeMode == rect.Name) //If the name of the rect is the same as the chosen theme type
                {
                    rect.StrokeThickness = selectedRectStrokeThickness; //Set the chosen-rect width border
                }
            }

            storyboard.Children.Add(thicknessAnimation);
            Storyboard.SetTargetProperty(thicknessAnimation, new PropertyPath(Grid.MarginProperty));
            Storyboard.SetTarget(thicknessAnimation, menuElement);
            storyboard.Begin(); //Start animation of opening
        }

        /// <summary>
        /// Closes the menu by moving it outside of the window
        /// <para>Parameter: "menuElement" Menu element used in the MainWindow </para>
        /// </summary>
        /// <param name="menuElement"> Menu element used in the MainWindow</param>
        public void CloseMenu(DependencyObject menuElement)
        {
            storyboard = new Storyboard();
            thicknessAnimation = new ThicknessAnimation()
            {
                From = nameOfMenu.Margin,
                To = closedPos,
                DecelerationRatio = 0.5f,
                Duration = new Duration(TimeSpan.FromMilliseconds(600)),
            };

            storyboard.Children.Add(thicknessAnimation);
            Storyboard.SetTargetProperty(thicknessAnimation, new PropertyPath(Grid.MarginProperty));
            Storyboard.SetTarget(thicknessAnimation, menuElement);
            storyboard.Begin();
        }

        private void CloseMenuBttn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ThemeManager.solidColorBrush = new SolidColorBrush()//If the mouse gets over the bttn to close the menu
            {
                Color = Color.FromArgb(255, 0, 0, 0), //Darken it 
            };
            CloseMenuBttn.Foreground = ThemeManager.solidColorBrush;
        }

        private void CloseMenuBttn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ThemeManager.solidColorBrush = new SolidColorBrush() //If the mouse gets out the bttn to close the menu
            {
                Color = Color.FromArgb(255, 202, 204, 207), //Set normal color
            };
            CloseMenuBttn.Foreground = ThemeManager.solidColorBrush;
        }

        private void CloseMenuBttn_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CloseMenu(nameOfMenu); //Close the menu
        }

        /// <summary>
        /// When the user hovers the mouse over any rectangle(theme mode or color)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Rectangles_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            (sender as Rectangle).StrokeThickness = selectedRectStrokeThickness; //Sets the width of the rect, over which the mouse is, the selected-rect border width
        }

        /// <summary>
        /// When the user's mouse leaves the area over any rectangle(theme mode or color)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Rectangles_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if ((sender as Rectangle).Name != Settings.Default.ThemeColor && (sender as Rectangle).Name != Settings.Default.ThemeMode) //If the mouse gets out the theme-select rects(unless it's the already chosen theme type)
            {
                (sender as Rectangle).StrokeThickness = unselectedRectStrokeThickness; //Set normal border width
            }
        }

        /// <summary>
        /// When the user click the rectangles that represent the theme COLORS
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ThemeColRects_MouseDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (((FrameworkElement)sender).Name == Settings.Default.ThemeColor)
                return;

            (sender as Rectangle).StrokeThickness = selectedRectStrokeThickness; //If a rect gets clicked set its border width as the selected one
            Settings.Default.ThemeColor = ((FrameworkElement)sender).Name; //Set the name of the rect as the option of the theme color
            Settings.Default.Save();
            Settings.Default.Reload();
            foreach (Rectangle rect in ThemeColorsSP.Children) //Set to all the other rects a non selected-rect border width
            {
                if (rect.Name != Settings.Default.ThemeColor)
                {
                    rect.StrokeThickness = unselectedRectStrokeThickness;
                }
            }
            MessageBoxResult response = MessageBoxResult.No;
            if (Settings.Default.Language == "it")
            {
                response = MessageBox.Show(LanguageManager.IT_ApplyThemeColorMsgBox, LanguageManager.IT_ApplyThemeColorMsgBoxCaption, MessageBoxButton.YesNo, MessageBoxImage.Information);
            }
            else if (Settings.Default.Language == "en")
            {
                response = MessageBox.Show(LanguageManager.EN_ApplyThemeColorMsgBox, LanguageManager.EN_ApplyThemeColorMsgBoxCaption, MessageBoxButton.YesNo, MessageBoxImage.Information);
            }

            if (response == MessageBoxResult.No)
            {
                return;
            }
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location); //Restart the application
            Application.Current.Shutdown(0);
        }

        /// <summary>
        /// When the user click the rectangles that represent the theme TYPES
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ThemeMode_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (((FrameworkElement)sender).Name == Settings.Default.ThemeMode)
                return;
            (sender as Rectangle).StrokeThickness = selectedRectStrokeThickness; //If a rect gets clicked set its border width as the selected one

            Settings.Default.ThemeMode = ((Rectangle)sender).Name;
            if (((Rectangle)sender).Name == "LightTheme" && Settings.Default.ThemeColor == "WhiteThemeColor") //If the selected theme mode is Light but the theme color is White
            {
                Settings.Default.ThemeColor = "DefaultThemeColor"; //Set the theme color to default to prevent readability issues
            }
            if (((Rectangle)sender).Name == "DarkTheme" && Settings.Default.ThemeColor != "WhiteThemeColor") //If the user selectes the dark theme and the theme color isn't already white
            {
                Settings.Default.ThemeColor = "WhiteThemeColor";
            }
            Settings.Default.Save();
            Settings.Default.Reload();

            //Set to all the other rects a non selected-rect border width
            foreach (Rectangle rect in ThemeModesSP.Children)
            {
                if (rect.Name != Settings.Default.ThemeMode)
                {
                    rect.StrokeThickness = unselectedRectStrokeThickness;
                }
            }
            MessageBoxResult response = MessageBoxResult.No;
            if (Settings.Default.Language == "it")
            {
                response = MessageBox.Show(LanguageManager.IT_ApplyThemeModeMsgBox, LanguageManager.IT_ApplyThemeModeMsgBoxCaption, MessageBoxButton.YesNo, MessageBoxImage.Information);
            }
            else if (Settings.Default.Language == "en")
            {
                response = MessageBox.Show(LanguageManager.EN_ApplyThemeModeMsgBox, LanguageManager.EN_ApplyThemeModeMsgBoxCaption, MessageBoxButton.YesNo, MessageBoxImage.Information);
            }
            if (response == MessageBoxResult.No)
            {
                return;
            }
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location); //Restart the application
            Application.Current.Shutdown(0);
        }

        private void LanguageComboBox_DropDownClosed(object sender, EventArgs e)
        {
            var previousLanguage = Settings.Default.Language;
            //If the user closes the dropdown menu without changing option
            if ((LanguageComboBox.SelectedItem as Label).Content.ToString().ToLower() == previousLanguage)
            {
                return;
            }

            Settings.Default.Language = (LanguageComboBox.SelectedItem as Label).Content.ToString().ToLower();
            Settings.Default.Save();
            Settings.Default.Reload();
            var response = MessageBoxResult.Yes;
            if (previousLanguage == "it")
            {
                response = MessageBox.Show(LanguageManager.IT_ApplyLanguageMsgBox, LanguageManager.IT_ApplyLanguageMsgBoxCaption, MessageBoxButton.YesNo, MessageBoxImage.Information);
            }
            else if (previousLanguage == "en")
            {
                response = MessageBox.Show(LanguageManager.EN_ApplyLanguageMsgBox, LanguageManager.EN_ApplyLanguageMsgBoxCaption, MessageBoxButton.YesNo, MessageBoxImage.Information);
            }
            if (response == MessageBoxResult.Yes)
            {
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location); //Restart the application
                Application.Current.Shutdown(0);
            }
        }

        private void ToggleSwitch_StateChanged(object sender, EventArgs e)
        {
            Settings.Default.SaveBothImages = SaveBothImagesToggleSwitch.IsToggled;
            Settings.Default.Save();
            Settings.Default.Reload();
        }
    }
}
