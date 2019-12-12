using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FOC.IO
{
    internal class IoOperations
    {
        // creates a folder if it doesn't exist
        public static void CreateFolder(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        // deletes all files in a directory (may cause async issues)
        public static void ClearFolder(string path)
        {
            var directory = new DirectoryInfo(path);
            foreach (var file in directory.GetFiles())
                file.Delete();
        }

        // returns the list of files from in a given folder, excluding the ones that contain excludeWord in their names
        public static List<string> GetFileList(string folderPath, string excludeWord)
        {
            var directoryInfo = new DirectoryInfo(folderPath);

            var files = directoryInfo.GetFiles()
                .Select(x => x.FullName)
                .Where(x => !x.ToLower().Contains(excludeWord))
                .OrderBy(x => x)
                .ToList();

            return files;
        }
    }
}