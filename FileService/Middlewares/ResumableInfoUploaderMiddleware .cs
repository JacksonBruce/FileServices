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
    public class ResumableInfoUploaderMiddleware : UploaderMiddleware
    {
        public ResumableInfoUploaderMiddleware(RequestDelegate next, IFileServiceProvider serviceProvider) : base(next, serviceProvider)
        {
        }
        protected override async Task Handler(HttpContext context)
        { 
            string key = context.Request.Query["key"];
            IResumableInfo info = null;
            IResumableCreator resumableCreator = context.RequestServices.GetRequiredService<IResumableCreator>();
            if (string.IsNullOrWhiteSpace(key))
            {
                if (long.TryParse(context.Request.Query["fileSize"], out long fileSize)
                     && int.TryParse(context.Request.Query["blobSize"], out int blobSize)
                     && long.TryParse(context.Request.Query["blobCount"], out long blobCount))
                {
                    string fileName = context.Request.Query["fileName"];
                    string _scheme = context.Request.Headers["scheme"];
                    string fileType= context.Request.Query["fileType"];
                    string dir = context.Request.Query["dir"];
                    string name = context.Request.Query["name"];
                    if (string.IsNullOrWhiteSpace(fileName))
                    {
                        await Error(context, "参数文件名称（fileName）是必须的");
                        return;
                    }
                    if (await ValidateResultHandler(resumableCreator.Validate(fileName,fileSize,_scheme),context))
                    {
                        return;
                    }
                    info = await resumableCreator.Create(fileName,
                        fileSize,
                        fileType,
                        blobCount,
                        blobSize,
                        _scheme,
                        dir,
                        name);
                }
            }
            else
            {
                info = await resumableCreator.Get(key);
            }
            await WriteJsonAsync(context, info == null ? null : new
            {
                key = info.Key,
                index = info.BlobIndex,
            });
        }

    }
}
