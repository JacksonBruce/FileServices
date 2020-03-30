using System;
using System.Collections.Generic;
using System.Text;
using Ufangx.FileServices;
using Ufangx.FileServices.Abstractions;
using Ufangx.FileServices.Models;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FileServiceBuilderServiceCollectionExtensions
    {
        public static IFileServiceBuilder AddFileServiceBuilder(this IServiceCollection services, Action<FileServiceOptions> configureBuilder = null) {
            services.AddTransient<IFileServiceProvider, FileServiceProvider>();
            return new FileServiceBuilder() { Services = services };
        }
    }
}
