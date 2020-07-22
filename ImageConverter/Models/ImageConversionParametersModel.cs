using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace ImageConverter
{
    public class ImageConversionParametersModel
    {
        /// <summary>
        /// Available formats to convert the image(s) to
        /// </summary>
        public static readonly List<string> availableFormats = new List<string> { "PNG", "JPG", "JPEG", "BMP", "GIF", "ICO", "CUR", "TIFF" };
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
        /// Times the gif will repeat: infinite(0)-10
        /// </summary>
        public int gifRepeatTimes { get; set; }

        /// <summary>
        /// Index of the color to replace the transparency of a png image with. 0=nothing, 1=white, 2=black
        /// </summary>
        public int colorToReplTheTranspWith { get; set; }

        /// <summary>
        /// Delay time (in centiseconds so that it doesn't have to be converted later on)
        /// </summary>
        public int delayTime { get; set; }

        /// <summary>
        /// Final quality of the converted image
        /// </summary>
        public int qualityLevel { get; set; }

        /// <summary>
        /// Type of compression for Tiff images
        /// </summary>
        public string compressionAlgo { get; set; }

        private string _savePath;
        /// <summary>
        /// Path where the image(s) will be saved, the default save path is the one of the first image to convert
        /// </summary>
        public string savePath
        {
            get { return _savePath; }
            set
            {
                if (Directory.Exists(value))
                    _savePath = value;
                else
                    throw new Exception("Invalid path, ImageConversionParametersModel.savePath");
            }
        }
    }
}
