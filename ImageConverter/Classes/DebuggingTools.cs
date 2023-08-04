using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageConverter.Classes
{
    /// <summary>
    /// Useful methods to be used during the debugging process in the Immediate Window
    /// </summary>
    public static class DebuggingTools
    {
        /// <summary>
        /// Dump the content of a MemoryStream in a txt file on the desktop
        /// </summary>
        /// <param name="memStream"></param>
        public static string DumpMemoryStream(MemoryStream memStream)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            try
            {
                using (Stream st = File.Create($"{desktopPath}\\MemoryDump.txt"))
                {
                    st.Write(memStream.ToArray(), 0, memStream.ToArray().Length);
                }
            }
            //If a file with the same name already exists, or the txt file is in use by another program
            catch (Exception)
            {
                try
                {
                    using (Stream st = File.Create($"{desktopPath}\\MemoryDump{Directory.GetFiles(desktopPath).Count()}.txt"))
                    {
                        st.Write(memStream.ToArray(), 0, memStream.ToArray().Length);
                    }
                }
                catch (Exception)
                {
                    return "Error, delete one of the already-existing memory dumps";
                }
            }
            if (File.Exists($"{desktopPath}\\MemoryDump.txt") || File.Exists($"{desktopPath}\\MemoryDump{Directory.GetFiles(desktopPath).Count()}.txt"))
                return "Success";
            else
                return "Error, can't find created file";
        }

        public static string GetFileBytesLength(string pathOfFile)
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
