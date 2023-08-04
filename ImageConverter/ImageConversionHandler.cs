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

using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Drawing;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using ImageConverter.HelperClasses;
using ImageConverter.Models;
using ImageConverter.Properties;

namespace ImageConverter
{
    public class ImageConversionHandler
    {
        /// <summary>
        /// Color to replace the transparency with. 0 = no color, 1 = white, 2 = black
        /// </summary>
        private int color = 0;

        /// <summary>
        /// Starts the conversion of one or more images to the specified format. 
        /// <para>Returns a dictionary containing a string(path of the image to convert) and a bool(was the conversion successful? true/false)</para>
        /// </summary>
        /// <param name="convParams"> ImageConversionParametersModel containing all the parameters of the conversion</param>
        /// <returns></returns>
        public async Task<Dictionary<string, bool>> StartConversionAsync(ConversionParamsModel convParams)
        {
            #region Prepare conversion variable etc
            color = convParams.colorToReplTheTranspWith;
            int qualityLevel = convParams.qualityLevel;
            List<string> pathsOfImagesToConvert = convParams.pathsOfImagesToConvert;
            string selectedFormat = convParams.format;
            int gifRepeatTimes = convParams.gifRepeatTimes;
            //Results of the conversion(s) with the corresponding image: if the conversion was sucessful or not
            var conversionsResults = new Dictionary<string, bool>();
            #endregion

            //Check whether the user is trying to convert an image to the same format, if yes don't convert by setting already its conversionResult to false
            pathsOfImagesToConvert.ForEach(delegate (string imageToConvertPath)
            {
                var imgToConvertExt = Path.GetExtension(imageToConvertPath).ToLower().Trim('.');
                if (imgToConvertExt == selectedFormat)
                    conversionsResults.Add(imageToConvertPath, false);
            });

            //Start the conversion of each image and, if wanted by the user, add thecompression task for that image
            foreach (var pathOfImgToConvert in pathsOfImagesToConvert)
            {
                /*If the conversion result of an image isn't already "false" convert it. The conversion result of 
                *an image may have already been evaluated to be false(unsuccessful) if the user wanted to convert it to its same format */
                bool resultHasBeenAlreadyEvaluated = conversionsResults.TryGetValue(pathOfImgToConvert, out resultHasBeenAlreadyEvaluated);
                if (resultHasBeenAlreadyEvaluated != true)
                {
                    if (selectedFormat == "png")
                    {
                        (bool result, string convertedImagePath) conversion;
                        //Default to true so that if the user hasn't chosen to reduce qlty, it won't affect the finalResult when merging conversion.Result and this one
                        bool qualityReductionResult = true;

                        if (qualityLevel != 100)//If the user wants to reduce the final image quality
                        {
                            if (Settings.Default.SaveBothImages == true) //If the user wants to save both versions of the converted image
                                conversion = await Task.Run(() => ConvertToPngAndSaveAsync(pathOfImgToConvert, convParams.saveDirectory));
                            else //if the user wants to save only the reduced-quality version of the converted image
                                conversion = await Task.Run(() => ConvertToPngAndSaveAsync(pathOfImgToConvert, Settings.Default.TempFolderPath));

                            //If the conversion has been successful compress the image
                            if (conversion.result == true)
                            {
                                var reductionParams = new QualityReductionParamsModel
                                {
                                    imgToReduceQualityPath = conversion.convertedImagePath,
                                    quality = convParams.qualityLevel,
                                    saveDirectory = convParams.saveDirectory,
                                };
                                qualityReductionResult = await Task.Run(() => ReduceImageQualityAsync(reductionParams));
                            }
                            else //if the conversion has been UNsuccessful set the qlty reduction to false as a consequence
                                qualityReductionResult = false;
                        }
                        else //If the user doesn't want to reduce the final image quality
                            conversion = await Task.Run(() => ConvertToPngAndSaveAsync(pathOfImgToConvert, convParams.saveDirectory));

                        //Merge the results of the processes and add the final value to the conversionResultsList
                        bool finalResult = conversion.result && qualityReductionResult; //NOTE: comment on qualityReductionResult's declaration line
                        conversionsResults.Add(pathOfImgToConvert, finalResult);
                    }
                    else if (selectedFormat == "jpeg" || selectedFormat == "jpg")
                    {
                        (bool result, string convertedImagePath) conversion;
                        bool qualityReductionResult = true; //Default as true so that if the user hasn't chosen to reduce qlty it won't affect the finalResult when merging the 2 results

                        if (qualityLevel != 100)//If the user wants to reduce the final image quality
                        {
                            if (Settings.Default.SaveBothImages == true) //If the user wants to save both versions of the converted image
                                conversion = await Task.Run(() => ConvertToJpegOrJpgAndSaveAsync(pathOfImgToConvert, selectedFormat, convParams.saveDirectory));
                            else //if the user wants to save only the reduced-quality version of the converted image
                                conversion = await Task.Run(() => ConvertToJpegOrJpgAndSaveAsync(pathOfImgToConvert, selectedFormat, Settings.Default.TempFolderPath));

                            //If the conversion has been successful compress the image
                            if (conversion.result == true)
                            {
                                var reductionParams = new QualityReductionParamsModel
                                {
                                    imgToReduceQualityPath = conversion.convertedImagePath,
                                    quality = convParams.qualityLevel,
                                    saveDirectory = convParams.saveDirectory,
                                };
                                qualityReductionResult = await Task.Run(() => ReduceImageQualityAsync(reductionParams));
                            }
                            else //if the conversion has been UNsuccessful set the qlty reduction to false as a consequence
                                qualityReductionResult = false;
                        }
                        else //If the user doesn't want to reduce the final image quality
                            conversion = await Task.Run(() => ConvertToJpegOrJpgAndSaveAsync(pathOfImgToConvert, selectedFormat, convParams.saveDirectory));

                        bool finalResult = conversion.result && qualityReductionResult; //NOTE: comment on line 99
                        conversionsResults.Add(pathOfImgToConvert, finalResult);
                    }
                    else if (selectedFormat == "bmp")
                    {
                        (bool result, string convertedImagePath) conversion;
                        bool qualityReductionResult = true; //Default as true so that if the user hasn't chosen to reduce qlty it won't affect the finalResult when merging the 2 results

                        if (qualityLevel != 100)//If the user wants to reduce the final image quality
                        {
                            if (Settings.Default.SaveBothImages == true) //If the user wants to save both versions of the converted image
                                conversion = await Task.Run(() => ConvertoToBmpAndSaveAsync(pathOfImgToConvert, convParams.saveDirectory));
                            else //if the user wants to save only the reduced-quality version of the converted image
                                conversion = await Task.Run(() => ConvertoToBmpAndSaveAsync(pathOfImgToConvert, Settings.Default.TempFolderPath));

                            //If the conversion has been successful compress the image
                            if (conversion.result == true)
                            {
                                var reductionParams = new QualityReductionParamsModel
                                {
                                    imgToReduceQualityPath = conversion.convertedImagePath,
                                    quality = convParams.qualityLevel,
                                    saveDirectory = convParams.saveDirectory,
                                };
                                qualityReductionResult = await Task.Run(() => ReduceImageQualityAsync(reductionParams));
                            }
                            else //if the conversion has been UNsuccessful set the qlty reduction to false as a consequence
                                qualityReductionResult = false;
                        }
                        else //If the user doesn't want to reduce the final image quality
                            conversion = await Task.Run(() => ConvertoToBmpAndSaveAsync(pathOfImgToConvert, convParams.saveDirectory));

                        bool finalResult = conversion.result && qualityReductionResult; //NOTE: comment on line 99
                        conversionsResults.Add(pathOfImgToConvert, finalResult);
                    }
                    else if (selectedFormat == "gif")
                    {
                        (bool result, string convertedImgPath) conversion;
                        List<bool> qltyReductionResults = new List<bool>();
                        //Default all values to true so that if the user hasn't chosen to reduce qlty it won't affect the finalResult when merging conversion.Result and these ones
                        foreach (var img in pathsOfImagesToConvert)
                            qltyReductionResults.Add(true);

                        if (qualityLevel != 100)
                        {
                            //Empty the list from the default values
                            qltyReductionResults.Clear();

                            //Reducing the gif's quality directly will break it, so first the images quality need to be reduced and then create the gif
                            UtilityMethods.EmptyFolder(Settings.Default.TempFolderPath); //Delete all files in the temp folder in case there are any leftover files
                            foreach (var img in pathsOfImagesToConvert) //Reduce each image quality and save it to the temp folder
                            {
                                qltyReductionResults.Add(await Task.Run(() => ReduceImageQualityAsync(new QualityReductionParamsModel
                                {
                                    imgToReduceQualityPath = img,
                                    saveDirectory = Settings.Default.TempFolderPath,
                                })));
                            }

                            //If there's been even ONE unsuccessful qlty-reduction the conversion can't be made,so set conversionResult to false as a consequence
                            if (qltyReductionResults.Contains(false))
                                conversion.result = false;
                            else //Otherwise if all have been successful, convert those images to gif
                            {
                                //Take all the quality-reduced images in the temp folder and convert them to a gif, saving it to the saveDirectory
                                List<string> qltyReducedImages = new List<string>();
                                qltyReducedImages = Directory.GetFiles(Settings.Default.TempFolderPath).ToList();
                                conversion = await Task.Run(() => ConvertToGifAndSaveAsync(qltyReducedImages, convParams.gifRepeatTimes, convParams.delayTime, convParams.saveDirectory));
                            }

                            //if the user wants both versions of the gif, save the normal-quality gif too in the save path
                            if (Settings.Default.SaveBothImages)
                                conversion = await Task.Run(() => ConvertToGifAndSaveAsync(pathsOfImagesToConvert, convParams.gifRepeatTimes, convParams.delayTime, convParams.saveDirectory));
                        }
                        else //If the user doesn't want to a reduced-quality gif just convert the given images
                        {
                            conversion = await Task.Run(() => ConvertToGifAndSaveAsync(pathsOfImagesToConvert, convParams.gifRepeatTimes, convParams.delayTime, convParams.saveDirectory));
                        }

                        //Merge conversions and qlty-reductions results
                        List<bool> finalResults = new List<bool>();
                        foreach (var qltyResult in qltyReductionResults)
                        {
                            finalResults.Add(conversion.result && qltyResult);
                        }

                        int i = 0;
                        foreach (var image in pathsOfImagesToConvert)
                        {
                            conversionsResults.Add(image, finalResults[i]);
                            i++;
                        }
                        break;
                    }
                    else if (selectedFormat == "ico" || selectedFormat == "cur")
                    {
                        (bool result, string convertedImagePath) conversion;
                        //Default to true so that if the user hasn't chosen to reduce qlty, it won't affect the finalResult when merging conversion.Result and this one
                        bool qltyReductionResult = true;

                        if (qualityLevel != 100)
                        {
                            //Reducing the Ico/cur file's quality directly will break it, so first the image's quality need to be reduced and then create the ico/cur
                            UtilityMethods.EmptyFolder(Settings.Default.TempFolderPath); //Delete all files in the temp folder in case there are any leftover files
                            //Reduce the image's quality and save it to the temp folder
                            qltyReductionResult = await Task.Run(() => ReduceImageQualityAsync(new QualityReductionParamsModel
                            {
                                imgToReduceQualityPath = pathOfImgToConvert,
                                saveDirectory = Settings.Default.TempFolderPath,
                            }));


                            //If the qlty-reduction has been unsuccessful the conversion can't be made,so set conversionResult to false as a consequence
                            if (qltyReductionResult == false)
                                conversion.result = false;
                            else //Otherwise if it has been successful, convert the image to ico/cur
                            {
                                //Take the quality-reduced image in the temp folder and convert it to a ico/cur, saving it to the saveDirectory
                                string[] qltyReducedImage = new string[1];
                                qltyReducedImage = Directory.GetFiles(Settings.Default.TempFolderPath);
                                conversion = await Task.Run(() => ConvertToIcoOrCurAndSaveAsync(qltyReducedImage[0], selectedFormat, convParams.iconSizes, convParams.saveDirectory));
                            }

                            //if the user wants both versions of the icon/cur file, save the normal-quality ico/cur file too in the save path
                            if (Settings.Default.SaveBothImages)
                                conversion = await Task.Run(() => ConvertToIcoOrCurAndSaveAsync(pathOfImgToConvert, selectedFormat, convParams.iconSizes, convParams.saveDirectory));
                        }
                        else //If the user doesn't want to a reduced-quality ico/cur just convert the given image
                        {
                            conversion = await Task.Run(() => ConvertToIcoOrCurAndSaveAsync(pathOfImgToConvert, selectedFormat, convParams.iconSizes, convParams.saveDirectory));
                        }

                        //Merge the results of the processes and add the final value to the conversionResultsList
                        bool finalResult = conversion.result && qltyReductionResult; //NOTE: comment on qltyReductionResult's declaration line
                        conversionsResults.Add(pathOfImgToConvert, finalResult);
                    }
                    else if (selectedFormat == "tiff")
                    {
                        (bool result, string convertedImagePath) conversion;
                        //Default to true so that if the user hasn't chosen to reduce qlty, it won't affect the finalResult when merging conversion.Result and this one
                        bool qualityReductionResult = true;

                        if (qualityLevel != 100)//If the user wants to reduce the final image quality
                        {
                            if (Settings.Default.SaveBothImages == true) //If the user wants to save both versions of the converted image
                                conversion = await Task.Run(() => ConvertToTiffAndSaveAsync(pathOfImgToConvert, convParams.tiffCompressionAlgo, convParams.saveDirectory));
                            else //if the user wants to save only the reduced-quality version of the converted image
                                conversion = await Task.Run(() => ConvertToTiffAndSaveAsync(pathOfImgToConvert, convParams.tiffCompressionAlgo, Settings.Default.TempFolderPath));

                            //If the conversion has been successful compress the image
                            if (conversion.result == true)
                            {
                                var reductionParams = new QualityReductionParamsModel
                                {
                                    imgToReduceQualityPath = conversion.convertedImagePath,
                                    quality = convParams.qualityLevel,
                                    saveDirectory = convParams.saveDirectory,
                                };
                                qualityReductionResult = await Task.Run(() => ReduceImageQualityAsync(reductionParams));
                            }
                            else //if the conversion has been UNsuccessful set the qlty reduction to false as a consequence
                                qualityReductionResult = false;
                        }
                        else //If the user doesn't want to reduce the final image quality
                            conversion = await Task.Run(() => ConvertToTiffAndSaveAsync(pathOfImgToConvert, convParams.tiffCompressionAlgo, convParams.saveDirectory));

                        //Merge the results of the processes and add the final value to the conversionResultsList
                        bool finalResult = conversion.result && qualityReductionResult; //NOTE: comment on qualityReductionResult's declaration line
                        conversionsResults.Add(pathOfImgToConvert, finalResult);
                    }
                }
            }

            //Empty the Temp Folder
            UtilityMethods.EmptyFolder(Settings.Default.TempFolderPath);
            GC.Collect();
            return conversionsResults;
        }

