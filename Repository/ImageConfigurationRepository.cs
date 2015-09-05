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
            return _cacheStorage.Exists(url);
        }

        public ImageDefaults Get(string url)
        {
            return _cacheStorage.Get<ImageDefaults>(url);
        }

        public void Save(ImageDefaults imageDefault)
        {
            _cacheStorage.Add(imageDefault.Url, imageDefault, CACHE_SECTION);
        }

        public void Remove(string url)
        {
            _cacheStorage.Remove(url);
        }

        public void Clear()
        {
            _cacheStorage.RemoveSection(CACHE_SECTION);
        }
    }
}