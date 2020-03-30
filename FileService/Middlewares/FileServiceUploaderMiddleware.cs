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
        private IFileService service;

        public IFileService Service
            => service??(service=serviceProvider.GetFileService());

        public FileServiceUploaderMiddleware(RequestDelegate next,
            IFileServiceProvider serviceProvider) : base(next, serviceProvider)
        {
        }
        protected override string GetRequestParams(string key)
        {
            return Context.Request.Form[key];
        }
        async Task SingleHandler(IFormFile file) {
            if (await ValidateResultHandler(Validate(file.FileName, file.Length)))
            {
                return;
            }
            string path = await GetStoreFileName(file.FileName);
            if (await Service.Save(path, file.OpenReadStream()))
            {
                await WriteJsonAsync(await SchemeHandler(path, file.ContentType, file.Length));
                return;
            }
            await Error(StringLocalizer["保存文件失败"]);

        }
        async Task MultiHandler(IFormFileCollection files) {
            foreach (var file in files)
            {
                if (await ValidateResultHandler(Validate(file.FileName, file.Length)))
                {
                    //如果有文件验证失败则返回
                    return;
                }
            }
            List<object> results = new List<object>();
            foreach (var file in files)
            {  
                string path = await GetStoreFileName(file.FileName);
                if (await Service.Save(path, file.OpenReadStream()))
                {
                    results.Add(await SchemeHandler(path, file.ContentType, file.Length));
                }
            }
            await WriteJsonAsync(results);
        }
        protected async override Task Handler(HttpContext context) {
            if (context.Request.Form.Files.Count > 1)
            {
                await MultiHandler(context.Request.Form.Files);

            }
            else if(context.Request.Form.Files.Count==1) {   
                await SingleHandler(context.Request.Form.Files[0]);
            }
        }
    }
}