        #region Convert-to-format methods
        /// <summary>
        /// Asynchronously converts an image to a PNG image and saves it in the specified directory
        /// </summary>
        /// <param name="pathOfImageToConvert"></param>
        /// <param name="savePath"></param>
        /// <returns>Bool conversionResult: specifies wether the conversion has been successful or not <br/>
        /// String convertedImagePath: path to the converted image</returns>
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
            convertedImagePath = $"{savePath}\\{imageToConvertName}_png.png";
            //Save the image
            using (Stream st = File.Create(convertedImagePath))
            {
                imageEncoder.Save(st);
                st.Close();
            }

            conversionResult = await CheckIfExists(convertedImagePath);

            #endregion
            return (conversionResult, convertedImagePath);
        }

        /// <summary>
        /// Asynchronously converts an image to a Jpeg or Jpg image and saves it in the specified directory
        /// </summary>
        /// <param name="pathOfImageToConvert"></param>
        /// <param name="format">Must be "jpeg" or "jpg"</param>
        /// <param name="saveDir"></param>
        /// <returns>Bool conversionResult: specifies wether the conversion has been successful or not <br/>
        /// String convertedImagePath: path to the converted image</returns>
        private async Task<(bool conversionResult, string convertedImagePath)> ConvertToJpegOrJpgAndSaveAsync(string pathOfImageToConvert, string format, string saveDir)
        {
            #region Set up infos about the image to convert etc.
            string imageToConvertName = Path.GetFileNameWithoutExtension(pathOfImageToConvert);
            string imageFormat = Path.GetExtension(pathOfImageToConvert).Trim('.');
            var imageEncoder = new JpegBitmapEncoder();
            string convertedImagePath;
            bool conversionResult;
            #endregion

            //Start process of conversion
            using (Stream st = File.OpenRead(pathOfImageToConvert))
            {
                var imageToConv = new BitmapImage();

                //if the user selected a color to convert the image transparency with and the image is a png replace transparency etc
                if (color != 0 && imageFormat == "png")
                {
                    Image imgToConvertAsImage = Image.FromStream(st);
                    using (Stream st2 = File.OpenRead(ReplaceTransparency(imgToConvertAsImage, Settings.Default.TempFolderPath)))
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
            convertedImagePath = $"{saveDir}\\{imageToConvertName}_{format}.{format}";
            //Save the image
            using (Stream st = File.Create(convertedImagePath))
            {
                imageEncoder.Save(st);
                st.Close();
            }

            conversionResult = await CheckIfExists(convertedImagePath);
            #endregion
            return (conversionResult, convertedImagePath);
        }

        /// <summary>
        /// Asynchronously converts an image to a BMP image and saves it in the specified directory
        /// </summary>
        /// <param name="pathOfImageToConvert"></param>
        /// <param name="saveDir">Path to the directory where the converted image will be saved</param>
        /// <returns>Bool conversionResult: specifies wether the conversion has been successful or not <br/>
        /// String convertedImagePath: path to the converted image</returns>
        private async Task<(bool conversionResult, string convertedImagePath)> ConvertoToBmpAndSaveAsync(string pathOfImageToConvert, string saveDir)
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
                    using (Stream st2 = File.OpenRead(ReplaceTransparency(imgToConvertImage, Settings.Default.TempFolderPath)))
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
            convertedImagePath = $"{saveDir}\\{imageToConvertName}_bmp.bmp";
            //Save the image
            using (Stream st = File.Create(convertedImagePath))
            {
                imageEncoder.Save(st);
                st.Close();
            }

            conversionResult = await CheckIfExists(convertedImagePath);
            #endregion
            return (conversionResult, convertedImagePath);
        }

        //TODO: Fix conversion to gif, sometimes the final gifs are buggy
        /// <summary>
        /// Asynchronously converts a group of images into a GIF and saves it in the specified directory
        /// </summary>
        /// <param name="pathsOfImagesToConvert"></param>
        /// <param name="repeatTimes">Times the gif will repeat, must be between 0 and 10(with 0 being equal to infinity)</param>
        /// <param name="delayTime">The number of hundredths of a second to wait before moving on to the next frame</param>
        /// <param name="saveDir"Path to the directory where the converted image will be saved></param>
        /// <returns>Bool conversionResult: specifies wether the conversion has been successful or not <br/>
        /// String convertedImagePath: path to the converted image</returns>
        private async Task<(bool conversionResult, string convertedImagePath)> ConvertToGifAndSaveAsync(List<string> pathsOfImagesToConvert, int repeatTimes, int delayTime, string saveDir)
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
                        using (Stream st2 = File.OpenRead(ReplaceTransparency(imgToConvertAsImage, Settings.Default.TempFolderPath)))
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
                convertedImagePath = $"{saveDir}\\{imageToConvertName}_gif.gif";
                File.WriteAllBytes(convertedImagePath, finalGifBytesList.ToArray());
                ms.Close();
            }

