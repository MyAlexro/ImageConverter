using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageConverter.Models
{
    class CompressionParametersModel
    {
        /// <summary>
        /// Path of the image to convert
        /// </summary>
        public string imageToCompressPath { get; set; }

        /// <summary>
        /// Level of the final quality of the compressed image
        /// </summary>
        public int quality { get; set; }

        /// <summary>
        /// Path where the image will be saved
        /// </summary>
        public string savePath { get; set; }
    }
}
