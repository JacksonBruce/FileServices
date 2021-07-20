using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ufangx.FileServices.Abstractions
{
    public interface IRootDirectory
    {
        Task<string> GetRoot();
    }
}
