using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Ufangx.FileServices.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ufangx.FileServices.Models;
using Microsoft.Extensions.Localization;

namespace Ufangx.FileServices.Middlewares
{
    public class ResumableServiceUploaderMiddleware : UploaderMiddleware
    {
        private IResumableService _service;
        private readonly ILogger<ResumableServiceUploaderMiddleware> logger;

        public IResumableService Service => _service ?? (_service = serviceProvider.GetResumableService());

        public ResumableServiceUploaderMiddleware(RequestDelegate next, 
            ILogger<ResumableServiceUploaderMiddleware> logger,
           IFileServiceProvider serviceProvider) : base(next, serviceProvider)
        {
            this.logger = logger;
        }
        protected override async Task Handler(HttpContext context) {     
            var blobIndex = long.Parse(GetRequestParams("blobIndex"));
            string resumableKey = GetRequestParams("key");
            bool finished = false;
            await Service.SaveBlob(new Blob() { BlobIndex = blobIndex, ResumableKey = resumableKey, Data = context.Request.Form.Files[0].OpenReadStream() },
               async (info,success) =>
               {
                   finished = true;
                   if (success)
                   {
                       await WriteJsonAsync(await SchemeHandler(info.StoreName, info.FileType, info.FileSize));
                   }
                   else {
                       await Error(StringLocalizer["保存文件失败"]);
                   }
               });
            if (!finished)
            {
                await WriteJsonAsync(true);
            }
        }
        protected override string GetRequestParams(string key)
        {
            return Context.Request.Form[key];
        }
    }
}
