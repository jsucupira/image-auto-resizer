using Domain;

namespace Contracts
{
    public interface IImageConfigurationRepository
    {
        bool Exists(string url);
        ImageDefaults Get(string url);
        void Save(ImageDefaults imageDefault);
        void Remove(string url);
    }
}