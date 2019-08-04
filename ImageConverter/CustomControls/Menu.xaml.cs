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
    /// I rect hanno il nome del colore del tema a cui corrispondono 
    /// </summary>
    public partial class Menu : UserControl
    {
        private Storyboard storyboard;
        private ThicknessAnimation thicknessAnimation;
        private FrameworkElement nameOfMenu;
        private Thickness closedPos = new Thickness(-262, 0, 0, 0); //posizione del menu quando è chiuso
        private Thickness openedPos = new Thickness(0, 0, 0, 0);  //posizione del menu quando è aperto
        private int selectedRectStrokeThickness = 3; //spessore del bordo del rect quando è selezionato o la freccetta del mouse c'è sopra
        private int unselectedRectStrokeThickness = 1; //spessore del bordo del rect non selezionato (normale)

        public Menu()
        {
            InitializeComponent();
            MenuSP.Background = ThemeManager.SelectedThemeType();//applica il tema quando il menu viene inizializzato
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
            nameOfMenu = nameOfMenuRect; //nome del menu che si vuole spostare
            storyboard = new Storyboard();
            thicknessAnimation = new ThicknessAnimation() //setta le proprietà dell'animazione
            {
                From = closedPos,
                To = openedPos,
                DecelerationRatio = 0.5f,
                Duration = new Duration(TimeSpan.FromMilliseconds(600)),
            };

            foreach (Rectangle rect in ThemeColorsSP.Children) // carica lo spessore del bordo dei rettangoli
            {
                if (Settings.Default.FontCol == rect.Name) //se il nome del rettangolo è uguale al colore del tema selezionato
                {
                    rect.StrokeThickness = selectedRectStrokeThickness; //gli mette il lo spessore da rect selezionato
                }
            }
            foreach (Rectangle rect in ThemeTypeSP.Children) // carica lo spessore del bordo dei rettangoli
            {
                if (Settings.Default.ThemeType == rect.Name) //se il nome del rettangolo è uguale al tipo di tema selezionato
                {
                    rect.StrokeThickness = selectedRectStrokeThickness; //gli mette il lo spessore da rect selezionato
                }
            }
            storyboard.Children.Add(thicknessAnimation);
            Storyboard.SetTargetProperty(thicknessAnimation, new PropertyPath(Grid.MarginProperty));
            Storyboard.SetTarget(thicknessAnimation, nameOfMenuRect);
            storyboard.Begin(); //inizia l'animazione
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
            ThemeManager.solidColorBrush = new SolidColorBrush() //se la freccetta del mouse va sopra il bttn per chiudere il menu
            {
                Color = Color.FromArgb(255, 0, 0, 0), // lo scurisce
            };
            CloseMenuBttn.Foreground = ThemeManager.solidColorBrush;
        }

        private void CloseMenuBttn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ThemeManager.solidColorBrush = new SolidColorBrush() //se la freccetta del mouse si toglie dal bttn per chiudere il menu
            {
                Color = Color.FromArgb(255, 202, 204, 207), //gli mette un colore più chiaro
            };
            CloseMenuBttn.Foreground = ThemeManager.solidColorBrush;
        }

        private void CloseMenuBttn_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CloseMenu(nameOfMenu); //chiude il menu
        }

        private void ThemeRects_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            (sender as Rectangle).StrokeThickness = selectedRectStrokeThickness; //mette al rect, sopra il quale è posizionata la freccetta del mouse, il bordo da rect selezionato 
        }

        private void ThemeRects_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if ((sender as Rectangle).Name != Settings.Default.FontCol) //se il mouse se ne va da sopra i rect per selezionare il colore del menu(a meno che non sia il rect con il nome del tema selezionato)
            {
                (sender as Rectangle).StrokeThickness = unselectedRectStrokeThickness; //gli mette il bordo da rect non selezionato
            }
        }

        private void ThemeColRects_MouseDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            (sender as Rectangle).StrokeThickness = selectedRectStrokeThickness; //se un rect è cliccato gli mette lo spessore del bordo come quello di un rect selezionato
            Settings.Default.FontCol = ((FrameworkElement)sender).Name; //modifica l'opzione del colore del tema con il nome del rect
            Settings.Default.Save();
            Settings.Default.Reload();
            foreach(Rectangle rect in ThemeColorsSP.Children) //rimette a tutti gli altri rect il bordo di un rect non selezionato
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
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location); //riavvia l'applicazione
            Application.Current.Shutdown(0);
        }

        private void ThemeType_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            (sender as Rectangle).StrokeThickness = selectedRectStrokeThickness; //se un rect è cliccato gli mette lo spessore del bordo come quello di un rect selezionato
            Settings.Default.ThemeType = ((Rectangle)sender).Name; ////modifica l'opzione del tipo di tema con il nome del rect
            if (((Rectangle)sender).Name == "LightTheme" && Settings.Default.FontCol == "WhiteFontCol") //se il tema che si seleziona è quello bianco e il colore del font è bianco
            {
                Settings.Default.FontCol = "DefaultFontCol"; //mette il colore del font a default così si riesce a leggere
            }
            Settings.Default.Save();
            Settings.Default.Reload();

            foreach(Rectangle rect in ThemeTypeSP.Children) //rimette a tutti gli altri rect il bordo di un rect non selezionato
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
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location); //riavvia l'applicazione
            Application.Current.Shutdown(0);
        }

        private void LanguageComboBox_DropDownClosed(object sender, EventArgs e)
        {
            var previousLanguage = Settings.Default.Language;
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
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location); //riavvia l'applicazione
                Application.Current.Shutdown(0);
            }
        }
    }
}
