using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using Ufangx.FileServices.Middlewares;

namespace Microsoft.AspNetCore.Builder
{
    public static class FileServiceAppBuilderExtensions
    {
        internal static PathString PathBase { get; private set; }
        public static IApplicationBuilder UseFileServices(this IApplicationBuilder app)
        {
            return UseFileServices(app, new PathString());
        }
        public static IApplicationBuilder UseFileServices(this IApplicationBuilder app, PathString pathMatch)
        {

            if (!pathMatch.HasValue)
            {
                pathMatch = "/FileServices";
            }
            PathBase = pathMatch;
            app.Map(pathMatch, pathRoute => {
                pathRoute.Map("/uploader", route =>
                {
                    route.MapWhen(ctx => string.Equals("GET", ctx.Request.Method, StringComparison.OrdinalIgnoreCase), a => a.UseMiddleware<ResumableInfoUploaderMiddleware>())
                    .MapWhen(ctx => string.Equals("DELETE", ctx.Request.Method, StringComparison.OrdinalIgnoreCase), a => a.UseMiddleware<ResumableInfoDeleteUploaderMiddleware>())
                    .MapWhen(ctx =>
                    string.Equals("POST", ctx.Request.Method, StringComparison.OrdinalIgnoreCase) && ctx.Request.Form.Files.Count== 1 && !string.IsNullOrWhiteSpace(ctx.Request.Form["key"]) && !string.IsNullOrWhiteSpace(ctx.Request.Form["blobIndex"]),
                    a => a.UseMiddleware<ResumableServiceUploaderMiddleware>())
                    .MapWhen(ctx => string.Equals("POST", ctx.Request.Method, StringComparison.OrdinalIgnoreCase) && ctx.Request.Form.Files.Count > 0,
                    a => a.UseMiddleware<FileServiceUploaderMiddleware>());
                });
                
            });
            return app;
        }
    }
}
