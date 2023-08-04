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
using System.Threading;
using System;
using System.Diagnostics;

namespace ImageConverter
{
    /// <summary> 
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static bool isValidDirectory; //if the dropped folder contains files that can be converted
        string[] givenFilesToConvert; //Paths of the dropped files on the ImgViewer or pass as startup arguments
        string[] pathsOfImagesToConvert; //paths of the images to convert that get passed to the ImageConversionHandler
        ImageSourceConverter imgSourceConverter = new ImageSourceConverter();
        List<bool> finishedConversions; //
        List<string> unsuccessfulConversions; //name of the images that didn't get converted
        int repGifTimes = 0;
        int delayTimeInCs = 50; //delay time (in centiseconds so that it doesn't have to be converted later)
        int replTranspWithCol = 0;
        Stopwatch timer = new Stopwatch(); //timer that measures the time needed to convert all the images

        public MainWindow()
        {
            InitializeComponent();
            MainWindowGrid.Background = ThemeManager.SelectedThemeType();
            TitleTextBox.Foreground = ThemeManager.SelectedFontColor();
            ThemeManager.solidColorBrush = new SolidColorBrush()
            {
                Color = ThemeManager.RunningOrStaticConversionTextBlockColor,
            };
            ConversionResultTextBlock.Foreground = ThemeManager.solidColorBrush;
            foreach (System.Windows.Controls.Label element in FormatComboBox.Items)
            {
                element.Background = ThemeManager.SelectedFontColor();
            } //applies the selected font color to every label in the format combobox
            if (Settings.Default.Language == "it")
            {
                ImgViewer.Source = imgSourceConverter.ConvertFromInvariantString("pack://application:,,,/Resources/ImageConverterDragAndDropIT.jpg") as ImageSource;
                ChooseFormatLabel.Content = LanguageManager.IT_ChooseFormatLabelTxt;
                WarningLabel.Content = LanguageManager.IT_WarningLabelTxt;
                ConversionResultTextBlock.Text = LanguageManager.IT_ConversionResultTextBlockRunningTxt;
                GifRepeatTimes.Content = LanguageManager.IT_GifLoopsOptionText;
                EmptyImgViewerCntxtMenuBttn.Header = LanguageManager.IT_EmpyBttnCntxtMenu;
                DelayTimeLabel.Content = LanguageManager.IT_DelayTimeLabelTxt;
            }//applies translation to all the visible controls
            else if (Settings.Default.Language == "en")
            {
                ImgViewer.Source = imgSourceConverter.ConvertFromInvariantString("pack://application:,,,/Resources/ImageConverterDragAndDropEN.png") as ImageSource;
                ChooseFormatLabel.Content = LanguageManager.EN_ChooseFormatLabelTxt;
                WarningLabel.Content = LanguageManager.EN_WarningLabelTxt;
                ConversionResultTextBlock.Text = LanguageManager.EN_ConversionResultTextBlockRunningTxt;
                GifRepeatTimes.Content = LanguageManager.EN_GifLoopOptionText;
                EmptyImgViewerCntxtMenuBttn.Header = LanguageManager.EN_EmpyBttnCntxtMenu;
                DelayTimeLabel.Content = LanguageManager.EN_DelayTimeLabelTxt;
            }
            if (!Directory.Exists($"{Path.GetTempPath()}\\ImageConverter"))
            {
                Directory.CreateDirectory($"{Path.GetTempPath()}\\ImageConverter");
                Settings.Default.TempFolderPath = $"{Path.GetTempPath()}\\ImageConverter";
            }//if the folder for ImageConverter in the temp direcory has been deleted create it again
        }

