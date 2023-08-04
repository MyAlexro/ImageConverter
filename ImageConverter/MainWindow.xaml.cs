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
using System.Linq;

namespace ImageConverter
{
    /// <summary> 
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// If the dropped folder contains files that can be converted
        /// </summary>
        static bool droppedFileisValidDirectory;

        /// <summary>
        /// Paths of the dropped files on the ImgViewer
        /// </summary>
        List<string> givenFilesToConvert;

        /// <summary>
        /// Paths of the images to convert that get passed to the ImageConversionHandler
        /// </summary>
        List<string> pathsOfImagesToConvert = new List<string>();

        /// <summary>
        /// List of finished conversions, the bool indicates wether it was finished successfully or not
        /// </summary>
        List<bool> finishedConversions;

        /// <summary>
        /// Name of the images that didn't get converted
        /// </summary>
        List<string> unsuccessfulConversions;

        int repGifTimes = 0;

        /// <summary>
        /// delay time (in centiseconds so that it doesn't have to be converted later)
        /// </summary>
        int gifDelayTimeinCs = 50;
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
            AddOrReplaceDroppedImages.Source = new BitmapImage(new System.Uri("pack://application:,,,/Resources/ReplaceImages.jpg"));
            ConversionResultTextBlock.Visibility = Visibility.Hidden;
            WarningLabel.Visibility = Visibility.Hidden;
            GifOptionsSP.Visibility = Visibility.Hidden;
            ReplaceTransparencySP.Visibility = Visibility.Hidden;
            if (Settings.Default.Language == "it")
            {
                ImgViewer.Source = new BitmapImage(new System.Uri("pack://application:,,,/Resources/ImageConverterDragAndDropIT.jpg"));
                StartConversionBttn.ButtonText = LanguageManager.IT_StartConversionLabelTxt;
                ChooseFormatLabel.Content = LanguageManager.IT_ChooseFormatLabelTxt;
                WarningLabel.Content = LanguageManager.IT_WarningLabelTxt;
                ConversionResultTextBlock.Text = LanguageManager.IT_ConversionResultTextBlockRunningTxt;
                GifRepeatTimes.Content = LanguageManager.IT_GifLoopsOptionText;
                EmptyImgViewerCntxtMenuBttn.Header = LanguageManager.IT_EmpyBttnCntxtMenu;
                GifFramesDelayTimeLabel.Content = LanguageManager.IT_DelayTimeLabelTxt;
                AddOrReplaceDroppedImages.ToolTip = LanguageManager.IT_ReplaceExistingImagesToolTip;
            }//applies translation to all the visible controls
            else if (Settings.Default.Language == "en")
            {
                ImgViewer.Source = new BitmapImage(new System.Uri("pack://application:,,,/Resources/ImageConverterDragAndDropEN.png"));
                StartConversionBttn.ButtonText = LanguageManager.EN_StartConversionLabelTxt;
                ChooseFormatLabel.Content = LanguageManager.EN_ChooseFormatLabelTxt;
                WarningLabel.Content = LanguageManager.EN_WarningLabelTxt;
                ConversionResultTextBlock.Text = LanguageManager.EN_ConversionResultTextBlockRunningTxt;
                GifRepeatTimes.Content = LanguageManager.EN_GifLoopOptionText;
                EmptyImgViewerCntxtMenuBttn.Header = LanguageManager.EN_EmpyBttnCntxtMenu;
                GifFramesDelayTimeLabel.Content = LanguageManager.EN_DelayTimeLabelTxt;
                AddOrReplaceDroppedImages.ToolTip = LanguageManager.EN_ReplaceExistingImagesToolTip;
            }

