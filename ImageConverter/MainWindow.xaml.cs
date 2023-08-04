using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Globalization;
using System.Media;
using ImageConverter.Properties;
using System.Threading.Tasks;
using System.Threading;

namespace ImageConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string[] droppedImage;
        ImageSourceConverter stringToImgSrcConverter;
        string pathofImgToConvert;
        ImageSourceConverter imgSourceConverter = new ImageSourceConverter();
        bool conversionFinished = false;

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
                ConversionResultLabel.Content = LanguageManager.IT_ConversionResultLabelRunningTxt;
            }
            else if (Settings.Default.Language == "en")
            {
                ImgViewer.Source = imgSourceConverter.ConvertFromInvariantString("pack://application:,,,/Resources/ImageConverterDragAndDropEN.png") as ImageSource;
                ChooseFormatLabel.Content = LanguageManager.EN_ChooseFormatLabelTxt;
                WarningLabel.Content = LanguageManager.EN_WarningLabelTxt;
                ConversionResultLabel.Content = LanguageManager.EN_ConversionResultLabelRunningTxt;
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
                else if (CultureInfo.CurrentCulture.ToString().Contains("en"))
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
            if ((e.Data.GetData(DataFormats.FileDrop) as string[]).Length > 1) //se l'utente sta tentando di convertire più di un'immagine
            {
                WarningLabel.Visibility = Visibility.Visible; //mostra l'avviso
            }
        }

        private void ImgViewer_DragLeave(object sender, DragEventArgs e)
        {
            WarningLabel.Visibility = Visibility.Hidden; //nasconde l'avviso che appare se l'utente sta tentando di convertire più di un'immagine
        }



        private void ImgViewer_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) != null)
            {
                droppedImage = e.Data.GetData(DataFormats.FileDrop) as string[]; //prende il file droppato dall'utente
                pathofImgToConvert = droppedImage[0]; //prende la path del file dall'array e la mette in una stringa

                if (!ImageConversionHandler.IsImage(pathofImgToConvert)) //se il file non è un'immagine
                {
                    pathofImgToConvert = string.Empty;
                    if (Settings.Default.Language == "it")
                    {
                        MessageBox.Show(LanguageManager.IT_CantConvertThisFile, "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else if (Settings.Default.Language == "en")
                    {
                        MessageBox.Show(LanguageManager.EN_CantConvertThisFile, "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    return;
                }
                //se invece è un immagine
                ConversionResultLabel.Visibility = Visibility.Hidden;
                stringToImgSrcConverter = new ImageSourceConverter();
                ImgViewer.Opacity = 1.0f; //rimette l'opacità delle immagini nell'ImgViewer a 1
                ImgViewer.Source = stringToImgSrcConverter.ConvertFromInvariantString(pathofImgToConvert) as ImageSource; //converte la path in ImageSource e la mostra nell'ImgViewer
                WarningLabel.Visibility = Visibility.Hidden; //nasconde il warningLabel in caso fosse stato messo visibile
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
                    MessageBox.Show(LanguageManager.EN_SelectFormatMsgBox, "Errore", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                return;
            }
            string selectedFormat = (((FormatComboBox.SelectedItem as System.Windows.Controls.Label).Content as string).ToLower());
            ConversionResultLabel.Visibility = Visibility.Visible;
            Task.Run(() => Loading());
            conversionFinished = await Task.Run(() => ImageConversionHandler.ConvertAndSaveAsync(selectedFormat, pathofImgToConvert));
            if (conversionFinished == true)
            {
                ThemeManager.solidColorBrush = new SolidColorBrush();
                ThemeManager.solidColorBrush.Color = ThemeManager.CompletedConversionLabelColor;
                ConversionResultLabel.Foreground = ThemeManager.solidColorBrush;
                if (Settings.Default.Language == "it")
                {
                    ConversionResultLabel.Content = LanguageManager.IT_ConversionResultLabelFinishedTxt;
                }
                else if (Settings.Default.Language == "en")
                {
                    ConversionResultLabel.Content = LanguageManager.EN_ConversionResultLabelFinishedTxt;
                }
            }
        }

        private void MenuBttn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Menu.OpenMenu(Menu);
        }

        private async Task Loading()
        {
            while (true)
            {
                System.Diagnostics.Debug.WriteLine("running");
                if (!(ConversionResultLabel.Content as string).Contains("..."))
                {
                    ConversionResultLabel.Content += ".";
                    Thread.Sleep(800);
                }
                else
                {
                    ConversionResultLabel.Content = (ConversionResultLabel.Content as string).Replace("...", string.Empty);
                }
            }

        }
    }
}
