using System;
using System.ComponentModel.Composition;
using Contracts;
using Core.MEF;

namespace Repository
{
    [MefExport(typeof(IImageRepository), "BlobStorage")]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ImageBlobRepository : IImageRepository
    {
        public void CreateDirectory()
        {
            throw new NotImplementedException();
        }

        public void DeleteFile(string fileName)
        {
            throw new NotImplementedException();
        }

        public void SaveFile(string fileName)
        {
            throw new NotImplementedException();
        }

        public bool Exists(string fileName)
        {
            throw new NotImplementedException();
        }

        public void SetFolderName(string folderName)
        {
            throw new NotImplementedException();
        }
    }
}