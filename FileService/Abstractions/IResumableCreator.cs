using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ufangx.FileServices.Models;

namespace Ufangx.FileServices.Abstractions
{
    public interface IResumableCreator
    {
        Task<IResumableInfo> Create(string fileName, long fileSize, string fileType, long blobCount, int blobSize, string schemeName = null, string dir = null, string name = null);
        Task<IResumableInfo> Get(string key);
        FileValidateResult Validate(string fileName, long fileSize, string schemeName);

    }
}
