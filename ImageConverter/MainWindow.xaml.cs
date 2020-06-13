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
        List<string> droppedFilesToConvert;
        /// <summary>
        /// Paths of the images to convert that get passed to the ImageConversionHandler
        /// </summary>
        List<string> pathsOfImagesToConvert = new List<string>();
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
        /// List of labels in the Options stackpanel
        /// </summary>
        List<Label> labels = new List<Label>();


        public MainWindow()
        {
            InitializeComponent();

            #region Apply theme type, colors and hide or show controls
            MainWindowGrid.Background = ThemeManager.SelectedThemeMode();
            TitleTextBox.Foreground = ThemeManager.SelectedThemeColor();
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
                                ((Label)label).Background = ThemeManager.SelectedThemeColor();
                            }
                        }
                    }
                }
            }
            //If the selected ThemeMode is DarkTheme the ThemeColor will be applied to the text of all the labels
            if (Settings.Default.ThemeMode == "DarkTheme")
            {
                foreach (Label label in FindLabelsInStackPanel(OptionsStackPanel))
                {
                    label.Foreground = ThemeManager.SelectedThemeColor();
                }
                ConversionResultTextBlock.Foreground = ThemeManager.SelectedThemeColor();
            }

            //Hide some controls that should not viewable at the initial state of the app
            AddOrReplaceDroppedImagesBttn.Source = new BitmapImage(new System.Uri("pack://application:,,,/Resources/ReplaceImages.png"));
            ConversionResultTextBlock.Visibility = Visibility.Collapsed;
            WarningLabel.Content = String.Empty;
            GifOptionsSP.Visibility = Visibility.Collapsed;
            ReplaceTransparencySP.Visibility = Visibility.Collapsed;
            QualityCompressionOptionSP.Visibility = Visibility.Collapsed;
            CompressionAlgoOptionSP.Visibility = Visibility.Collapsed;
            #endregion

            #region Apply translation to all the visible controls
            if (Settings.Default.Language == "it")
            {
                ImgViewer.Source = new BitmapImage(new System.Uri("pack://application:,,,/Resources/ImageConverterDragAndDropIT.jpg"));
                StartConversionBttn.ButtonText = LanguageManager.IT_StartConversionLabelTxt;
                ChooseFormatLabel.Content = LanguageManager.IT_ChooseFormatLabelTxt;
                ConversionResultTextBlock.Text = LanguageManager.IT_ConversionResultTextBlockRunning;
                GifRepeatTimes.Content = LanguageManager.IT_GifLoopsOptionText;
                EmptyImgViewerCntxtMenuBttn.Header = LanguageManager.IT_EmpyBttnCntxtMenu;
                GifFramesDelayTimeLabel.Content = LanguageManager.IT_DelayTimeLabelTxt;
                AddOrReplaceDroppedImagesBttn.ToolTip = LanguageManager.IT_ReplaceExistingImagesToolTip;
                ReplacePngTransparencyLabel.Content = LanguageManager.IT_ReplacePngTransparencyLabelTxt;
                QualityLabel.Content = LanguageManager.IT_QualityLabelText;
                CompressionAlgoLabel.Content = LanguageManager.IT_CompressionAlgoLabelText;
                NoneAlgoLabel.Content = LanguageManager.IT_NoneAlgoLabelText;
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
                StartConversionBttn.ButtonText = LanguageManager.EN_StartConversionLabelTxt;
                ChooseFormatLabel.Content = LanguageManager.EN_ChooseFormatLabelTxt;
                ConversionResultTextBlock.Text = LanguageManager.EN_ConversionResultTextBlockRunning;
                GifRepeatTimes.Content = LanguageManager.EN_GifLoopsOptionText;
                EmptyImgViewerCntxtMenuBttn.Header = LanguageManager.EN_EmpyBttnCntxtMenu;
                GifFramesDelayTimeLabel.Content = LanguageManager.EN_DelayTimeLabelTxt;
                AddOrReplaceDroppedImagesBttn.ToolTip = LanguageManager.EN_ReplaceExistingImagesToolTip;
                ReplacePngTransparencyLabel.Content = LanguageManager.EN_ReplacePngTransparencyLabelTxt;
                QualityLabel.Content = LanguageManager.EN_QualityLabelText;
                CompressionAlgoLabel.Content = LanguageManager.EN_CompressionAlgoLabelText;
                NoneAlgoLabel.Content = LanguageManager.EN_NoneAlgoLabelText;

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

            //if the folder for ImageConverter in the temp direcory has been deleted create it again
            if (!Directory.Exists($"{Path.GetTempPath()}\\ImageConverter"))
            {
                Directory.CreateDirectory($"{Path.GetTempPath()}ImageConverter");
                Settings.Default.TempFolderPath = $"{Path.GetTempPath()}ImageConverter";
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            if (Settings.Default.FirstRun == true)
            {
                //set app language based off the default pc language 
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
                if (CheckIfGivenFilesAreImages(droppingFiles) == false)
                {
                    if (Settings.Default.Language == "it") { WarningLabel.Content = LanguageManager.IT_WarningUnsupportedFile; }
                    else if (Settings.Default.Language == "en") { WarningLabel.Content = LanguageManager.EN_WarningUnsupportedFile; }
                }
                //Check if the any of the dropping-files are already present in the list of images to convert
                if (pathsOfImagesToConvert.Count == 0)
                    return;
                foreach (var file in droppingFiles)
                {
                    if (pathsOfImagesToConvert.Contains(file) && (string)AddOrReplaceDroppedImagesBttn.Tag == "Add")
                    {
                        if (Settings.Default.Language == "it") { WarningLabel.Content = LanguageManager.IT_SomeImagesAreAlreadyPresent; }
                        else if (Settings.Default.Language == "en") { WarningLabel.Content = LanguageManager.EN_SomeImagesAreAlreadyPresent; }
                        break;
                    }
                }
            }
        }

        private void ImgViewer_DragLeave(object sender, DragEventArgs e)
        {
            droppedFileisValidDirectory = false;
            WarningLabel.Content = String.Empty; //nasconde l'avviso che appare se l'utente sta tentando di convertire più di un'immagine
        }

        private void ImgViewer_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) != null)
            {
                //---if the warning label content is NOT empty the dropped file there must be a problem with the dropped files, so ignore the dropped files---
                if ((string)WarningLabel.Content != String.Empty)
                {
                    WarningLabel.Content = String.Empty;
                    return;
                }

                #region Resets and adjust various GUI controls
                ThemeManager.solidColorBrush = new SolidColorBrush
                {
                    Color = ThemeManager.RunningOrStaticConversionTextBlockColor
                };
                ConversionResultTextBlock.Foreground = ThemeManager.solidColorBrush;

                ReplaceTransparencySP.Visibility = Visibility.Collapsed;
                ConversionResultTextBlock.Visibility = Visibility.Collapsed;
                QualityCompressionOptionSP.Visibility = Visibility.Visible;
                ImagesNameLabel.Text = string.Empty;

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
                GetImagesToConvertAndPrepareGUI(droppedFilesToConvert);
                LoadPreviewImage(pathsOfImagesToConvert);

                StartConversionBttn.IsEnabled = true;
            }
        }

        private async void StartConversionBttn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //if a format hasn't been selected prompt user to select one and stop conversion start
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
            foreach (var image in pathsOfImagesToConvert)
            {
                if (!Directory.GetFiles(Path.GetDirectoryName(pathsOfImagesToConvert[0])).Contains(image))
                {
                    if (Settings.Default.Language.ToLower() == "it") { MessageBox.Show(LanguageManager.IT_CantFindDroppedImagesInOriginalFolder, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                    if (Settings.Default.Language.ToLower() == "en") { MessageBox.Show(LanguageManager.EN_CantFindDroppedImagesInOriginalFolder, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                    return;
                }
            }

            #region Prepares GUI controls
            if (Settings.Default.Language == "it") ConversionResultTextBlock.Text = LanguageManager.IT_ConversionResultTextBlockRunning;
            if (Settings.Default.Language == "en") ConversionResultTextBlock.Text = LanguageManager.EN_ConversionResultTextBlockRunning;
            ConversionResultTextBlock.Visibility = Visibility.Collapsed; //necessary because if the user converts one image two times in a row it would seem like the conversion didn't start
            ThemeManager.solidColorBrush.Color = ThemeManager.RunningOrStaticConversionTextBlockColor;
            ConversionResultTextBlock.Foreground = ThemeManager.solidColorBrush; //sets the color of the textblock
            ConversionResultTextBlock.Visibility = Visibility.Visible; //makes the label of the state of the conversion visible
            StartConversionBttn.IsEnabled = false; //while a conversion is ongoing the convertbttn gets disabled
            #endregion

            finishedConversions = new Dictionary<string, bool>();

            // Prepare image conversion parameters
            ImageConversionParametersModel conversionParameters = new ImageConversionParametersModel()
            {
                format = ((FormatComboBox.SelectedItem as Label).Content as string).ToLower(),
                pathsOfImagesToConvert = pathsOfImagesToConvert,
                gifRepeatTimes = repGifTimes,
                colorToReplTheTranspWith = ReplTranspColCB.SelectedIndex,
                delayTime = Convert.ToInt32((GifFramesDelayOptionsCB.SelectedItem as Label).Content) / 10,
                qualityLevel = Convert.ToInt32(QualityLevelTextBox.Text.Trim('%', ' ')),
                compressionAlgo = (FormatComboBox.SelectedItem as Label)?.Content.ToString().ToLower(),
            };

            //adds a dot each 500ms during conversion
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
            ticker.Start();
            timer.Start();

            finishedConversions = await Task.Run(() => ImageConversionHandler.StartConversionAsync(conversionParameters));

            timer.Stop();
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
            ticker.Abort();
            #region Displays the result(s) of the conversion(s)
            //if there were no errors
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
            }
            //if there was any error
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
            }
            #endregion
            timer.Reset();

            StartConversionBttn.IsEnabled = true; //re-enables the convertbttn to convert another image
            Thread.Sleep(500); //Add delay otherwise if the user pressed the button right after re-enabling it, it would become black
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

            if (selectedValue == "GIF")
                GifOptionsSP.Visibility = Visibility.Visible;
            else if (selectedValue == "TIFF")
                CompressionAlgoOptionSP.Visibility = Visibility.Visible;
            else
            {
                GifOptionsSP.Visibility = Visibility.Collapsed;
                CompressionAlgoOptionSP.Visibility = Visibility.Collapsed;
            }

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
            pathsOfImagesToConvert.Clear();
            droppedFilesToConvert.Clear();
            EmptyImgViewerCntxtMenuBttn.IsEnabled = false;
            foreach (var control in OptionsStackPanel.Children)
            {
                if (((StackPanel)control).Name != "ChooseFormatSP")
                {
                    ((StackPanel)control).Visibility = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// Determines wether the dropped images replace or add up to the previous dropped images
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddOrReplaceDroppedImagesBttn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image imageControl = sender as Image;

            //Chande button option when the user clicks it
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

        #region Utility methods
        /// <summary>
        /// Finds all labels in a stackpanel
        /// </summary>
        /// <param name="stackpanel"></param>
        /// <returns>Returns a list containing all the labels</returns>
        private List<Label> FindLabelsInStackPanel(StackPanel stackpanel)
        {
            foreach (var control in stackpanel.Children)
            {
                if (control?.GetType() == typeof(StackPanel))
                {
                    FindLabelsInStackPanel((StackPanel)control);
                }
                else if (control?.GetType() == typeof(Label))
                {
                    labels.Add(control as Label);
                }
            }
            return labels;
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
                //check if the file is a folder and check if it contains any image, if yes then the files are ok to convert
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
                }
                //if it's not a folder or it doesn't contain any images
                else { droppedFileisValidDirectory = false; }

                //if the file isn't an image
                if (!ImageConversionHandler.IsImage(file) && !droppedFileisValidDirectory)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets the images dropped by the user on the ImgViewer control and prepares the GUI consequently
        /// </summary>
        /// <param name="droppedFilesToConvert"> Files dropped on the ImgViewer control, if the user drops a folder it would be the first element</param>
        public void GetImagesToConvertAndPrepareGUI(List<string> droppedFilesToConvert)
        {
            //If the user wants to replace the already dropped images
            if ((string)AddOrReplaceDroppedImagesBttn.Tag == "Replace")
            {
                //Empty list of images to convert
                pathsOfImagesToConvert = new List<string>();
            }
            foreach (var file in droppedFilesToConvert)
            {
                FileAttributes attr = File.GetAttributes(file);
                //If the file is a folder add the images inside it
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    var folder = file;
                    foreach (var image in Directory.GetFiles(folder))
                    {
                        pathsOfImagesToConvert.Add(image);
                    }
                }
                //add the image
                else { pathsOfImagesToConvert.Add(file); }
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
            //loads the preview image from a stream, if the image was used directly it would have remained in use even after emptying the ImgViewer and so it couldn't be (in case) deleted
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
        #endregion
    }
}