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
        private IFileServiceProvider serviceProvider;


        public UploaderMiddleware(RequestDelegate next, IFileServiceProvider serviceProvider)
        {
            this.next = next;
            this.serviceProvider = serviceProvider;
        }
   
        async Task<bool> Authenticate(HttpContext context)
        {
            if (serviceProvider.AuthenticationSchemes?.Count() > 0)
            {
                foreach (var scheme in serviceProvider.AuthenticationSchemes)
                {
                    var result = await context.AuthenticateAsync(scheme);
                    if (result.Succeeded)
                    {
                        context.User = result.Principal;
                        return true;
                    }
                }
                return false;
            }
            return context.User.Identity.IsAuthenticated;
        }
        
        protected abstract Task Handler(HttpContext context);
        protected async Task<bool> ValidateResultHandler(FileValidateResult fileValidateResult,HttpContext context)
        {
            if (fileValidateResult == FileValidateResult.Successfully) return false;
            await Error(context,fileValidateResult == FileValidateResult.Invalid ? "不支持的文件类型" : "文件大小超过了最大限制");
            return await Task.FromResult(true);
        }
        protected Task Error(HttpContext context, object error, int code = 500)
        {
            context.Response.StatusCode = code;
            return WriteJsonAsync(context,error);
        }
        protected async Task Error(HttpContext context, string message, int code = 500)
        {
            var localizer = context.RequestServices.GetService<IStringLocalizer>();
            string localMessage = localizer != null ? localizer[message] : message;
            await Error(context, (object)localMessage, code);
        }
      
        protected Task WriteAsync(HttpContext context, string content) => WriteJsonAsync(context,content);
        protected async Task WriteJsonAsync(HttpContext context, object obj)
        {

            context.Response.ContentType = "application/json; charset=UTF-8";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(obj,
                    Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    }), Encoding.UTF8);
        }

        public async Task Invoke(HttpContext context)
        {
            if (await Authenticate(context))
            {
                await Handler(context);
                return;
            }
            await Error(context, "身份认证失败！", 401);
        }

    }
}
