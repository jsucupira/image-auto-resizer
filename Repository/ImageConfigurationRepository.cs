using System.ComponentModel.Composition;
using Contracts;
using Core.MEF;
using Domain;

namespace Repository
{
    [Export(typeof(IImageConfigurationRepository))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ImageConfigurationRepository : IImageConfigurationRepository
    {
        private readonly ICacheDataStorage _cacheStorage = ObjectContainer.Resolve<ICacheDataStorage>();
        private const string CACHE_SECTION = "ImageConfiguration";

        public bool Exists(string url)
        {
            return _cacheStorage.Exists(url, CACHE_SECTION);
        }

        public ImageDefaults Get(string url)
        {
            return _cacheStorage.Get<ImageDefaults>(url, CACHE_SECTION);
        }

        public void Save(ImageDefaults imageDefault)
        {
            _cacheStorage.Add(imageDefault.Url, imageDefault, CACHE_SECTION);
        }

        public void Remove(string url)
        {
            _cacheStorage.Remove(url, CACHE_SECTION);
        }

        public void Clear()
        {
            _cacheStorage.RemoveAll(CACHE_SECTION);
        }
    }
}