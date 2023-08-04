using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ImageConverter.Properties;

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
            MenuSP.Background = ThemeManager.SelectedThemeMode();
            SettingsLabel.Foreground = ThemeManager.SelectedThemeColor();
            labels = FindLabels(MenuSP);
            foreach (var label in labels)
            {
                label.Foreground = ThemeManager.SelectedThemeColor();
            }
            #endregion
            #region Apply translations
            if (Settings.Default.Language == "it")
            {
                SettingsLabel.Content = LanguageManager.IT_SettingsLabelTxt;
                ThemeColorLabel.Content = LanguageManager.IT_ThemeColorLabelTxT;
                ThemeLabel.Content = LanguageManager.IT_ThemeLabelTxt;
                CreditsLabel.Content = LanguageManager.IT_CreditsLabelTxt;
                LanguageComboBox.SelectedIndex = Array.IndexOf(LanguageManager.languages, "it");
                LanguageOptionLabel.Content = LanguageManager.IT_LanguageLabelTxt;
            }
            else if (Settings.Default.Language == "en")
            {
                SettingsLabel.Content = LanguageManager.EN_SettingsLabelTxt;
                ThemeColorLabel.Content = LanguageManager.EN_ThemeColorLabelTxT;
                ThemeLabel.Content = LanguageManager.EN_ThemeLabelTxt;
                CreditsLabel.Content = LanguageManager.EN_CreditsLabelTxt;
                LanguageComboBox.SelectedIndex = Array.IndexOf(LanguageManager.languages, "en");
                LanguageOptionLabel.Content = LanguageManager.EN_LanguageLabelTxt;
            }
            #endregion
        }

        /// <summary>
        /// Opens the menu by moving it into the visible window
        /// <para>Parameter: "menuElement" Menu element used in the MainWindow</para>
        /// </summary>
        /// <param name="menuElement"> Menu element used in the MainWindow</param>
        public void OpenMenu(FrameworkElement menuElement)
        {
            nameOfMenu = menuElement; //name of the menu that you want to move
            storyboard = new Storyboard();
            thicknessAnimation = new ThicknessAnimation() //set animation properties
            {
                From = closedPos,
                To = openedPos,
                DecelerationRatio = 0.5f,
                Duration = new Duration(TimeSpan.FromMilliseconds(600)),
            };

            foreach (Rectangle rect in ThemeColorsSP.Children) //sets the width of the border of the rects in ThemeColorSP
            {
                if (Settings.Default.ThemeColor == rect.Name) //if the name of the rect is the same as the chosen theme color
                {
                    rect.StrokeThickness = selectedRectStrokeThickness; //set the chosen-rect width border
                }
            }
            foreach (Rectangle rect in ThemeModesSP.Children) //sets the width of the border of the rects in ThemeModeSP
            {
                if (Settings.Default.ThemeMode == rect.Name) //if the name of the rect is the same as the chosen theme type
                {
                    rect.StrokeThickness = selectedRectStrokeThickness; //set the chosen-rect width border
                }
            }
            storyboard.Children.Add(thicknessAnimation);
            Storyboard.SetTargetProperty(thicknessAnimation, new PropertyPath(Grid.MarginProperty));
            Storyboard.SetTarget(thicknessAnimation, menuElement);
            storyboard.Begin(); //start animation of opening
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
            ThemeManager.solidColorBrush = new SolidColorBrush() //if the mouse gets over the bttn to close the menu
            {
                Color = Color.FromArgb(255, 0, 0, 0), //darken it 
            };
            CloseMenuBttn.Foreground = ThemeManager.solidColorBrush;
        }

        private void CloseMenuBttn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ThemeManager.solidColorBrush = new SolidColorBrush() //if the mouse gets out the bttn to close the menu
            {
                Color = Color.FromArgb(255, 202, 204, 207), //set normal color
            };
            CloseMenuBttn.Foreground = ThemeManager.solidColorBrush;
        }

        private void CloseMenuBttn_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CloseMenu(nameOfMenu); //close the menu
        }

        /// <summary>
        /// When the user hovers the mouse over any rectangle(theme mode or color)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Rectangles_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            (sender as Rectangle).StrokeThickness = selectedRectStrokeThickness; //sets the width of the rect, over which the mouse is, the selected-rect border width
        }

        /// <summary>
        /// When the user's mouse leaves the area over any rectangle(theme mode or color)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Rectangles_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if ((sender as Rectangle).Name != Settings.Default.ThemeColor && (sender as Rectangle).Name != Settings.Default.ThemeMode) //if the mouse gets out the theme-select rects(unless it's the already chosen theme type)
            {
                (sender as Rectangle).StrokeThickness = unselectedRectStrokeThickness; //set normal border width
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

            (sender as Rectangle).StrokeThickness = selectedRectStrokeThickness; //if a rect gets clicked set its border width as the selected one
            Settings.Default.ThemeColor = ((FrameworkElement)sender).Name; //set the name of the rect as the option of the theme color
            Settings.Default.Save();
            Settings.Default.Reload();
            foreach(Rectangle rect in ThemeColorsSP.Children) //set to all the other rects a non selected-rect border width
            {
                if (rect.Name != Settings.Default.ThemeColor)
                {
                    rect.StrokeThickness = unselectedRectStrokeThickness;
                }
            }
            MessageBoxResult response = MessageBoxResult.No;
            if (Settings.Default.Language == "it")
            {
                response = MessageBox.Show(LanguageManager.IT_ApplyThemeColorMsgBox,"Cambia colore tema",MessageBoxButton.YesNo,MessageBoxImage.Information);
            }
            else if (Settings.Default.Language == "en")
            {
                response = MessageBox.Show(LanguageManager.EN_ApplyThemeColorMsgBox, "Change theme color", MessageBoxButton.YesNo, MessageBoxImage.Information);
            }

            if (response == MessageBoxResult.No)
            {
                return;
            }
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location); //restart the application
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
            (sender as Rectangle).StrokeThickness = selectedRectStrokeThickness; //if a rect gets clicked set its border width as the selected one

            Settings.Default.ThemeMode = ((Rectangle)sender).Name;
            if (((Rectangle)sender).Name == "LightTheme" && Settings.Default.ThemeColor == "WhiteThemeColor") //if the selected theme mode is Light but the theme color is White
            {
                Settings.Default.ThemeColor = "DefaultThemeColor"; //set the theme color to default to prevent readability issues
            }
            if(((Rectangle)sender).Name == "DarkTheme" && Settings.Default.ThemeColor != "WhiteThemeColor") //If the user selectes the dark theme and the theme color isn't already white
            {
                Settings.Default.ThemeColor = "WhiteThemeColor";
            }
            Settings.Default.Save();
            Settings.Default.Reload();

            // set to all the other rects a non selected-rect border width
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
                response = MessageBox.Show(LanguageManager.IT_ApplyThemeMsgBox, "Cambia tema", MessageBoxButton.YesNo, MessageBoxImage.Information);
            }
            else if (Settings.Default.Language == "en")
            {
                response = MessageBox.Show(LanguageManager.EN_ApplyThemeMsgBox, "Change theme", MessageBoxButton.YesNo, MessageBoxImage.Information);
            }
            if (response == MessageBoxResult.No)
            {
                return;
            }
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location); //restart the application
            Application.Current.Shutdown(0);
        }

        private void LanguageComboBox_DropDownClosed(object sender, EventArgs e)
        {
            var previousLanguage = Settings.Default.Language;
            if((LanguageComboBox.SelectedItem as Label).Content.ToString().ToLower() == previousLanguage) //if the user closes the dropdown menu without changing option
            {
                return;
            }
            Settings.Default.Language = (LanguageComboBox.SelectedItem as Label).Content.ToString().ToLower();
            Settings.Default.Save();
            Settings.Default.Reload();
            var response = MessageBoxResult.Yes;
            if (previousLanguage == "it")
            {
                response = MessageBox.Show(LanguageManager.IT_ApplyLanguageMsgBox, "Applica lingua", MessageBoxButton.YesNo, MessageBoxImage.Information);
            }
            else if (previousLanguage == "en")
            {
                response = MessageBox.Show(LanguageManager.EN_ApplyLanguageMsgBox, "Apply language", MessageBoxButton.YesNo, MessageBoxImage.Information);
            }
            if (response == MessageBoxResult.Yes)
            {
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location); //restart the application
                Application.Current.Shutdown(0);
            }
        }

        /// <summary>
        /// Finds all labels in a stackpanel
        /// </summary>
        /// <param name="stackpanel"></param>
        /// <returns>Returns a list containing all the labels</returns>
        private List<Label> FindLabels(StackPanel stackpanel)
        {
            foreach (var control in stackpanel.Children)
            {
                if (control == null)
                    break;

                if (control.GetType() == typeof(StackPanel))
                {
                    FindLabels((StackPanel)control);
                }
                else if (control.GetType() == typeof(Label))
                {
                    labels.Add(control as Label);
                }
            }
            return labels;
        }
    }
}
