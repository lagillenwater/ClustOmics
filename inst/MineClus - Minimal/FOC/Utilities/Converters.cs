using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FOC.Utilities
{
    internal class Converters
    {
        public static string ListToString<T>(List<T> input, string separator, bool addSpace)
        {
            if (input.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();

            foreach (var value in input)
            {
                sb.Append(value + separator);
                if (addSpace)
                    sb.Append(" ");
            }

            sb.Length -= separator.Length;

            return sb.ToString();
        }

        public static string GetParentFolderPath(string path)
        {
            var currentPath = path.Split('\\').ToList();
            currentPath.RemoveAt(currentPath.Count - 1);
            var parentPath = ListToString(currentPath, "\\", false);
            return parentPath;
        }
    }
}