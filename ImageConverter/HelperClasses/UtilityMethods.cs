
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;

namespace ImageConverter.HelperClasses
{
    public class UtilityMethods
    {
        /// <summary>
        /// Checks wether the given path of the file is an image
        /// </summary>
        /// <param type="string" name="pathOfFile"> path of the file that needs to be checked </param>
        /// <returns type="bool" name="IsImage"> true if the file is an image, otherwise false</returns>
        public static bool IsOrContainsImage(string pathOfFile)
        {
            string filePath = pathOfFile.ToLower();
            foreach (var format in ImageConversionParametersModel.availableFormats)
            {
                if (filePath.Contains($".{format.ToLower()}"))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks wether the given paths of the files are images or are folders containing images
        /// </summary>
        /// <param type="string" name="pathOfFile"> path of the file that needs to be checked </param>
        /// <returns type="bool" name="IsImage"> True if any of the files is an image or is a folder containing an image, else returns False</returns>
        public static bool IsOrContainsImage(string[] pathOfFiles)
        {
            //If the dropped folder contains images
            bool fileisValidDirectory = false;

            foreach (var file in pathOfFiles)
            {
                //Check if the file is a folder and check if it contains any image, if yes then the files are ok to convert
                if (File.GetAttributes(file) == FileAttributes.Directory)
                {
                    string[] filesInDir = Directory.GetFiles(file);
                    foreach (var fileInDir in filesInDir)
                    {
                        if (IsOrContainsImage(fileInDir))
                        {
                            fileisValidDirectory = true;
                        }
                        else
                        {
                            fileisValidDirectory = false;
                            return false;
                        }
                    }
                }
                //If it's not a folder or it doesn't contain any images
                else { fileisValidDirectory = false; }

                //If the file isn't an image
                if (IsOrContainsImage(file) == false && !fileisValidDirectory)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Finds all labels in a stackpanel
        /// </summary>
        /// <param name="stackpanel"></param>
        /// <returns>Returns a list containing all the labels</returns>
        public static List<Label> FindLabelsInStackPanel(StackPanel stackpanel)
        {
            //List of labels in the Options stackpanel
            List<Label> labels = new List<Label>();

            foreach (var control in stackpanel.Children)
            {
                if (control?.GetType() == typeof(StackPanel))
                {
                    labels.AddRange(FindLabelsInStackPanel((StackPanel)control));
                }
                else if (control?.GetType() == typeof(Label))
                {
                    labels.Add(control as Label);
                }
            }
            return labels;
        }

        public static List<TextBlock> FindTextBlocksInStackPanel(StackPanel stackpanel)
        {
            //List of labels in the Options stackpanel
            List<TextBlock> textBlocks = new List<TextBlock>();

            foreach (var control in stackpanel.Children)
            {
                if (control?.GetType() == typeof(StackPanel))
                {
                    textBlocks.AddRange(FindTextBlocksInStackPanel((StackPanel)control));
                }
                else if (control?.GetType() == typeof(TextBlock))
                {
                    textBlocks.Add(control as TextBlock);
                }
            }
            return textBlocks;
        }

        /// <summary>
        /// Returns a list of all the Labels contained in all the ComboBoxes in a Stackpanel passed by reference
        /// </summary>
        /// <param name="stackPanel"></param>
        /// <returns>List of Label</returns>
        public static List<Label> FindLabelsInComboBoxesInSPs(ref StackPanel stackPanel)
        {
            List<Label> labels = new List<Label>();

            foreach (var control in stackPanel.Children)
            {
                if (control?.GetType() == typeof(StackPanel))
                {
                    StackPanel sp = (StackPanel)control;
                    labels.AddRange(FindLabelsInComboBoxesInSPs(ref sp));
                }
                else if (control?.GetType() == typeof(ComboBox))
                {
                    ComboBox comboBox = (ComboBox)control;
                    foreach(var item in comboBox.Items)
                    {
                        if(item?.GetType() == typeof(Label))
                        {
                            labels.Add((Label)item);
                        }
                    }
                }
            }
            return labels;
        }

        /// <summary>
        /// Empty a folder by deleting all its content
        /// </summary>
        /// <param name="path"></param>
        internal static void EmptyFolder(string path)
        {
            foreach (var file in Directory.GetFiles(path))
            {
                File.Delete(file);
            }
        }
    }
}
