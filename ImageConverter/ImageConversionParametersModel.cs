using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace ImageConverter
{
    public class ImageConversionParametersModel
    {
        List<string> availableFormats = new List<string> { "PNG", "JPG", "JPEG", "BMP", "GIF", "ICO", "CUR", "TIFF" };
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
                    if (value == availableFormat)
                        _format = value;
                    throw new Exception("A non-supported format has been selected");
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
        private int _gifRepeatTimes = 0;
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
    }
}
