using Domain;

namespace Contracts
{
    public interface IDevice
    {
        ImageSizes GetImageSize(string userAgent);
    }
}