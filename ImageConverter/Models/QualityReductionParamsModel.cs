namespace ImageConverter.Models
{
    public class QualityReductionParamsModel
    {
        /// <summary>
        /// Path of the image to convert
        /// </summary>
        public string imgToReduceQualityPath { get; set; }

        private int _quality;
        /// <summary>
        /// Final quality of the converted image(s)
        /// </summary>
        public int quality
        {
            get
            {
                return _quality;
            }
            set
            {
                if (value >= ConversionParamsModel.minQualityLevel && value <= ConversionParamsModel.maxQualityLevel)
                {
                    _quality = value;
                }
            }
        }

        /// <summary>
        /// Path to the directory in which the image will be saved
        /// </summary>
        public string saveDirectory { get; set; }
    }
}
