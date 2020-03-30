using Qiniu;
using System;
using System.Collections.Generic;
using System.Text;
using Ufangx.FileServices.Abstractions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class QiniuFileServiceExtensions
    {
        public static IFileServiceBuilder AddQiniuFileService<TResumableInfoService>(this IFileServiceBuilder builder , Action<FileServiceOptions> action)
            where TResumableInfoService: QiniuResumableInfoService
            //class,IResumableInfoService
        {
            var services = builder.Services;
            services.AddTransient<IFileService, FileService>();
            services.AddTransient<IResumableInfoService, TResumableInfoService>();
            services.AddTransient<IResumableService, ResumableService>();
            services.Configure(action);
            return builder;
        }
  
        public static IFileServiceBuilder AddQiniuFileService(this IFileServiceBuilder builder, Action<FileServiceOptions> action)
            => AddQiniuFileService<QiniuResumableInfoService>(builder, action);
    }
}
