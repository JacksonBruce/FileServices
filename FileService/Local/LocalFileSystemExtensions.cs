using System;
using System.Collections.Generic;
using System.Text;
using Ufangx.FileServices;
using Ufangx.FileServices.Abstractions;
using Ufangx.FileServices.Caching;
using Ufangx.FileServices.Local;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LocalFileSystemExtensions
    {
        public static IFileServiceBuilder AddLocalServices<TResumableInfoService>(this IFileServiceBuilder builder,Action<LocalFileOption> action)where TResumableInfoService: class,IResumableInfoService
        {
            var services = builder.Services;
            services.AddTransient<IFileService, LocalFileService>();
            services.AddTransient<IResumableInfoService, TResumableInfoService>();
            services.AddTransient<IResumableService, LocalResumableService>();
            services.Configure(action);
            return builder;
        }
        public static IFileServiceBuilder AddLocalServices(this IFileServiceBuilder builder, Action<LocalFileOption> action)
            => AddLocalServices<CacheResumableInfoService<LocalResumableInfo>>(builder, action);
    }
}
