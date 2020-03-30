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
        private IResumableInfoService service;

        public IResumableInfoService Service => service ?? (service = Context.RequestServices.GetRequiredService<IResumableInfoService>());

        public ResumableInfoUploaderMiddleware(RequestDelegate next, 
            IFileServiceProvider serviceProvider) : base(next, serviceProvider)
        {
        }
        protected override async Task Handler(HttpContext context) {
            string key = GetRequestParams("key");
            IResumableInfo info = null;
            if (string.IsNullOrWhiteSpace(key))
            {
                if (long.TryParse(GetRequestParams("fileSize"), out long fileSize)
                     && int.TryParse(GetRequestParams("blobSize"), out int blobSize)
                     && long.TryParse(GetRequestParams("blobCount"), out long blobCount))
                {
                    string fileName = GetRequestParams("fileName");
                    if (string.IsNullOrWhiteSpace(fileName)) {
                        await Error(StringLocalizer["参数文件名称（fileName）是必须的"]);
                        return;
                    }
                    if (await ValidateResultHandler(Validate(fileName, fileSize))) {
                        return;
                    }
                    info = await Service.Create(await GetStoreFileName(fileName),
                        fileName,
                        fileSize,
                        GetRequestParams("fileType"),
                        blobCount,
                        blobSize);
                }
            }
            else
            { 
                info = await Service.Get(key);
            }
            await WriteJsonAsync(info == null ? null : new
            {
                key = info.Key,
                index = info.BlobIndex,
            }); 
        }

        protected override string GetRequestParams(string key)
        {
            return Context.Request.Query[key];
        }
    }
}
