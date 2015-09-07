using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Runtime.Caching;
using Contracts;

namespace Caching
{
    [Export(typeof (ICacheDataStorage))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CacheDataStorage : ICacheDataStorage
    {
        private static readonly ObjectCache _cache = MemoryCache.Default;
        private const string DEFAULT_CACHE_KEY = "__CACHE_{0}_{1}";
        private static readonly CacheItemPolicy _defaultPolicy = new CacheItemPolicy {Priority = CacheItemPriority.Default};
        private static readonly HashSet<string> _keys = new HashSet<string>();

        public void Add(string key, object dataObject, string section, CacheItemPolicy policy)
        {
            key = string.Format(DEFAULT_CACHE_KEY, section, key);
            _cache.Set(key, dataObject, policy);
            _keys.Add(key);
        }

        public bool Exists(string key, string section)
        {
            key = string.Format(DEFAULT_CACHE_KEY, section, key);
            return _cache[key] != null;
        }

        public T Get<T>(string key, string section)
        {
            key = string.Format(DEFAULT_CACHE_KEY, section, key);
            object cacheValue = _cache[key];
            return cacheValue == null ? default(T) : (T) cacheValue;
        }

        public void Add(string key, object dataObject, string section)
        {
            Add(key, dataObject, section, _defaultPolicy);
        }

        public void Remove(string key, string section)
        {
            key = string.Format(DEFAULT_CACHE_KEY, section, key);
            _cache.Remove(key);
            _keys.Remove(key);
        }

        public void RemoveAll(string section)
        {
            foreach (string key in _keys)
                Remove(key, section);
        }
    }
}