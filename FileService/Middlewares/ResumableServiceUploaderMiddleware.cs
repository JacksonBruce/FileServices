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
        public ResumableServiceUploaderMiddleware(RequestDelegate next, IFileServiceProvider serviceProvider) : base(next, serviceProvider)
        {
        }
        protected override async Task Handler(HttpContext context)
        {
            var blobIndex = long.Parse(context.Request.Form["blobIndex"]);
            string resumableKey = context.Request.Form["key"];
            string _scheme = context.Request.Headers["scheme"]; 
            var uploader = context.RequestServices.GetRequiredService<IBlobUploader>();
            await WriteJsonAsync(context, await uploader.Handle(new Blob()
            {
                BlobIndex = blobIndex,
                ResumableKey = resumableKey,
                Data = context.Request.Form.Files[0].OpenReadStream()
            }, _scheme));
        } 
    }
}
