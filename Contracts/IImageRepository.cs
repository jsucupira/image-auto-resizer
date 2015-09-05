namespace Contracts
{
    public interface IImageRepository
    {
        void CreateDirectory();
        void DeleteFile(string fileName);
        void SaveFile(string fileName);
        bool Exists(string fileName);
        void SetFolderName(string folderName);
    }
}