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
        private IResumableService service;

        public IResumableService Service => service??(service=serviceProvider.GetResumableService());
        public ResumableInfoDeleteUploaderMiddleware(RequestDelegate next, 
            IFileServiceProvider serviceProvider) : base(next, serviceProvider)
        {
           
        }
        protected override async Task Handler(HttpContext context) {
            await WriteJsonAsync(await Service.DeleteBlobs(GetRequestParams("key")));
        }
        protected override string GetRequestParams(string key)
        {
            return Context.Request.Form[key];
        }
    }
}
