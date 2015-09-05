using System.Runtime.Caching;

namespace Contracts
{
    public interface ICacheDataStorage
    {
        void Add(string key, object dataObject);
        void Add(string key, object dataObject, CacheItemPolicy policy);
        void Add(string key, object dataObject, string section);
        void Add(string key, object dataObject, CacheItemPolicy policy, string section);
        bool Exists(string key);
        T Get<T>(string key);
        void Remove(string key);
        void RemoveSection(string section);
    }
}