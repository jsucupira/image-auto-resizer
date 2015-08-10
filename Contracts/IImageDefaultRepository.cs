using Domain;

namespace Contracts
{
    public interface IImageRepository
    {
        bool Exists(string url);
        ImageDefaults Get(string url);
        void Save(ImageDefaults imageDefault);
        void Remove(string url);
    }
}