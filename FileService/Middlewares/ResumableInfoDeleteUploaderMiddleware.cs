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
using Microsoft.Extensions.Localization;

namespace Ufangx.FileServices.Middlewares
{
    public class ResumableInfoDeleteUploaderMiddleware : UploaderMiddleware
    {
        public ResumableInfoDeleteUploaderMiddleware(RequestDelegate next, IFileServiceProvider serviceProvider) : base(next, serviceProvider)
        {
        }
        protected override async Task Handler(HttpContext context)
        {
            string key = context.Request.Query["key"];
            var service= context.RequestServices.GetRequiredService<IResumableService>();
            await WriteJsonAsync(context, await service.DeleteBlobs(key));
        }
    }
}
