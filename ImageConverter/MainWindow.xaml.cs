using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Globalization;
using ImageConverter.Properties;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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
        bool GifInLoopOpt = true;

        public MainWindow()
        {
            InitializeComponent();
            MainWindowGrid.Background = ThemeManager.SelectedThemeType();
            TitleTextBox.Foreground = ThemeManager.SelectedFontColor();
            ThemeManager.solidColorBrush = new SolidColorBrush();
            ThemeManager.solidColorBrush.Color = ThemeManager.RunningConversionLabelColor;
            ConversionResultTextBlock.Foreground = ThemeManager.solidColorBrush;
            foreach (System.Windows.Controls.Label element in FormatComboBox.Items)
            {
                element.Background = ThemeManager.SelectedFontColor();
            }
            if (Settings.Default.Language == "it")
            {
                ImgViewer.Source = imgSourceConverter.ConvertFromInvariantString("pack://application:,,,/Resources/ImageConverterDragAndDropIT.jpg") as ImageSource;
                ChooseFormatLabel.Content = LanguageManager.IT_ChooseFormatLabelTxt;
                WarningLabel.Content = LanguageManager.IT_WarningLabelTxt;
                ConversionResultTextBlock.Text = LanguageManager.IT_ConversionResultTextBlockRunningTxt;
                GifLoopOptionCB.Content = LanguageManager.IT_GifLoopOptionCheckBoxText;
                EmptyImgViewerCntxtMenuBttn.Header = LanguageManager.IT_EmpyBttnCntxtMenu;
            }
            else if (Settings.Default.Language == "en")
            {
                ImgViewer.Source = imgSourceConverter.ConvertFromInvariantString("pack://application:,,,/Resources/ImageConverterDragAndDropEN.png") as ImageSource;
                ChooseFormatLabel.Content = LanguageManager.EN_ChooseFormatLabelTxt;
                WarningLabel.Content = LanguageManager.EN_WarningLabelTxt;
                ConversionResultTextBlock.Text = LanguageManager.EN_ConversionResultTextBlockRunningTxt;
                GifLoopOptionCB.Content = LanguageManager.EN_GifLoopOptionCheckBoxText;
                EmptyImgViewerCntxtMenuBttn.Header = LanguageManager.EN_EmpyBttnCntxtMenu;
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
            ConversionResultTextBlock.Foreground = ThemeManager.solidColorBrush;
            ConversionResultTextBlock.Visibility = Visibility.Hidden;
            ImagesNameLabel.Text = string.Empty;
            #endregion

            droppedImages = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (e.Data.GetData(DataFormats.FileDrop) != null) //if the warning label is visible, so if the file(s) is/are not image(s)
            {
                if (WarningLabel.Visibility == Visibility.Visible)
                {
                    WarningLabel.Visibility = Visibility.Hidden;
                    return;
                }
                pathsOfImagesToConvert = droppedImages;
                if (droppedImages.Length == 1) ImagesNameLabel.Text += Path.GetFileName(droppedImages[0]);
                else
                {
                    foreach (string imagePath in droppedImages)
                    {
                        ImagesNameLabel.Text += Path.GetFileName(imagePath + ", ");
                    }
                } //shows the name of the image(s) under the image container
                if (Settings.Default.Language == "it") ConversionResultTextBlock.Text = LanguageManager.IT_ConversionResultTextBlockRunningTxt;
                if (Settings.Default.Language == "en") ConversionResultTextBlock.Text = LanguageManager.EN_ConversionResultTextBlockRunningTxt;
                stringToImgSrcConverter = new ImageSourceConverter();
                ImgViewer.Opacity = 1.0f; //sets imageviewer opacity to 1
                using (var st = File.OpenRead(pathsOfImagesToConvert[0]))
                {
                    var imageToShow = new BitmapImage();
                    imageToShow.BeginInit();
                    imageToShow.StreamSource = st;
                    imageToShow.CacheOption = BitmapCacheOption.OnLoad;
                    imageToShow.EndInit();
                    ImgViewer.Source = imageToShow;
                    st.Close();
                } //loads image to show from a stream and shows it, if the image was used directly it would've 
                                                                             //remained in use even after emptying the ImgViewer and so couldn't be deleted
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
            } //if a format hasn't been selected prompt user to select one and return

            finishedConversions = new List<bool>();
            string selectedFormat = ((FormatComboBox.SelectedItem as System.Windows.Controls.Label).Content as string).ToLower(); //takes the selected format
            ConversionResultTextBlock.Visibility = Visibility.Visible; //makes the label of the state of the conversion visible
            ConvertImgBttn.IsEnabled = false; //while a conversion is ongoing the convertbttn gets disabled
            if (Settings.Default.Language == "it")
            {
                ConversionResultTextBlock.Text = LanguageManager.IT_ConversionResultTextBlockRunningTxt;
            }
            else if (Settings.Default.Language == "en")
            {
                ConversionResultTextBlock.Text = LanguageManager.EN_ConversionResultTextBlockRunningTxt;
            }

            finishedConversions = await Task.Run(() => ImageConversionHandler.StartConversion(selectedFormat, pathsOfImagesToConvert, GifInLoopOpt));
            #region gets the unsuccessful conversions
            unsuccessfulConversions = new List<string>();
            int i = 0;
            foreach (var conversion in finishedConversions)
            {
                if (conversion == false)
                {
                    unsuccessfulConversions.Add(Path.GetFileName(pathsOfImagesToConvert[i]));
                }
                i++;
            }
            #endregion
            #region displays the result(s) of the conversion(s)
            ConversionResultTextBlock.Visibility = Visibility.Visible;
            if (unsuccessfulConversions.Count == 0)
            {
                ThemeManager.solidColorBrush = new SolidColorBrush();
                ThemeManager.solidColorBrush.Color = ThemeManager.CompletedConversionTextBlockColor;
                ConversionResultTextBlock.Foreground = ThemeManager.solidColorBrush;
                if (Settings.Default.Language == "it")
                {
                    if (pathsOfImagesToConvert.Length == 1) ConversionResultTextBlock.Text = LanguageManager.IT_ConversionResultTextBlockFinishedTxt;
                    else ConversionResultTextBlock.Text = LanguageManager.IT_MultipleConversionResultTextBlockFinishedTxt;
                }
                else if (Settings.Default.Language == "en")
                {
                    ConversionResultTextBlock.Text = LanguageManager.EN_ConversionResultTextBlockFinishedTxt;
                }
            } //if there were no errors
            else
            {
                ThemeManager.solidColorBrush = new SolidColorBrush();
                ThemeManager.solidColorBrush.Color = ThemeManager.CompletedWithErrorsConversionTextBlockColor;
                ConversionResultTextBlock.Foreground = ThemeManager.solidColorBrush;
                if (Settings.Default.Language == "it")
                {
                    ConversionResultTextBlock.Text = LanguageManager.IT_UnsuccConversionResultTextBlockFinishedTxt;
                }
                else if (Settings.Default.Language == "en")
                {
                    ConversionResultTextBlock.Text = LanguageManager.EN_UnsuccConversionResultTextBlockFinishedTxt;
                }
                foreach (var conversion in unsuccessfulConversions)
                {
                    ConversionResultTextBlock.Text += conversion + ", ";
                }
            } //if there was any error
            #endregion

            ConvertImgBttn.IsEnabled = true; //re-enables the convertbttn to convert another image
        }

        private void MenuBttn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Menu.OpenMenu(Menu);
        }

        private void FormatComboBox_DropDownClosed(object sender, System.EventArgs e)
        {
            if ((((ComboBox)sender).SelectedItem as Label)?.Content.ToString() == "GIF")// the null coalescing operator (?.) is needed beacause if the user closes the combobox menu
            {                                                                           // without selecting a format the selected item would be null, casuing a NullReferenceException
                GifLoopOptionCB.Visibility = Visibility.Visible;
            }
            else
            {
                GifLoopOptionCB.Visibility = Visibility.Hidden;
            }
        }

        private void GifLoopOption_Click(object sender, RoutedEventArgs e)
        {
            if (GifLoopOptionCB.IsChecked == false)
            {
                GifInLoopOpt = false;
            }
            else
            {
                GifInLoopOpt = true;
            }
        }

        private void EmptyImgViewerCntxtMenuBttn_Click(object sender, RoutedEventArgs e)
        {
            ImgViewer.ClearValue(Image.SourceProperty);
            if (Settings.Default.Language == "it") { ImgViewer.Source = new BitmapImage(new System.Uri("pack://application:,,,/Resources/ImageConverterDragAndDropIT.jpg")); }
            else if (Settings.Default.Language == "en") { ImgViewer.Source = new BitmapImage(new System.Uri("pack://application:,,,/Resources/ImageConverterDragAndDropEN.png")); }
            stringToImgSrcConverter = null;
            ConvertImgBttn.IsEnabled = false;
            ImagesNameLabel.Text = string.Empty;
            ImgViewer.Opacity = 0.3f;
            droppedImages = null;
            stringToImgSrcConverter = null;
            pathsOfImagesToConvert = null;
            imgSourceConverter = null;
        }
    }
}
