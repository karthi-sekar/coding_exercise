using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AsyncCache.Contracts;
using DotNetObjectsExt;

namespace AsyncCache.Lib.DataSerializers
{
    internal sealed class JsonSerializer : ICacheDataSerializer
    {
        private readonly AutoResetEvent _waitHandle = new AutoResetEvent(true);
        private readonly string _fileName;
        private readonly DirectoryInfo _folder;
        private readonly IEnumerable<Type> _knownTypes;

        public JsonSerializer(string filenameWithoutExtension, DirectoryInfo folder, IEnumerable<Type> knownTypes)
        {
            _fileName = filenameWithoutExtension;
            _folder = folder;
            _knownTypes = knownTypes;
        }

        public async Task Serialize<T>(T obj)
        {
            var lockTaken = false;
            try
            {
                lockTaken = _waitHandle.WaitOne();
                await obj.ToJsonAsync(_folder, _fileName, _knownTypes).ConfigureAwait(false);
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
                var deserialData = await Task.Run(() => _fileName.FromJson<T>(_folder, _knownTypes)).ConfigureAwait(false);
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