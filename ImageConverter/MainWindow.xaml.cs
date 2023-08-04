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
using System.Text.RegularExpressions;
using System.Linq;
using ImageConverter.Classes;
using System.Runtime.InteropServices;
using System.Windows.Documents.Serialization;

namespace ImageConverter
{
    /// <summary> 
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Paths of the dropped files on the ImgViewer
        /// </summary>
        List<string> droppedFilesToConvert;
        /// <summary>
        /// Paths of the images to convert that get passed to the ImageConversionHandler
        /// </summary>
        List<string> local_pathsOfImagesToConvert = new List<string>();
        /// <summary>
        /// List of images names with their finished conversions result, the bool indicates wether it was finished successfully or not
        /// </summary>
        Dictionary<string, bool> finishedConversions;
        /// <summary>
        /// Name of the images that didn't get converted
        /// </summary>
        List<string> unsuccessfulConversions;
        /// <summary>
        /// Selected value in the GifRepTimes ComboBox, the default value is zero(infinity)
        /// </summary>
        int repGifTimes = 0;
        /// <summary>
        /// Timer that measures the time spent converting all the images
        /// </summary>
        Stopwatch timer = new Stopwatch();
        /// <summary>
        /// Image that gets set as the ImgViewer source
        /// </summary>
        BitmapImage previewImage;
        /// <summary>
        /// Stream that loads the preview image into memory and subsequently sets it as the ImgViwer source
        /// </summary>
        FileStream previewImageStream;


