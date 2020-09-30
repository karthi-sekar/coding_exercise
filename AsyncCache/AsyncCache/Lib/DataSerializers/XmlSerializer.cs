using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AsyncCache.Contracts;
using DotNetObjectsExt;

namespace AsyncCache.Lib.DataSerializers
{
    internal sealed class XmlSerializer : ICacheDataSerializer
    {
        private readonly AutoResetEvent _waitHandle = new AutoResetEvent(true);
        private readonly string _fileName;
        private readonly DirectoryInfo _folder;
        private readonly IEnumerable<Type> _knownTypes;
        private readonly bool _indent;

        public XmlSerializer(string filenameWithoutExtension, DirectoryInfo folder,
            IEnumerable<Type> knownTypes, bool indent)
        {
            _fileName = filenameWithoutExtension;
            _folder = folder;
            _knownTypes = knownTypes;
            _indent = indent;
        }

        public async Task Serialize<T>(T obj)
        {
            var lockTaken = false;
            try
            {
                lockTaken = _waitHandle.WaitOne();
                await obj.ToXml(_fileName, _folder, _knownTypes, _indent).ConfigureAwait(false);
            }
            finally
            {
                if (lockTaken)
                {
                    _waitHandle.Set();
                }
            }
        }

        public async Task<T> DeserializeAsync<T>()
        {
            var lockTaken = false;
            try
            {
                lockTaken = _waitHandle.WaitOne();
                var deserialData =
                    await Task.Run(() => _fileName.FromXml<T>(_folder, _knownTypes)).ConfigureAwait(false);
                if (deserialData == null)
                {
                    throw new NullReferenceException($"No file or null data. File:{_fileName},folder:{_folder.FullName}");
                }
                return deserialData;
            }
            finally
            {
                if (lockTaken)
                {
                    _waitHandle.Set();
                }
            }
        }
    }
}