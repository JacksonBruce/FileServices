using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ufangx.FileServices.Models;

namespace Ufangx.FileServices.Abstractions
{
    public interface IResumableService: IFileService
    {
        Task<bool> SaveBlob(Blob blob, Func<IResumableInfo,bool, Task> finished =null, CancellationToken token = default(CancellationToken));
        Task<bool> DeleteBlobs(string key, CancellationToken token = default(CancellationToken));

    }
}
