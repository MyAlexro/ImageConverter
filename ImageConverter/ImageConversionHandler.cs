/*MIT License
*Copyright (c) 2021 Alessandro Dinardo
*
*Permission is hereby granted, free of charge, to any person obtaining a copy
*of this software and associated documentation files (the "Software"), to deal
*in the Software without restriction, including without limitation the rights
*to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
*copies of the Software, and to permit persons to whom the Software is
*furnished to do so, subject to the following conditions:

*The above copyright notice and this permission notice shall be included in all
*copies or substantial portions of the Software.
*
*THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
*IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
*FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
*AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
*LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
*OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

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
using ImageConverter.HelperClasses;
using ImageConverter.Models;

namespace ImageConverter
{
    public class ImageConversionHandler
    {
        /// <summary>
        /// Format to convert the images to
        /// </summary>
        private string chosenFormat;
        /// <summary>
        /// Color to replace the transparency with. 0 = no color, 1 = white, 2 = black
        /// </summary>
        private int color = 0;
        /// <summary>
        /// Value of the final quality after compression of the image
        /// </summary>
        private int compressionQuality = 0;
        /// <summary>
        /// Delay time between two frames of a gif in centiseconds
        /// </summary>
        int delayTime;
        /// <summary>
        /// Type of compression for Tiff images
        /// </summary>
        private string tiffCompressionAlgo;
        /// <summary>
        /// Path where the converted image(s) will be saved
        /// </summary>
        private string savePath = string.Empty;
        /// <summary>
        /// List of compression tasks that will be executed when all the images have been converted
        /// </summary>
        private List<CompressionParametersModel> compressionTasksParameters;

        /// <summary>
        /// Starts the conversion of one or more images to the specified format. Returns a string(path of the converted image) and a bool(was the conversion successful? true/false)
        /// </summary>
        /// <param name="conversionParameters"> ImageConversionParametersModel containing all the parameters of the conversion</param>
        /// <returns></returns>
        public async Task<Dictionary<string, bool>> StartConversionAsync(ImageConversionParametersModel conversionParameters)
        {
            #region Prepare conversion variable etc
            chosenFormat = conversionParameters.format;
            color = conversionParameters.colorToReplTheTranspWith;
            compressionQuality = conversionParameters.qualityLevel;
            tiffCompressionAlgo = conversionParameters.compressionAlgo;
            savePath = conversionParameters.savePath;
            List<string> pathsOfImagesToConvert = conversionParameters.pathsOfImagesToConvert;
            string selectedFormat = conversionParameters.format;
            int gifRepeatTimes = conversionParameters.gifRepeatTimes;
            delayTime = conversionParameters.delayTime;
            //Results of the conversion(s) with the corresponding image: if the conversion was sucessful or not
            var conversionsResults = new Dictionary<string, bool>();
            //List of the results of the compression for each image
            var compressionResults = new Dictionary<string, bool>();
            compressionTasksParameters = new List<CompressionParametersModel>();
            #endregion

            //Check whether the user is trying to convert an image to the same format, if yes don't convert by setting already its conversionResult to false
            pathsOfImagesToConvert.ForEach(delegate (string imageToConvertPath)
            {
                var imgToConvertExt = Path.GetExtension(imageToConvertPath).ToLower().Trim('.');
                if (imgToConvertExt == selectedFormat)
                {
                    conversionsResults.Add(imageToConvertPath, false);
                }
            });

            //Start the conversion of each image and add the compression task for that image
            foreach (var imageToConvertPath in pathsOfImagesToConvert)
            {
                (bool conversionResult, string convertedImagePath) resultsTuple;

                /*If the conversion result of an image isn't already "false" convert it. The conversion result of 
                *an image may have already been evaluated to be false(unsuccessful) if the user wanted to convert it to its same format */
