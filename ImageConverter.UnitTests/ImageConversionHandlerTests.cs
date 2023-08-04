using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImageConverter.UnitTests
{
    [TestClass]
    public class ImageConversionHandlerTests
    {
        [TestMethod]
        public void IsImage_FileIsNOTAnImage_ReturnsFalse()
        {
            string pathOfFile = @"C:\RandomFile.txt";

            bool result = ImageConversionHandler.IsImage(pathOfFile);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsImage_FileIsAnImage_ReturnsTrue()
        {
            string[] pathsOfFiles = new string[] { "C:\\randomfile.png", "C:\\randomfile.bmp", "C:\\randomfile.png" };
            bool[] results = new bool[3];

            for(int i = 0; i <= 2;  i++)
            {
                results[i] = ImageConversionHandler.IsImage(pathsOfFiles[i]);
            }
            foreach(var result in results)
            {
                Assert.IsTrue(result);
            }
        }
    }
}
