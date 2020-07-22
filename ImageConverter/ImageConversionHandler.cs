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
        private BitmapEncoder imageEncoder;

        private Encoder imageCompressorEncoder;
        private ImageCodecInfo imgToCompressCodecInfo;
        private EncoderParameter qualityParam;
        private EncoderParameters encoderParameters;

        /// <summary>
        /// Results of the conversion(s) with the corresponding image: if the conversion was sucessful or not
        /// </summary>
        private Dictionary<string, bool> conversionsResults;
        /// <summary>
        /// List of the results of the compression for each image
        /// </summary>
        private Dictionary<string, bool> compressionResults;

        /// <summary>
        /// Format to convert the images to
        /// </summary>
        private string chosenFormat;
        /// <summary>
        /// Temporary IMAGE located the user's temp folder, it could be a png after the ReplaceTransparency or the CompressImageAsync
        /// </summary>
        private string tempImgPath = null;
        /// <summary>
        /// Name of the image to convert
        /// </summary>
        private string imageToConvertName;
        /// <summary>
        /// directory of the image to convert
        /// </summary>
        private string pathToImageToConvertDirectory;
        /// <summary>
        /// Color to replace the transparency with. 0 = no color, 1 = white, 2 = black
        /// </summary>
        private int color = 0;
        /// <summary>
        /// Value of the final quality after compression of the image
        /// </summary>
        private int chosenQuality = 0;
        /// <summary>
        /// Delay time between two frames of a gif in centiseconds
        /// </summary>
        int delayTime;
        /// <summary>
        /// Type of compression for Tiff images
        /// </summary>
        private string chosenCompressionAlgo;
        /// <summary>
        /// Format of the image to convert
        /// </summary>
        private string imageFormat = string.Empty;
        /// <summary>
        /// Path where the image(s) will be saved
        /// </summary>
        private string savePath = string.Empty;
        /// <summary>
        /// List of compression tasks that will be executed when all the images have been converted
        /// </summary>
        private List<Task<bool>> compressionsTasks;

        /// <summary>
        /// Starts the conversion of one or more images to the specified format. Returns a string(path of the converted image) and a bool(was the conversion successful? true/false)
        /// </summary>
        /// <param name="conversionParameters"> ImageConversionParametersModel containing all the parameters of the conversion</param>
        /// <returns></returns>
        //string selectedFormat, List<string> pathsOfImagesToConvert, int gifRepeatTimes, int colorToReplTheTranspWith, int delayTime, int qualityLevel, string compressionAlgo
        public async Task<Dictionary<string, bool>> StartConversionAsync(ImageConversionParametersModel conversionParameters)
        {
            chosenFormat = conversionParameters.format;
            color = conversionParameters.colorToReplTheTranspWith;
            chosenQuality = conversionParameters.qualityLevel;
            chosenCompressionAlgo = conversionParameters.compressionAlgo;
            savePath = conversionParameters.savePath;
            List<string> pathsOfImagesToConvert = conversionParameters.pathsOfImagesToConvert;
            string selectedFormat = conversionParameters.format;
            int gifRepeatTimes = conversionParameters.gifRepeatTimes;
            delayTime = conversionParameters.delayTime;
            conversionsResults = new Dictionary<string, bool>();
            compressionResults = new Dictionary<string, bool>();
            compressionsTasks = new List<Task<bool>>();

            //Check whether the user is trying to convert an image to the same format, if yes don't convert it and set its conversionResult to false
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
                /*If the conversion result of an image isn't already "false" convert it. The conversion result of 
                *an image may have already been evaluated to be false(unsuccessful) if the user wanted to convert it to the same format */
                bool resultHasBeenAlreadyEvaluated = conversionsResults.TryGetValue(imageToConvertPath, out resultHasBeenAlreadyEvaluated);
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
                    else if (selectedFormat == "tiff")
                    {
                        conversionsResults.Add(imageToConvertPath, await Task.Run(() => ToTiffAsync(imageToConvertPath, selectedFormat)));
                    }
                }
            }

            #region Start the compression(if wanted) and change conversion results based on success of compression. Eventually combine the results of the conversions and compressions
            compressionResults = new Dictionary<string, bool>();

            /*If the user has set a quality for the compression and there's any possible compression, 
            *compress the image(s) and combine the results of the compression(s) with the conversion(s) one(s)*/
            if (chosenQuality != 100 && compressionsTasks.Count != 0)
            {
                //Start compressing the images
                List<bool> compressionResultsBoolList = await Task.Run(() => StartCompressionParallelAsync());
                //Add the compression results to the dictionary with its corresponding image, if it's a gif only the final gif will be compressed and have a result
                if (selectedFormat.ToLower() == "gif")
                {
                    for (int i = 0; i <= pathsOfImagesToConvert.Count - 1; i++)
                    {
                        compressionResults.Add(pathsOfImagesToConvert.ElementAt(i), compressionResultsBoolList.ElementAt(0));
                    }
                }
                else
                {
                    for (int i = 0; i <= pathsOfImagesToConvert.Count - 1; i++)
                    {
                        compressionResults.Add(pathsOfImagesToConvert.ElementAt(i), compressionResultsBoolList.ElementAt(i));
                    }
                }

                //Combine the results of the conversions and compressions, if one wasn't successfull set the final one(conversionResults) as false too
                for (int i = 0; i <= conversionsResults.Count - 1; i++)
                {
                    if (conversionsResults.Keys.ElementAt(i) == compressionResults.Keys.ElementAt(i))
                    {
                        conversionsResults[conversionsResults.Keys.ElementAt(i)] = conversionsResults.Values.ElementAt(i) && compressionResults.Values.ElementAt(i);
                    }
                }
            }
            #endregion

            //Deletes the content of the temporary folder in case there are any images that haven't been deleted(for example the image with the transparency replaced but still not converted)
            foreach (var file in Directory.GetFiles(Settings.Default.TempFolderPath))
            {
                File.Delete(file);
            }

            return conversionsResults;
        }

        /// <summary>
        /// Start the parallel asynchronous compressions of the given images
        /// </summary>
        /// <param name="pathsOfImagesToCompress"></param>
        /// <returns></returns>
        public async Task<List<bool>> StartCompressionParallelAsync()
        {
            foreach (var compressionTask in compressionsTasks)
            {
                //Execute task and when it finishes add its value to the list
                Task.Run(() => compressionTask);
            }
            //When the list of all the tasks completes, return the list containing all the results of the compressions
            return (await Task.WhenAll(compressionsTasks)).ToList();
        }

        #region Convert-to-formats methods
        private async Task<bool> ToPngAsync(string pathOfImageToConvert)
        {
            #region  set up image infos to convert etc.
            imageToConvertName = Path.GetFileNameWithoutExtension(pathOfImageToConvert);
            pathToImageToConvertDirectory = Path.GetDirectoryName(pathOfImageToConvert);
            imageEncoder = new PngBitmapEncoder();
            string convertedImagePath = $"{savePath}\\{imageToConvertName}_{chosenFormat}.{chosenFormat}";
            bool conversionResult = false;
            #endregion

            //Loads image to convert from a stream and converts it, from a stream because otherwise it the image to conv. would remain in use and couldn't be deleted
            using (Stream st = File.OpenRead(pathOfImageToConvert))
            {
                var imageToConv = new BitmapImage();
                imageToConv.BeginInit();
                imageToConv.StreamSource = st;
                imageToConv.CacheOption = BitmapCacheOption.OnLoad;
                imageToConv.EndInit();
                imageEncoder.Frames.Add(BitmapFrame.Create(imageToConv));
                st.Close();
            }

            #region saves the image and checks whether it was saved correctly in the conversionResult var
            using (Stream st = File.Create($"{savePath}\\{imageToConvertName}_{chosenFormat}.{chosenFormat}"))
            {
                imageEncoder.Save(st);
                st.Close();
            }
            conversionResult = await Task.Run(() => CheckIfSavedCorrectlyAsync($"{savePath}\\{imageToConvertName}_{chosenFormat}.{chosenFormat}"));
            #endregion

            //If the user decided to compress the image, add the compression task to the compressionsTasks list
            if (chosenQuality != 100)
            {
                compressionsTasks.Add(CompressImageAsync(convertedImagePath, chosenFormat, chosenQuality));
            }

            return conversionResult;
        }

        private async Task<bool> ToJpegOrJpgAsync(string pathOfImageToConvert, string format)
        {
            #region Set up infos about the image to convert etc.
            imageToConvertName = Path.GetFileNameWithoutExtension(pathOfImageToConvert);
            imageFormat = Path.GetExtension(pathOfImageToConvert).Trim('.');
            pathToImageToConvertDirectory = Path.GetDirectoryName(pathOfImageToConvert);
            imageEncoder = new JpegBitmapEncoder();
            string convertedImagePath = $"{savePath}\\{imageToConvertName}_{chosenFormat}.{chosenFormat}";
            bool conversionResult = false;
            #endregion

            //Start process of conversion
            using (Stream st = File.OpenRead(pathOfImageToConvert))
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

            #region Saves the image and checks whether it was saved correctly, return result
            using (Stream st = File.Create($"{savePath}\\{imageToConvertName}_{chosenFormat}.{chosenFormat}"))
            {
                imageEncoder.Save(st);
                st.Close();
            }
            conversionResult = await Task.Run(() => CheckIfSavedCorrectlyAsync($"{savePath}\\{imageToConvertName}_{chosenFormat}.{chosenFormat}"));
            #endregion

            //If the user decided to compress the image, add the compression task to the compressionsTasks list
            if (chosenQuality != 100 && conversionResult == true)
            {
                compressionsTasks.Add(CompressImageAsync(convertedImagePath, chosenFormat, chosenQuality));
            }

            return conversionResult;
        }

        private async Task<bool> ToBmpAsync(string pathOfImageToConvert)
        {
            #region  set up image infos to convert etc.
            imageToConvertName = Path.GetFileNameWithoutExtension(pathOfImageToConvert);
            imageFormat = Path.GetExtension(pathOfImageToConvert).Trim('.');
            pathToImageToConvertDirectory = Path.GetDirectoryName(pathOfImageToConvert);
            imageEncoder = new BmpBitmapEncoder();
            string convertedImagePath = $"{savePath}\\{imageToConvertName}_{chosenFormat}.{chosenFormat}";
            bool conversionResult = false;
            #endregion

            //loads image to convert from a stream, (in case) replace the transparency, and converts it
            using (Stream st = File.OpenRead(pathOfImageToConvert))
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

            #region saves the image and checks whether it was saved correctly
            using (Stream st = File.Create($"{savePath}\\{imageToConvertName}_{chosenFormat}.{chosenFormat}"))
            {
                imageEncoder.Save(st);
                st.Close();
            }
            conversionResult = await Task.Run(() => CheckIfSavedCorrectlyAsync($"{savePath}\\{imageToConvertName}_{chosenFormat}.{chosenFormat}"));
            #endregion

            //If the user decided to compress the image, add the compression task to the compressionsTasks list
            if (chosenQuality != 100 && conversionResult == true)
            {
                compressionsTasks.Add(CompressImageAsync(convertedImagePath, chosenFormat, chosenQuality));
            }

            return conversionResult;
        }

        //TODO: Fix conversion to gif, sometimes the final gifs are buggy, when images are pngs the delay doesn't work 
        private async Task<bool> ImagesToGifAsync(List<String> imagesToConvertPaths, int repeatTimes, int delayTime)
        {
            #region  set up image infos to convert etc.
            imageToConvertName = Path.GetFileNameWithoutExtension(imagesToConvertPaths[0]);
            pathToImageToConvertDirectory = Path.GetDirectoryName(imagesToConvertPaths[0]);
            imageEncoder = new GifBitmapEncoder();
            string convertedImagePath = $"{savePath}\\{imageToConvertName}_{chosenFormat}.{chosenFormat}";
            bool conversionResult = false;
            #endregion

            //Adds each image to the encoder, (after replacing the transparency in case the image is a png)
            foreach (var image in imagesToConvertPaths)
            {
                imageFormat = Path.GetExtension(image).Trim('.');
                //Loads image to convert from a stream, (in case) replace transparency and converts it
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

            //Adds the application extensions and graphic control extension blocks to the gif file structure
            using (var ms = new MemoryStream())
            {
                //Saves the "basic" gif to the memory
                imageEncoder.Save(ms);
                var gifBytesArr = ms.ToArray();

                // This is the NETSCAPE2.0 Application Extension to set the repeat times of the gif
                var applicationExtension = new byte[] { 33, 255, 11, 78, 69, 84, 83, 67, 65, 80, 69, 50, 46, 48, 3, 1, (byte)repeatTimes, 0, 0 };
                // This is the graphic control extension block to set the delay time between two frames
                var graphicExtension = new byte[] { 33, 249, 4, 0, (byte)delayTime, 0, 0, 0 };

                //Get the list of the bytes of the gif from its previous array, it's a copy of the gif
                var gifBytesList = gifBytesArr.ToList();

                #region Add graphic control extension block to the bytes list
                int a = 1;
                for (int i = 0; i < gifBytesList.Count; i++)
                {
                    if (gifBytesList[i] == 44 && gifBytesList[i + 1] == 0 && gifBytesList[i + 2] == 0 && gifBytesList[i + 3] == 0 && gifBytesList[i + 4] == 0)
                    {
                        Debug.WriteLine($"Found start of image descriptor block at index: {i}.\nImage descriptor n°{a}");
                        //insert new graphic extension
                        gifBytesList.InsertRange(i, graphicExtension);
                        Debug.WriteLine($"Added graphic control extension block at index {i}\n");
                        i += 18;//skip lenght of an image descriptor block to prevent an infinite loop
                        a++;
                    }
                }
                #endregion
                #region Add application extension block
                var finalGifBytesList = new List<byte>();
                //Add the Header Block and the Logical Screen Descriptor of the basic gif to the final gif
                finalGifBytesList.AddRange(gifBytesList.Take(13));
                //Add the custom application extension
                finalGifBytesList.AddRange(applicationExtension);
                //Adds the rest of the basic gif with the added graphic extensions(skipping the first 13 bytes, which are the header block etc.)
                finalGifBytesList.AddRange(gifBytesList.Skip(13));
                #endregion
                //Write changes to the already created initial gif
                File.WriteAllBytes($"{savePath}\\{imageToConvertName}_{chosenFormat}.gif", finalGifBytesList.ToArray());
                ms.Close();
            }

            //Checks wether the gif was saved correctly
            conversionResult = await Task.Run(() => CheckIfSavedCorrectlyAsync($"{savePath}\\{imageToConvertName}_{chosenFormat}.{chosenFormat}"));

            //If the user decided to compress the image, add the compression task to the compressionsTasks list
            if (chosenQuality != 100 && conversionResult == true)
            {
                compressionsTasks.Add(CompressImageAsync(convertedImagePath, chosenFormat, chosenQuality));
            }

            return conversionResult;
        }

        private async Task<bool> ToIcoOrCurAsync(string pathOfImageToConvert, string format)
        {
            var imgToConvExt = Path.GetExtension(pathOfImageToConvert).ToLower();
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
            imageToConvertName = Path.GetFileNameWithoutExtension(pathOfImageToConvert);
            imageFormat = Path.GetExtension(pathOfImageToConvert).Trim('.');
            pathToImageToConvertDirectory = Path.GetDirectoryName(pathOfImageToConvert);
            Image imgToConvert = Image.FromFile(pathOfImageToConvert);
            var memStream = new MemoryStream();
            var binWriter = new BinaryWriter(memStream);
            string convertedImagePath = $"{savePath}\\{imageToConvertName}_{chosenFormat}.{chosenFormat}";
            bool conversionResult = false;
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
                    //if the user selected a color to convert the image transparency with
                    if (color != 0 && imageFormat == "png")
                    {
                        using (Stream st = File.OpenRead(ReplaceTransparency(imgToConvert)))
                        {
                            var imgToConvWithReplacedTransp = Image.FromStream(st);
                            imgToConvWithReplacedTransp.Save(memStream, ImageFormat.Png);
                            st.Close();
                        }
                    }
                    else
                        imgToConvert.Save(memStream, ImageFormat.Png);
                }
                /*if the image to convert is a bmp then the BITMAPFILEHEADER block has to be removed, reads the bmp image bytes sequence and
                 *removes it then writes it in the memory stream */
                else
                {
                    byte[] bmpBytes = File.ReadAllBytes(pathOfImageToConvert);
                    List<byte> bmpBytesList = bmpBytes.ToList();
                    for (int i = 0; i < 14; i++)
                    {
                        bmpBytesList.RemoveAt(0);
                    }
                    bmpBytes = bmpBytesList.ToArray();
                    memStream.Write(bmpBytes, 0, bmpBytes.Length);

                }

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
                MessageBox.Show(messageBoxText: $"StackTrace: {e.StackTrace}", caption: e.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            #region Saves icon or cur and checkes whether it was saved correctly, dispose objects
            Icon convertedIcon;
            convertedIcon = new Icon(memStream);
            memStream.Close();
            using (Stream st = File.Create($"{savePath}\\{imageToConvertName}_{chosenFormat}.{chosenFormat}"))
            {
                convertedIcon.Save(st);
                st.Close();
            }
            #region Dispose objects
            binWriter.Dispose();
            memStream.Dispose();
            convertedIcon.Dispose();
            imgToConvert.Dispose();
            #endregion

            conversionResult = await Task.Run(() => CheckIfSavedCorrectlyAsync($"{savePath}\\{imageToConvertName}_{chosenFormat}.{chosenFormat}"));
            #endregion

            //If the user decided to compress the image, add the compression task to the compressionsTasks list
            if (chosenQuality != 100 && conversionResult == true)
            {
                compressionsTasks.Add(CompressImageAsync(convertedImagePath, chosenFormat, chosenQuality));
            }

            return conversionResult;
        }

        private async Task<bool> ToTiffAsync(string pathOfImageToConvert, string compressionAlgo)
        {
            #region  set up image infos to convert etc.
            imageToConvertName = Path.GetFileNameWithoutExtension(pathOfImageToConvert);
            pathToImageToConvertDirectory = Path.GetDirectoryName(pathOfImageToConvert);
            imageEncoder = new TiffBitmapEncoder();
            string convertedImagePath = $"{savePath}\\{imageToConvertName}_{chosenFormat}.{chosenFormat}";
            bool conversionResult = false;
            #endregion

            //Start process of conversion
            using (Stream st = File.OpenRead(pathOfImageToConvert))
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

            //Set compression type in the encoder
            switch (compressionAlgo.ToLower())
            {
                default:
                    break;

                case "Default":
                    ((TiffBitmapEncoder)imageEncoder).Compression = TiffCompressOption.Default;
                    break;

                case "ccitt3":
                    ((TiffBitmapEncoder)imageEncoder).Compression = TiffCompressOption.Ccitt3;
                    break;

                case "ccitt4":
                    ((TiffBitmapEncoder)imageEncoder).Compression = TiffCompressOption.Ccitt4;
                    break;

                case "lzw":
                    ((TiffBitmapEncoder)imageEncoder).Compression = TiffCompressOption.Lzw;
                    break;

                case "rle":
                    ((TiffBitmapEncoder)imageEncoder).Compression = TiffCompressOption.Rle;
                    break;

                case "zip":
                    ((TiffBitmapEncoder)imageEncoder).Compression = TiffCompressOption.Zip;
                    break;
            }

            #region Saves the image and checks whether it was saved correctly, return result
            using (Stream st = File.Create($"{savePath}\\{imageToConvertName}_{chosenFormat}.{chosenFormat}"))
            {
                imageEncoder.Save(st);
                st.Close();
            }
            conversionResult = await Task.Run(() => CheckIfSavedCorrectlyAsync($"{savePath}\\{imageToConvertName}_{chosenFormat}.{chosenFormat}"));
            #endregion

            //If the user decided to compress the image, add the compression task to the compressionsTasks list
            if (chosenQuality != 100)
            {
                compressionsTasks.Add(CompressImageAsync(convertedImagePath, chosenFormat, chosenQuality));
            }

            return conversionResult;
        }
        #endregion


        /// <summary>
        /// Compress an image, it gets called after an image has already been converted 
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="formatOfImageToCompress"></param>
        /// <param name="compressionType"></param>
        /// <param name="quality"></param>
        /// <param name="destinationPath"> Path where to save the image, if not specificed, the image will be saved to its original version's path</param>
        /// <returns></returns>
        private async Task<bool> CompressImageAsync(string imagePath, string formatOfImageToCompress, int quality, string destinationPath = "")
        {
            #region Set up variables
            encoderParameters = new EncoderParameters();
            string pathOfImageToCompressDirectory = Path.GetDirectoryName(imagePath);
            string imageName = Path.GetFileNameWithoutExtension(imagePath);
            //Get image codec info for the encoder based on the mime type of the image
            foreach (var codecInfo in ImageCodecInfo.GetImageEncoders())
            {
                if (codecInfo.MimeType == $"image/{formatOfImageToCompress.ToLower()}")
                {
                    imgToCompressCodecInfo = codecInfo;
                    break;
                }
                else if (formatOfImageToCompress.ToLower() == "jpg" || formatOfImageToCompress.ToLower() == "jpeg" && codecInfo.MimeType == $"image/jpg" || codecInfo.MimeType == $"image/jpeg")
                {
                    imgToCompressCodecInfo = codecInfo;
                }
            }
            imageCompressorEncoder = Encoder.Quality;

            qualityParam = new EncoderParameter(imageCompressorEncoder, quality);
            encoderParameters.Param[0] = qualityParam;
            #endregion

            //Open converted image, compress it and save a copy to the same directory
            using (Stream st = File.OpenRead(imagePath))
            {
                var imageToCompress = new Bitmap(st);
                //The path where to save the image is its original version's one, the name of the image already contains its extension since it has been already converted
                if (destinationPath == "")
                {
                    imageToCompress.Save($"{savePath}\\{imageName}_Compressed.{formatOfImageToCompress}", imgToCompressCodecInfo, encoderParameters);
                }
                else
                {
                    imageToCompress.Save($"{destinationPath}\\{imageName}_Compressed.{formatOfImageToCompress}", imgToCompressCodecInfo, encoderParameters);
                }
                st.Close();
            }

            if (destinationPath == "")
                return await Task.Run(() => CheckIfSavedCorrectlyAsync($"{savePath}\\{imageName}_Compressed.{formatOfImageToCompress}"));
            else
                return await Task.Run(() => CheckIfSavedCorrectlyAsync($"{destinationPath}\\{imageName}_Compressed.{formatOfImageToCompress}"));
        }

        /// <summary>
        /// Takes an Image as input, replaces its transparency and returns the path where it has been saved (in the temp folder)
        /// </summary>
        /// <param name="img">Image to which replace the transparency</param>
        /// <returns name="tempImgPath"> path where the image with the transparency replaced has been saved </returns>
        private string ReplaceTransparency(Image img)
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

        #region Utility methods

        /// <summary>
        /// Checks if an image has been converted correctly and thus if it has been saved correctly
        /// </summary>
        /// <param name="directoryOfImageToConvert"> path to the folder where the image has been saved to</param>
        /// <param name="imageName"> image of the name that has been converted and saved</param>
        /// <returns></returns>
        private async Task<bool> CheckIfSavedCorrectlyAsync(string pathOfImageToCheck)
        {
            //if the conversion was successful and the file of the converted image exists: return true
            if (await Task.Run(() => File.Exists(pathOfImageToCheck)))
            {
                return true;
            }
            //otherwise: return false
            else
            {
                return false;
            }

        }
        #endregion
    }
}
