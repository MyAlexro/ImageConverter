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
using System.Diagnostics;

namespace ImageConverter
{
    public class ImageConversionHandler
    {
        private static BitmapEncoder imageEncoder;

        /// <summary>
        /// Results of the conversion(s) with the corresponding image: if the conversion was sucessful or not
        /// </summary>
        private static Dictionary<string, bool?> conversionsResults;

        private static string chosenFormat;
        private static string tempImgPath = null;
        /// <summary>
        /// Name of the image to convert
        /// </summary>
        private static string imageName;
        /// <summary>
        /// directory of the image to convert
        /// </summary>
        private static string pathOfImageToConvert;
        /// <summary>
        /// Color to replace the transparency with. 0 = no color, 1 = white, 2 = black
        /// </summary>
        private static int color = 0;
        /// <summary>
        /// Format of the image to convert
        /// </summary>
        private static string imageFormat = String.Empty;

        /// <summary>
        /// Checks wether the given path of the file leads to an image
        /// </summary>
        /// <param type="string" name="pathOfFile"> path of the file that needs to be checked </param>
        /// <returns type="bool" name="IsImage"> true if the file is an image, otherwise false</returns>
        public static bool IsImage(string pathOfFile)
        {
            string filePath = pathOfFile.ToLower();
            if (filePath.Contains(".jpg") || filePath.Contains(".jpeg") || filePath.Contains(".png") || filePath.Contains(".bmp") || filePath.Contains(".ico") || filePath.Contains(".gif") || filePath.Contains(".cur") || filePath.Contains(".tiff"))
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// Starts the conversion of one or more images to the specified format. Returns a string(path of the converted image) and a bool(was the conversion successful? true/false)
        /// </summary>
        /// <param name="selectedFormat"> format to which convert the image</param>
        /// <param name="pathsOfImagesToConvert"> path of the image to convert </param>
        /// <param name="gifRepeatTimes"> the times the gif shall repeat: infinite(0)-10 </param>
        /// <param name="colorToReplTheTranspWith"> Color to replace the transparency of a png image with</param>
        /// <param name="delayTime"> delay between two frames of a gif</param>
        /// <returns></returns>
        public static async Task<Dictionary<string, bool?>> StartConversionAsync(string selectedFormat, List<string> pathsOfImagesToConvert, int gifRepeatTimes, int colorToReplTheTranspWith, int delayTime)
        {
            chosenFormat = selectedFormat;
            color = colorToReplTheTranspWith;
            conversionsResults = new Dictionary<string, bool?>();

            //Checks whether the user is trying to convert an image to the same format, if yes don't convert it and set its conversionResult to false
            foreach (var imageToConvertPath in pathsOfImagesToConvert)
            {
                var imgToConvertExt = Path.GetExtension(imageToConvertPath).ToLower().Trim('.');
                if (imgToConvertExt == selectedFormat)
                {
                    conversionsResults.Add(imageToConvertPath, false);
                }
            }

            //Start the conversion of each image
            foreach (var imageToConvertPath in pathsOfImagesToConvert)
            {
                /*If the conversion result of an image isn't false convert it. The conversion result of 
                *an image may have already been evaluated to be false(unsuccessful) if the user wanted to convert it to the same format */
                bool? resultHasBeenAlreadyEvaluated = conversionsResults.TryGetValue(imageToConvertPath, out resultHasBeenAlreadyEvaluated);
                if (resultHasBeenAlreadyEvaluated != true)
                {
                    if (selectedFormat == "png")
                    {
                        conversionsResults.Add(imageToConvertPath, await Task.Run(() => ToPngAsync(imageToConvertPath)));
                    }
                    else if (selectedFormat == "jpeg" || selectedFormat == "jpg")
                    {
                        conversionsResults.Add(imageToConvertPath, await Task.Run(() => ToJpegOrJpgAsync(imageToConvertPath, selectedFormat)));
                    }
                    else if (selectedFormat == "bmp")
                    {
                        conversionsResults.Add(imageToConvertPath, await Task.Run(() => ToBmpAsync(imageToConvertPath)));
                    }
                    else if (selectedFormat == "gif")
                    {
                        conversionsResults.Add(imageToConvertPath, await Task.Run(() => ImagesToGifAsync(pathsOfImagesToConvert, gifRepeatTimes, delayTime)));
                        break;
                    }
                    else if (selectedFormat == "ico" || selectedFormat == "cur")
                    {
                        conversionsResults.Add(imageToConvertPath, await Task.Run(() => ToIcoOrCurAsync(imageToConvertPath, selectedFormat)));
                    }
                }
            }

            //Deletes the temporary images in the temp folder(for example the image with the transparency replaced but still not converted)
            if (tempImgPath != null)
                File.Delete(tempImgPath);

            return conversionsResults;
        }

        private static async Task<bool> ToPngAsync(string pathOfImage)
        {
            #region  set up image infos to convert etc.
            imageName = Path.GetFileNameWithoutExtension(pathOfImage);
            pathOfImageToConvert = Path.GetDirectoryName(pathOfImage);
            imageEncoder = new PngBitmapEncoder();
            #endregion

            //Loads image to convert from a stream and converts it, from a stream because otherwise it the image to conv. would remain in use and couldn't be deleted
            using (Stream st = File.OpenRead(pathOfImage))
            {
                var imageToConv = new BitmapImage();
                imageToConv.BeginInit();
                imageToConv.StreamSource = st;
                imageToConv.CacheOption = BitmapCacheOption.OnLoad;
                imageToConv.EndInit();
                imageEncoder.Frames.Add(BitmapFrame.Create(imageToConv));
                st.Close();
            }

            #region saves the image and checks whether it was save correctly
            using (Stream st = File.Create($"{pathOfImageToConvert}\\{imageName}_{chosenFormat}.png"))
            {
                imageEncoder.Save(st);
                st.Close();
            }
            return await Task.Run(() => CheckIfSavedCorrectlyAsync(pathOfImageToConvert, imageName));
            #endregion
        }

        private static async Task<bool> ToJpegOrJpgAsync(string pathOfImage, string format)
        {
            #region Set up infos about the image to convert etc.
            imageName = Path.GetFileNameWithoutExtension(pathOfImage);
            imageFormat = Path.GetExtension(pathOfImage).Trim('.');
            pathOfImageToConvert = Path.GetDirectoryName(pathOfImage);
            imageEncoder = new JpegBitmapEncoder();
            #endregion

            using (Stream st = File.OpenRead(pathOfImage))
            {
                var imageToConv = new BitmapImage();

                //if the user selected a color to convert the image transparency with and the image is a png replace transparency etc
                if (color != 0 && imageFormat == "png")
                {
                    Image imgToConvertAsImage = Image.FromStream(st);
                    using (Stream st2 = File.OpenRead(ReplaceTransparency(imgToConvertAsImage)))
                    {
                        imageToConv.BeginInit();
                        imageToConv.StreamSource = st2;
                        imageToConv.CacheOption = BitmapCacheOption.OnLoad;
                        imageToConv.EndInit();
                        imageEncoder.Frames.Add(BitmapFrame.Create(imageToConv));
                        st2.Close();
                    }
                }
                //else loads directly the image to convert from the stream and converts it
                else
                {
                    imageToConv.BeginInit();
                    imageToConv.StreamSource = st;
                    imageToConv.CacheOption = BitmapCacheOption.OnLoad;
                    imageToConv.EndInit();
                    imageEncoder.Frames.Add(BitmapFrame.Create(imageToConv));
                }
            }

            #region Saves image based on format(jpeg or jpg) and checkes whether the converted image was saved correctly
            if (format == "jpeg")
            {
                using (Stream st = File.Create($"{pathOfImageToConvert}\\{imageName}_{chosenFormat}.jpeg"))
                {
                    imageEncoder.Save(st);
                    st.Close();
                }
            }
            else
            {
                using (Stream st = File.Create($"{pathOfImageToConvert}\\{imageName}_{chosenFormat}.jpg"))
                {
                    imageEncoder.Save(st);
                    st.Close();
                }
            }

            return await Task.Run(() => CheckIfSavedCorrectlyAsync(pathOfImageToConvert, imageName));
            #endregion
        }

        private static async Task<bool> ToBmpAsync(string pathOfImage)
        {
            #region  set up image infos to convert etc.
            imageName = Path.GetFileNameWithoutExtension(pathOfImage);
            imageFormat = Path.GetExtension(pathOfImage).Trim('.');
            pathOfImageToConvert = Path.GetDirectoryName(pathOfImage);
            imageEncoder = new BmpBitmapEncoder();
            #endregion

            //loads image to convert from a stream, eventually replace the transparency, and converts it
            using (Stream st = File.OpenRead(pathOfImage))
            {
                var imageToConv = new BitmapImage();

                //if the user selected a color to convert the image transparency with and image is png
                if (color != 0 && imageFormat == "png")
                {
                    Image imgToConvertImage = Image.FromStream(st);
                    using (Stream st2 = File.OpenRead(ReplaceTransparency(imgToConvertImage)))
                    {
                        imageToConv.BeginInit();
                        imageToConv.StreamSource = st2;
                        imageToConv.CacheOption = BitmapCacheOption.OnLoad;
                        imageToConv.EndInit();
                        imageEncoder.Frames.Add(BitmapFrame.Create(imageToConv));
                        st2.Close();
                    }
                }
                else
                {
                    imageToConv.BeginInit();
                    imageToConv.StreamSource = st;
                    imageToConv.CacheOption = BitmapCacheOption.OnLoad;
                    imageToConv.EndInit();
                    imageEncoder.Frames.Add(BitmapFrame.Create(imageToConv));
                }
                st.Close();
            }

            #region Saves bmp image and checkes whether the converted image was saved correctly
            using (Stream st = File.Create($"{pathOfImageToConvert}\\{imageName}_{chosenFormat}.bmp"))
            {
                imageEncoder.Save(st);
                st.Close();
            }

            return await Task.Run(() => CheckIfSavedCorrectlyAsync(pathOfImageToConvert, imageName));
            #endregion
        }

        //TODO: Fix conversion to gif, sometimes the final gifs are buggy, when images are pngs the delay doesn't work 
        private static async Task<bool> ImagesToGifAsync(List<String> imagesPaths, int repeatTimes, int delayTime)
        {
            #region  set up image infos to convert etc.
            imageName = Path.GetFileNameWithoutExtension(imagesPaths[0]);
            pathOfImageToConvert = Path.GetDirectoryName(imagesPaths[0]);
            imageEncoder = new GifBitmapEncoder();
            #endregion

            foreach (var image in imagesPaths)
            {
                imageFormat = Path.GetExtension(image).Trim('.');
                //Loads image to convert from a stream, eventually replace transparency and converts it
                using (Stream st = File.OpenRead(image))
                {
                    var imageToConv = new BitmapImage();

                    //If the user has chosen to replace the background of a png image with a color
                    if (color != 0 && imageFormat == "png") 
                    {
                        Image imgToConvertAsImage = Image.FromStream(st);
                        using (Stream st2 = File.OpenRead(ReplaceTransparency(imgToConvertAsImage)))
                        {
                            imageToConv.BeginInit();
                            imageToConv.StreamSource = st2;
                            imageToConv.CacheOption = BitmapCacheOption.OnLoad;
                            imageToConv.EndInit();
                            imageEncoder.Frames.Add(BitmapFrame.Create(imageToConv));
                            st2.Close();
                        }
                    }
                    else
                    {
                        imageToConv.BeginInit();
                        imageToConv.StreamSource = st;
                        imageToConv.CacheOption = BitmapCacheOption.OnLoad;
                        imageToConv.EndInit();

                        imageEncoder.Frames.Add(BitmapFrame.Create(imageToConv));
                    }

                    st.Close();
                }
            }

            //adds the application extensions and graphic control extension blocks to the gif file structure
            using (var ms = new MemoryStream())
            {
                imageEncoder.Save(ms);
                var gifBytesArr = ms.ToArray();

                // This is the NETSCAPE2.0 Application Extension to set the repeat times of the gif
                var applicationExtension = new byte[] { 33, 255, 11, 78, 69, 84, 83, 67, 65, 80, 69, 50, 46, 48, 3, 1, (byte)repeatTimes, 0, 0 };
                // This is the graphic control extension block to set the delay time between two frames
                var graphicExtension = new byte[] { 33, 249, 4, 0, (byte)delayTime, 0, 0, 0 };

                var gifBytesList = gifBytesArr.ToList();

                #region add graphic control extension block
                int a = 1;
                for (int i = 0; i < gifBytesList.Count; i++)
                {
                    if (gifBytesList[i] == 44 && gifBytesList[i + 1] == 0 && gifBytesList[i + 2] == 0 && gifBytesList[i + 3] == 0 && gifBytesList[i + 4] == 0)
                    {
                        Debug.WriteLine($"Found start of image descriptor block at index: {i}.\nImage descriptor n°{a}");
                        //insert new graphic extension
                        gifBytesList.InsertRange(i, graphicExtension);
                        Debug.WriteLine($"Added graphic control extension block at index {i}\n");
                        i += 18;//lenght of an image descriptor block
                        a++;
                    }
                }
                #endregion
                #region adds application extension block
                var gifFinalBytesList = new List<byte>();
                gifFinalBytesList.AddRange(gifBytesList.Take(13));
                gifFinalBytesList.AddRange(applicationExtension);
                gifFinalBytesList.AddRange(gifBytesList.Skip(13));
                #endregion

                File.WriteAllBytes($"{pathOfImageToConvert}\\{imageName}_{chosenFormat}.gif", gifFinalBytesList.ToArray());
                ms.Close();

            }

            return await Task.Run(() => CheckIfSavedCorrectlyAsync(pathOfImageToConvert, imageName));
        } 

        private static async Task<bool> ToIcoOrCurAsync(string pathOfImage, string format)
        {
            var imgToConvExt = Path.GetExtension(pathOfImage).ToLower();
            #region if the image to convert isn't a png or bmp image it can't be converterd: return false
            if (imgToConvExt != ".png" && imgToConvExt != ".bmp")
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
            imageName = Path.GetFileNameWithoutExtension(pathOfImage);
            imageFormat = Path.GetExtension(pathOfImage).Trim('.');
            pathOfImageToConvert = Path.GetDirectoryName(pathOfImage);
            Image imgToConvert = Image.FromFile(pathOfImage);
            var memStream = new MemoryStream();
            var binWriter = new BinaryWriter(memStream);
            #endregion

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
            binWriter.Write(start); //offset #12 of image data from the beginning of the ico/cur file
            #endregion
            try
            {
                #region Image data
                if (imgToConvExt == ".png")
                {
                    if (color != 0 && imageFormat == "png")
                    {
                        using (Stream st = File.OpenRead(ReplaceTransparency(imgToConvert)))
                        {
                            var imgToConvWithReplacedTransp = Image.FromStream(st);
                            imgToConvWithReplacedTransp.Save(memStream, ImageFormat.Png);
                            st.Close();
                        }
                    }//if the user selected a color to convert the image transparency with
                    else
                        imgToConvert.Save(memStream, ImageFormat.Png);
                }
                else
                {
                    byte[] bmpBytes = File.ReadAllBytes(pathOfImage);
                    List<byte> bmpBytesList = bmpBytes.ToList();
                    for (int i = 0; i < 14; i++)
                    {
                        bmpBytesList.RemoveAt(0);
                    }
                    bmpBytes = bmpBytesList.ToArray();
                    memStream.Write(bmpBytes, 0, bmpBytes.Length);

                }//if the image to convert is a bmp then the BITMAPFILEHEADER block has to be removed, reads the bmp image bytes sequence and removes it then writes it in the memory stream

                var imageSize = (int)memStream.Position - start;
                memStream.Seek(sizeHere, SeekOrigin.Begin);
                binWriter.Write(imageSize);
                memStream.Seek(0, SeekOrigin.Begin);
                #endregion
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
                System.Diagnostics.Debug.WriteLine("\n");
                System.Diagnostics.Debug.WriteLine(e.Message);
                MessageBox.Show(messageBoxText:$"StackTrace: {e.StackTrace}", caption:e.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            #region saves icon or cur and checkes whether it was saved correctly
            Icon convertedIcon;
            convertedIcon = new Icon(memStream);
            memStream.Close();
            using (Stream st = File.Create($"{pathOfImageToConvert}\\{imageName}_{chosenFormat}.{chosenFormat}"))
            {
                convertedIcon.Save(st);
                st.Close();
            }


            #region dispose objects
            binWriter.Dispose();
            memStream.Dispose();
            convertedIcon.Dispose();
            imgToConvert.Dispose();
            #endregion

            return await Task.Run(() => CheckIfSavedCorrectlyAsync(pathOfImageToConvert, imageName));
            #endregion
        }

        /// <summary>
        /// Takes an Image as input, replaces its transparency and returns the path where it has been saved (in the temp folder)
        /// </summary>
        /// <param name="img">Image to which replace the transparency</param>
        /// <returns name="tempImgPath"> path where the image with the transparency replaced has been saved </returns>
        private static string ReplaceTransparency(Image img)
        {
            Bitmap imgWithTranspReplaced = new Bitmap(img.Width, img.Height);
            Graphics g = Graphics.FromImage(imgWithTranspReplaced);

            //replace transparency with white
            if (color == 1)
            {
                g.Clear(Color.White);
            }
            //replace transparency with black
            else
            {
                g.Clear(Color.Black);
            }
            g.DrawImage(img, 0, 0);

            #region Saves imgWithTranspReplaced in the temp folder, dispose objects and returns its path
            imgWithTranspReplaced.Save($"{Settings.Default.TempFolderPath}\\tempImgWithTranspReplaced.png");
            tempImgPath = $"{Settings.Default.TempFolderPath}\\tempImgWithTranspReplaced.png";
            imgWithTranspReplaced.Dispose();
            g.Dispose();

            return tempImgPath;
            #endregion
        }

        private static Task<bool> CompressImageAsync(string imagePath, object compressionType, int quality)
        {
            return null;
        }

        /// <summary>
        /// Checks if an image has been converted correctly and thus if it has been saved correctly
        /// </summary>
        /// <param name="directoryOfImageToConvert"> path to the folder where the image has been saved to</param>
        /// <param name="imageName"> image of the name that has been converted and saved</param>
        /// <returns></returns>
        private static async Task<bool> CheckIfSavedCorrectlyAsync(string directoryOfImageToConvert, string imageName)
        {
            //if the conversion was successful and the file of the converted image exists: return true
            if (await Task.Run(() => File.Exists($"{directoryOfImageToConvert}\\{imageName}_{chosenFormat}.{chosenFormat}")))
            {
                return true;
            }
            //otherwise: return false
            else
            {
                return false;
            }

        }
    }
}
