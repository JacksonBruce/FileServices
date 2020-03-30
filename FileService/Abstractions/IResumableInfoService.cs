using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ufangx.FileServices.Abstractions
{
    public interface IResumableInfoService
    {
        Task<IResumableInfo> Create(string storeName, string fileName, long fileSize, string fileType, long blobCount, int blobSize);
        Task<IResumableInfo> Get(string key);
        Task<bool> Update(IResumableInfo resumable);
        Task<bool> Delete(IResumableInfo resumable);
       
    }
}
