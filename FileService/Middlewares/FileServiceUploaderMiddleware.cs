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
    public class FileServiceUploaderMiddleware: UploaderMiddleware
    {
        public FileServiceUploaderMiddleware(RequestDelegate next, IFileServiceProvider serviceProvider) : base(next, serviceProvider)
        {
        }

        protected async override Task Handler(HttpContext context) {
            string _scheme = context.Request.Headers["scheme"];
            string dir = context.Request.Form["dir"];
            string fileName = context.Request.Form["Name"];
            var uploader = context.RequestServices.GetRequiredService<IUploader>();
            if (context.Request.Form.Files.Count > 1)
            { 
                if (await ValidateResultHandler(uploader.Validate(context.Request.Form.Files, _scheme),context))
                {
                    //如果有文件验证失败则返回
                    return;
                }
                await WriteJsonAsync(context, await uploader.Handle(context.Request.Form.Files, _scheme));
                return;

            }
            else if (context.Request.Form.Files.Count == 1)
            {
                if (await ValidateResultHandler(uploader.Validate(context.Request.Form.Files[0], _scheme), context))
                {
                    return;
                }
                await WriteJsonAsync(context, await uploader.Handle(context.Request.Form.Files[0], _scheme));
            }
        }
    }
}
