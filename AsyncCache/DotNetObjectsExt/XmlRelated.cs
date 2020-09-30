using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DotNetObjectsExt
{
    public static class XmlRelated
    {
        public const string XmlExt = "xml";
        private const int BufferSize4K = 4096;

        public static Task ToXml<T>(this T obj, string filenameWithoutExt, DirectoryInfo parent,
            IEnumerable<Type> knownTypes = null, bool indent = false)
        {
            return obj.ToXml(filenameWithoutExt.ToFileInfo(XmlExt, parent), knownTypes, indent);
        }

        private static async Task ToXml<T>(this T obj, FileSystemInfo fileInfo, IEnumerable<Type> knownTypes, bool indent)
        {
            using (var writer = XmlWriter.Create(fileInfo.FullName, new XmlWriterSettings
            {
                Async = true,
                Encoding = Encoding.UTF8,
                Indent = indent
            }))
            {
                var serializer = new DataContractSerializer(obj.GetType(), knownTypes);
                serializer.WriteObject(writer, obj);
                await writer.FlushAsync().ConfigureAwait(false);
            }
        }

        public static T FromXml<T>(this string xmlFilenameWithoutExtension, DirectoryInfo parent,
            IEnumerable<Type> knownTypes = null)
        {
            return xmlFilenameWithoutExtension.ToFileInfo(XmlExt, parent).FromXml<T>(knownTypes);
        }

        private static T FromXml<T>(this FileSystemInfo fileInfo, IEnumerable<Type> knownTypes)
        {
            fileInfo.Refresh();
            if (!fileInfo.Exists)
            {
                return default(T);
            }
            using (
                var xmlFile = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.None,
                    BufferSize4K, FileOptions.Asynchronous))
            {
                using (var xmlReader = XmlReader.Create(xmlFile, new XmlReaderSettings
                {
                    Async = true
                }))
                {
                    var serializer = new DataContractSerializer(typeof(T), knownTypes);
                    return (T) serializer.ReadObject(xmlReader);
                }
            }
        }
    }
}