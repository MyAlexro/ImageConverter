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

        private static string imageName; //name of the image to convert
        private static string directoryOfImageToConvert; //directory of the image to convert
        private static int color = 0; //color to replace the transparency with

        public static bool IsImage(string pathOfFile)
        {
            string filePath = pathOfFile.ToLower();
            if (filePath.Contains(".jpg") || filePath.Contains(".jpeg") || filePath.Contains(".png") || filePath.Contains(".bmp") || filePath.Contains(".ico") || filePath.Contains(".gif") || filePath.Contains(".ico") || filePath.Contains(".tiff"))
            {
                return true;
            }
            return false;
        }

        public static async Task<List<bool>> StartConversion(string format, string[] pathsOfImagesToConvert, int gifRepeatTimes, int colorToReplTheTranspWith, int delayTime)
        {
            if (colorToReplTheTranspWith != 0)
            {
                MessageBox.Show("Replace transparency not implemented yet", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                var list = new List<bool>();
                list.Add(false);
                return list;

                color = colorToReplTheTranspWith;
            }

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
            foreach (var imagePath in pathsOfImagesToConvert)
            {
                if (format == "png")
                {
                    conversionsResults.Add(await Task.Run(() => ToPng(imagePath)));
                }
                else if (format == "jpeg" || format == "jpg")
                {
                    conversionsResults.Add(await Task.Run(() => ToJpegOrJpg(imagePath, format)));
                }
                else if (format == "bmp")
                {
                    conversionsResults.Add(await Task.Run(() => ToBmp(imagePath)));
                }
                else if (format == "gif")
                {
                    conversionsResults.Add(await Task.Run(() => ImagesToGif(pathsOfImagesToConvert, gifRepeatTimes, delayTime)));
                    break;
                }
                else if (format == "ico" || format == "cur")
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
            pngEncoder = new PngBitmapEncoder();
            #endregion
            using (Stream st = File.OpenRead(pathOfImage))
            {
                var imageToConv = new BitmapImage();
                imageToConv.BeginInit();
                imageToConv.StreamSource = st;
                imageToConv.CacheOption = BitmapCacheOption.OnLoad;
                imageToConv.EndInit();
                pngEncoder.Frames.Add(BitmapFrame.Create(imageToConv));
                st.Close();
            }//loads image to convert from a stream and converts it

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
            jpegOrJpgEncoder = new JpegBitmapEncoder();
            #endregion
            using (Stream st = File.OpenRead(pathOfImage))
            {
                var imageToConv = new BitmapImage();
                imageToConv.BeginInit();
                imageToConv.StreamSource = st;
                imageToConv.CacheOption = BitmapCacheOption.OnLoad;
                imageToConv.EndInit();
                jpegOrJpgEncoder.Frames.Add(BitmapFrame.Create(imageToConv));

                st.Close();
            }//loads image to convert from a stream and converts it

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
            bmpEncoder = new BmpBitmapEncoder();
            #endregion
            using (Stream st = File.OpenRead(pathOfImage))
            {
                var imageToConv = new BitmapImage();
                imageToConv.BeginInit();
                imageToConv.StreamSource = st;
                imageToConv.CacheOption = BitmapCacheOption.OnLoad;
                imageToConv.EndInit();

                bmpEncoder.Frames.Add(BitmapFrame.Create(imageToConv));
                st.Close();
            }//loads image to convert from a stream and converts it

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

        private static async Task<bool> ImagesToGif(string[] imagesPaths, int repeatTimes, int delayTime)
        {
            #region  set up image infos to convert etc.
            imageName = Path.GetFileNameWithoutExtension(imagesPaths[0]);
            directoryOfImageToConvert = Path.GetDirectoryName(imagesPaths[0]);
            gifEncoder = new GifBitmapEncoder();
            #endregion

            foreach (var image in imagesPaths)
            {
                using (Stream st = File.OpenRead(image))
                {
                    var imageToConv = new BitmapImage();
                    imageToConv.BeginInit();
                    imageToConv.StreamSource = st;
                    imageToConv.CacheOption = BitmapCacheOption.OnLoad;
                    imageToConv.EndInit();

                    gifEncoder.Frames.Add(BitmapFrame.Create(imageToConv));
                    st.Close();
                }//loads image to convert from a stream and converts it
            }
            using (var ms = new MemoryStream())
            {
                gifEncoder.Save(ms);
                var fileBytes = ms.ToArray();

                // This is the NETSCAPE2.0 Application Extension to set the repeat times of the gif
                var applicationExtension = new byte[] { 33, 255, 11, 78, 69, 84, 83, 67, 65, 80, 69, 50, 46, 48, 3, 1, (byte)repeatTimes, 0, 0 };

                // This is the graphic control extension block to set the delay time between two frames
                var graphicExtension = new byte[] { 33, 249, 4, 0, (byte)delayTime, 0, 0, 0 };
                var fileBytesList = fileBytes.ToList<byte>();

                #region add graphic control extension block
                int a = 1;
                for (int i = 0; i < fileBytesList.Count; i++)
                {
                    if(fileBytesList[i] == 44 && fileBytesList[i+1] == 0 && fileBytesList[i+2] == 0 && fileBytesList[i+3] == 0 && fileBytesList[i+4] == 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"Found start of image descriptor block at index: {i}.\nImage descriptor n°{a}");
                        fileBytesList.InsertRange(i, graphicExtension);
                        System.Diagnostics.Debug.WriteLine($"Added graphic control extension block at index {i}\n");
                        i += 18;
                        a++;
                    }
                }
                fileBytes = fileBytesList.ToArray();
                #endregion
                var newBytes = new List<byte>();
                #region adds application extension block
                newBytes.AddRange(fileBytes.Take(13));
                newBytes.AddRange(applicationExtension);
                newBytes.AddRange(fileBytes.Skip(13));
                #endregion

                File.WriteAllBytes($"{directoryOfImageToConvert}\\{imageName}_Gif.gif", newBytes.ToArray());
                ms.Close();

            }//add the application extensions and graphic control extension blocks

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

                binWriter.Write((short)1); //offset #2

                binWriter.Write((short)1);   //offset #4: number of resolutions of the image in the final icon 
                #endregion
                #region Structure of image directory
                byte w = (byte)imgToConvert.Width;
                if (w > 255)
                    w = 0;
                binWriter.Write((byte)w); //offset #0: image width

                byte h = (byte)imgToConvert.Height;
                if (h > 255) h = 0;
                binWriter.Write(h); //offset #1: image height

                binWriter.Write((byte)0);    //offset #2: number of colors in palette
                binWriter.Write((byte)0);    //offset #3: reserved

                if (format == "ico")
                    binWriter.Write((short)0); //offset #4: (if ico) number of color planes
                else
                    binWriter.Write((short)1); //offset #4: (if cur) horizontal coordinates of hotspot,in pixels, from the left

                if (format == "ico")
                    binWriter.Write((short)1); //offset #6: (if ico) bits per pixel
                else
                    binWriter.Write((short)1); //offset #6: (if cur) vertical coordinates of hotspot,in pixels, from the top

                var sizeHere = memStream.Position;
                binWriter.Write((int)sizeHere);    //offset #8: size of image data

                var start = (int)memStream.Position + 4;
                binWriter.Write(start); //offset #12: of image data from the beginning of the ico/cur file
                #endregion
                #region Image data
                if (ext == ".png")
                    imgToConvert.Save(memStream, ImageFormat.Png);
                else
                {
                    byte[] bmpBytes = File.ReadAllBytes(imgToConvertPath);
                    List<byte> bytesList = bmpBytes.ToList<byte>();
                    for (int i = 0; i <= 14; i++)
                    {
                        bytesList.RemoveAt(i);
                    }
                    bmpBytes = bytesList.ToArray();
                    Stream stream = new MemoryStream(bmpBytes);
                    imgToConvert = Image.FromStream(stream);
                    imgToConvert.Save(memStream, ImageFormat.Png);
                }//if the image to convert is a bmp then the BITMAPFILEHEADER has to be deleted

                var imageSize = (int)memStream.Position - start;
                memStream.Seek(sizeHere, SeekOrigin.Begin);
                binWriter.Write(imageSize);
                memStream.Seek(0, SeekOrigin.Begin);
                #endregion
            }
            catch (Exception)
            {
                return false;
            }

            #region saves icon or cur and checkes whether it was saved correctly
            Icon convertedIcon;
            convertedIcon = new Icon(memStream);
            memStream.Close();
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

        private static async Task<BitmapImage> ReplaceTransparency(Image img)
        {
            return new BitmapImage();
        }
    }
}
