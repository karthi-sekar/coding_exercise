using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DotNetObjectsExt
{
    public static class JsonRelated
    {
        private const string JsonExt = "json";
        private const int BufferSize4K = 4096;
        private static readonly DateTimeFormat Format = new DateTimeFormat("yyyyMMddHHmmss", new CultureInfo("en-US"));

        public static Task ToJsonAsync<T>(this T obj, DirectoryInfo parent, string filenameWithoutExt,
            IEnumerable<Type> knownTypes = null)
        {
            return filenameWithoutExt.ToFileInfo(JsonExt, parent).ToJsonAsync(obj, knownTypes);
        }

        private static async Task ToJsonAsync<T>(this FileSystemInfo fileInfo, T obj, IEnumerable<Type> knownTypes)
        {
            using (
                var jsonFile = new FileStream(fileInfo.FullName, FileMode.Create, FileAccess.ReadWrite, FileShare.None,
                    BufferSize4K, FileOptions.Asynchronous))
            {
                var serializer = new DataContractJsonSerializer(typeof(T), new DataContractJsonSerializerSettings
                {
                    KnownTypes = knownTypes,
                    DateTimeFormat = Format,
                    SerializeReadOnlyTypes = false,
                    IgnoreExtensionDataObject = true
                });
                serializer.WriteObject(jsonFile, obj);
                await jsonFile.FlushAsync().ConfigureAwait(false);
            }
        }

        public static T FromJson<T>(this string jsonFilenameWithoutExtension, DirectoryInfo parent,
            IEnumerable<Type> knownTypes = null)
        {
            return jsonFilenameWithoutExtension.ToFileInfo(JsonExt, parent).FromJson<T>(knownTypes);
        }

        private static T FromJson<T>(this FileSystemInfo fileInfo, IEnumerable<Type> knownTypes)
        {
            fileInfo.Refresh();
            if (!fileInfo.Exists)
            {
                return default(T);
            }
            using (
                var jsonFile = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.None,
                    BufferSize4K, FileOptions.Asynchronous))
            {
                var serializer = new DataContractJsonSerializer(typeof(T), new DataContractJsonSerializerSettings
                {
                    KnownTypes = knownTypes,
                    DateTimeFormat = Format,
                    SerializeReadOnlyTypes = false,
                    IgnoreExtensionDataObject = true
                });
                return (T)serializer.ReadObject(jsonFile);
            }
        }
    }
}