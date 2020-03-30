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
using Microsoft.AspNetCore.Authentication;
using System.Linq;
using Ufangx.FileServices.Models;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Serialization;

namespace Ufangx.FileServices.Middlewares
{
    public abstract class UploaderMiddleware
    {
        protected readonly RequestDelegate next;
        protected readonly IFileServiceProvider serviceProvider;
   

        public UploaderMiddleware(RequestDelegate next,IFileServiceProvider serviceProvider)
        {
            this.next = next;
            this.serviceProvider = serviceProvider;
        }
        protected HttpContext Context { get; private set; }
        private IStringLocalizer stringLocalizer;
        protected IStringLocalizer StringLocalizer 
            => stringLocalizer??(stringLocalizer=Context.RequestServices.GetService<IStringLocalizer>()??new DefaultStringLocalizer());

     
        protected abstract string GetRequestParams(string key);
        protected async Task<string> GetStoreFileName(string originName) {

            string dir = GetRequestParams("dir");
            string fileName = GetRequestParams("Name");
            string scheme = GetScheme();
            //如果客户端指定了
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                return Path.Combine(serviceProvider.GetStoreDirectory(scheme), dir, fileName).Replace('\\', '/');
            }
            //否则生成文件名称
            return await serviceProvider.GenerateFileName(originName, scheme, dir);
        
        }
        async Task<bool> Authenticate(HttpContext context) {
            if (serviceProvider.AuthenticationSchemes?.Count() > 0) {
                foreach ( var scheme in serviceProvider.AuthenticationSchemes)
                {
                    var result = await context.AuthenticateAsync(scheme);
                    if (result.Succeeded) {
                        context.User = result.Principal;
                        return true;
                    }
                }
                return false;
            }
            return context.User.Identity.IsAuthenticated;
            //return true;
        }
        string _scheme;
        protected string GetScheme() {
            if (_scheme == null)
            {
                _scheme =Context.Request.Headers["scheme"];
                if (string.IsNullOrWhiteSpace(_scheme))
                {
                    _scheme = serviceProvider.DefaultSchemeName ?? string.Empty;
                }
            }
            return _scheme;
        }
        protected async Task<object> SchemeHandler(string path,string fileType,long fileSize) {
            var handler = serviceProvider.GetHandler(GetScheme());
            if (handler == null) return path;
            return await handler.Handler(path, fileType, fileSize);
        
        }
        protected FileValidateResult Validate(string fileName,long fileSize) {
            return serviceProvider.Validate(GetScheme(), fileName, fileSize);
        }
        protected abstract Task Handler(HttpContext context);
        protected async Task<bool> ValidateResultHandler(FileValidateResult fileValidateResult)
        {
            if (fileValidateResult == FileValidateResult.Successfully) return false;
            await Error(StringLocalizer[fileValidateResult == FileValidateResult.Invalid ? "不支持的文件类型" : "文件大小超过了最大限制"]);
            return true;
        }
        protected Task Error(object error, int code = 500) { 
            Context.Response.StatusCode = code;
            return WriteJsonAsync(error);
        }
        protected Task Error(string message, int code = 500)
        => Error((object)message, code);
        protected Task WriteAsync(string content) => WriteJsonAsync(content);
        protected Task WriteJsonAsync(object obj)
            => Context.Response.WriteAsync(JsonConvert.SerializeObject(obj,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }), Encoding.UTF8);
        public async Task Invoke(HttpContext context)
        {
            Context = context;
            context.Response.ContentType = "application/json; charset=UTF-8";
            if (await Authenticate(context)) {
                await Handler(context);
                return;
            }
            context.Response.StatusCode = 401;
            await WriteJsonAsync(StringLocalizer["身份认证失败！"].Value);

           

           

        }

    }
}
