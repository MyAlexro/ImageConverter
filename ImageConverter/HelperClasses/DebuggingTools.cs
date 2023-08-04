using System;
using System.IO;
using System.Linq;

namespace ImageConverter.HelperClasses
{
    /// <summary>
    /// Useful methods to be used during the debugging process in the Immediate Window
    /// </summary>
    public class DebuggingTools
    {
        private int i = 1;
        /// <summary>
        /// Dump the content of a MemoryStream in a txt file to to the specified directory, otherwise on the desktop if 
        /// <br>the directoryPath parameter is not specified</br>
        /// <para></para>
        /// <br>Returns:</br>
        /// <br>String. "Success" if everything went correctly, "Error" and information about the error</br>
        /// </summary>
        /// <param name="memStream"></param>
        public string DumpMemoryStream(MemoryStream memStream, string directoryPath = "")
        {
            //If the user hasn't specified 
            if(directoryPath == "")
                directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            try
            {
                using (Stream st = File.Create($"{directoryPath}\\MemoryDump{i}.txt"))
                {
                    st.Write(memStream.ToArray(), 0, memStream.ToArray().Length);
                }
            }
            //If a file with the same name already exists, or the txt file is in use by another program
            catch (Exception)
            {
                try
                {
                    using (Stream st = File.Create($"{directoryPath}\\MemoryDump{Directory.GetFiles(directoryPath).Count()}.txt"))
                    {
                        st.Write(memStream.ToArray(), 0, memStream.ToArray().Length);
                    }
                }
                catch (Exception)
                {
                    return "Error, delete one of the already-existing memory dumps";
                }
            }
            if (File.Exists($"{directoryPath}\\MemoryDump{i}.txt") || File.Exists($"{directoryPath}\\MemoryDump{Directory.GetFiles(directoryPath).Count()}.txt"))
            {
                i++;
                return "Success";
            }
            else
                return "Error, can't find created file";
        }

        /// <summary>
        /// Get the length of the array of bytes read from the file using the File.ReadAllBytes method
        /// </summary>
        /// <param name="pathOfFile"></param>
        /// <returns></returns>
        public string GetFileBytesLength(string pathOfFile)
        {
            try
            {
                byte[] fileBytes = File.ReadAllBytes(pathOfFile);
                return $"Length: {fileBytes.Length}";
            }
            catch
            {
                return "Error";
            }
        }
    }
}
