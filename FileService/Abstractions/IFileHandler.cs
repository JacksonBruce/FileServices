using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ufangx.FileServices.Abstractions
{
    public interface IFileHandler
    {
        Task<object> Handler(string path,string fileType,long fileSize);
    }
}
