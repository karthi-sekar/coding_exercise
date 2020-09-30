using System.IO;

namespace DotNetObjectsExt
{
    public static class StringExt
    {
        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// If string is null, returns empty string else converts the input string to lower case and trims whitespaces.
        /// </summary>
        /// <param name="possibleNullValue">string value</param>
        public static string UnsafeTrimLower(this string possibleNullValue)
        {
            return possibleNullValue?.ToLower().Trim() ?? string.Empty;
        }

        /// <summary>
        /// Converts non-null string to lower case and trims whitepsaces.
        /// </summary>
        /// <param name="nonNullValue">Non-null string</param>
        public static string SafeTrimLower(this string nonNullValue)
        {
            return nonNullValue.ToLower().Trim();
        }

        public static DirectoryInfo ToDirectoryInfo(this string value)
        {
            value.ThrowIfNullOrWhiteSpace($"{nameof(value)}");
            return Directory.CreateDirectory(value);
        }

        public static FileInfo ToFileInfo(this string filename, string extention, DirectoryInfo parent)
        {
            filename.ThrowIfNullOrWhiteSpace($"{nameof(filename)}");
            extention.ThrowIfNullOrWhiteSpace($"{nameof(extention)}");
            parent.ThrowIfNull(nameof(parent));

            return (filename + "." + extention).ToFileInfo(parent);
        }

        private static FileInfo ToFileInfo(this string filename, DirectoryInfo parent)
        {
            parent.Refresh();
            if (!parent.Exists)
            {
                parent = parent.FullName.ToDirectoryInfo();
            }
            return new FileInfo(Path.Combine(parent.FullName, filename));
        }
    }
}