        private void MainWindow1_Loaded(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.FirstRun == true)
            {
                if (CultureInfo.CurrentCulture.ToString().Contains("it"))
                {
                    Settings.Default.Language = "it";
                }//set app language based off the default pc language 
                else
                {
                    Settings.Default.Language = "en";
                }

                //Creates a folder for ImageConverter in the temp directory
                if (!Directory.Exists($"{Path.GetTempPath()}\\ImageConverter"))
                {
                    Directory.CreateDirectory($"{Path.GetTempPath()}\\ImageConverter");
                    Settings.Default.TempFolderPath = $"{Path.GetTempPath()}\\ImageConverter";
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
            if (e.Data.GetData(DataFormats.FileDrop) != null)
            {
                string[] droppingFiles = e.Data.GetData(DataFormats.FileDrop) as string[]; //if the user is trying to convert more than one file
                if(CheckIfGivenFilesAreImages(droppingFiles) == false)
                {
                    WarningLabel.Visibility = Visibility.Visible;
                }
            }
        }

        private void ImgViewer_DragLeave(object sender, DragEventArgs e)
        {
            isValidDirectory = false;
            WarningLabel.Visibility = Visibility.Hidden; //nasconde l'avviso che appare se l'utente sta tentando di convertire più di un'immagine
        }

        private void ImgViewer_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) != null)
            {
                if (WarningLabel.Visibility == Visibility.Visible)
                {
                    WarningLabel.Visibility = Visibility.Hidden;
                    return;
                } //if the warning label IS visible the dropped file is not an image, so ignore the dropped files and exit from func

                #region Resets various GUI controls
                ThemeManager.solidColorBrush = new SolidColorBrush
                {
                    Color = ThemeManager.RunningOrStaticConversionTextBlockColor
                };
                ConversionResultTextBlock.Foreground = ThemeManager.solidColorBrush;
                ConversionResultTextBlock.Visibility = Visibility.Hidden;
                ImagesNameLabel.Text = string.Empty;
                if (FormatComboBox.SelectedValue?.ToString() != "System.Windows.Controls.Label: GIF")
                {
                    GifOptionsSP.Visibility = Visibility.Hidden;
                }
                ReplaceTransparencySP.Visibility = Visibility.Hidden;
                #endregion

                givenFilesToConvert = e.Data.GetData(DataFormats.FileDrop) as string[];
                GetImagesToConvertAndPrepareGUI(givenFilesToConvert); //Directly gets the images because the warning label is hidden, so the dropped files must be images
                LoadPreviewImage(pathsOfImagesToConvert);

                StartConversionBttn.IsEnabled = true;
            }
        }

        private async void StartConversionBttn_MouseDown(object sender, MouseButtonEventArgs e)
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
            } //if a format hasn't been selected prompt user to select one and stop conversion

            #region Prepares GUI controls
            if (Settings.Default.Language == "it") ConversionResultTextBlock.Text = LanguageManager.IT_ConversionResultTextBlockRunningTxt;
            if (Settings.Default.Language == "en") ConversionResultTextBlock.Text = LanguageManager.EN_ConversionResultTextBlockRunningTxt;
            ConversionResultTextBlock.Visibility = Visibility.Hidden; //necessary because if the user converts one image two times in a row it would seem like the conversion didn't start
            ThemeManager.solidColorBrush.Color = ThemeManager.RunningOrStaticConversionTextBlockColor;
            ConversionResultTextBlock.Foreground = ThemeManager.solidColorBrush; //sets the color of the textblock
            ConversionResultTextBlock.Visibility = Visibility.Visible; //makes the label of the state of the conversion visible
            StartConversionBttn.IsEnabled = false; //while a conversion is ongoing the convertbttn gets disabled
            #endregion

            finishedConversions = new List<bool>();
            string selectedFormat = ((FormatComboBox.SelectedItem as System.Windows.Controls.Label).Content as string).ToLower(); //takes the selected format

            if (Settings.Default.Language == "it")
            {
                ConversionResultTextBlock.Text = LanguageManager.IT_ConversionResultTextBlockRunningTxt;
            }
            else if (Settings.Default.Language == "en")
            {
                ConversionResultTextBlock.Text = LanguageManager.EN_ConversionResultTextBlockRunningTxt;
            }

            Thread ticker = new Thread(() =>
            {
                while (true)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        ConversionResultTextBlock.Text += ".";
                    });
                    Thread.Sleep(800);
                }
            }); //adds a dot each 800ms during conversion
            ticker.IsBackground = true;
            ticker.Start();
            timer.Start();

            finishedConversions = await Task.Run(() => ImageConversionHandler.StartConversion(selectedFormat, pathsOfImagesToConvert, repGifTimes, replTranspWithCol, delayTimeInCs));

            timer.Stop();
            #region counts the unsuccessful conversions
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
            ticker.Abort();
            #region displays the result(s) of the conversion(s)
            ConversionResultTextBlock.Visibility = Visibility.Visible;
            if (unsuccessfulConversions.Count == 0)
            {
                ThemeManager.solidColorBrush = new SolidColorBrush
                {
                    Color = ThemeManager.CompletedConversionTextBlockColor
                };
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
                ConversionResultTextBlock.Text += $" in {(int)timer.Elapsed.TotalMilliseconds}ms"; //time taken to convert the images in milliseconds
            } //if there were no errors
            else
            {
                ThemeManager.solidColorBrush = new SolidColorBrush
                {
                    Color = ThemeManager.CompletedWithErrorsConversionTextBlockColor
                };
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
            timer.Reset();

            StartConversionBttn.IsEnabled = true; //re-enables the convertbttn to convert another image
        }

        private void MenuBttn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Menu.OpenMenu(Menu);
        }

        private void FormatComboBox_DropDownClosed(object sender, System.EventArgs e)
        {
            var selectedValue = (((ComboBox)sender).SelectedItem as Label)?.Content.ToString();

            if (selectedValue == "GIF")
                GifOptionsSP.Visibility = Visibility.Visible;
            else
                GifOptionsSP.Visibility = Visibility.Hidden;
        }

        private void ReplTranspColCB_DropDownClosed(object sender, EventArgs e)
        {
            var selectedColor = ((ComboBox)sender).SelectedIndex;
            replTranspWithCol = selectedColor;
        }

        private void GifRepTimesCB_DropDownClosed_1(object sender, EventArgs e)
        {
            var selectedValue = (((ComboBox)sender).SelectedItem as Label)?.Content.ToString();
            if (selectedValue == "∞")
            {
                repGifTimes = 0;
                return;
            }
            repGifTimes = Convert.ToInt32(selectedValue);
        }

        private void DelayTimesCB_DropDownClosed(object sender, EventArgs e)
        {
            var comboBox = (ComboBox)sender;
            var delayTimeInt = Convert.ToInt32((comboBox.SelectedItem as Label).Content);
            delayTimeInCs = delayTimeInt / 10;
        }

        private void ImageViewerContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (ImgViewer.Source.ToString() != "pack://application:,,,/Resources/ImageConverterDragAndDropIT.jpg" && ImgViewer.Source.ToString() != "pack://application:,,,/Resources/ImageConverterDragAndDropEN.png")
            {
                EmptyImgViewerCntxtMenuBttn.IsEnabled = true;
            }
        }

        private void EmptyImgViewerCntxtMenuBttn_Click(object sender, RoutedEventArgs e)
        {
            ImgViewer.Source = null;
            if (Settings.Default.Language == "it") { ImgViewer.Source = new BitmapImage(new System.Uri("pack://application:,,,/Resources/ImageConverterDragAndDropIT.jpg")); }
            else if (Settings.Default.Language == "en") { ImgViewer.Source = new BitmapImage(new System.Uri("pack://application:,,,/Resources/ImageConverterDragAndDropEN.png")); }
            StartConversionBttn.IsEnabled = false;
            ImagesNameLabel.Text = string.Empty;
            ImgViewer.Opacity = 0.3f;
            pathsOfImagesToConvert = null;
            imgSourceConverter = null;
            EmptyImgViewerCntxtMenuBttn.IsEnabled = false;
            GifOptionsSP.Visibility = Visibility.Hidden;
            ReplaceTransparencySP.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Checks wether the files that the user wants to convert are images or not.
        /// The files are given by dropping them on the ImgViewer control
        /// </summary>
        /// <param name="files"></param>
        public static bool CheckIfGivenFilesAreImages(string[] files)
        {
            foreach (var file in files)
            {
                FileAttributes attr = File.GetAttributes(file);
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    string[] filesInDir = Directory.GetFiles(file);
                    foreach (var fileInDir in filesInDir)
                    {
                        if (ImageConversionHandler.IsImage(fileInDir))
                        {
                            isValidDirectory = true;
                        }
                        else
                        {
                            isValidDirectory = false;
                            return false;
                        }
                    }
                } //check if the file is a folder and check if it contains any image, if yes then the files are valid
                if (!ImageConversionHandler.IsImage(file) && !isValidDirectory)
                {
                    return false;
                }//if the file isn't an image
            }
            return true;
        }

        /// <summary>
        /// Gets the images dropped by the user on the ImgViewer control and prepares the GUI consequently
        /// </summary>
        /// <param name="givenFilesToConvert"> Files passed by the user as arguments or dropped on the ImgViewer control </param>
        public void GetImagesToConvertAndPrepareGUI(string[] givenFilesToConvert)
        {
            if (isValidDirectory)
            {
                pathsOfImagesToConvert = Directory.GetFiles(givenFilesToConvert[0]);
            }//if the dropped file is a folder the image(s) to convert will be inside it
            else
            {
                pathsOfImagesToConvert = givenFilesToConvert;
            }//else if the user has dropped directly the image(s)

            if (pathsOfImagesToConvert.Length == 1) ImagesNameLabel.Text += Path.GetFileName(pathsOfImagesToConvert[0]);
            else
            {
                foreach (string imagePath in pathsOfImagesToConvert)
                {
                    ImagesNameLabel.Text += Path.GetFileName(imagePath + ", ");
                }
            } //shows the name(s) of the image(s) under the ImgViewer

            foreach (string imagePath in pathsOfImagesToConvert)
            {
                if (Path.GetExtension(imagePath).ToLower() == ".png")
                {
                    ReplaceTransparencySP.Visibility = Visibility.Visible;
                }
            } // If the image is a png enable the option to replace the transparency
        }

        /// <summary>
        /// Load the image to see in the ImgViewer control
        /// </summary>
        /// <param name="pathsOfImagesToConvert"></param>
        public void LoadPreviewImage(string[] pathsOfImagesToConvert)
        {
            ImgViewer.Opacity = 1.0f; //sets ImgViewer opacity to 1
            using (var st = File.OpenRead(pathsOfImagesToConvert[0]))
            {
                var imageToShow = new BitmapImage();
                imageToShow.BeginInit();
                imageToShow.StreamSource = st;
                imageToShow.CacheOption = BitmapCacheOption.OnLoad;
                imageToShow.EndInit();
                ImgViewer.Source = imageToShow;
                st.Close();
            } //loads the preview image from a stream, if the image was used directly it would have remained in use even after emptying the ImgViewer and so it couldn't be deleted eventually
        }

    }
}
