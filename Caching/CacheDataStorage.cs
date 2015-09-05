using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Runtime.Caching;
using Contracts;

namespace Caching
{
    [Export(typeof (ICacheDataStorage))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class CacheDataStorage : ICacheDataStorage
    {
        private const string CACHE_KEY = "__Caching_{0}";
        private static readonly ObjectCache _cache = MemoryCache.Default;
        private static readonly Dictionary<string, List<string>> _cacheSections = new Dictionary<string, List<string>>();
        private static readonly CacheItemPolicy _defaultPolicy = new CacheItemPolicy {Priority = CacheItemPriority.Default};

        public void Add(string key, object dataObject, CacheItemPolicy policy)
        {
            key = string.Format(CACHE_KEY, key);
            _cache.Set(key, dataObject, policy);
        }

        public void Add(string key, object dataObject, string section)
        {
            Add(key, dataObject, _defaultPolicy, section);
        }

        public void Add(string key, object dataObject, CacheItemPolicy policy, string section)
        {
            key = string.Format(CACHE_KEY, key);
            _cache.Set(key, dataObject, policy);

            if (!_cacheSections.ContainsKey(section))
                _cacheSections[section] = new List<string>();

            if (!_cacheSections[section].Contains(key))
                _cacheSections[section].Add(key);
        }

        public bool Exists(string key)
        {
            key = string.Format(CACHE_KEY, key);
            return _cache[key] != null;
        }

        public T Get<T>(string key)
        {
            key = string.Format(CACHE_KEY, key);
            object cacheValue = _cache[key];
            return cacheValue == null ? default(T) : (T) cacheValue;
        }

        public void Add(string key, object dataObject)
        {
            Add(key, dataObject, _defaultPolicy);
        }

        public void Remove(string key)
        {
            key = string.Format(CACHE_KEY, key);
            _cache.Remove(key);
        }

        public void RemoveSection(string section)
        {
            if (!_cacheSections.ContainsKey(section)) return;

            foreach (string key in _cacheSections[section])
                Remove(key);

            _cacheSections.Remove(section);
        }
    }
}