        public MainWindow()
        {
            InitializeComponent();

            #region Checks wether the values of the settings are valid. If they aren't, set the default value
            if (Settings.Default.Language != "it" && Settings.Default.Language != "en")
            {
                Settings.Default.Language = "en";
                Settings.Default.Save();
                Settings.Default.Reload();
                Process.Start(Application.ResourceAssembly.Location); //Restart the application
                Application.Current.Shutdown(0);
            }
            if (ThemeManager.ThemeColors.Contains(Settings.Default.ThemeColor) == false)
            {
                Settings.Default.ThemeColor = ThemeManager.ThemeColors[0];
                Settings.Default.Save();
                Settings.Default.Reload();
                Process.Start(Application.ResourceAssembly.Location); //Restart the application
                Application.Current.Shutdown(0);
            }
            if (ThemeManager.ThemeModes.Contains(Settings.Default.ThemeMode) == false)
            {
                Settings.Default.ThemeMode = ThemeManager.ThemeModes[0];
                Settings.Default.Save();
                Settings.Default.Reload();
                Process.Start(Application.ResourceAssembly.Location); //Restart the application
                Application.Current.Shutdown(0);
            }
            if (Settings.Default.TempFolderPath == "" || Directory.Exists(Settings.Default.TempFolderPath) == false)
            {
                Directory.CreateDirectory($"{Path.GetTempPath()}ImageConverter");
                Settings.Default.TempFolderPath = $"{Path.GetTempPath()}ImageConverter";
                Settings.Default.Save();
                Settings.Default.Reload();
            }
            #endregion

            #region Apply theme type, colors and hide or show controls
            MainWindowGrid.Background = ThemeManager.SolidColorBrushOfSelectedThemeMode();
            TitleTextBox.Foreground = ThemeManager.SolidColorBrushOfSelectedThemeColor();
            ThemeManager.solidColorBrush = new SolidColorBrush()
            {
                Color = ThemeManager.RunningOrStaticConversionTextBlockColor,
            };
            ConversionResultTextBlock.Foreground = ThemeManager.solidColorBrush;

            //Applies the selected theme color to every label in the comboboxes
            foreach (var control in OptionsStackPanel.Children)
            {
                if (control.GetType() != typeof(StackPanel))
                    break;
                foreach (var innerControl in ((StackPanel)control).Children)
                {
                    if (innerControl.GetType() == typeof(ComboBox))
                    {
                        foreach (var label in ((ComboBox)innerControl).Items)
                        {
                            if (label.GetType() == typeof(Label))
                            {
                                ((Label)label).Background = ThemeManager.SolidColorBrushOfSelectedThemeColor();
                            }
                        }
                    }
                }
            }
            //If the selected ThemeMode is DarkTheme the ThemeColor will be applied to the text of all the labels
            if (Settings.Default.ThemeMode == "DarkTheme")
            {
                foreach (Label label in UtilityMethods.FindLabelsInStackPanel(OptionsStackPanel))
                {
                    label.Foreground = ThemeManager.SolidColorBrushOfSelectedThemeColor();
                }
                ConversionResultTextBlock.Foreground = ThemeManager.SolidColorBrushOfSelectedThemeColor();
            }

            //Hide some controls that should not viewable at the initial state of the app
            AddOrReplaceDroppedImagesBttn.Source = new BitmapImage(new System.Uri("pack://application:,,,/Resources/ReplaceImages.png"));
            ConversionResultTextBlock.Visibility = Visibility.Collapsed;
            WarningTextBlock.Text = string.Empty;
            GifOptionsSP.Visibility = Visibility.Collapsed;
            ReplaceTransparencySP.Visibility = Visibility.Collapsed;
            QualityOptionSP.Visibility = Visibility.Collapsed;
            CompressionAlgoOptionSP.Visibility = Visibility.Collapsed;
            SavePathOptionSP.Visibility = Visibility.Collapsed;
            #endregion
            #region Apply translation to all the visible controls
            if (Settings.Default.Language == "it")
            {
                ImgViewer.Source = new BitmapImage(new System.Uri("pack://application:,,,/Resources/ImageConverterDragAndDropIT.jpg"));
                StartConversionBttn.ButtonText = LanguageManager.IT_StartConversionBttnText;
                ChooseFormatLabel.Content = LanguageManager.IT_ChooseFormatLabelText;
                ConversionResultTextBlock.Text = LanguageManager.IT_ConversionResultTextBlockRunning;
                GifRepeatTimes.Content = LanguageManager.IT_GifRepeatTimesLabelText;
                EmptyImgViewerCntxtMenuBttn.Header = LanguageManager.IT_ImageViewerContextMenuText;
                GifFramesDelayTimeLabel.Content = LanguageManager.IT_GifFramesDelayTimeLabelText;
                AddOrReplaceDroppedImagesBttn.ToolTip = LanguageManager.IT_ReplaceExistingImagesToolTip;
                ReplacePngTransparencyLabel.Content = LanguageManager.IT_ReplacePngTransparencyLabelText;
                QualityLabel.Content = LanguageManager.IT_QualityLabelText;
                CompressionAlgoLabel.Content = LanguageManager.IT_CompressionAlgoLabelText;
                //Each item in the combobox that contains a series of colors to replace a png transparency with
                foreach (Label item in ReplTranspColCB.Items)
                {
                    switch (item.Content)
                    {
                        case "Nothing":
                            item.Content = LanguageManager.IT_Nothing;
                            break;
                        case "White":
                            item.Content = LanguageManager.IT_White;
                            break;
                        case "Black":
                            item.Content = LanguageManager.IT_Black;
                            break;
                        default:
                            return;
                    }
                }
            }
            else if (Settings.Default.Language == "en")
            {
                ImgViewer.Source = new BitmapImage(new System.Uri("pack://application:,,,/Resources/ImageConverterDragAndDropEN.png"));
                StartConversionBttn.ButtonText = LanguageManager.EN_StartConversionBttnText;
                ChooseFormatLabel.Content = LanguageManager.EN_ChooseFormatLabelText;
                ConversionResultTextBlock.Text = LanguageManager.EN_ConversionResultTextBlockRunning;
                GifRepeatTimes.Content = LanguageManager.EN_GifRepeatTimesLabelText;
                EmptyImgViewerCntxtMenuBttn.Header = LanguageManager.EN_ImageViewerContextMenuText;
                GifFramesDelayTimeLabel.Content = LanguageManager.EN_GifRepeatTimesLabelText;
                AddOrReplaceDroppedImagesBttn.ToolTip = LanguageManager.EN_ReplaceExistingImagesToolTip;
                ReplacePngTransparencyLabel.Content = LanguageManager.EN_ReplacePngTransparencyLabelText;
                QualityLabel.Content = LanguageManager.EN_QualityLabelText;
                CompressionAlgoLabel.Content = LanguageManager.EN_CompressionAlgoLabelText;

                //Each item in the combobox that contains a series of colors to replace a png transparency with
                foreach (Label item in ReplTranspColCB.Items)
                {
                    switch (item.Content)
                    {
                        case "Nothing":
                            item.Content = LanguageManager.EN_Nothing;
                            break;
                        case "White":
                            item.Content = LanguageManager.EN_White;
                            break;
                        case "Black":
                            item.Content = LanguageManager.EN_Black;
                            break;
                        default:
                            return;
                    }
                }
            }
            #endregion
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.FirstRun == true)
            {
                //Set app language based off the default pc language 
                if (CultureInfo.CurrentCulture.ToString().Contains("it"))
                {
                    Settings.Default.Language = "it";
                }
                else
                {
                    Settings.Default.Language = "en";
                }

                //Creates a folder for ImageConverter in the temp directory
                if (!Directory.Exists($"{Path.GetTempPath()}ImageConverter"))
                {
                    Directory.CreateDirectory($"{Path.GetTempPath()}ImageConverter");
                    Settings.Default.TempFolderPath = $"{Path.GetTempPath()}ImageConverter";
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
                string[] droppingFiles = e.Data.GetData(DataFormats.FileDrop) as string[]; //Get files the user is trying to convert
                //If the droopping files aren't images
                if (UtilityMethods.IsOrContainsImage(droppingFiles) == false)
                {
                    if (Settings.Default.Language == "it") { WarningTextBlock.Text = LanguageManager.IT_WarningUnsupportedFile; }
                    else if (Settings.Default.Language == "en") { WarningTextBlock.Text = LanguageManager.EN_WarningUnsupportedFile; }
                }

                //Check if the any of the dropping-files are already present in the list of images to convert
                if (local_pathsOfImagesToConvert.Count != 0 && (string)AddOrReplaceDroppedImagesBttn.Tag == "Add")
                {
                    foreach (var file in droppingFiles)
                    {
                        //if the file is a folder check the files inside it
                        if (File.GetAttributes(file) == FileAttributes.Directory)
                        {
                            foreach (var fileInFolder in Directory.GetFiles(file))
                            {
                                if (local_pathsOfImagesToConvert.Contains(fileInFolder))
                                {
                                    if (Settings.Default.Language == "it") { WarningTextBlock.Text = LanguageManager.IT_SomeImagesAreAlreadyPresent; }
                                    else if (Settings.Default.Language == "en") { WarningTextBlock.Text = LanguageManager.EN_SomeImagesAreAlreadyPresent; }
                                    break;
                                }
                            }
                        }
                        //Else check the files
                        else if (local_pathsOfImagesToConvert.Contains(file))
                        {
                            if (Settings.Default.Language == "it") { WarningTextBlock.Text = LanguageManager.IT_SomeImagesAreAlreadyPresent; }
                            else if (Settings.Default.Language == "en") { WarningTextBlock.Text = LanguageManager.EN_SomeImagesAreAlreadyPresent; }
                            break;
                        }

                    }
                }
            }
        }

