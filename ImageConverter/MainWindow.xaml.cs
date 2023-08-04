using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Globalization;
using ImageConverter.Properties;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Collections.Generic;

namespace ImageConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string[] droppedImages;
        ImageSourceConverter stringToImgSrcConverter;
        string[] pathsOfImagesToConvert;
        ImageSourceConverter imgSourceConverter = new ImageSourceConverter();
        List<bool> finishedConversions;
        List<string> unsuccessfulConversions;

        public MainWindow()
        {
            InitializeComponent();
            MainWindowGrid.Background = ThemeManager.SelectedThemeType();
            TitleTextBox.Foreground = ThemeManager.SelectedFontColor();
            ThemeManager.solidColorBrush = new SolidColorBrush();
            ThemeManager.solidColorBrush.Color = ThemeManager.RunningConversionLabelColor;
            ConversionResultLabel.Foreground = ThemeManager.solidColorBrush;
            foreach (System.Windows.Controls.Label element in FormatComboBox.Items)
            {
                element.Background = ThemeManager.SelectedFontColor();
            }
            if (Settings.Default.Language == "it")
            {
                ImgViewer.Source = imgSourceConverter.ConvertFromInvariantString("pack://application:,,,/Resources/ImageConverterDragAndDropIT.jpg") as ImageSource;
                ChooseFormatLabel.Content = LanguageManager.IT_ChooseFormatLabelTxt;
                WarningLabel.Content = LanguageManager.IT_WarningLabelTxt;
                ConversionResultLabel.Text = LanguageManager.IT_ConversionResultLabelRunningTxt;
            }
            else if (Settings.Default.Language == "en")
            {
                ImgViewer.Source = imgSourceConverter.ConvertFromInvariantString("pack://application:,,,/Resources/ImageConverterDragAndDropEN.png") as ImageSource;
                ChooseFormatLabel.Content = LanguageManager.EN_ChooseFormatLabelTxt;
                WarningLabel.Content = LanguageManager.EN_WarningLabelTxt;
                ConversionResultLabel.Text = LanguageManager.EN_ConversionResultLabelRunningTxt;
            }
        }

        private void MainWindow1_Loaded(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.FirstRun == true)
            {
                if (CultureInfo.CurrentCulture.ToString().Contains("it")) //imposta la lingua dell'applicazione dalla lingua di sistema
                {
                    Settings.Default.Language = "it";
                }
                else
                {
                    Settings.Default.Language = "en";
                }
                Settings.Default.FirstRun = false;
                Settings.Default.Save();
                Settings.Default.Reload();
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
        }

        private void ImgViewer_DragOver(object sender, DragEventArgs e)
        {
            string[] droppingFiles = e.Data.GetData(DataFormats.FileDrop) as string[]; //se l'utente sta tentando di convertire più di un'immagine
            foreach (var file in droppingFiles)
            {
                if (ImageConversionHandler.IsImage(file) == false)
                {
                    WarningLabel.Visibility = Visibility.Visible;
                    break;
                }
            }
        }

        private void ImgViewer_DragLeave(object sender, DragEventArgs e)
        {
            WarningLabel.Visibility = Visibility.Hidden; //nasconde l'avviso che appare se l'utente sta tentando di convertire più di un'immagine
        }

        private void ImgViewer_Drop(object sender, DragEventArgs e)
        {
            #region resets controls
            ThemeManager.solidColorBrush = new SolidColorBrush();
            ThemeManager.solidColorBrush.Color = ThemeManager.RunningConversionLabelColor;
            ConversionResultLabel.Foreground = ThemeManager.solidColorBrush;
            ConversionResultLabel.Visibility = Visibility.Hidden;
            ImageNameLabel.Text = string.Empty;
            #endregion

            droppedImages = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (e.Data.GetData(DataFormats.FileDrop) != null) //if the warning label is visible, so if the file(s) is/are not image(s)
            {
                if (WarningLabel.Visibility == Visibility.Visible)
                {
                    WarningLabel.Visibility = Visibility.Hidden;
                    return;
                }

                //if it's an image
                pathsOfImagesToConvert = droppedImages;
                if (droppedImages.Length == 1) ImageNameLabel.Text += Path.GetFileName(droppedImages[0]);
                else
                {
                    foreach (string imagePath in droppedImages) //shows the name of the image(s) under the image container
                    {
                        ImageNameLabel.Text += Path.GetFileName(imagePath + ", ");
                    }
                }
                if (Settings.Default.Language == "it") ConversionResultLabel.Text = LanguageManager.IT_ConversionResultLabelRunningTxt;
                if (Settings.Default.Language == "en") ConversionResultLabel.Text = LanguageManager.EN_ConversionResultLabelRunningTxt;
                stringToImgSrcConverter = new ImageSourceConverter();
                ImgViewer.Opacity = 1.0f; //sets imageviewer opacity to 1
                ImgViewer.Source = stringToImgSrcConverter.ConvertFromInvariantString(pathsOfImagesToConvert[0]) as ImageSource; //converts the path in ImageSource and shows it in ImgViewer
                WarningLabel.Visibility = Visibility.Hidden; //hides the warning label in case it the user tried to convert a non valid file but then dropped a valid file
                ConvertImgBttn.IsEnabled = true;
            }
        }

        private async void ConvertRoundBttn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (FormatComboBox.SelectedItem == null)
            {
                if (Settings.Default.Language == "it")
                {
                    MessageBox.Show(LanguageManager.IT_SelectFormatMsgBox, "Errore", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (Settings.Default.Language == "en")
                {
                    MessageBox.Show(LanguageManager.EN_SelectFormatMsgBox, "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                return;
            } //if a format hasn't been selected 

            //else
            finishedConversions = new List<bool>();
            string selectedFormat = ((FormatComboBox.SelectedItem as System.Windows.Controls.Label).Content as string).ToLower(); //takes the selected format
            ConversionResultLabel.Visibility = Visibility.Visible; //makes the label of the state of the conversion visible
            ConvertImgBttn.IsEnabled = false; //while a conversion is ongoing the convertbttn gets disabled

            foreach (string imagePath in pathsOfImagesToConvert)
            {
                finishedConversions.Add(await Task.Run(() => ImageConversionHandler.ConvertAndSaveAsync(selectedFormat, imagePath)));
            } //executes the ConvertAndSaveAsync task for each image to convert

            #region gets the unsuccessful conversions
            unsuccessfulConversions = new List<string>();
            int i = 0;
            foreach (var conversion in finishedConversions)
            {
                if (conversion == true)
                {
                    unsuccessfulConversions.Add(Path.GetFileName(pathsOfImagesToConvert[i]));
                }
                i++;
            }
            #endregion

            #region displays the result(s) of the conversion(s)
            ConversionResultLabel.Visibility = Visibility.Visible;
            if (unsuccessfulConversions.Count == 0)
            {
                ThemeManager.solidColorBrush = new SolidColorBrush();
                ThemeManager.solidColorBrush.Color = ThemeManager.CompletedConversionLabelColor;
                ConversionResultLabel.Foreground = ThemeManager.solidColorBrush;
                if (Settings.Default.Language == "it")
                {
                    if (pathsOfImagesToConvert.Length == 1) ConversionResultLabel.Text = LanguageManager.IT_ConversionResultLabelFinishedTxt;
                    else ConversionResultLabel.Text = LanguageManager.IT_MultipleConversionResultLabelFinishedTxt;
                }
                else if (Settings.Default.Language == "en")
                {
                    ConversionResultLabel.Text = LanguageManager.EN_ConversionResultLabelFinishedTxt;
                }
            }
            else
            {
                ThemeManager.solidColorBrush = new SolidColorBrush();
                ThemeManager.solidColorBrush.Color = ThemeManager.CompletedWithErrorsConversionLabelColor;
                ConversionResultLabel.Foreground = ThemeManager.solidColorBrush;
                if (Settings.Default.Language == "it")
                {
                    ConversionResultLabel.Text = LanguageManager.IT_UnsuccConversionResultLabelFinishedTxt;
                }
                else if (Settings.Default.Language == "en")
                {
                    ConversionResultLabel.Text = LanguageManager.EN_UnsuccConversionResultLabelFinishedTxt;
                }
                foreach (var conversion in unsuccessfulConversions)
                {
                    ConversionResultLabel.Text += conversion + ", ";
                }
            }
            #endregion
            ConvertImgBttn.IsEnabled = true; //re-enables the convertbttn to convert another image
        }

        private void MenuBttn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Menu.OpenMenu(Menu);
        }

        private void Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ConversionResultLabel.Visibility = Visibility.Visible; //label sullo stato della conversione visibile
            ConversionResultLabel.Text += ".";
        }

    }
}
