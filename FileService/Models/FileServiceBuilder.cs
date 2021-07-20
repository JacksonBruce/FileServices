using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Ufangx.FileServices.Abstractions;
using Ufangx.FileServices.Models;

namespace Ufangx.FileServices.Models
{
    public class FileServiceBuilder : IFileServiceBuilder
    {
        public IServiceCollection Services { get; set; }

        public IFileServiceBuilder AddAuthenticationScheme(string scheme)
        {
            Services.Configure<FileServiceOptions>(opt => opt.AddAuthenticationScheme(scheme));
            return this;
        }

        public IFileServiceBuilder AddAuthenticationSchemes(IEnumerable<string> schemes)
        {
            Services.Configure<FileServiceOptions>(opt => opt.AddAuthenticationSchemes(schemes));
            return this;
        }

        public IFileServiceBuilder AddScheme(string name, Action<FileServiceScheme> configureBuilder)
        {
            Services.Configure<FileServiceOptions>(opt => opt.AddScheme(name, configureBuilder));
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="THandler"></typeparam>
        /// <param name="name"></param>
        /// <param name="storeDirectory"></param>
        /// <param name="supportExtensions"></param>
        /// <param name="LimitedSize"></param>
        /// <returns></returns>
        public IFileServiceBuilder AddScheme<THandler>(string name, string storeDirectory = null, IEnumerable<string> supportExtensions = null, long? LimitedSize = null) 
            where THandler :class, IFileHandler
        {
          
            Services.Configure<FileServiceOptions>(opt => opt.AddScheme<THandler>(name, storeDirectory, supportExtensions, LimitedSize));
            //Services.AddTransient<THandler>();//外面注入
            return this;
        }
    }
}
