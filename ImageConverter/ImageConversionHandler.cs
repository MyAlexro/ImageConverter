using ImageConverter.Properties;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Generic;
using System.Linq;

namespace ImageConverter
{
    public class ImageConversionHandler
    {
        private static List<bool> conversionsResults; //results of the conversion(s): if the conversion was sucessful or not

        private static PngBitmapEncoder pngEncoder;
        private static JpegBitmapEncoder jpegOrJpgEncoder;
        private static BmpBitmapEncoder bmpEncoder;
        private static GifBitmapEncoder gifEncoder;

        private static Uri imageUri; //uri of the image to convert
        private static string imageName; //name of the image to convert
        private static string directoryOfImageToConvert; //directory of the image to convert

        public static bool IsImage(string pathOfFile)
        {
            string filePath = pathOfFile.ToLower();
            if (filePath.Contains(".jpg") || filePath.Contains(".jpeg") || filePath.Contains(".png") || filePath.Contains(".bmp") || filePath.Contains(".ico") || filePath.Contains(".gif") || filePath.Contains(".ico") || filePath.Contains(".tiff"))
            {
                return true;
            }
            return false;
        }

        public static async Task<List<bool>> StartConversion(string format, string[] pathsOfImagesToConvert, bool gifInLoop)
        {
            conversionsResults = new List<bool>();
            foreach (var imageToConvertPath in pathsOfImagesToConvert)
            {
                var imgToConvertExt = Path.GetExtension(imageToConvertPath).ToLower();
                if (imgToConvertExt == ("." + format))
                {
                    if (Settings.Default.Language == "it")
                    {
                        MessageBox.Show(LanguageManager.IT_CantConvertImageToSameFormat, "Errore", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else if (Settings.Default.Language == "en")
                    {
                        MessageBox.Show(LanguageManager.EN_CantConvertImageToSameFormat, "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    conversionsResults.Add(false);
                    return conversionsResults;
                }
            } //checks whether the user is trying to convert the image to the same format, converting an image to the same format causes an error because it will try to overwrite the image that is in use by the program
            if (format == "png")
            {
                foreach (var imagePath in pathsOfImagesToConvert)
                {
                    conversionsResults.Add(await Task.Run(() => ToPng(imagePath)));
                }
            }
            else if (format == "jpeg" || format == "jpg")
            {
                foreach (var imagePath in pathsOfImagesToConvert)
                {
                    conversionsResults.Add(await Task.Run(() => ToJpegOrJpg(imagePath, format)));
                }
            }
            else if (format == "bmp")
            {
                foreach (var imagePath in pathsOfImagesToConvert)
                {
                    conversionsResults.Add(await Task.Run(() => ToBmp(imagePath)));
                }
            }
            else if (format == "gif")
            {
                conversionsResults.Add(await Task.Run(() => ImagesToGif(pathsOfImagesToConvert, gifInLoop)));
            }
            else if (format == "ico" || format == "cur")
            {
                foreach (var imagePath in pathsOfImagesToConvert)
                {
                    conversionsResults.Add(await Task.Run(() => ToIconOrCur(imagePath, format)));
                }
            }

            return conversionsResults;
        }

        private static async Task<bool> ToPng(string pathOfImage)
        {
            #region  set up image infos to convert etc.
            imageName = Path.GetFileNameWithoutExtension(pathOfImage);
            directoryOfImageToConvert = Path.GetDirectoryName(pathOfImage);
            imageUri = new Uri(pathOfImage);
            pngEncoder = new PngBitmapEncoder();
            #endregion
            pngEncoder.Frames.Add(BitmapFrame.Create(imageUri));

            #region saves the image and checks whether it was save correctly
            using (Stream st = File.Create($"{directoryOfImageToConvert}\\{imageName}.png"))
            {
                pngEncoder.Save(st);
                st.Close();
            }
            if (File.Exists($"{directoryOfImageToConvert}\\{imageName}.png"))
            {
                return true;
            } //if the conversion was successful and the file of the converted image exists: return true
            else
            {
                return false;
            } //otherwise: return false
            #endregion
        }

        private static async Task<bool> ToJpegOrJpg(string pathOfImage, string format)
        {
            #region  set up image infos to convert etc.
            imageName = Path.GetFileNameWithoutExtension(pathOfImage);
            directoryOfImageToConvert = Path.GetDirectoryName(pathOfImage);
            imageUri = new Uri(pathOfImage);
            jpegOrJpgEncoder = new JpegBitmapEncoder();
            #endregion
            jpegOrJpgEncoder.Frames.Add(BitmapFrame.Create(imageUri));

            #region Saves image based on format(jpeg or jpg) and checks wether it was saved correctly
            if (format == "jpeg")
            {
                using (Stream st = File.Create($"{directoryOfImageToConvert}\\{imageName}.jpeg"))
                {
                    jpegOrJpgEncoder.Save(st);
                    st.Close();
                }
            }
            else
            {
                using (Stream st = File.Create($"{directoryOfImageToConvert}\\{imageName}.jpg"))
                {
                    jpegOrJpgEncoder.Save(st);
                    st.Close();
                }
            }
            if (File.Exists($"{directoryOfImageToConvert}\\{imageName}.jpeg") || File.Exists($"{directoryOfImageToConvert}\\{imageName}.jpg"))
            {
                return true;
            }
            else
            {
                return false;
            }
            #endregion
        }

        private static async Task<bool> ToBmp(string pathOfImage)
        {
            #region  set up image infos to convert etc.
            imageName = Path.GetFileNameWithoutExtension(pathOfImage);
            directoryOfImageToConvert = Path.GetDirectoryName(pathOfImage);
            imageUri = new Uri(pathOfImage);
            bmpEncoder = new BmpBitmapEncoder();
            #endregion
            bmpEncoder.Frames.Add(BitmapFrame.Create(imageUri));

            #region saves bmp image and checkes whether it was saved correctly
            using (Stream st = File.Create($"{directoryOfImageToConvert}\\{imageName}.bmp"))
            {
                bmpEncoder.Save(st);
                st.Close();
            }
            if (File.Exists($"{directoryOfImageToConvert}\\{imageName}.bmp"))
            {
                return true;
            }
            else
            {
                return false;
            }
            #endregion
        }

        private static async Task<bool> ImagesToGif(string[] imagesPaths, bool inLoop)
        {
            #region  set up image infos to convert etc.
            imageName = Path.GetFileNameWithoutExtension(imagesPaths[0]);
            directoryOfImageToConvert = Path.GetDirectoryName(imagesPaths[0]);
            gifEncoder = new GifBitmapEncoder();
            #endregion

            try
            {
                foreach (var image in imagesPaths)
                {
                    imageUri = new Uri(image);
                    gifEncoder.Frames.Add(BitmapFrame.Create(imageUri));
                }
                if (inLoop == true)
                {
                    using (var ms = new MemoryStream())
                    {
                        gifEncoder.Save(ms);
                        var fileBytes = ms.ToArray();
                        // This is the NETSCAPE2.0 Application Extension.
                        var applicationExtension = new byte[] { 33, 255, 11, 78, 69, 84, 83, 67, 65, 80, 69, 50, 46, 48, 3, 1, 0, 0, 0 };
                        var newBytes = new List<byte>();
                        newBytes.AddRange(fileBytes.Take(13));
                        newBytes.AddRange(applicationExtension);
                        newBytes.AddRange(fileBytes.Skip(13));
                        File.WriteAllBytes($"{directoryOfImageToConvert}\\{imageName}_Gif.gif", newBytes.ToArray());
                        ms.Close();
                    }//add the bytes to put the gif in loop and saves it
                }
                else
                {
                    using (Stream st = File.Create($"{directoryOfImageToConvert}\\{imageName}_Gif.gif"))
                    {
                        gifEncoder.Save(st);
                        st.Close();
                    }
                }//saves the gif using the Save() method of GifBitmapEncoder
            }
            catch (Exception)
            {
                return false;
            }

            if (File.Exists($"{directoryOfImageToConvert}\\{imageName}_Gif.gif"))
            {
                return true;
            } //if the conversion was successful and the file of the converted image exists: return true
            else
            {
                return false;
            } //otherwise: return false
        }

        private static async Task<bool> ToIconOrCur(string imgToConvertPath, string format)
        {
            var ext = Path.GetExtension(imgToConvertPath).ToLower();
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
                return false;
            }
            #endregion

            #region  set up image infos to convert etc.
            imageName = Path.GetFileNameWithoutExtension(imgToConvertPath);
            directoryOfImageToConvert = Path.GetDirectoryName(imgToConvertPath);
            Image imgToConvert = Image.FromFile(imgToConvertPath);
            var memStream = new MemoryStream();
            var binWriter = new BinaryWriter(memStream);
            #endregion

            try
            {
                #region Icon header
                binWriter.Write((short)0);   //offset #0: reserved

                byte type;
                if (format == "ico") type = 1;
                else type = 2;
                binWriter.Write((short)type); //offset #2: specify ico or cur file

                binWriter.Write((short)1);   //offset #4: number of resolutions of the image in the final icon  
                #endregion
                #region Structure of image directory
                var w = imgToConvert.Width;
                if (w >= 256) w = 0;
                binWriter.Write((byte)w); //offset #0: image width

                var h = imgToConvert.Height;
                if (h >= 256) h = 0;
                binWriter.Write((byte)h); //offset #1: image height

                binWriter.Write((byte)0);    //offset #2: number of colors in palette
                binWriter.Write((byte)0);    //offset #3: reserved

                if (format == "ico") binWriter.Write((short)0); //offset #4: (if ico) number of color planes
                else binWriter.Write((short)1); //offset #4: (if cur) horizontal coordinates of hotspot,in pixels, from the left

                if (format == "ico") binWriter.Write((short)0); //offset #6: (if ico) bits per pixel
                else binWriter.Write((short)1); //offset #6: (if cur) vertical coordinates of hotspot,in pixels, from the left

                var sizeHere = memStream.Position;
                binWriter.Write((int)0);    //offset #8: size of image data

                var start = (int)memStream.Position + 4;
                binWriter.Write(start); //offset #12: of image data from the beginning of the ico/cur file
                #endregion
                #region Image data
                if (ext == ".png") imgToConvert.Save(memStream, ImageFormat.Png);
                else imgToConvert.Save(memStream, ImageFormat.Bmp);
                var imageSize = (int)memStream.Position - start;
                memStream.Seek(sizeHere, SeekOrigin.Begin);
                binWriter.Write(imageSize);
                memStream.Seek(0, SeekOrigin.Begin);
                #endregion
            }
            catch(Exception)
            {
                return false;
            }

            #region saves icon or cur and checkes whether it was saved correctly
            Icon convertedIcon = new Icon(memStream);
            if (format == "ico")
            {
                using (Stream st = File.Create($"{directoryOfImageToConvert}\\{imageName}.ico"))
                {
                    convertedIcon.Save(st);
                    st.Close();
                }
            }
            else if (format == "cur")
            {
                using (Stream st = File.Create($"{directoryOfImageToConvert}\\{imageName}.cur"))
                {
                    convertedIcon.Save(st);
                    st.Close();
                }
            }

            if (File.Exists($"{directoryOfImageToConvert}\\{imageName}.ico") || File.Exists($"{directoryOfImageToConvert}\\{imageName}.cur"))
            {
                return true;
            }
            else
            {
                return false;
            }
            #endregion
        }
    }
}
