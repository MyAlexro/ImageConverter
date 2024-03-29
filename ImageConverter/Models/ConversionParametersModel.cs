﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace ImageConverter
{
    public class ConversionParamsModel
    {
        /// <summary>
        /// Available formats to convert the image(s) to
        /// </summary>
        public static readonly List<string> availableFormats = new List<string> { "PNG", "JPG", "JPEG", "BMP", "GIF", "ICO", "CUR", "TIFF" };
        /// <summary>
        /// Available sizes in the final icon
        /// </summary>
        public static readonly List<string> availableIconSizes = new List<string> { "16x16", "24x24", "32x32", "48x48", "64x64", "96x96", "128x128", "192x192", "256x256" };

        /// <summary>
        /// Maximum allowed value of the delay time between two frames of a gif
        /// </summary>
        public const int maxDelayTime = 2500;
        /// <summary>
        /// Minimum allowed value of the delay time between two frames of a gif
        /// </summary>
        public const int minDelayTime = 1;
        /// <summary>
        /// Maximum allowed value of the quality level used in the compression of the image
        /// </summary>
        public const int maxQualityLevel = 100;
        /// <summary>
        /// Minimum allowed value of the quality level used in the compression of the image
        /// </summary>
        public const int minQualityLevel = 0;

        private string _format;
        /// <summary>
        /// Format to which convert the images
        /// </summary>
        public string format
        {
            get { return _format; }
            set
            {
                foreach (var availableFormat in availableFormats)
                {
                    if (value == availableFormat.ToLower())
                    {
                        _format = value;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// List containing all the paths of the images to convert
        /// </summary>
        public List<string> pathsOfImagesToConvert { get; set; }

        /// <summary>
        /// Index of the color to replace the transparency of a png image with. 0=nothing, 1=white, 2=black
        /// </summary>
        public int colorToReplTheTranspWith { get; set; }

        /// <summary>
        /// Times the gif will repeat: infinite(0)-10
        /// </summary>
        public int gifRepeatTimes { get; set; }

        /// <summary>
        /// Delay time (in centiseconds so that it doesn't have to be converted later on)
        /// </summary>
        public int delayTime { get; set; }

        private List<string> _iconSizes;
        /// <summary>
        /// List of the available sizes in the final icon chosen by the user
        /// </summary>
        public List<string> iconSizes
        {
            get
            {
                return _iconSizes;
            }

            set
            {
                if (value.Count > 1 || value.Count <= availableIconSizes.Count)
                {
                    _iconSizes = value;
                }
                else
                {
                    throw new Exception("Too much or too few values assigned to iconSizes");
                }
                foreach (var item in value)
                {
                    if (!availableIconSizes.Contains(item))
                        throw new Exception("One of the assigned icon sizes isn't available");
                }
            }
        }

        /// <summary>
        /// Type of compression for Tiff images
        /// </summary>
        public string tiffCompressionAlgo { get; set; }

        /// <summary>
        /// Final quality of the converted image
        /// </summary>
        public int qualityLevel { get; set; }

        /// <summary>
        /// Dimensions for the resized image
        /// </summary>
        public (int width, int height) resizeDimensions { get; set; }

        /// <summary>
        /// Whether the user wants to resize the converted image or not
        /// </summary>
        public bool resize { get; set; }

        private string _saveDirectory;
        /// <summary>
        /// Path where the image(s) will be saved, the default save path is the one of the first image to convert
        /// </summary>
        public string saveDirectory
        {
            get { return _saveDirectory; }
            set
            {
                if (Directory.Exists(value))
                    _saveDirectory = value;
                else
                    throw new Exception("Invalid path, ImageConversionParametersModel.savePath");
            }
        }
    }
}