            if (!Directory.Exists($"{Path.GetTempPath()}\\ImageConverter"))
            {
                Directory.CreateDirectory($"{Path.GetTempPath()}ImageConverter");
                Settings.Default.TempFolderPath = $"{Path.GetTempPath()}ImageConverter";
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
                if (CheckIfGivenFilesAreImages(droppingFiles) == false)
                {
                    WarningLabel.Visibility = Visibility.Visible;
                }
            }
        }

        private void ImgViewer_DragLeave(object sender, DragEventArgs e)
        {
            droppedFileisValidDirectory = false;
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
                givenFilesToConvert = new List<string>();

                foreach (var file in e.Data.GetData(DataFormats.FileDrop) as string[]) { givenFilesToConvert.Add(file); }
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
                    Thread.Sleep(500);
                }
            }); //adds a dot each 500ms during conversion
            ticker.IsBackground = true;
            ticker.Start();
            timer.Start();

            finishedConversions = await Task.Run(() => ImageConversionHandler.StartConversion(selectedFormat, pathsOfImagesToConvert, repGifTimes, replTranspWithCol, gifDelayTimeinCs));

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
                    if (pathsOfImagesToConvert.Count == 1) ConversionResultTextBlock.Text = LanguageManager.IT_ConversionResultTextBlockFinishedTxt;
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

        /// <summary>
        /// Opens side menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuBttn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Menu.OpenMenu(Menu);
        }

        /// <summary>
        /// Sets the format to convert the images to
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormatComboBox_DropDownClosed(object sender, System.EventArgs e)
        {
            var selectedValue = (((ComboBox)sender).SelectedItem as Label)?.Content.ToString();

            if (selectedValue == "GIF")
                GifOptionsSP.Visibility = Visibility.Visible;
            else
                GifOptionsSP.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Sets the color to replace the transparency of a png image with, !when the ComboBox closes it means that an element has been selected!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReplTranspColCB_DropDownClosed(object sender, EventArgs e)
        {
            var selectedColor = ((ComboBox)sender).SelectedIndex;
            replTranspWithCol = selectedColor;
        }

        /// <summary>
        /// Sets the value for how many times the gif shall repeat, !When the ComboBox closes it means that an element has been selected!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Sets the value for the delay between the frames of a gif has been selected, !When the ComboBox closes it means that an element has been selected!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GifFramesDelayOptionsCB_DropDownClosed(object sender, EventArgs e)
        {
            var comboBox = (ComboBox)sender;
            var delayTimeInt = Convert.ToInt32((comboBox.SelectedItem as Label).Content);
            gifDelayTimeinCs = delayTimeInt / 10;
        }

        /// <summary>
        /// If there are images to convert enable the EmptyImgViewerMenuBttn in the context menu of the ImgViewer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageViewerContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (ImgViewer.Source.ToString() != "pack://application:,,,/Resources/ImageConverterDragAndDropIT.jpg" && ImgViewer.Source.ToString() != "pack://application:,,,/Resources/ImageConverterDragAndDropEN.png")
            {
                EmptyImgViewerCntxtMenuBttn.IsEnabled = true;
            }
        }

        /// <summary>
        /// Buttons that empties the ImgViewer and the queue of images to convert
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EmptyImgViewerCntxtMenuBttn_Click(object sender, RoutedEventArgs e)
        {
            ImgViewer.Source = null;
            if (Settings.Default.Language == "it") { ImgViewer.Source = new BitmapImage(new System.Uri("pack://application:,,,/Resources/ImageConverterDragAndDropIT.jpg")); }
            else if (Settings.Default.Language == "en") { ImgViewer.Source = new BitmapImage(new System.Uri("pack://application:,,,/Resources/ImageConverterDragAndDropEN.png")); }
            StartConversionBttn.IsEnabled = false;
            ImagesNameLabel.Text = string.Empty;
            ImgViewer.Opacity = 0.3f;
            pathsOfImagesToConvert = null;
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
                            droppedFileisValidDirectory = true;
                        }
                        else
                        {
                            droppedFileisValidDirectory = false;
                            return false;
                        }
                    }
                } //check if the file is a folder and check if it contains any image, if yes then the files are valid
                if (!ImageConversionHandler.IsImage(file) && !droppedFileisValidDirectory)
                {
                    return false;
                }//if the file isn't an image
            }
            return true;
        }

        /// <summary>
        /// Gets the images dropped by the user on the ImgViewer control and prepares the GUI consequently
        /// </summary>
        /// <param name="givenFilesToConvert"> Files dropped on the ImgViewer control, if the user drops a folder it would be the first element</param>
        public void GetImagesToConvertAndPrepareGUI(List<String> givenFilesToConvert)
        {
            //If the user wants to replace the already dropped images
            if ((string)AddOrReplaceDroppedImages.Tag == "ReplaceImages")
            {
                //if the dropped file is a folder the image(s) to convert will be inside it
                if (droppedFileisValidDirectory)
                {
                    pathsOfImagesToConvert = Directory.GetFiles(givenFilesToConvert[0]).ToList();
                }
                //else if the user has dropped directly the image(s)
                else
                {
                    pathsOfImagesToConvert = givenFilesToConvert;
                }
            }

            //If the user wants to add more images to convert
            else
            {
                //if the dropped file is a folder the image(s) to convert will be inside it
                if (droppedFileisValidDirectory)
                {
                    foreach (var imagePath in Directory.GetFiles(givenFilesToConvert[0]))
                    {
                        pathsOfImagesToConvert.Add(imagePath);
                    }
                }
                //else if the user has dropped directly the image(s)
                else
                {
                    pathsOfImagesToConvert.AddRange(givenFilesToConvert);
                }
            }

            #region Adds name(s) of the image(s) to the textblock under ImgViewer
            if (pathsOfImagesToConvert.Count == 1) ImagesNameLabel.Text += Path.GetFileName(pathsOfImagesToConvert[0]);
            else
            {
                foreach (string imagePath in pathsOfImagesToConvert)
                {
                    ImagesNameLabel.Text += Path.GetFileName(imagePath + ", ");
                }
            } //shows the name(s) of the image(s) under the ImgViewer
            #endregion

            //Checks if any image is a png, if so enable the option to replace its transparency
            foreach (string imagePath in pathsOfImagesToConvert)
            {
                if (Path.GetExtension(imagePath).ToLower() == ".png")
                {
                    ReplaceTransparencySP.Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// Loads the image to see in the ImgViewer control
        /// </summary>
        /// <param name="pathsOfImagesToConvert"></param>
        public void LoadPreviewImage(List<string> pathsOfImagesToConvert)

        {
            ImgViewer.Opacity = 1.0f; //sets ImgViewer opacity to 1(max)
            //loads the preview image from a stream, if the image was used directly it would have remained in use even after emptying the ImgViewer and so it couldn't be eventually deleted
            using (var st = File.OpenRead(pathsOfImagesToConvert[0]))
            {
                var imageToShow = new BitmapImage();
                imageToShow.BeginInit();
                imageToShow.StreamSource = st;
                imageToShow.CacheOption = BitmapCacheOption.OnLoad;
                imageToShow.EndInit();
                ImgViewer.Source = imageToShow;
                st.Close();
            }
        }

        /// <summary>
        /// Determines wether the dropped images replace or add up to the previous dropped images
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddOrReplaceDroppedImages_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image imageControl = sender as Image;

            if ((string)imageControl.Tag == "ReplaceImages")
            {
                imageControl.Tag = "AddImages";
                imageControl.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/AddImages.jpg"));
                if (Settings.Default.Language.ToLower() == "it") { imageControl.ToolTip = LanguageManager.IT_AddToExistingImagesToolTip; }
                else if (Settings.Default.Language.ToLower() == "en") { imageControl.ToolTip = LanguageManager.EN_AddToExistingImagesToolTip; }
            }
            else
            {
                imageControl.Tag = "ReplaceImages";
                imageControl.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/ReplaceImages.jpg"));
                if (Settings.Default.Language.ToLower() == "it") { imageControl.ToolTip = LanguageManager.IT_ReplaceExistingImagesToolTip; }
                else if (Settings.Default.Language.ToLower() == "en") { imageControl.ToolTip = LanguageManager.EN_ReplaceExistingImagesToolTip; }
            }
        }
    }
}