using System.Runtime.Caching;

namespace Contracts
{
    public interface ICacheDataStorage
    {
        void Add(string key, object dataObject, string section);
        void Add(string key, object dataObject, string section, CacheItemPolicy policy);
        bool Exists(string key, string section);
        T Get<T>(string key, string section);
        void Remove(string key, string section);
        void RemoveAll(string section);
    }
}