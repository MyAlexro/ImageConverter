﻿/*MIT License
*Copyright (c) 2023 Alessandro Dinardo
*
*Permission is hereby granted, free of charge, to any person obtaining a copy
*of this software and associated documentation files (the "Software"), to deal
*in the Software without restriction, including without limitation the rights
*to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
*copies of the Software, and to permit persons to whom the Software is
*furnished to do so, subject to the following conditions:

*The above copyright notice and this permission notice shall be included in all
*copies or substantial portions of the Software.
*
*THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
*IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
*FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
*AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
*LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
*OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

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
using ImageConverter.HelperClasses;

namespace ImageConverter
{
    /// <summary> 
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Paths of the images to convert that get passed to ImageConversionHandler
        /// </summary>
        List<string> local_pathsOfImagesToConvert = new List<string>();
        /// <summary>
        /// List of images names with their finished conversions result, the bool indicates wether it was finished successfully or not
        /// </summary>
        Dictionary<string, bool> finishedConversions;
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
        /// Stream that loads the preview image into memory and subsequently sets it as the ImgViewer source
        /// </summary>
        FileStream previewImageStream;
        (int width, int height) originalImgDimensions;


        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            DebuggingTools DebuggingTools = new DebuggingTools();
            Debug.WriteLine("DEBUG MODE. DEBUGGING TOOLS HAVE BEEN INSTANTIATED");
#endif

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
                Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown(0);
            }
            if (ThemeManager.ThemeModes.Contains(Settings.Default.ThemeMode) == false)
            {
                Settings.Default.ThemeMode = ThemeManager.ThemeModes[0];
                Settings.Default.Save();
                Settings.Default.Reload();
                Process.Start(Application.ResourceAssembly.Location);
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

            #region Apply theme type and themecolor
            MainWindowGrid.Background = ThemeManager.SolidColorBrushOfThemeMode();
            TitleTextBox.Foreground = ThemeManager.SolidColorBrushOfThemeColor();
            ThemeManager.solidColorBrush = new SolidColorBrush()
            {
                Color = ThemeManager.RunningOrStaticConversionTextBlockColor,
            };
            ConversionResultTextBlock.Foreground = ThemeManager.solidColorBrush;

            //Applies the selected theme color to every label's background in every ComboBox
            foreach (var control in OptionsStackPanel.Children)
            {
                foreach (var label in UtilityMethods.FindLabelsInComboBoxesInSPs(ref OptionsStackPanel))
                {
                    label.Background = ThemeManager.SolidColorBrushOfThemeColor();
                }
            }

            //If the selected ThemeMode is DarkTheme the ThemeColor will be applied to the text of all the labels in the options stackpanel
            if (Settings.Default.ThemeMode == "DarkTheme")
            {
                ThemeManager.ApplyThemeColorToLabelsInSP(ref OptionsStackPanel);
                ImagesNamesTextBlock.Foreground = ThemeManager.SolidColorBrushOfThemeColor();
                ConversionResultTextBlock.Foreground = ThemeManager.SolidColorBrushOfThemeColor();
                IconSizesTextBlock.Foreground = ThemeManager.SolidColorBrushOfThemeColor();
                foreach (CheckBox checkBox in IconSizesMatrixGrid.Children)
                {
                    checkBox.Foreground = ThemeManager.SolidColorBrushOfThemeColor();
                }
            }
            #endregion
            #region Hide some controls that should not viewable at the initial state of the app
            InsertNewImagesModeBttn.Source = new BitmapImage(new System.Uri("pack://application:,,,/Resources/ReplaceImages.png"));
            ConversionResultTextBlock.Visibility = Visibility.Collapsed;
            WarningTextBlock.Text = string.Empty;
            GifOptionsSP.Visibility = Visibility.Collapsed;
            ReplaceTransparencySP.Visibility = Visibility.Collapsed;
            QualityOptionSP.Visibility = Visibility.Collapsed;
            TiffOptionsSP.Visibility = Visibility.Collapsed;
            IcoOptionsSP.Visibility = Visibility.Collapsed;
            SavePathOptionSP.Visibility = Visibility.Collapsed;
            ResizingOptionSP.Visibility = Visibility.Collapsed;
            #endregion
            #region Apply translation to all the visible controls
            if (Settings.Default.Language == "it")
            {
                ImgViewer.Source = new BitmapImage(new System.Uri("pack://application:,,,/Resources/ImageConverterDragAndDropIT.png"));
                StartConversionBttn.ButtonText = LanguageManager.IT_StartConversionBttnText;
                ChooseFormatLabel.Content = LanguageManager.IT_ChooseFormatLabelText;
                ConversionResultTextBlock.Text = LanguageManager.IT_ConversionResultTextBlockRunning;
                GifRepeatTimes.Content = LanguageManager.IT_GifRepeatTimesLabelText;
                EmptyImgViewerCntxtMenuBttn.Header = LanguageManager.IT_ImageViewerContextMenuText;
                GifFramesDelayTimeLabel.Content = LanguageManager.IT_GifFramesDelayTimeLabelText;
                InsertNewImagesModeBttn.ToolTip = LanguageManager.IT_ReplaceExistingImagesToolTip;
                ReplacePngTransparencyLabel.Content = LanguageManager.IT_ReplacePngTransparencyLabelText;
                QualityLabel.Content = LanguageManager.IT_QualityLabelText;
                CompressionAlgoLabel.Content = LanguageManager.IT_CompressionAlgoLabelText;
                ImageSavePathLabel.Content = LanguageManager.IT_ImageSavePathLabelText;
                ChooseFolderBttn.Content = LanguageManager.IT_ChooseFolderBttnText;
                IconSizesTextBlock.Text = LanguageManager.IT_IconSizesTextBlockText;
                ResizingOptionLabel.Content = LanguageManager.IT_ResizingOptionLabelText;
                WidthResLabel.Content = LanguageManager.IT_WidthResLabelText;
                HeightResLabel.Content = LanguageManager.IT_HeightResLabelText;
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
                GifFramesDelayTimeLabel.Content = LanguageManager.EN_GifFramesDelayTimeLabelText;
                InsertNewImagesModeBttn.ToolTip = LanguageManager.EN_ReplaceExistingImagesToolTip;
                ReplacePngTransparencyLabel.Content = LanguageManager.EN_ReplacePngTransparencyLabelText;
                QualityLabel.Content = LanguageManager.EN_QualityLabelText;
                CompressionAlgoLabel.Content = LanguageManager.EN_CompressionAlgoLabelText;
                ImageSavePathLabel.Content = LanguageManager.EN_ImageSavePathLabelText;
                ChooseFolderBttn.Content = LanguageManager.EN_ChooseFolderBttnText;
                IconSizesTextBlock.Text = LanguageManager.EN_IconSizesTextBlockText;
                ResizingOptionLabel.Content = LanguageManager.EN_ResizingOptionLabelText;
                WidthResLabel.Content = LanguageManager.EN_WidthResLabelText;
                HeightResLabel.Content = LanguageManager.EN_HeightResLabelText;
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
                    Settings.Default.Language = "it";
                else
                    Settings.Default.Language = "en";

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

        #region ImgViewer and related controls methods
        /// <summary>
        /// Checks wether the files the user wants to drop are images/contain valid images
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImgViewer_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) != null)
            {
                //Get files the user is trying to convert
                string[] droppingFiles = e.Data.GetData(DataFormats.FileDrop) as string[];
                //If the droopping files aren't images
                if (UtilityMethods.IsOrContainsImages(droppingFiles) == false)
                {
                    if (Settings.Default.Language == "it") { WarningTextBlock.Text = LanguageManager.IT_WarningUnsupportedFile; }
                    else if (Settings.Default.Language == "en") { WarningTextBlock.Text = LanguageManager.EN_WarningUnsupportedFile; }
                }

                //If the user wants to ADD (not replace) some new images to the already present ones, check if the any of the dropping-files 
                //are already present in the list of images to convert
                if (local_pathsOfImagesToConvert.Count != 0 && (string)InsertNewImagesModeBttn.Tag == "Add")
                {
                    foreach (var file in droppingFiles)
                    {
                        //if the file is a folder check the files inside it
                        if (File.GetAttributes(file) == FileAttributes.Directory)
                        {
                            var folder = new DirectoryInfo(file).GetFiles("*", SearchOption.AllDirectories);
                            foreach (var fileInFolder in folder)
                            {
                                if (local_pathsOfImagesToConvert.Contains(fileInFolder.FullName))
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

        /// <summary>
        /// Takes the dropped files and prepares GUI accordingly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImgViewer_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) != null)
            {
                //---If the warning label content is NOT empty there must be a problem with the file(s) the user wants to convert, so ignore the dropped files---
                if (WarningTextBlock.Text != string.Empty)
                {
                    WarningTextBlock.Text = string.Empty; //Empty it because otherwise the error would remain visible
                    return;
                }

                List<string> droppedFiles = new List<string>(); //Paths of the dropped files on the ImgViewer

                //Gets the dropped files and folders
                foreach (var file in e.Data.GetData(DataFormats.FileDrop) as string[])
                {
                    droppedFiles.Add(file);
                }
                //Gets all the images and the ones in the folders (because the warning label is hidden, the dropped files must be images)
                local_pathsOfImagesToConvert = GetImagesToConvertAndPrepareGUI(droppedFiles);
                LoadPreviewImage(local_pathsOfImagesToConvert);

                //Set default savePath as the parent folder of the first image
                SavePathTextBlock.Text = Path.GetDirectoryName(local_pathsOfImagesToConvert[0]);
                StartConversionBttn.IsEnabled = true;
            }
        }

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
            if (Settings.Default.Language == "it") { ImgViewer.Source = new BitmapImage(new System.Uri("pack://application:,,,/Resources/ImageConverterDragAndDropIT.png")); }
            else if (Settings.Default.Language == "en") { ImgViewer.Source = new BitmapImage(new System.Uri("pack://application:,,,/Resources/ImageConverterDragAndDropEN.png")); }
            StartConversionBttn.IsEnabled = false;
            ImagesNamesTextBlock.Text = string.Empty;
            ImgViewer.Opacity = 0.3f;
            local_pathsOfImagesToConvert.Clear();
            previewImage = null;
            previewImageStream?.Close();
            previewImageStream?.Dispose();
            FormatComboBox.SelectedIndex = -1;
            ConversionResultTextBlock.Text = string.Empty;
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
        /// Select the images to convert using a file dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImgViewer_MouseDown_SelectImages(object sender, MouseButtonEventArgs e)
        {
            if (local_pathsOfImagesToConvert.Count != 0) //Disable the selection of images if there are already some images to convert
                return;

            // "PNG", "JPG", "JPEG", "BMP", "GIF", "ICO", "CUR", "TIFF"
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            string images = "";
            if (Settings.Default.Language == "it") { dialog.Title = LanguageManager.IT_OpenFileDialogTitle; images = LanguageManager.IT_Images; }
            else if (Settings.Default.Language == "en") { dialog.Title = LanguageManager.EN_OpenFileDialogTitle; images = LanguageManager.EN_Images; }

            dialog.Filter = $"{images}|*.png; *.jpg; *.jpeg; *.bmp; *.gif; *.ico; *.cur; *.tiff|"
                            + "Png|*.png|"
                            + "Jpg|*.jpg|"
                            + "Jpeg|*.jpeg|"
                            + "Bmp|*.bmp|"
                            + "Gif|*.gif|"
                            + "Ico|*.ico|"
                            + "Cur|*.cur|"
                            + "Tiff|*.tiff";
            dialog.DereferenceLinks = true;
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            dialog.Multiselect = true;
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result != System.Windows.Forms.DialogResult.Cancel)
            {
                local_pathsOfImagesToConvert = GetImagesToConvertAndPrepareGUI(dialog.FileNames.ToList());
                LoadPreviewImage(local_pathsOfImagesToConvert);
            }
            dialog.Dispose();
            GC.Collect();
            StartConversionBttn.IsEnabled = true;
        }

        /// <summary>
        /// If the mouse enters the control and the user still hasn't chosen any images, set cursor hand
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImgViewer_MouseEnter(object sender, MouseEventArgs e)
        {
            if (local_pathsOfImagesToConvert.Count == 0)
                ImgViewer.Cursor = Cursors.Hand;
            else
                ImgViewer.Cursor = Cursors.Arrow;
        }
        #endregion

        #region Conversion options controls methods
        /// <summary>
        /// Sets the format to convert the images to
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormatComboBox_DropDownClosed(object sender, System.EventArgs e)
        {
            /* The null conditional operator (?.) is needed beacause if the user closes the combobox menu without 
             * selecting a format the selected item would be null and its content can't be accessed, casuing a NullReferenceException */
            var selectedValue = (((ComboBox)sender).SelectedItem as Label)?.Content.ToString();
            if (selectedValue == null)
                return;

            if (selectedValue == "GIF")
                GifOptionsSP.Visibility = Visibility.Visible;
            else
                GifOptionsSP.Visibility = Visibility.Collapsed;

            if (selectedValue == "TIFF")
                TiffOptionsSP.Visibility = Visibility.Visible;
            else
                TiffOptionsSP.Visibility = Visibility.Collapsed;

            if (selectedValue == "ICO" || selectedValue == "CUR")
                IcoOptionsSP.Visibility = Visibility.Visible;
            else
                IcoOptionsSP.Visibility = Visibility.Collapsed;

            if (selectedValue != "ICO" && selectedValue != "GIF" && selectedValue != "CUR")
                ResizingOptionSP.Visibility = Visibility.Visible;
            else
                ResizingOptionSP.Visibility = Visibility.Collapsed;

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

        /// <summary>
        /// Sanitize text of QualityLevelTextBox if there are letters or other special characters present, if the value exceeds 100 or is lower than 1.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QualityLevelTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            const int minLevel = ConversionParamsModel.minQualityLevel;
            const int maxLevel = ConversionParamsModel.maxQualityLevel;
            TextBox txtBox = (TextBox)sender;
            string text = txtBox.Text;
            int value;
            bool validInput = int.TryParse(text.Trim('%'), out value);

            if (validInput && value >= minLevel && value <= maxLevel && text[0] != '0' && text[text.Length - 1] == '%'
                && text[0] != '%' && text[text.Length - 1] == '%' && text[text.Length - 2] != '%' && text.Contains(" ") == false)
                return;

            if (value > maxLevel && validInput)
                txtBox.Text = maxLevel.ToString() + "%";
            else if ((value < minLevel && validInput) || text.Trim('%') == "")
                txtBox.Text = minLevel.ToString() + "%";

            //If the user deleted the % symbol 
            if (text[text.Length - 1] != '%' && text[text.Length - 1] != ' ')
                txtBox.Text += "%";

            //Delete every character except numbers from 0 to 9
            if (validInput == false)
                txtBox.Text = Regex.Replace(txtBox.Text, "[^0-9]", "");
            //Replace any zeros before the number unless the text is going to be empty
            if (text != String.Empty && text[0] == '0' && text.Trim('%').Length != 1)
                txtBox.Text = Regex.Replace(txtBox.Text, @"\A0+", "");

            //If the user added symbols at the start
            if (text[0] == '%')
                txtBox.Text = txtBox.Text.TrimStart('%');
            //If the user added % symbols at the end
            if (text.Length > 1)
            {
                if (text[text.Length - 1] == '%' && text[text.Length - 2] == '%')
                {
                    text = text.TrimEnd('%');
                    txtBox.Text = text + "%";
                }
            }

            //Remove white spaces and tabs
            if (text.Contains(' '))
            {
                txtBox.Text = Regex.Replace(text, @"\s+", "");
            }
        }

        /// <summary>
        /// Sanitize text of GifFramesDelayOptionTextBox if there are letters or other special characters present, if the value exceeds 2500 or is lower than 1.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GifFramesDelayOptionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            const int minLevel = ConversionParamsModel.minDelayTime;
            const int maxLevel = ConversionParamsModel.maxDelayTime;
            TextBox txtBox = (TextBox)sender;
            string text = txtBox.Text;
            int value;
            bool validInput = int.TryParse(text, out value);

            if (validInput && value <= maxLevel & value >= minLevel && txtBox.Text[0] != '0' && text.Contains(' ') == false)
                return;

            if (value > maxLevel && validInput) { txtBox.Text = maxLevel.ToString(); }
            else if ((value < minLevel && validInput) || text == "") { txtBox.Text = minLevel.ToString(); }

            //Delete every character except numbers from 0 to 9
            if (validInput == false)
                txtBox.Text = Regex.Replace(txtBox.Text, "[^0-9]", "");
            //Replace any zeros before the number unless the text is going to be empty
            if (text != String.Empty && text[0] == '0')
                txtBox.Text = Regex.Replace(txtBox.Text, @"\A0+", "");

            //Remove white spaces and tabs
            if (text.Contains(' '))
            {
                txtBox.Text = Regex.Replace(text, @"\s+", "");
            }
        }

        /// <summary>
        /// Choose where to save the converted image(s)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Reset the resizing dimensions to the original dimensions of the image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetResizingDimensions_Click(object sender, RoutedEventArgs e)
        {
            ImgWidthResTextBlock.Text = originalImgDimensions.width.ToString();
            ImgHeightResTextBlock.Text = originalImgDimensions.height.ToString();
        }

        /// <summary>
        /// Input validation for dimensions of resized image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImgDimensionsResTextBlock_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        #endregion


        private async void StartConversionBttn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //If a format hasn't been selected prompt user to select one and don't convert
            if (FormatComboBox.SelectedItem == null)
            {
                if (Settings.Default.Language == "it")
                    MessageBox.Show(LanguageManager.IT_SelectFormatMsgBox, "Errore", MessageBoxButton.OK, MessageBoxImage.Information);
                else if (Settings.Default.Language == "en")
                    MessageBox.Show(LanguageManager.EN_SelectFormatMsgBox, "Error", MessageBoxButton.OK, MessageBoxImage.Information);

                return;
            }
            //If one or more images don't exist anymore(moved or deleted), don't convert
            foreach (var image in local_pathsOfImagesToConvert)
            {
                if (File.Exists(image) == false)
                {
                    if (Settings.Default.Language.ToLower() == "it") { MessageBox.Show(LanguageManager.IT_CantFindImagesToConvertInOriginalFolder, "Errore", MessageBoxButton.OK, MessageBoxImage.Error); }
                    if (Settings.Default.Language.ToLower() == "en") { MessageBox.Show(LanguageManager.EN_CantFindImagesToConvertInOriginalFolder, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
                    return;
                }
            }

            //Get the selected icon sizes. If the user hasn't selected at least 1 option in the IconSizesMatrixGrid don't convert
            List<string> selectedIconSizes = new List<string>();
            var selectedFormat = (FormatComboBox.SelectedItem as Label).Content.ToString().ToLower();
            if (selectedFormat == "ico" || selectedFormat == "cur") //If the user has selected the ico format
            {
                selectedIconSizes = new List<string>();
                foreach (CheckBox option in IconSizesMatrixGrid.Children)
                {
                    if ((bool)option.IsChecked == true)
                        selectedIconSizes.Add(option.Content.ToString());
                }
                if (selectedIconSizes.Count == 0)
                {
                    if (Settings.Default.Language == "it") { MessageBox.Show(LanguageManager.IT_SelectOneIconSize, "Errore", MessageBoxButton.OK, MessageBoxImage.Exclamation); }
                    if (Settings.Default.Language == "en") { MessageBox.Show(LanguageManager.EN_SelectOneIconSize, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation); }
                    return;
                }
            }

            //Get the dimensions for the resized image and sanitize input and check if the user has changed the resize parameters and so if actually wants to resize
            Regex regex = new Regex("[^0-9]");       
            regex.Replace(ImgHeightResTextBlock.Text, String.Empty);
            (int width, int height) resizedImgDimensions;
            resizedImgDimensions.width = Convert.ToInt32(regex.Replace(ImgWidthResTextBlock.Text, String.Empty));
            resizedImgDimensions.height = Convert.ToInt32(regex.Replace(ImgHeightResTextBlock.Text, String.Empty));
            bool wantsResize = true;
            if (resizedImgDimensions.width == originalImgDimensions.width && resizedImgDimensions.height == originalImgDimensions.height)
                wantsResize = false;

            #region Prepares GUI controls
            if (Settings.Default.Language == "it") ConversionResultTextBlock.Text = LanguageManager.IT_ConversionResultTextBlockRunning;
            if (Settings.Default.Language == "en") ConversionResultTextBlock.Text = LanguageManager.EN_ConversionResultTextBlockRunning;
            ConversionResultTextBlock.Visibility = Visibility.Collapsed; //Necessary because if the user converts one image two times in a row it would seem like the conversion didn't start
            ThemeManager.solidColorBrush.Color = ThemeManager.RunningOrStaticConversionTextBlockColor;
            ConversionResultTextBlock.Foreground = ThemeManager.solidColorBrush; //Sets the color of the textblock
            ConversionResultTextBlock.Visibility = Visibility.Visible; //Makes the label of the state of the conversion visible
            StartConversionBttn.IsEnabled = false; //While a conversion is ongoing the convertbttn gets disabled
            //Add a dot to the conversion result textblock
            Thread ticker = new Thread(() =>
            {
                int i = 0;
                while (true)
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (i >= 3)
                        {
                            ConversionResultTextBlock.Text = ConversionResultTextBlock.Text.TrimEnd('.');
                            i = 0;
                        }
                        ConversionResultTextBlock.Text += ".";
                        i++;
                    });
                    Thread.Sleep(300);
                }
            });
            ticker.IsBackground = true;

            #endregion

            finishedConversions = new Dictionary<string, bool>();

            ConversionParamsModel conversionParameters = new ConversionParamsModel();
            try
            {
                //Prepare image conversion parameters
                conversionParameters = new ConversionParamsModel()
                {
                    format = (FormatComboBox.SelectedItem as Label).Content.ToString().ToLower(),
                    pathsOfImagesToConvert = local_pathsOfImagesToConvert,
                    gifRepeatTimes = repGifTimes,
                    colorToReplTheTranspWith = ReplTranspColCB.SelectedIndex,
                    delayTime = Convert.ToInt32(GifFramesDelayOptionTextBox.Text) / 10, //Divided by 10 because the delay is needed in centiseconds
                    iconSizes = selectedIconSizes,
                    qualityLevel = Convert.ToInt32(QualityLevelTextBox.Text.Trim('%', ' ')),
                    tiffCompressionAlgo = (CompressionTypesCB.SelectedItem as Label)?.Content.ToString().ToLower(),
                    saveDirectory = SavePathTextBlock.Text,
                    resizeDimensions = resizedImgDimensions,
                    resize = wantsResize,
                };

                //Adds a dot during conversion to show the loading
                ticker.Start();
                //Starts timer to measure how long the conversion takes
                timer.Start();

                //Start conversion
                ImageConversionHandler imgConvHandler = new ImageConversionHandler();
                finishedConversions = await Task.Run(() => imgConvHandler.StartConversionAsync(conversionParameters));

                timer.Stop();
                ticker.Abort();

                #region Counts the unsuccessful conversions
                //List containing the names of the images that didn't get converted successfully
                List<string> unsuccessfulConversions = new List<string>();

                foreach (var conversion in finishedConversions)
                {
                    if (conversion.Value == false)
                        unsuccessfulConversions.Add(Path.GetFileName(conversion.Key));
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
                    ThemeManager.solidColorBrush = new SolidColorBrush { Color = ThemeManager.CompletedWithErrorsConversionTextBlockColor };
                    ConversionResultTextBlock.Foreground = ThemeManager.solidColorBrush;

                    if (Settings.Default.Language == "it")
                        ConversionResultTextBlock.Text = LanguageManager.IT_UnsuccConversionResultTextBlockFinishedText;
                    else if (Settings.Default.Language == "en")
                        ConversionResultTextBlock.Text = LanguageManager.EN_UnsuccConversionResultTextBlockFinishedText;

                    //Display the unsuccessful conversions
                    foreach (var conversion in unsuccessfulConversions)
                        ConversionResultTextBlock.Text += conversion + ", ";
                }
                #endregion
                timer.Reset();
            }
            catch (Exception ex)
            {
                timer.Stop();
                timer.Reset();
                ticker.Abort();
                //Create error log
                using (StreamWriter st = File.CreateText($"{Settings.Default.TempFolderPath}\\ErrorLog.txt"))
                {
                    st.WriteLine(DateTime.Now);
                    st.WriteLine("Conversion parameters:");
                    st.WriteLine(conversionParameters.pathsOfImagesToConvert);

                    st.WriteLine("\n Message:");
                    st.WriteLine(ex.Message + "\n -----------------------------------------------------------");

                    st.WriteLine("\n StackTrace:");
                    st.WriteLine(ex.StackTrace + "\n -----------------------------------------------------------");

                    st.WriteLine("\n Source:");
                    st.WriteLine(ex.Source + "\n -----------------------------------------------------------");

                    st.WriteLine("\n TargetSite:");
                    st.WriteLine(ex.TargetSite + "\n -----------------------------------------------------------");

                    st.WriteLine("\n InnerException:");
                    st.WriteLine(ex.InnerException + "\n -----------------------------------------------------------");

                    st.WriteLine("\n Data:");
                    st.WriteLine(ex.Data + "\n -----------------------------------------------------------");

                    st.WriteLine("\n Help link:");
                    st.WriteLine(ex.HelpLink);
                }

                //Show error msgBox
                if (Settings.Default.Language == "it") { MessageBox.Show(LanguageManager.IT_UnexpectedErrorMsgBoxText, "Unexpected error", MessageBoxButton.OK, MessageBoxImage.Error); }
                else if (Settings.Default.Language == "en") { MessageBox.Show(LanguageManager.EN_UnexpectedErrorMsgBoxText, "Unexpected error", MessageBoxButton.OK, MessageBoxImage.Error); }

                //Open explorer in the temp folder with the error log selected
                string argument = "/select, \"" + $"{Settings.Default.TempFolderPath}\\ErrorLog.txt" + "\"";
                Process.Start("explorer.exe", argument);

                ThemeManager.solidColorBrush = new SolidColorBrush
                {
                    Color = ThemeManager.CompletedWithErrorsConversionTextBlockColor
                };
                ConversionResultTextBlock.Foreground = ThemeManager.solidColorBrush;
                ConversionResultTextBlock.Text = "error";
            }

            StartConversionBttn.Visibility = Visibility.Hidden; //Unbug the button
            Thread.Sleep(100);
            StartConversionBttn.Visibility = Visibility.Visible;
            StartConversionBttn.IsEnabled = true; //Re-enables the convertbttn to convert another image
        }

        /// <summary>
        /// Gets the images dropped by the user in the ImgViewer control and prepares the GUI consequently
        /// </summary>
        /// <param name="droppedFilesToConvert"> Files dropped on the ImgViewer control, if the user drops a folder it would be the first element</param>
        public List<string> GetImagesToConvertAndPrepareGUI(List<string> droppedFilesToConvert)
        {
            #region Resets and adjust various GUI controls properties
            ThemeManager.solidColorBrush = new SolidColorBrush
            {
                Color = ThemeManager.RunningOrStaticConversionTextBlockColor
            };
            ConversionResultTextBlock.Foreground = ThemeManager.solidColorBrush;

            ReplaceTransparencySP.Visibility = Visibility.Collapsed;
            ConversionResultTextBlock.Visibility = Visibility.Collapsed;
            QualityOptionSP.Visibility = Visibility.Visible;
            SavePathOptionSP.Visibility = Visibility.Visible;
            #endregion

            List<string> newPathsOfImagesToConvert = new List<string>();

            //Gets the new dropped images
            foreach (var filePath in droppedFilesToConvert)
            {
                FileAttributes attr = File.GetAttributes(filePath);
                //If the file is a folder add the images inside it
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    var folder = new DirectoryInfo(filePath).GetFiles("*", SearchOption.AllDirectories);
                    foreach (var image in folder)
                    {
                        newPathsOfImagesToConvert.Add(image.FullName);
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

            //Take the dimensions of the first image in the list and set it as the default resize dimensions
            using (FileStream firstImageStream = File.OpenRead(newPathsOfImagesToConvert[0]))
            {
                BitmapImage firstImage = new BitmapImage();
                firstImage.BeginInit();
                firstImage.StreamSource = firstImageStream;
                firstImage.CacheOption = BitmapCacheOption.None;
                firstImage.EndInit();

                originalImgDimensions = (firstImage.PixelWidth, firstImage.PixelHeight);

                ImgWidthResTextBlock.Text = firstImage.PixelWidth.ToString();
                ImgHeightResTextBlock.Text = firstImage.PixelHeight.ToString();
                firstImageStream.Close();
                firstImageStream.Dispose();
            }

            #region Adds or sets name(s) of the image(s) to the ImagesNamesTextBlock under ImgViewer
            //If the user wants to replace the already dropped images, clear the textblock
            if ((string)InsertNewImagesModeBttn.Tag == "Replace")
            {
                ImagesNamesTextBlock.Text = string.Empty;
            }
            //Add the images paths
            for (int i = 0; i < newPathsOfImagesToConvert.Count; i++)
            {
                string imageName = Path.GetFileName(newPathsOfImagesToConvert[i]);

                if (newPathsOfImagesToConvert.Count != 1 && i == newPathsOfImagesToConvert.Count - 1)
                    ImagesNamesTextBlock.Text += $"{imageName}";
                else
                    ImagesNamesTextBlock.Text += $"{imageName}, ";
            }
            #endregion

            if ((string)InsertNewImagesModeBttn.Tag == "Replace")
            {
                //Set default savePath as the parent folder of the first image
                SavePathTextBlock.Text = Path.GetDirectoryName(newPathsOfImagesToConvert[0]);
                return newPathsOfImagesToConvert;
            }
            else if ((string)InsertNewImagesModeBttn.Tag == "Add")
                return local_pathsOfImagesToConvert.Concat(newPathsOfImagesToConvert).ToList();
            else
                return null;
        }

        /// <summary>
        /// Loads the image to see in the ImgViewer control and gets its Width and height
        /// </summary>
        /// <param name="pathsOfImagesToConvert"></param>
        public void LoadPreviewImage(List<string> pathsOfImagesToConvert)
        {
            previewImageStream?.Dispose();
            previewImageStream?.Close();
            previewImage = null;

            ImgViewer.Opacity = 1.0f; //Sets ImgViewer opacity to 1(max)

            //Loads the preview image from a stream just to get dimensions
            using (previewImageStream = File.OpenRead(pathsOfImagesToConvert[0]))
            {
                previewImage = new BitmapImage();
                previewImage.BeginInit();
                previewImage.StreamSource = previewImageStream;
                previewImage.CacheOption = BitmapCacheOption.OnLoad;
                //Reduce height resolution of image to reduce memory usage in case of large images
                previewImage.DecodePixelHeight = (int)ImgViewer.Height;
                previewImage.DecodePixelWidth = (int)ImgViewer.Width;
                previewImage.EndInit();

                ImgViewer.Source = previewImage;
                //Freeze bitmapimage to make in umodifiable and prevent the system to spend more resources on it
                previewImage.Freeze();
                previewImageStream.Close();
            }


            //Loads the preview image from a stream, if the image was used directly it would have remained in use even after emptying the ImgViewer and so it couldn't be (in case) deleted
            using (previewImageStream = File.OpenRead(pathsOfImagesToConvert[0]))
            {
                previewImage = new BitmapImage();
                previewImage.BeginInit();
                previewImage.StreamSource = previewImageStream;
                previewImage.CacheOption = BitmapCacheOption.OnLoad;
                //Reduce height resolution of image to reduce memory usage in case of large images
                previewImage.DecodePixelHeight = (int)ImgViewer.Height;
                previewImage.DecodePixelWidth = (int)ImgViewer.Width;
                previewImage.EndInit();

                ImgViewer.Source = previewImage;
                //Freeze bitmapimage to make in umodifiable and prevent the system to spend more resources on it
                previewImage.Freeze();
                previewImageStream.Close();
            }
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
        /// If Enter is pressed while the StartConversionBttn is focused, start conversion
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartConversionBttn_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                StartConversionBttn_MouseDown(StartConversionBttn, null);
        }
    }
}