bool resultHasBeenAlreadyEvaluated = conversionsResults.TryGetValue(imageToConvertPath, out resultHasBeenAlreadyEvaluated);
                if (resultHasBeenAlreadyEvaluated != true)
                {
                    if (selectedFormat == "png")
                    {
                        /* If the user wants to compress the image and wants only the converted&compressed image to be saved, save the converted-only image in the 
                        * temp folder because the compressed one will be saved in the chosen savePath later */
                        if (compressionQuality != 100 && Settings.Default.SaveBothImages == false)
                            resultsTuple = await Task.Run(() => ConvertToPngAndSaveAsync(imageToConvertPath, Settings.Default.TempFolderPath));
                        //otherwise if the user doesn't want to compress it, or wants both the versions of the image, save it to the chosen savePath
                        else
                            resultsTuple = await Task.Run(() => ConvertToPngAndSaveAsync(imageToConvertPath, savePath));

                        //If the user wants to compress the image, if the conversion has been successful, add the compression task for the converted image
                        if (compressionQuality != 100 && resultsTuple.conversionResult == true)
                        {
                            compressionTasksParameters.Add(new CompressionParametersModel
                            {
                                imageToCompressPath = resultsTuple.convertedImagePath,
                                quality = compressionQuality,
                                savePath = savePath,
                            });
                        }
                        conversionsResults.Add(imageToConvertPath, resultsTuple.conversionResult);
                    }
                    else if (selectedFormat == "jpeg" || selectedFormat == "jpg")
                    {
                        if (compressionQuality != 100 && Settings.Default.SaveBothImages == false)
                            resultsTuple = await Task.Run(() => ConvertToJpegOrJpgAndSaveAsync(imageToConvertPath, chosenFormat, Settings.Default.TempFolderPath));
                        else
                            resultsTuple = await Task.Run(() => ConvertToJpegOrJpgAndSaveAsync(imageToConvertPath, chosenFormat, savePath));

                        if (compressionQuality != 100 && resultsTuple.conversionResult == true)
                        {
                            compressionTasksParameters.Add(new CompressionParametersModel
                            {
                                imageToCompressPath = resultsTuple.convertedImagePath,
                                quality = compressionQuality,
                                savePath = savePath,
                            });
                        }
                        conversionsResults.Add(imageToConvertPath, resultsTuple.conversionResult);
                    }
                    else if (selectedFormat == "bmp")
                    {
                        if (compressionQuality != 100 && Settings.Default.SaveBothImages == false)
                            resultsTuple = await Task.Run(() => ConvertoToBmpAndSaveAsync(imageToConvertPath, Settings.Default.TempFolderPath));
                        else
                            resultsTuple = await Task.Run(() => ConvertoToBmpAndSaveAsync(imageToConvertPath, savePath));

                        if (compressionQuality != 100 && resultsTuple.conversionResult == true)
                        {
                            compressionTasksParameters.Add(new CompressionParametersModel
                            {
                                imageToCompressPath = resultsTuple.convertedImagePath,
                                quality = compressionQuality,
                                savePath = savePath,
                            });
                        }
                        conversionsResults.Add(imageToConvertPath, resultsTuple.conversionResult);
                    }
                    else if (selectedFormat == "gif")
                    {
                        //Compressing directly the gif will break it, so add the compression tasks of the original images (then create the "compressed" gif with the compressed images)

                        /*If the user wants only the compressed gif, don't create the gif with the uncompressed images because it's useless, add the tasks for the compression of
                        * the images that will be used to create the gif later on in the start compression region*/
                        if (compressionQuality != 100 && Settings.Default.SaveBothImages == false)
                        {
                            foreach (var imagePath in pathsOfImagesToConvert)
                            {
                                compressionTasksParameters.Add(new CompressionParametersModel
                                {
                                    imageToCompressPath = imagePath,
                                    quality = compressionQuality,
                                    savePath = Settings.Default.TempFolderPath,
                                });
                            }
                        }
                        //Else if the user wants both version, create the uncompressed gif, then add the compression tasks for the images that will be used to create the gif later on in the start compression region
                        else if (compressionQuality != 100 && Settings.Default.SaveBothImages == true)
                        {
                            resultsTuple = await Task.Run(() => ConvertToGifAndSaveAsync(pathsOfImagesToConvert, gifRepeatTimes, delayTime, savePath));
                            //Adds the conversion result for each image
                            pathsOfImagesToConvert.ForEach(delegate (string imagePath) { conversionsResults.Add(imagePath, resultsTuple.conversionResult); });

                            foreach (var imagePath in pathsOfImagesToConvert)
                            {
                                if (resultsTuple.conversionResult == true)
                                {
                                    compressionTasksParameters.Add(new CompressionParametersModel
                                    {
                                        imageToCompressPath = imagePath,
                                        quality = compressionQuality,
                                        savePath = Settings.Default.TempFolderPath,
                                    });
                                }
                            }
                        }
                        //If the user doesn't want to compress the gif, convert the images into gif.
                        else
                        {
                            resultsTuple = await Task.Run(() => ConvertToGifAndSaveAsync(pathsOfImagesToConvert, gifRepeatTimes, delayTime, savePath));
                            pathsOfImagesToConvert.ForEach(delegate (string imagePath) { conversionsResults.Add(imagePath, resultsTuple.conversionResult); });
                        }
                        break;
                    }
                    else if (selectedFormat == "ico" || selectedFormat == "cur")
                    {
                        /* Compressing directly the ico/cur will break it, so add the compression tasks of the original images (then create the 
                        * "compressed" ico/cur with the compressed images) */

                        //Same things from the gif^
                        if (compressionQuality != 100 && Settings.Default.SaveBothImages == false)
                        {
                            foreach (var imagePath in pathsOfImagesToConvert)
                            {
                                compressionTasksParameters.Add(new CompressionParametersModel
                                {
                                    imageToCompressPath = imagePath,
                                    quality = compressionQuality,
                                    savePath = Settings.Default.TempFolderPath,
                                });
                            }
                        }
                        else if (compressionQuality != 100 && Settings.Default.SaveBothImages == true)
                        {
                            resultsTuple = await Task.Run(() => ConvertToIcoOrCurAndSaveAsync(pathsOfImagesToConvert, selectedFormat, savePath));
                            pathsOfImagesToConvert.ForEach(delegate (string imagePath) { conversionsResults.Add(imagePath, resultsTuple.conversionResult); });
                            foreach (var imagePath in pathsOfImagesToConvert)
                            {
                                if (resultsTuple.conversionResult == true)
                                {
                                    compressionTasksParameters.Add(new CompressionParametersModel
                                    {
                                        imageToCompressPath = imagePath,
                                        quality = compressionQuality,
                                        savePath = Settings.Default.TempFolderPath,
                                    });
                                }
                            }
                        }
                        else if (compressionQuality == 100)
                        {
                            resultsTuple = await Task.Run(() => ConvertToIcoOrCurAndSaveAsync(pathsOfImagesToConvert, selectedFormat, savePath));
                            pathsOfImagesToConvert.ForEach(delegate (string imagePath) { conversionsResults.Add(imagePath, resultsTuple.conversionResult); });
                        }
                        break;
                    }
                    else if (selectedFormat == "tiff")
                    {
                        if (compressionQuality != 100 && Settings.Default.SaveBothImages == false)
                            resultsTuple = await Task.Run(() => ConvertToTiffAndSaveAsync(imageToConvertPath, tiffCompressionAlgo, Settings.Default.TempFolderPath));
                        else
                            resultsTuple = await Task.Run(() => ConvertToTiffAndSaveAsync(imageToConvertPath, tiffCompressionAlgo, savePath));

                        if (compressionQuality != 100 && resultsTuple.conversionResult == true)
                        {
                            compressionTasksParameters.Add(new CompressionParametersModel
                            {
                                imageToCompressPath = resultsTuple.convertedImagePath,
                                quality = compressionQuality,
                                savePath = savePath,
                            });
                        }
                        conversionsResults.Add(imageToConvertPath, resultsTuple.conversionResult);
                    }
                }
            }

            #region Start the compression(if wanted) and change conversion results based on success of compression. Eventually combine the results of the conversions and compressions

            compressionResults = new Dictionary<string, bool>();

            /*If the user has set a quality for the compression and there's any possible compression, 
            *compress the image(s) and combine the results of the compression(s) with the conversion(s) one(s)*/
            if (compressionTasksParameters.Count != 0 && compressionQuality != 100)
            {
                //Start the image-compression tasks in compressionsTasks list
                List<bool> compressionResultsBoolList = await Task.Run(() => StartCompressionsParallelAsync());

                //Add the compression results to the dictionary with its corresponding image 
                //if the selected format is gif, the compressed gif needs to be created because the compression tasks compressed the images that compose the gif, not the gif itself
                if (selectedFormat.ToLower() == "gif")
                {
                    var compressedGifConversionResults = await Task.Run(() => ConvertToGifAndSaveAsync(Directory.GetFiles(Settings.Default.TempFolderPath).ToList(),
                                                                                                                                                           gifRepeatTimes,
                                                                                                                                                                delayTime,
                                                                                                                                                                savePath));
                    for (int i = 0; i < pathsOfImagesToConvert.Count; i++)
                    {
                        compressionResults.Add(pathsOfImagesToConvert.ElementAt(i), compressedGifConversionResults.conversionResult);
                    }
                }
                //if it's ico, the compressed ico needs to be created because the compression tasks compressed the images that compose the icon, not the icon itself
                else if (selectedFormat.ToLower() == "ico")
                {
                    var compressedIcoConversionResults = await Task.Run(() => ConvertToIcoOrCurAndSaveAsync(Directory.GetFiles(Settings.Default.TempFolderPath).ToList(),
                                                                                                                                                            chosenFormat,
                                                                                                                                                               savePath));
                    for (int i = 0; i < pathsOfImagesToConvert.Count; i++)
                    {
                        compressionResults.Add(pathsOfImagesToConvert.ElementAt(i), compressedIcoConversionResults.conversionResult);
                    }
                }
                else
                {
                    for (int i = 0; i < pathsOfImagesToConvert.Count; i++)
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

            //Empty the Temp Folder, which may contain previous temp images
            UtilityMethods.EmptyFolder(Settings.Default.TempFolderPath);

            GC.Collect();

            return conversionsResults;
        }

        /// <summary>
        /// Start the parallel asynchronous compressions of the given images
        /// </summary>
        /// <param name="pathsOfImagesToCompress"></param>
        /// <returns></returns>
        public async Task<List<bool>> StartCompressionsParallelAsync()
        {
            var compressionsTasks = new List<Task<bool>>();
            foreach (var compressionParam in compressionTasksParameters)
            {
                //Execute task and when it finishes add its value to the list
                compressionsTasks.Add(CompressImageAsync(compressionParam));
            }
            //When the list of all the tasks completes, return the list containing all the results of the compressions
            return (await Task.WhenAll(compressionsTasks)).ToList();
        }

        #region Convert-to-format methods
        /// <summary>
        /// <br>Asynchronously converts an image to a PNG image, saves it to the specified path</br>
        /// <br>with savePath and returns the result of the conversion and the path of the converted image</br>
        /// </summary>
        /// <param name="pathOfImageToConvert"></param>
        /// <returns></returns>
        private async Task<(bool conversionResult, string convertedImagePath)> ConvertToPngAndSaveAsync(string pathOfImageToConvert, string savePath)
        {
            #region  set up image infos to convert etc.
            string imageToConvertName = Path.GetFileNameWithoutExtension(pathOfImageToConvert);
            var imageEncoder = new PngBitmapEncoder();
            string convertedImagePath;
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

            #region Saves the image and checks whether it was saved correctly
            convertedImagePath = $"{savePath}\\{imageToConvertName}_{chosenFormat}.{chosenFormat}";
            //Save the image
            using (Stream st = File.Create(convertedImagePath))
            {
                imageEncoder.Save(st);
                st.Close();
            }

            conversionResult = await CheckIfSavedCorrectlyAsync(convertedImagePath);

            #endregion
            return (conversionResult, convertedImagePath);
        }

        /// <summary>
        /// Asynchronously convert an image to a Jpeg or Jpg image and save it to the savePath
        /// </summary>
        /// <param name="pathOfImageToConvert"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        private async Task<(bool conversionResult, string convertedImagePath)> ConvertToJpegOrJpgAndSaveAsync(string pathOfImageToConvert, string format, string savePath)
        {
            #region Set up infos about the image to convert etc.
            string imageToConvertName = Path.GetFileNameWithoutExtension(pathOfImageToConvert);
            string imageFormat = Path.GetExtension(pathOfImageToConvert).Trim('.');
            var imageEncoder = new JpegBitmapEncoder();
            string convertedImagePath;
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
                st.Close();
            }

            #region Saves the image and checks whether it was saved correctly
            convertedImagePath = $"{savePath}\\{imageToConvertName}_{chosenFormat}.{chosenFormat}";
            //Save the image
            using (Stream st = File.Create(convertedImagePath))
            {
                imageEncoder.Save(st);
                st.Close();
            }

            conversionResult = await CheckIfSavedCorrectlyAsync(convertedImagePath);
            #endregion
            return (conversionResult, convertedImagePath);
        }

        /// <summary>
        /// Asynchronously convert an image to a Bmp image and save it to the savePath
        /// </summary>
        /// <param name="pathOfImageToConvert"></param>
        /// <returns></returns>
        private async Task<(bool conversionResult, string convertedImagePath)> ConvertoToBmpAndSaveAsync(string pathOfImageToConvert, string savePath)
        {
            #region  Set up image infos to convert etc.
            string imageToConvertName = Path.GetFileNameWithoutExtension(pathOfImageToConvert);
            string imageFormat = Path.GetExtension(pathOfImageToConvert).Trim('.');

            var imageEncoder = new BmpBitmapEncoder();
            string convertedImagePath;
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

            #region Saves the image and checks whether it was saved correctly
            convertedImagePath = $"{savePath}\\{imageToConvertName}_{chosenFormat}.{chosenFormat}";
            //Save the image
            using (Stream st = File.Create(convertedImagePath))
            {
                imageEncoder.Save(st);
                st.Close();
            }

            conversionResult = await CheckIfSavedCorrectlyAsync(convertedImagePath);
            #endregion
            return (conversionResult, convertedImagePath);
        }

        //TODO: Fix conversion to gif, sometimes the final gifs are buggy
        /// <summary>
        /// Asynchronously convert a group of images into a Gif image and save it to the savePath
        /// </summary>
        /// <param name="pathsOfImagesToConvert">List containing the paths of the images to convert</param>
        /// <param name="repeatTimes"> Times the gif will be repeated(loop(0), 1-10)</param>
        /// <param name="delayTime"> Delay between two frames in centiseconds</param>
        /// <returns></returns>
        private async Task<(bool conversionResult, string convertedImagePath)> ConvertToGifAndSaveAsync(List<string> pathsOfImagesToConvert, int repeatTimes, int delayTime, string savePath)
        {
            #region  set up image infos to convert etc.
            string imageToConvertName = Path.GetFileNameWithoutExtension(pathsOfImagesToConvert[0]);
            var imageEncoder = new GifBitmapEncoder();
            bool conversionResult = false;
            string convertedImagePath;
            bool oneOfImagesIsgPng = false;
            foreach (var path in pathsOfImagesToConvert)
            {
                oneOfImagesIsgPng = Path.GetExtension(path).Trim('.').ToLower().Contains("png");
                if (oneOfImagesIsgPng)
                    break;
            }
            #endregion

            //Adds each image to the encoder, (after replacing the transparency in case the image is a png)
            foreach (var imagePath in pathsOfImagesToConvert)
            {
                string imageFormat = Path.GetExtension(imagePath).Trim('.');
                //Loads image to convert from a stream, (in case) replace transparency and converts it
                using (Stream st = File.OpenRead(imagePath))
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
                var graphicControlExtension = new byte[] { 33, 249, 4, 0, (byte)delayTime, 0, 0, 0 };

                //Get the list of the bytes of the gif from its previous array, it's a copy of the gif
                var gifBytesList = gifBytesArr.ToList();

                #region Add one graphic control extension block for each image to the bytes list
                int a = 1;

                /* If one of the images is a png, the encoder will automatically add the GCE block because the it needs to set the 
                 * Transparency Color Flag since the png may have some transparency, so the program just needs to modify the Delay Time byte */
                if (oneOfImagesIsgPng)
                {
                    Debug.WriteLine("One or more images is a png, proceeding to only modify the Delay Time byte");
                    for (int i = 0; i < gifBytesList.Count; i++)
                    {
                        //The fourth byte can be a value between 1-6, indicating the disposal method
                        if (gifBytesList[i] == 33 && gifBytesList[i + 1] == 249 && gifBytesList[i + 2] == 4 && 1 <= gifBytesList[i + 3] && gifBytesList[i + 3] <= 6)
                        {
                            Debug.Write($"Found GCE block n°{a}, at index {i}. ");
                            //Modify Delay Time byte with the chosen value
                            gifBytesList[i + 4] = (byte)delayTime;
                            Debug.WriteLine($"Modified Delay Time byte at index {i + 4}.");
                            a++;
                        }
                    }
                }
                //If none of the images are a png, then the GCE block needs to be entirely added
                else
                {
                    Debug.WriteLine("None of the images is a png, proceeding to add the GCE block for each image");
                    for (int i = 0; i < gifBytesList.Count; i++)
                    {
                        if (gifBytesList[i] == 44 && gifBytesList[i + 1] == 0 && gifBytesList[i + 2] == 0 && gifBytesList[i + 3] == 0 && gifBytesList[i + 4] == 0)
                        {
                            Debug.WriteLine($"Found start of image descriptor block at index: {i}.\nImage descriptor n°{a}");
                            //Insert new graphic extension
                            gifBytesList.InsertRange(i, graphicControlExtension);
                            Debug.WriteLine($"Added graphic control extension block at index {i}\n");
                            i += 18;//Skip lenght of an image descriptor block to prevent an infinite loop
                            a++;
                        }
                    }
                }
                #endregion

                #region Add application extension block and save the gif
                var finalGifBytesList = new List<byte>();
                //Add the Header Block and the Logical Screen Descriptor of the basic gif to the final gif
                finalGifBytesList.AddRange(gifBytesList.Take(13));
                //Add the custom application extension
                finalGifBytesList.AddRange(applicationExtension);
                //Adds the rest of the basic gif with the added graphic extensions(skipping the first 13 bytes, which are the header block etc.)
                finalGifBytesList.AddRange(gifBytesList.Skip(13));
                #endregion

                //Create gif and write the final bytes 
                convertedImagePath = $"{savePath}\\{imageToConvertName}_{chosenFormat}.{chosenFormat}";
                File.WriteAllBytes(convertedImagePath, finalGifBytesList.ToArray());
                ms.Close();
            }

            //Checks wether the gif was saved correctly
            conversionResult = await CheckIfSavedCorrectlyAsync(convertedImagePath);

            return (conversionResult, convertedImagePath);
        }

        /// <summary>
        /// Asynchronously convert an image to a Ico or Cur image and save it to the savePath
        /// </summary>
        /// <param name="pathOfImageToConvert"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        private async Task<(bool conversionResult, string convertedImagePath)> ConvertToIcoOrCurAndSaveAsync(List<string> pathsOfImagesToConvert, string format, string savePath)
        {
            //If the image to convert isn't a png or bmp image it can't be converterd: return false
            foreach (var imagePath in pathsOfImagesToConvert)
            {
                string imgToConvExt = Path.GetExtension(imagePath).ToLower();
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
                    return (false, null);
                }
            };

            #region Set up image infos to convert etc.
            string firstImageName = Path.GetFileNameWithoutExtension(pathsOfImagesToConvert[0]);
            List<string> imagesFormats = new List<string>();
            List<Image> imagesToConvert = new List<Image>();
            pathsOfImagesToConvert.ForEach(delegate (string imagePath)
            {
                imagesFormats.Add(Path.GetExtension(imagePath).Trim('.').ToLower());
                imagesToConvert.Add(Image.FromFile(imagePath));
            });
            var memStream = new MemoryStream();
            var binWriter = new BinaryWriter(memStream);
            int[] imagesDataSizeStartPositions = new int[pathsOfImagesToConvert.Count];
            int[] OffsetsFromStartPositions = new int[pathsOfImagesToConvert.Count];
            string convertedImagePath;
            #endregion

            #region Write ICONDIR 
            binWriter.Write((short)0); //Offset #0: reserved

            if (format == "ico")
                binWriter.Write((short)1); //Offset #2: Specifies ICO(1)
            else
                binWriter.Write((short)2); //or CUR(2)

            binWriter.Write((short)pathsOfImagesToConvert.Count); //Offset #4: Number of images in the icon file
            #endregion

            #region Write an ICONDIRENTRY for each image
            int index = 0;
            foreach (var image in imagesToConvert)
            {
                if (image.Width == 256)     //Offset #0: image width. If the width is 256 write 0
                    binWriter.Write((byte)0);
                else
                    binWriter.Write((byte)image.Width);

                if (image.Height == 256)     //Offset #1: image height. If the height is 256 write 0
                    binWriter.Write((byte)0);
                else
                    binWriter.Write((byte)image.Height);

                binWriter.Write((byte)image.Palette.Entries.Count()); //Offset #2: Number of colors in the color palette used by the image. It's 0 if a Color palette isn't used

                binWriter.Write((byte)0); //Offset #3: Reserved

                if (format == "ico")
                    binWriter.Write((short)1); //Offset #4: number of color planes
                else
                    binWriter.Write((short)1); //Offset #4: Horizontal coordinates of the hotspot in number of pixels from the left

                if (format == "ico")
                    binWriter.Write((short)Image.GetPixelFormatSize(image.PixelFormat)); //Offset #6: Number of bits per pixel
                else
                    binWriter.Write((short)1); //Offset #6: Vertical coordinates of the hotspot in number of pixels from the left

                imagesDataSizeStartPositions[index] = (int)memStream.Position;
                binWriter.Write((int)0); //Offset #8: Size of the image's data in bytes, write some placeholders 0s because the actual value will be written later on

                OffsetsFromStartPositions[index] = (int)memStream.Position;
                binWriter.Write((int)0); //Offset #12: Offset of BMP or PNG data from the beginning of the ICO/CUR file, write some placeholders 0s...

                index++;
            }
            #endregion

            #region Write Image Data (in bytes) of each image and the offsets #8 and #12 in its corresponding ICONDIRENTRY
            index = 0;
            foreach (var imgToConvExt in imagesFormats)
            {
                var imgToConvert = imagesToConvert[index];

                if (imgToConvExt == "png")
                {
                    byte[] pngData;

                    //Get position of the image's data start
                    var imageDataStart = (int)memStream.Position;
                    //Write the position of the image's data start to offset #12 in the ICONDIRENTRY of the current image
                    memStream.Position = OffsetsFromStartPositions[index];
                    binWriter.Write(imageDataStart);

                    //If the user has chosen to replace the png transparency
                    if (color != 0)
                    {
                        //Write image data to the position of the image's data start
                        pngData = File.ReadAllBytes(ReplaceTransparency(imgToConvert));
                        memStream.Position = imageDataStart;
                        binWriter.Write(pngData);
                    }
                    else
                    {
                        //Write image data
                        pngData = File.ReadAllBytes(pathsOfImagesToConvert[index]);
                        memStream.Position = imageDataStart;
                        binWriter.Write(pngData);
                    }
                    //Write image data size to the Offset #8 in the ICONDIRENTRY of the current image
                    memStream.Position = imagesDataSizeStartPositions[index];
                    binWriter.Write(pngData.Length);
                }
                else if (imgToConvExt == "bmp") //if the image to convert is a bmp then the BITMAPFILEHEADER block has to be removed
                {
                    //Get position of the image's data start
                    var imageDataStart = (int)memStream.Position;
                    //Write the position of the image's data start to offset #12 in the ICONDIRENTRY of the current image
                    memStream.Position = OffsetsFromStartPositions[index];
                    binWriter.Write(imageDataStart);
                    //Remove file header from the BMP image, which occupies the first 14 bytes of the file
                    byte[] bmpBytes = File.ReadAllBytes(pathsOfImagesToConvert[index]);
                    List<byte> bmpBytesList = bmpBytes.ToList();
                    bmpBytesList.RemoveRange(0, 14);
                    //Write image data to the position of the image's data start
                    bmpBytes = bmpBytesList.ToArray();
                    memStream.Position = imageDataStart;
                    binWriter.Write(bmpBytes);
                    //Write image data size to the offset #8 in the ICONDIRENTRY of the current image
                    memStream.Position = imagesDataSizeStartPositions[index];
                    binWriter.Write(bmpBytes.Length);
                }

                //Bring memorystream cursor to end
                memStream.Position = memStream.Length;
                index++;
            }
            #endregion

            await Task.Run(() => memStream.FlushAsync());

            #region Save icon or cur and checkes whether it was saved correctly
            convertedImagePath = $"{savePath}\\{firstImageName}_{chosenFormat}.{chosenFormat}";
            using (Stream st = File.Create(convertedImagePath))
            {
                st.Write(memStream.ToArray(), 0, memStream.ToArray().Length);
                st.Close();
            }

            bool conversionResult = await Task.Run(() => CheckIfSavedCorrectlyAsync(convertedImagePath));
            #endregion

            #region Dispose objects
            memStream.Close();
            binWriter.Close();
            memStream.Dispose();
            binWriter.Dispose();
            foreach (var image in imagesToConvert)
            {
                image.Dispose();
            }
            #endregion

            return (conversionResult, convertedImagePath);
        }

        /// <summary>
        /// Asynchronously convert an image to a Tiff image and save it to the savePath
        /// </summary>
        /// <param name="pathOfImageToConvert"></param>
        /// <param name="compressionAlgo"></param>
        /// <returns></returns>
        private async Task<(bool conversionResult, string convertedImagePath)> ConvertToTiffAndSaveAsync(string pathOfImageToConvert, string compressionAlgo, string savePath)
        {
            #region Set up image infos to convert etc.
            string imageToConvertName = Path.GetFileNameWithoutExtension(pathOfImageToConvert);
            string imageFormat = Path.GetExtension(pathOfImageToConvert).Trim('.');
            var imageEncoder = new TiffBitmapEncoder();
            string convertedImagePath;
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
                st.Close();
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
            convertedImagePath = $"{savePath}\\{imageToConvertName}_{chosenFormat}.{chosenFormat}";
            using (Stream st = File.Create(convertedImagePath))
            {
                imageEncoder.Save(st);
                st.Close();
            }
            conversionResult = await CheckIfSavedCorrectlyAsync(convertedImagePath);
            #endregion

            return (conversionResult, convertedImagePath);
        }
        #endregion

        /// <summary>
        /// Compress an image 
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="formatOfImageToCompress"></param>
        /// <param name="compressionType"></param>
        /// <param name="quality"></param>
        /// <param name="savePath"> Path where to save the image, if not specificed, the image will be saved to its original version's path</param>
        /// <returns></returns>
        private async Task<bool> CompressImageAsync(CompressionParametersModel parameters)
        {
            #region Set up variables
            ImageCodecInfo imgToCompressCodecInfo = null;
            string imagePath = parameters.imageToCompressPath;
            int quality = parameters.quality;
            string savePath = parameters.savePath;
            var encoderParameters = new EncoderParameters();
            string imageName = Path.GetFileNameWithoutExtension(imagePath);
            string compressedImagePath;
            string formatOfImageToCompress = Path.GetExtension(imagePath).Trim('.');
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
            var imageCompressorEncoder = Encoder.Quality;
            var qualityEncoderParam = new EncoderParameter(imageCompressorEncoder, quality);
            encoderParameters.Param[0] = qualityEncoderParam;
            #endregion

            //The path where to save the image is its original version's one, the name of the image already contains its extension since it has been already converted
            compressedImagePath = $"{savePath}\\{imageName}_Compressed.{formatOfImageToCompress}";

            //Open converted image, compress it and save a copy to the same directory
            using (Stream st = File.OpenRead(imagePath))
            {
                var imageToCompress = new Bitmap(st);
                await Task.Run(() => imageToCompress.Save(compressedImagePath, imgToCompressCodecInfo, encoderParameters));

                st.Close();
            }

            return await CheckIfSavedCorrectlyAsync(compressedImagePath);
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
            //Temporary IMAGE located the user's temp folder, it could be a png after the ReplaceTransparency or the CompressImageAsync
            string tempImgPath = $"{Settings.Default.TempFolderPath}\\tempImgWithTranspReplaced.png";
            imgWithTranspReplaced.Dispose();
            g.Dispose();

            return tempImgPath;
            #endregion
        }

        /// <summary>
        /// Checks if an image has been converted correctly and thus if it has been saved correctly
        /// </summary>
        /// <param name="directoryOfImageToConvert"> path to the folder where the image has been saved to</param>
        /// <param name="imageName"> image of the name that has been converted and saved</param>
        /// <returns></returns>
        private async Task<bool> CheckIfSavedCorrectlyAsync(string imagePath)
        {
            //if the conversion was successful and the file of the converted image exists: return true
            if (await Task.Run(() => File.Exists(imagePath)))
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
