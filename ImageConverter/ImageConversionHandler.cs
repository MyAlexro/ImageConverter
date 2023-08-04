using ImageConverter.Properties;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
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

        private static Image imageToConvert; //image to convert
        private static string imageName; //name of the image to convert
        private static string directoryOfImageToConvert; //directory of the image to convert
        private static string pathOfImageToConvert; //path of the image to convert

        public static bool IsImage(string pathOfFile)
        {
            string filePath = pathOfFile.ToLower();
            if (filePath.Contains(".jpg") || filePath.Contains(".jpeg") || filePath.Contains(".png") || filePath.Contains(".bmp") || filePath.Contains(".ico") || filePath.Contains(".gif") || filePath.Contains(".tif") || filePath.Contains(".tiff"))
            {
                return true;
            }
            return false;
        }

        public static async Task<bool> ConvertAndSaveAsync(string format, string imageToConvertPath)
        {
            try
            {


                System.Diagnostics.Debug.WriteLine($"Converting: {imageToConvertPath}");
                #region  set up image to convert etc.
                pathOfImageToConvert = imageToConvertPath;
                imageName = Path.GetFileNameWithoutExtension(imageToConvertPath);
                imageToConvert = Image.FromFile(imageToConvertPath);
                directoryOfImageToConvert = Path.GetDirectoryName(imageToConvertPath);
                #endregion

                if (Path.GetExtension(imageToConvertPath) == ("." + format))
                {
                    if (Settings.Default.Language == "it")
                    {
                        MessageBox.Show(LanguageManager.IT_CantConvertImageToSameFormat, "", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else if (Settings.Default.Language == "en")
                    {
                        MessageBox.Show(LanguageManager.EN_CantConvertImageToSameFormat, "", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    return false;
                } //if converting image to same format return false

                if (format == "ico" || format == "cur")
                {
                    Icon icon = IconOrCurFromImage(imageToConvert, format);
                    if (icon == null) { return false; }
                    using (Stream st = File.Create($"{directoryOfImageToConvert}\\{imageName}.{format}"))
                    {
                        icon.Save(st);
                        st.Close();
                    }
                }
                else
                {
                    #region set up encoder etc. to convert image
                    if (format == "jpg") { imageCodecInfo = GetEncoderInfo("image/jpeg"); }
                    else { imageCodecInfo = GetEncoderInfo($"image/{format}"); }//prende l'encoder in base al formato scelto
                    imageEncoder = Encoder.Quality;
                    encoderParametersArr = new EncoderParameters(1);
                    qualityParameter = new EncoderParameter(imageEncoder, 100L);
                    encoderParametersArr.Param[0] = qualityParameter;
                    #endregion
                    imageToConvert.Save($"{directoryOfImageToConvert}\\{imageName}.{format}", imageCodecInfo, encoderParametersArr);
                }//if it's any other format

                if (File.Exists($"{directoryOfImageToConvert}\\{imageName}.{format}"))
                {
                    return true;
                } //if the conversion was successful and the file of the converted image exists: return true
                else
                {
                    return false;
                } //otherwise: return false
            }
            catch(Exception)
            {
                return false;
            }
        }

        private static ImageCodecInfo GetEncoderInfo(String mimeType) //gets the informations for the encoder based on the chosen format
        {
            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
            for (int i = 0; i <= 4; i++) { encoders[i].MimeType.ToString(); }

            for (int i = 0; i < encoders.Length; i++)
            {
                if (encoders[i].MimeType == mimeType)
                    return encoders[i];
            }
            return null;
        }

        public static Icon IconOrCurFromImage(Image imgToConvert, string format)
        {
            var ext = Path.GetExtension(pathOfImageToConvert).ToLower();
            #region if the image to convert isn't a png or bmp image it can't be converterd: return null
            if (ext != ".png" && ext != ".bmp")
            {
                if (Settings.Default.Language == "it")
                {
                    MessageBox.Show(LanguageManager.IT_CantConvertThisImageToIco, "", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (Settings.Default.Language == "en")
                {
                    MessageBox.Show(LanguageManager.EN_CantConvertThisImageToIco, "", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return null;
            }
            #endregion


            var memStream = new MemoryStream();
            var binWriter = new BinaryWriter(memStream);

            binWriter.Write((short)0);   //offset #1: reserved
            #region offset #2: ico or cur 
            byte type;
            if (format == "ico") type = 1;
            else type = 2;
            binWriter.Write((short)type);
            #endregion
            binWriter.Write((short)1);   //offset #3: number of resolutions in the final icon  
            #region offset #4 and #5: width and height of image, the value 0 is equal to 256
            var w = imgToConvert.Width;
            if (w >= 256) w = 0;
            binWriter.Write((byte)w);
            var h = imgToConvert.Height;
            if (h >= 256) h = 0;
            binWriter.Write((byte)h);
            #endregion
            binWriter.Write((byte)0);    // offset #6: number of colors in palette
            binWriter.Write((byte)0);    // offset #7: reserved
            binWriter.Write((short)0);   // offset #8: number of color planes(ico), horizontal coordinates of hotspot,in pixels, from the left(cur)
            binWriter.Write((short)0);   // offset #9: bits per pixel(ico), vertical coordinates of hotspot,in pixels, from the left(cur)
            #region offset #10: size of image data
            var size = memStream.Position;
            binWriter.Write((int)0);
            #endregion
            #region offset of image data from the beginning of the ico/cur file
            var start = (int)memStream.Position + 4;
            binWriter.Write(start);
            #endregion

            if (ext == ".png") { imgToConvert.Save(memStream, ImageFormat.Png); }
            else { imgToConvert.Save(memStream, ImageFormat.Bmp); }
            var imageSize = (int)memStream.Position - start;
            memStream.Seek(size, SeekOrigin.Begin);
            binWriter.Write(imageSize);
            memStream.Seek(0, SeekOrigin.Begin);

            return new Icon(memStream);
        }
    }
}
