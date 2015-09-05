using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts;
using Core.MEF;

namespace Repository
{
    [MefExport(typeof(IImageRepository), "FileSystem")]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ImageFileSystemRepository : IImageRepository
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
