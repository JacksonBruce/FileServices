using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ufangx.FileServices.Models;

namespace Ufangx.FileServices.Abstractions
{
    public interface IBlobUploader
    {
        Task<object> Handle(Blob blob, string schemeName = null);
    }
}
