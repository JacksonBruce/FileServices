using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Ufangx.FileServices.Models;

namespace Ufangx.FileServices.Abstractions
{
    public interface IFileServiceBuilder
    {
        IServiceCollection Services { get; }
        IFileServiceBuilder AddScheme(string name, Action<FileServiceScheme> configureBuilder);
        IFileServiceBuilder AddScheme<THandler>(string name, string storeDirectory = null, IEnumerable<string> supportExtensions = null, long? LimitedSize = null) where THandler :class, IFileHandler;
        IFileServiceBuilder AddAuthenticationScheme(string scheme);
        IFileServiceBuilder AddAuthenticationSchemes(IEnumerable<string> schemes);
    }
}
