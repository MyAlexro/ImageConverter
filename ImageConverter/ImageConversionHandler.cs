using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Media;
using System.Threading.Tasks;

namespace ImageConverter
{
    class ImageConversionHandler
    {
        static ImageFormatConverter imageConverter;
        private static Image ImgToConvert;
        private static Image convertedImage;

        public static bool IsImage(string filePath)
        {
            if (filePath.Contains(".jpg") || filePath.Contains(".jpeg") || filePath.Contains(".png") || filePath.Contains(".bmp") || filePath.Contains(".ico") || filePath.Contains(".gif"))
            {
                return true;
            }
            return false;
        }

        public static async Task<Image> ConvertTo(string format, string ImageToConvertPath)
        {
            if (format == "jpeg")
            {

            }
            else if(format == "jpg")
            {

            }
            else if (format == "png")
            {

            }
            else if (format == "bmp")
            {

                
            }
            else if (format == "ico")
            {

                
            }
            else if (format == "gif")
            {

            }
            return convertedImage;
        }
    }
}
