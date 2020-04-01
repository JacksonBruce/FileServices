using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ufangx.FileServices.Abstractions;
using Ufangx.FileServices.Models;

namespace Ufangx.FileServices.Services
{
    public class BlobUploader :Uploader, IBlobUploader
    {
        private readonly IResumableService service;

        public BlobUploader(IResumableService fileService, IFileServiceProvider serviceProvider, IHttpContextAccessor contextAccessor) : base(fileService, serviceProvider, contextAccessor)
        {
            this.service = fileService;
        }

        public async Task<object> Handle(Blob blob,string schemeName=null)
        {
            bool finished = false;
            object result =null;
            var ok= await service.SaveBlob(blob,
               async (info, success) =>
               {
                   finished = true;
                   if (success)
                   {
                       result =await SchemeHandler(GetScheme(schemeName), info.StoreName, info.FileType, info.FileSize);
                   }
                   else
                   {
                       result = false;
                   }
               });
            return finished ? result : ok;


        }
    }
}
