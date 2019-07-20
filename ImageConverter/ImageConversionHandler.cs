using ImageConverter.Properties;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace ImageConverter
{
    class ImageConversionHandler
    {
        private static ImageCodecInfo imageCodecInfo;
        private static Encoder imageEncoder;
        private static EncoderParameter qualityParameter;
        private static EncoderParameters encoderParametersArr;

        private static Image imageToConvert; //immagine da convertire
        private static string imageName; //nome dell'immagine da convertire
        private static string directoryOfImageToConvert; //path dell'immagine da convertire in cui salvare l'immagine convertita

        public static bool IsImage(string filePath)
        {
            if (filePath.Contains(".jpg") || filePath.Contains(".jpeg") || filePath.Contains(".png") || filePath.Contains(".bmp") || filePath.Contains(".ico") || filePath.Contains(".gif"))
            {
                return true;
            }
            return false;
        }

        public static async Task<bool> ConvertAndSaveAsync(string format, string imageToConvertPath)
        {
            #region  set up image to convert etc.
            imageName = Path.GetFileNameWithoutExtension(imageToConvertPath);
            imageToConvert = Image.FromFile(imageToConvertPath);
            directoryOfImageToConvert = Path.GetDirectoryName(imageToConvertPath);
            #endregion

            if (Path.GetExtension(imageToConvertPath) == ("."+format))
            {
                if (Settings.Default.Language == "it")
                {
                    MessageBox.Show(LanguageManager.EN_CantConvertImageToSameFormat, "", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (Settings.Default.Language == "en")
                {
                    MessageBox.Show(LanguageManager.EN_CantConvertImageToSameFormat, "", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return false;
            }
            if (format == "ico") //se il formato in cui convertire l'immagine è ico. L'ho messo separato dagli altri formati perchè non ci sono Encoder con MimeType ico
            {
                imageToConvert.Save($"{directoryOfImageToConvert}\\{imageName}.{format}", ImageFormat.Icon);
            }
            else //se è un qualsiasi altro formato
            {
                #region set up encoder etc. to convert image
                imageCodecInfo = GetEncoderInfo($"image/{format}"); //prende l'encoder in base al formato scelto
                imageEncoder = Encoder.Quality;
                encoderParametersArr = new EncoderParameters(1);
                qualityParameter = new EncoderParameter(imageEncoder, 100L);
                encoderParametersArr.Param[0] = qualityParameter;
                #endregion

                imageToConvert.Save($"{directoryOfImageToConvert}\\{imageName}.{format}", imageCodecInfo, encoderParametersArr);
            }

            if (File.Exists($"{directoryOfImageToConvert}\\{imageName}.{format}")) //se la conversione è riuscita e quindi c'è l'immagine convertita
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
            for (int i = 0; i < encoders.Length; i++)
            {
                if (encoders[i].MimeType == mimeType)
                    return encoders[i];
            }
            return null;
        }
    }
}
