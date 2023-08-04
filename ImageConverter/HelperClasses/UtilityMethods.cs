
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
        public static bool IsValidImage(string pathOfFile)
        {
            string filePath = pathOfFile.ToLower();
            foreach (var format in ConversionParamsModel.availableFormats)
            {
                if (filePath.Contains($".{format.ToLower()}"))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks wether the given file(s) are images. If a path points to a folder, the files in its sub-folders will be checked too.
        /// </summary>
        /// <param name="paths">Array of string which should point to a file or a folder</param>
        /// <returns></returns>
        public static bool IsOrContainsImages(string[] paths)
        {
            foreach (var path in paths)
            {
                if (File.GetAttributes(path) == FileAttributes.Directory)
                {
                    var filesInDir = new DirectoryInfo(path).GetFiles("*", SearchOption.AllDirectories);
                    foreach (var file in filesInDir)
                    {
                        if (IsValidImage(file.FullName) == false)
                            return false;
                    }
                }
                else if (IsValidImage(path) == false)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Finds all labels in Panel elements(Grid, StackPanel etc.)
        /// </summary>
        /// <param name="stackpanel"></param>
        /// <returns>Returns a list containing all the labels</returns>
        public static List<Label> FindAllLabelsInPanelOrDerivedObjs(Panel panel)
        {
            //List of labels in the panel
            List<Label> labels = new List<Label>();

            foreach (var control in panel.Children)
            {
                if((bool)control?.GetType().IsSubclassOf(typeof(Panel)))
                {

                }    
                if (control?.GetType() == typeof(StackPanel))
                {
                    labels.AddRange(FindAllLabelsInPanelOrDerivedObjs((StackPanel)control));
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
