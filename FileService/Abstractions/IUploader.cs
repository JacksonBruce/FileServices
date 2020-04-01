using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ufangx.FileServices.Models;

namespace Ufangx.FileServices.Abstractions
{
    public interface IUploader
    {
        FileValidateResult Validate(IFormFile file, string schemeName=null);
        FileValidateResult Validate(IFormFileCollection files, string schemeName=null);
        Task<object> Handle(IFormFileCollection files, string schemeName = null);
        Task<object> Handle(IFormFile file,string schemeName=null, string dir=null,string name=null);
    }
}
