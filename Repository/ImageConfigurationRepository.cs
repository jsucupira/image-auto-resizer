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
        private readonly ICacheDataStorage _cacheStorage = ObjectContainer.Container.GetExportedValue<ICacheDataStorage>();

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
            _cacheStorage.Add(imageDefault.Url, imageDefault);
        }

        public void Remove(string url)
        {
            _cacheStorage.Remove(url);
        }
    }
}