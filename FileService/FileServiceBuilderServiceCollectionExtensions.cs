using System;
using System.Collections.Generic;
using System.Text;
using Ufangx.FileServices;
using Ufangx.FileServices.Abstractions;
using Ufangx.FileServices.Models;
using Ufangx.FileServices.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FileServiceBuilderServiceCollectionExtensions
    {
        public static IFileServiceBuilder AddFileServices(this IServiceCollection services, Action<FileServiceOptions> configureBuilder = null) {
            services.Configure(configureBuilder);
            services.AddSingleton<IFileServiceProvider, FileServiceProvider>();
            services.AddTransient<IUploader, Uploader>();
            services.AddTransient<IResumableCreator, ResumableCreator>();
            services.AddTransient<IBlobUploader, BlobUploader>();
            return new FileServiceBuilder() { Services = services };
        }
    }
}
