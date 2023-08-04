using System;
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
        private Thickness closedPos = new Thickness(-262, 0, 0, 0); //position of the menu when it's closed
        private Thickness openedPos = new Thickness(0, 0, 0, 0);  //position of the menu when it's opened
        private int selectedRectStrokeThickness = 3; //width of the border of the rect when it's selected or the mouse is over it
        private int unselectedRectStrokeThickness = 1; //width of the border of the rect when it's not selected (normal state)

        public Menu()
        {
            InitializeComponent();
            MenuSP.Background = ThemeManager.SelectedThemeType();//apply the chosen theme when initialized
            SettingsLabel.Foreground = ThemeManager.SelectedFontColor();
            if (Settings.Default.Language == "it")
            {
                SettingsLabel.Content = LanguageManager.IT_SettingsLabelTxt;
                FontColorLabel.Content = LanguageManager.IT_FontColorLabelTxT;
                ThemeLabel.Content = LanguageManager.IT_ThemeLabelTxt;
                CreditsLabel.Content = LanguageManager.IT_CreditsLabelTxt;
                LanguageComboBox.SelectedIndex = Array.IndexOf(LanguageManager.languages, "it");
                LanguageOptionLabel.Content = LanguageManager.IT_LanguageLabelTxt;
            }
            else if (Settings.Default.Language == "en")
            {
                SettingsLabel.Content = LanguageManager.EN_SettingsLabelTxt;
                FontColorLabel.Content = LanguageManager.EN_FontColorLabelTxT;
                ThemeLabel.Content = LanguageManager.EN_ThemeLabelTxt;
                CreditsLabel.Content = LanguageManager.EN_CreditsLabelTxt;
                LanguageComboBox.SelectedIndex = Array.IndexOf(LanguageManager.languages, "en");
                LanguageOptionLabel.Content = LanguageManager.EN_LanguageLabelTxt;
            }
        }

        public void OpenMenu(FrameworkElement nameOfMenuRect)
        {
            nameOfMenu = nameOfMenuRect; //name of the menu that you want to move
            storyboard = new Storyboard();
            thicknessAnimation = new ThicknessAnimation() //set animation properties
            {
                From = closedPos,
                To = openedPos,
                DecelerationRatio = 0.5f,
                Duration = new Duration(TimeSpan.FromMilliseconds(600)),
            };

            foreach (Rectangle rect in FontColorsSP.Children) //sets the width of the border of the rects in FontcolorSP
            {
                if (Settings.Default.FontCol == rect.Name) //if the name of the rect is the same as the chosen font color
                {
                    rect.StrokeThickness = selectedRectStrokeThickness; //set the chosen-rect width border
                }
            }
            foreach (Rectangle rect in ThemeTypeSP.Children) //sets the width of the border of the rects in ThemeTypeSP
            {
                if (Settings.Default.ThemeType == rect.Name) //if the name of the rect is the same as the chosen theme type
                {
                    rect.StrokeThickness = selectedRectStrokeThickness; //set the chosen-rect width border
                }
            }
            storyboard.Children.Add(thicknessAnimation);
            Storyboard.SetTargetProperty(thicknessAnimation, new PropertyPath(Grid.MarginProperty));
            Storyboard.SetTarget(thicknessAnimation, nameOfMenuRect);
            storyboard.Begin(); //start animation of opening
        }

        public void CloseMenu(DependencyObject nameOfMenuRect)
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
            Storyboard.SetTarget(thicknessAnimation, nameOfMenuRect);
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

        private void ThemeRects_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            (sender as Rectangle).StrokeThickness = selectedRectStrokeThickness; //sets the width of the rect, over which the mouse is, the selected-rect border width
        }

        private void ThemeRects_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if ((sender as Rectangle).Name != Settings.Default.FontCol && (sender as Rectangle).Name != Settings.Default.ThemeType) //if the mouse gets out the theme-select rects(unless it's the already chosen theme type)
            {
                (sender as Rectangle).StrokeThickness = unselectedRectStrokeThickness; //set normal border width
            }
        }

        private void FontColRects_MouseDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (((FrameworkElement)sender).Name == Settings.Default.FontCol)
                return;

            (sender as Rectangle).StrokeThickness = selectedRectStrokeThickness; //if a rect gets clicked set its border width as the selected one
            Settings.Default.FontCol = ((FrameworkElement)sender).Name; //set the name of the rect as the option of the font color
            Settings.Default.Save();
            Settings.Default.Reload();
            foreach(Rectangle rect in FontColorsSP.Children) //set to all the other rects a non selected-rect border width
            {
                if (rect.Name != Settings.Default.FontCol)
                {
                    rect.StrokeThickness = unselectedRectStrokeThickness;
                }
            }
            MessageBoxResult response = MessageBoxResult.No;
            if (Settings.Default.Language == "it")
            {
                response = MessageBox.Show(LanguageManager.IT_ApplyFontColorMsgBox,"Cambia colore font",MessageBoxButton.YesNo,MessageBoxImage.Information);
            }
            else if (Settings.Default.Language == "en")
            {
                response = MessageBox.Show(LanguageManager.EN_ApplyFontColorMsgBox, "Change font color", MessageBoxButton.YesNo, MessageBoxImage.Information);
            }

            if (response == MessageBoxResult.No)
            {
                return;
            }
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location); //restart the application
            Application.Current.Shutdown(0);
        }

        private void ThemeType_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (((FrameworkElement)sender).Name == Settings.Default.ThemeType)
                return;
            (sender as Rectangle).StrokeThickness = selectedRectStrokeThickness; //if a rect gets clicked set its border width as the selected one
            Settings.Default.ThemeType = ((Rectangle)sender).Name; //set the name of the rect as the option of the theme type
            if (((Rectangle)sender).Name == "LightTheme" && Settings.Default.FontCol == "WhiteFontCol") //if the selected theme is light but the font color is white
            {
                Settings.Default.FontCol = "DefaultFontCol"; //set the font color to default to prevent readability issues
            }
            else
            {
                Settings.Default.FontCol = "WhiteFontCol";
            }
            Settings.Default.Save();
            Settings.Default.Reload();

            foreach(Rectangle rect in ThemeTypeSP.Children) // set to all the other rects a non selected-rect border width
            {
                if (rect.Name != Settings.Default.ThemeType)
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
    }
}