        private void ImgViewer_DragLeave(object sender, DragEventArgs e)
        {
            WarningTextBlock.Text = string.Empty; //Hides warning label by emptying it
        }

        private void ImgViewer_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) != null)
            {
                //---If the warning label content is NOT empty there must be a problem with the file(s) the user wants to convert, so ignore the dropped files---
                if ((string)WarningTextBlock.Text != string.Empty)
                {
                    WarningTextBlock.Text = string.Empty;
                    return;
                }

                #region Resets and adjust various GUI controls properties
                ThemeManager.solidColorBrush = new SolidColorBrush
                {
                    Color = ThemeManager.RunningOrStaticConversionTextBlockColor
                };
                ConversionResultTextBlock.Foreground = ThemeManager.solidColorBrush;

                ReplaceTransparencySP.Visibility = Visibility.Collapsed;
                ConversionResultTextBlock.Visibility = Visibility.Collapsed;
                QualityOptionSP.Visibility = Visibility.Visible;
                ImagesNameTextBlock.Text = string.Empty;
                SavePathOptionSP.Visibility = Visibility.Visible;

                if (FormatComboBox.SelectedValue?.ToString() != "System.Windows.Controls.Label: GIF")
                {
                    GifOptionsSP.Visibility = Visibility.Collapsed;
                }
                #endregion

                droppedFilesToConvert = new List<string>();

                //Gets the dropped files and folders
                foreach (var file in e.Data.GetData(DataFormats.FileDrop) as string[])
                {
                    droppedFilesToConvert.Add(file);
                }
                //Gets all the images and the ones in the folders because the warning label is hidden, so the dropped files must be images
                local_pathsOfImagesToConvert = GetImagesToConvertAndPrepareGUI(droppedFilesToConvert);
                LoadPreviewImage(local_pathsOfImagesToConvert);

                //Set default savePath as the parent folder of the first image
                SavePathTextBlock.Text = Path.GetDirectoryName(local_pathsOfImagesToConvert[0]);
                StartConversionBttn.IsEnabled = true;
            }
        }

        private async void StartConversionBttn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //If a format hasn't been selected prompt user to select one and stop conversion start
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
            }
            //If one or more images aren't in their original folder anymore stop conversion start
            foreach (var image in local_pathsOfImagesToConvert)
            {
                if (!Directory.GetFiles(Path.GetDirectoryName(local_pathsOfImagesToConvert[0])).Contains(image))
                {
                    if (Settings.Default.Language.ToLower() == "it") { MessageBox.Show(LanguageManager.IT_CantFindDroppedImagesInOriginalFolder, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                    if (Settings.Default.Language.ToLower() == "en") { MessageBox.Show(LanguageManager.EN_CantFindDroppedImagesInOriginalFolder, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                    return;
                }
            }

            #region Prepares GUI controls
            if (Settings.Default.Language == "it") ConversionResultTextBlock.Text = LanguageManager.IT_ConversionResultTextBlockRunning;
            if (Settings.Default.Language == "en") ConversionResultTextBlock.Text = LanguageManager.EN_ConversionResultTextBlockRunning;
            ConversionResultTextBlock.Visibility = Visibility.Collapsed; //Necessary because if the user converts one image two times in a row it would seem like the conversion didn't start
            ThemeManager.solidColorBrush.Color = ThemeManager.RunningOrStaticConversionTextBlockColor;
            ConversionResultTextBlock.Foreground = ThemeManager.solidColorBrush; //Sets the color of the textblock
            ConversionResultTextBlock.Visibility = Visibility.Visible; //Makes the label of the state of the conversion visible
            StartConversionBttn.IsEnabled = false; //While a conversion is ongoing the convertbttn gets disabled
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
            });
            ticker.IsBackground = true;

            #endregion

            finishedConversions = new Dictionary<string, bool>();

            try
            {
                //Prepare image conversion parameters
                ImageConversionParametersModel conversionParameters = new ImageConversionParametersModel()
                {
                    format = ((FormatComboBox.SelectedItem as Label).Content as string).ToLower(),
                    pathsOfImagesToConvert = local_pathsOfImagesToConvert,
                    gifRepeatTimes = repGifTimes,
                    colorToReplTheTranspWith = ReplTranspColCB.SelectedIndex,
                    delayTime = Convert.ToInt32((GifFramesDelayOptionsCB.SelectedItem as Label).Content) / 10,
                    qualityLevel = Convert.ToInt32(QualityLevelTextBox.Text.Trim('%', ' ')),
                    compressionAlgo = (CompressionTypesCB.SelectedItem as Label)?.Content.ToString().ToLower(),
                    savePath = SavePathTextBlock.Text,
                };
                #region Adds a dot each 500ms during conversion
                ticker.Start();
                timer.Start();
                #endregion

                //Start conversion
                ImageConversionHandler imgConvHandler = new ImageConversionHandler();
                finishedConversions = await Task.Run(() => imgConvHandler.StartConversionAsync(conversionParameters));

                timer.Stop();
                ticker.Abort();


                #region Counts the unsuccessful conversions
                unsuccessfulConversions = new List<string>();
                int i = 0;
                foreach (var conversion in finishedConversions)
                {
                    if (conversion.Value == false)
                    {
                        unsuccessfulConversions.Add(Path.GetFileName(conversion.Key));
                    }
                    i++;
                }
                #endregion
                #region Displays the result(s) of the conversion(s)
                //If there were no errors
                if (unsuccessfulConversions.Count == 0)
                {
                    ThemeManager.solidColorBrush = new SolidColorBrush
                    {
                        Color = ThemeManager.CompletedConversionTextBlockColor
                    };
                    ConversionResultTextBlock.Foreground = ThemeManager.solidColorBrush;
                    if (Settings.Default.Language == "it")
                    {
                        if (local_pathsOfImagesToConvert.Count == 1) ConversionResultTextBlock.Text = LanguageManager.IT_ConversionResultTextBlockFinishedText;
                        else ConversionResultTextBlock.Text = LanguageManager.IT_MultipleConversionResultTextBlockFinishedText;
                    }
                    else if (Settings.Default.Language == "en")
                    {
                        ConversionResultTextBlock.Text = LanguageManager.EN_ConversionResultTextBlockFinishedText;
                    }
                    ConversionResultTextBlock.Text += $" in {(int)timer.Elapsed.TotalMilliseconds}ms"; //Time taken to convert the images in milliseconds
                }
                //If there was any error
                else
                {
                    ThemeManager.solidColorBrush = new SolidColorBrush
                    {
                        Color = ThemeManager.CompletedWithErrorsConversionTextBlockColor
                    };
                    ConversionResultTextBlock.Foreground = ThemeManager.solidColorBrush;
                    if (Settings.Default.Language == "it")
                    {
                        ConversionResultTextBlock.Text = LanguageManager.IT_UnsuccConversionResultTextBlockFinishedText;
                    }
                    else if (Settings.Default.Language == "en")
                    {
                        ConversionResultTextBlock.Text = LanguageManager.EN_UnsuccConversionResultTextBlockFinishedText;
                    }
                    foreach (var conversion in unsuccessfulConversions)
                    {
                        ConversionResultTextBlock.Text += conversion + ", ";
                    }
                }
                #endregion
                timer.Reset();
            }
            catch (Exception ex)
            {
                timer.Stop();
                timer.Reset();
                ticker.Abort();
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ConversionResultTextBlock.Text = String.Empty;
            }

            Thread.Sleep(500); //Add delay otherwise if the user pressed the button right after re-enabling it, it would become black
            StartConversionBttn.IsEnabled = true; //Re-enables the convertbttn to convert another image
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

        #region Dropdown menus methods
        /// <summary>
        /// Sets the format to convert the images to
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormatComboBox_DropDownClosed(object sender, System.EventArgs e)
        {
            /* The null coalescing operator (?.) is needed beacause if the user closes the combobox menu without 
             * selecting a format the selected item would be null and its content can't be accessed, casuing a NullReferenceException */
            var selectedValue = (((ComboBox)sender).SelectedItem as Label)?.Content.ToString();
            if (selectedValue == null)
                return;

            if (selectedValue == "GIF")
            {
                GifOptionsSP.Visibility = Visibility.Visible;
            }
            else
                GifOptionsSP.Visibility = Visibility.Collapsed;

            if (selectedValue == "TIFF")
            {
                CompressionAlgoOptionSP.Visibility = Visibility.Visible;
            }
            else
                CompressionAlgoOptionSP.Visibility = Visibility.Collapsed;

        }

        /// <summary>
        /// Sets the value for how many times the gif shall repeat, !When the ComboBox closes it means that an element has been selected!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GifRepTimesCB_DropDownClosed(object sender, EventArgs e)
        {
            var selectedValue = (((ComboBox)sender).SelectedItem as Label)?.Content.ToString();
            if (selectedValue == "∞")
            {
                repGifTimes = 0;
                return;
            }
            repGifTimes = Convert.ToInt32(selectedValue);
        }
        #endregion


        /// <summary>
        /// If there are images to convert enable the EmptyImgViewerMenuBttn in the context menu of the ImgViewer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageViewerContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (local_pathsOfImagesToConvert.Count != 0)
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
            ImagesNameTextBlock.Text = string.Empty;
            ImgViewer.Opacity = 0.3f;
            local_pathsOfImagesToConvert.Clear();
            droppedFilesToConvert.Clear();
            previewImage = null;
            previewImageStream?.Dispose();
            previewImageStream?.Close();
            //Hide all conversion options
            foreach (var control in OptionsStackPanel.Children)
            {
                if (((StackPanel)control).Name != "ChooseFormatSP")
                {
                    ((StackPanel)control).Visibility = Visibility.Collapsed;
                }
            }
            EmptyImgViewerCntxtMenuBttn.IsEnabled = false;
        }

        /// <summary>
        /// Determines wether the dropped images replace or add up to the previous dropped images
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddOrReplaceDroppedImagesBttn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image imageControl = sender as Image;

            //Change button option when the user clicks it
            if ((string)imageControl.Tag == "Replace")
            {
                imageControl.Tag = "Add";
                imageControl.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/AddImages.png"));
                if (Settings.Default.Language.ToLower() == "it") { imageControl.ToolTip = LanguageManager.IT_AddToExistingImagesToolTip; }
                else if (Settings.Default.Language.ToLower() == "en") { imageControl.ToolTip = LanguageManager.EN_AddToExistingImagesToolTip; }
            }
            else
            {
                imageControl.Tag = "Replace";
                imageControl.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/ReplaceImages.png"));
                if (Settings.Default.Language.ToLower() == "it") { imageControl.ToolTip = LanguageManager.IT_ReplaceExistingImagesToolTip; }
                else if (Settings.Default.Language.ToLower() == "en") { imageControl.ToolTip = LanguageManager.EN_ReplaceExistingImagesToolTip; }
            }
        }

        /// <summary>
        /// Sanitize text of QualityLevelTextBox if there are letters or other special characters present, if the value exceeds 100 or is lower than 1.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QualityLevelTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = QualityLevelTextBox.Text;
            int value;
            if (int.TryParse(text.Trim('%'), out value) && value < 100 && value >= 0 && text.Contains("%"))
                return;
            QualityLevelTextBox.Text = Regex.Replace(QualityLevelTextBox.Text, "[^0-9]", "") + "%";
            if (value > 100) { QualityLevelTextBox.Text = "100%"; }
            else if (value < 1) { QualityLevelTextBox.Text = "0%"; }
        }


        /// <summary>
        /// Gets the images dropped by the user on the ImgViewer control and prepares the GUI consequently
        /// </summary>
        /// <param name="droppedFilesToConvert"> Files dropped on the ImgViewer control, if the user drops a folder it would be the first element</param>
        public List<string> GetImagesToConvertAndPrepareGUI(List<string> droppedFilesToConvert)
        {
            List<string> newPathsOfImagesToConvert = new List<string>();

            //Gets the new dropped images
            foreach (var filePath in droppedFilesToConvert)
            {
                FileAttributes attr = File.GetAttributes(filePath);
                //If the file is a folder add the images inside it
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    var folder = filePath;
                    foreach (var image in Directory.GetFiles(folder))
                    {
                        newPathsOfImagesToConvert.Add(image);
                    }
                }
                //Add the image
                else { newPathsOfImagesToConvert.Add(filePath); }
            }

            //Checks if any image is a png, if so enable the option to replace its transparency
            foreach (string imagePath in newPathsOfImagesToConvert)
            {
                if (Path.GetExtension(imagePath).ToLower() == ".png")
                {
                    ReplaceTransparencySP.Visibility = Visibility.Visible;
                }
            }

            #region Adds or sets name(s) of the image(s) to the ImagesNamesTextBlock under ImgViewer
            //If the user wants to replace the already dropped images, clear the textblock
            if ((string)AddOrReplaceDroppedImagesBttn.Tag == "Replace" && ImagesNameTextBlock.Text != string.Empty)
                ImagesNameTextBlock.Text = string.Empty;

            //Add the images paths
            for (int i = 0; i < newPathsOfImagesToConvert.Count; i++)
            {
                string imageName = Path.GetFileName(newPathsOfImagesToConvert[i]);
                
                if (newPathsOfImagesToConvert.Count != 1 && i == newPathsOfImagesToConvert.Count - 1)
                    ImagesNameTextBlock.Text += $"{imageName}";
                else
                    ImagesNameTextBlock.Text += $"{imageName}, ";
            }
            #endregion

            if ((string)AddOrReplaceDroppedImagesBttn.Tag == "Replace")
                return newPathsOfImagesToConvert;
            else if ((string)AddOrReplaceDroppedImagesBttn.Tag == "Add")
                return local_pathsOfImagesToConvert.Concat(newPathsOfImagesToConvert).ToList();
            else
                return null;

        }

        /// <summary>
        /// Loads the image to see in the ImgViewer control
        /// </summary>
        /// <param name="pathsOfImagesToConvert"></param>
        public void LoadPreviewImage(List<string> pathsOfImagesToConvert)

        {
            previewImageStream?.Dispose();
            previewImageStream?.Close();
            previewImage = null;

            ImgViewer.Opacity = 1.0f; //Sets ImgViewer opacity to 1(max)
            //Loads the preview image from a stream, if the image was used directly it would have remained in use even after emptying the ImgViewer and so it couldn't be (in case) deleted
            using (previewImageStream = File.OpenRead(pathsOfImagesToConvert[0]))
            {
                previewImage = new BitmapImage();
                previewImage.BeginInit();
                previewImage.StreamSource = previewImageStream;
                previewImage.CacheOption = BitmapCacheOption.OnLoad;
                //Reduce height resolution of image
                previewImage.DecodePixelHeight = (int)ImgViewer.Height;
                previewImage.EndInit();

                ImgViewer.Source = previewImage;
                //Freeze bitmapimage to make in umodifiable and prevent the system to spend more resources on it
                previewImage.Freeze();
                previewImageStream.Close();
            }
        }

        private void ChooseFolderBttn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog browserDialog = new System.Windows.Forms.FolderBrowserDialog();
            browserDialog.ShowNewFolderButton = true;
            browserDialog.RootFolder = Environment.SpecialFolder.Desktop;
            if (Settings.Default.Language == "it") { browserDialog.Description = LanguageManager.IT_BrowserDialogDescription; }
            else { browserDialog.Description = LanguageManager.EN_BrowserDialogDescription; }
            var dialogResult = browserDialog.ShowDialog();
            if (dialogResult == System.Windows.Forms.DialogResult.OK)
            {
                SavePathTextBlock.Text = browserDialog.SelectedPath;
                browserDialog.Dispose();
            }
        }
    }
}