            //Checks wether the gif was saved correctly
            conversionResult = await CheckIfExists(convertedImagePath);

            return (conversionResult, convertedImagePath);
        }

        /// <summary>
        /// Asynchronously converts an image to a Ico or Cur image and saves it in the specified directory
        /// </summary>
        /// <param name="pathOfImageToConvert"></param>
        /// <param name="format">Must be "ico" or "cur"</param>
        /// <param name="iconSizes">List containing the icon sizes that will be available in the final icon. All the standard sizes can be specified</param>
        /// <param name="saveDir">Path to the directory where the converted image will be saved</param>
        /// <returns>Bool conversionResult: specifies wether the conversion has been successful or not <br/>
        /// String convertedImagePath: path to the converted image</returns>
        private async Task<(bool conversionResult, string convertedImagePath)> ConvertToIcoOrCurAndSaveAsync(string pathOfImageToConvert, string format, List<string> iconSizes, string saveDir)
        {
            //If the image to convert isn't a png or bmp image it can't be converterd: return false
            string originalImgToConvExt = Path.GetExtension(pathOfImageToConvert).ToLower();
            if (originalImgToConvExt != ".png" && originalImgToConvExt != ".bmp")
            {
                if (Settings.Default.Language == "it")
                {
                    MessageBox.Show(LanguageManager.IT_CantConvertThisImageToIco, "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (Settings.Default.Language == "en")
                {
                    MessageBox.Show(LanguageManager.EN_CantConvertThisImageToIco, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return (false, null);
            }

            //Resize the image to the specified sizes
            List<string> resizedImagesToConvert = new List<string>();
            foreach (var size in iconSizes)
            {
                switch (size)
                {
                    case "16x16":
                        resizedImagesToConvert.Add(ResizeImage(pathOfImageToConvert, 16, 16, Settings.Default.TempFolderPath));
                        break;
                    case "24x24":
                        resizedImagesToConvert.Add(ResizeImage(pathOfImageToConvert, 24, 24, Settings.Default.TempFolderPath));
                        break;
                    case "32x32":
                        resizedImagesToConvert.Add(ResizeImage(pathOfImageToConvert, 32, 32, Settings.Default.TempFolderPath));
                        break;
                    case "48x48":
                        resizedImagesToConvert.Add(ResizeImage(pathOfImageToConvert, 48, 48, Settings.Default.TempFolderPath));
                        break;
                    case "64x64":
                        resizedImagesToConvert.Add(ResizeImage(pathOfImageToConvert, 64, 64, Settings.Default.TempFolderPath));
                        break;
                    case "96x96":
                        resizedImagesToConvert.Add(ResizeImage(pathOfImageToConvert, 96, 96, Settings.Default.TempFolderPath));
                        break;
                    case "128x128":
                        resizedImagesToConvert.Add(ResizeImage(pathOfImageToConvert, 128, 128, Settings.Default.TempFolderPath));
                        break;
                    case "192x192":
                        resizedImagesToConvert.Add(ResizeImage(pathOfImageToConvert, 192, 192, Settings.Default.TempFolderPath));
                        break;
                    case "256x256":
                        resizedImagesToConvert.Add(ResizeImage(pathOfImageToConvert, 256, 256, Settings.Default.TempFolderPath));
                        break;
                }
            }

            #region Set up image infos to convert etc.
            string originalImgName = Path.GetFileNameWithoutExtension(pathOfImageToConvert);
            string originalImgFormat = Path.GetExtension(pathOfImageToConvert).Trim('.').ToLower();
            List<Image> imagesAsImageType = new List<Image>();
            resizedImagesToConvert.ForEach(delegate (string imagePath)
            {
                imagesAsImageType.Add(Image.FromFile(imagePath));
            });
            var memStream = new MemoryStream();
            var binWriter = new BinaryWriter(memStream);
            int[] imagesDataSizeStartPositions = new int[resizedImagesToConvert.Count];
            int[] OffsetsFromStartPositions = new int[resizedImagesToConvert.Count];
            string convertedImagePath;
            #endregion

            #region Write ICONDIR 
            binWriter.Write((short)0); //Offset #0: reserved

            if (format == "ico")
                binWriter.Write((short)1); //Offset #2: Specifies ICO(1)
            else
                binWriter.Write((short)2); //or CUR(2)

            binWriter.Write((short)resizedImagesToConvert.Count); //Offset #4: Number of images in the icon file
            #endregion

            #region Write an ICONDIRENTRY for each image
            int index = 0;
            foreach (var image in imagesAsImageType)
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
            foreach (var imgToConvert in imagesAsImageType)
            {
                if (originalImgFormat == "png")
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
                        pngData = File.ReadAllBytes(ReplaceTransparency(imgToConvert, Settings.Default.TempFolderPath));
                        memStream.Position = imageDataStart;
                        binWriter.Write(pngData);
                    }
                    else
                    {
                        //Write image data
                        pngData = File.ReadAllBytes(resizedImagesToConvert[index]);
                        memStream.Position = imageDataStart;
                        binWriter.Write(pngData);
                    }
                    //Write image data size to the Offset #8 in the ICONDIRENTRY of the current image
                    memStream.Position = imagesDataSizeStartPositions[index];
                    binWriter.Write(pngData.Length);
                }
                else if (originalImgFormat == "bmp") //if the image to convert is a bmp then the BITMAPFILEHEADER block has to be removed
                {
                    //Get position of the image's data start
                    var imageDataStart = (int)memStream.Position;
                    //Write the position of the image's data start to offset #12 in the ICONDIRENTRY of the current image
                    memStream.Position = OffsetsFromStartPositions[index];
                    binWriter.Write(imageDataStart);
                    //Remove file header from the BMP image, which occupies the first 14 bytes of the file
                    byte[] bmpBytes = File.ReadAllBytes(resizedImagesToConvert[index]);
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
            convertedImagePath = $"{saveDir}\\{originalImgName}_{format}.{format}";
            using (Stream st = File.Create(convertedImagePath))
            {
                st.Write(memStream.ToArray(), 0, memStream.ToArray().Length);
                st.Close();
            }

            bool conversionResult = await Task.Run(() => CheckIfExists(convertedImagePath));
            #endregion

            #region Dispose objects
            memStream.Close();
            binWriter.Close();
            memStream.Dispose();
            binWriter.Dispose();
            foreach (var image in imagesAsImageType) { image.Dispose(); }
            #endregion

            return (conversionResult, convertedImagePath);
        }

        /// <summary>
        /// Asynchronously converts an image to a Tiff image and saves it in the specified directory
        /// </summary>
        /// <param name="pathOfImageToConvert">Path to the image to convert</param>
        /// <param name="compressionAlgo">One of the standard compression algortihms for tiff images</param>
        /// <param name="saveDir">Path to the directory where the converted image will be saved</param>
        /// <returns></returns>
        private async Task<(bool conversionResult, string convertedImagePath)> ConvertToTiffAndSaveAsync(string pathOfImageToConvert, string compressionAlgo, string saveDir)
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
                    using (Stream st2 = File.OpenRead(ReplaceTransparency(imgToConvertAsImage, Settings.Default.TempFolderPath)))
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
            convertedImagePath = $"{saveDir}\\{imageToConvertName}_tiff.tiff";
            using (Stream st = File.Create(convertedImagePath))
            {
                imageEncoder.Save(st);
                st.Close();
            }
            conversionResult = await CheckIfExists(convertedImagePath);
            #endregion

            return (conversionResult, convertedImagePath);
        }
        #endregion

        /// <summary>
        /// Asynchronously reduce the quality of an image and saves it in the specified directory
        /// </summary>
        /// <param name="parameters">QualityReductionParamsModel containing all the parameters for the quality-reduction of the image</param>
        /// <returns>Bool result: which specifies wether the quality-reduction has been successful
        /// </returns>
        private async Task<bool> ReduceImageQualityAsync(QualityReductionParamsModel parameters)
        {
            #region Set up variables
            ImageCodecInfo imgToReduceQltyCodecInfo = null;
            string imagePath = parameters.imgToReduceQualityPath;
            int quality = parameters.quality;
            string saveDir = parameters.saveDirectory;
            var encoderParameters = new EncoderParameters();
            string imageName = Path.GetFileNameWithoutExtension(imagePath);
            string reducedQltyImagePath;
            string formatOfImgToReduceQlty = Path.GetExtension(imagePath).Trim('.');
            //Get image codec info for the encoder based on the mime type of the image
            foreach (var codecInfo in ImageCodecInfo.GetImageEncoders())
            {
                if (codecInfo.MimeType == $"image/{formatOfImgToReduceQlty.ToLower()}")
                {
                    imgToReduceQltyCodecInfo = codecInfo;
                    break;
                }
                else if (formatOfImgToReduceQlty.ToLower() == "jpg" || formatOfImgToReduceQlty.ToLower() == "jpeg" && codecInfo.MimeType == $"image/jpg" || codecInfo.MimeType == $"image/jpeg")
                    imgToReduceQltyCodecInfo = codecInfo;
            }

            var imageEncoder = Encoder.Quality;
            var qualityEncoderParam = new EncoderParameter(imageEncoder, quality);
            encoderParameters.Param[0] = qualityEncoderParam;
            #endregion

            //The path where to save the image is its original version's one, the name of the image already contains its extension since it has been already converted
            reducedQltyImagePath = $"{saveDir}\\{imageName}_ReducedQuality.{formatOfImgToReduceQlty}";

            //Open converted image, reduce its quality it and save a copy to the same directory
            using (Stream st = File.OpenRead(imagePath))
            {
                var imgToReduceQlty = new Bitmap(st);
                await Task.Run(() => imgToReduceQlty.Save(reducedQltyImagePath, imgToReduceQltyCodecInfo, encoderParameters));
                imgToReduceQlty.Dispose();
                st.Close();
            }
            return await CheckIfExists(reducedQltyImagePath);
        }

        /// <summary>
        /// Takes the given Image, replaces its transparency and saves it in the specified directory
        /// </summary>
        /// <param name="img"></param>
        /// <param name="saveDirectory"></param>
        /// <returns>The path to the image with the replaced transparency</returns>
        private string ReplaceTransparency(Image img, string saveDirectory)
        {
            Bitmap imgWithTranspReplaced = new Bitmap(img.Width, img.Height);
            Graphics g = Graphics.FromImage(imgWithTranspReplaced);

            //replace transparency with white
            if (color == 1)
                g.Clear(Color.White);
            //replace transparency with black
            else
                g.Clear(Color.Black);

            g.DrawImage(img, 0, 0);

            #region Saves imgWithTranspReplaced in the temp folder, dispose objects and returns its path
            imgWithTranspReplaced.Save($"{saveDirectory}\\tempImgWithTranspReplaced.png", ImageFormat.Png);
            //Temporary IMAGE located the user's temp folder, it could be a png after the ReplaceTransparency or the ReduceImageQualityAsync
            string tempImgPath = $"{saveDirectory}\\tempImgWithTranspReplaced.png";
            imgWithTranspReplaced.Dispose();
            g.Dispose();

            return tempImgPath;
            #endregion
        }

        /// <summary>
        /// Resizes an image to the specified dimensions and saves it in the specified directory
        /// </summary>
        /// <param name="pathOfImgToResize"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="saveDirectory">Path to the folder where the image will be saved</param>
        /// <returns></returns>
        private string ResizeImage(string pathOfImgToResize, int width, int height, string saveDirectory)
        {
            Bitmap imgToResize;
            using (Stream st = File.OpenRead(pathOfImgToResize))
            {
                imgToResize = new Bitmap(st);
                st.Close();
            }
            var imgToResizeFormat = Path.GetExtension(pathOfImgToResize).Trim('.');
            //Get imgToResize format as ImageFormat type
            ImageFormatConverter typeConverter = new ImageFormatConverter();
            ImageFormat imgFormat = (ImageFormat)typeConverter.ConvertFromString(imgToResizeFormat);

            var destRect = new Rectangle(0, 0, width, height);
            var resizedImg = new Bitmap(width, height, imgToResize.PixelFormat);

            resizedImg.SetResolution(imgToResize.HorizontalResolution, imgToResize.VerticalResolution);
            using (var graphics = Graphics.FromImage(resizedImg))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(imgToResize, destRect, 0, 0, imgToResize.Width, imgToResize.Height, GraphicsUnit.Pixel, wrapMode);

                    wrapMode.Dispose();
                }
                graphics.Dispose();
            }
            var filesInSavePath = Directory.GetFiles(saveDirectory).Length;
            string savedImgPath = $"{saveDirectory}\\ResizedImage{filesInSavePath}.{imgToResizeFormat}";
            resizedImg.Save(savedImgPath, imgFormat);

            imgToResize.Dispose();
            resizedImg.Dispose();

            return savedImgPath;
        }

        /// <summary>
        /// Checks if the given image exists
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        private async Task<bool> CheckIfExists(string imagePath)
        {
            //if the conversion was successful and the file of the converted image exists: return true
            if (await Task.Run(() => File.Exists(imagePath)))
                return true;
            //otherwise: return false
            else
                return false;
        }
    }
}