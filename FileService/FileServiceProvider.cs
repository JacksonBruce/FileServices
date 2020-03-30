using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using Ufangx.FileServices.Abstractions;
using Ufangx.FileServices.Models;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.DependencyInjection;

namespace Ufangx.FileServices
{
    public class FileServiceProvider : IFileServiceProvider,IDisposable
    {
        private readonly FileServiceOptions options;
        private readonly IServiceProvider serviceProvider;
        private readonly IServiceScope serviceScope;

        public FileServiceProvider(IOptions<FileServiceOptions> options, IServiceProvider serviceProvider)
        {
            this.options = options.Value;
            this.serviceScope = serviceProvider.CreateScope();
            this.serviceProvider = serviceScope.ServiceProvider;
        }

        public IEnumerable<string> AuthenticationSchemes => options.AuthenticationSchemes;

        public string DefaultSchemeName => options.DefaultScheme;

        FileServiceScheme GetScheme(string name) {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("message", nameof(name));
            }
            var scheme = options.SchemeMap[name];
            if (scheme == null)
            {
                throw new Exception($"无效的文件服务方案名称“{name}”");
            }
            return scheme;
        }
        public IFileHandler GetHandler(string schemeName)
        {
            if (string.IsNullOrWhiteSpace(schemeName)) return null;
            var scheme = GetScheme(schemeName);
            if (scheme.HandlerType == null) return null;
            if (!typeof(IFileHandler).IsAssignableFrom(scheme.HandlerType)) {
                throw new Exception($"类型“{scheme.HandlerType.FullName}”没有实现“{typeof(IFileHandler).FullName}”接口");
            }
            return serviceProvider.GetService(scheme.HandlerType) as IFileHandler;



        }

        public string GetStoreDirectory(string schemeName)
        => string.IsNullOrWhiteSpace(schemeName) ? string.Empty : GetScheme(schemeName).StoreDirectory;

        public async Task<string> GenerateFileName(string originName, string schemeName, string directory = null)
        {
            if (string.IsNullOrWhiteSpace(originName))
            {
                throw new ArgumentException("message", nameof(originName));
            }

            FileNameRule nameRule = options?.RuleOptions?.Rule ?? FileNameRule.Ascending;
            if (nameRule == FileNameRule.Custom && options?.RuleOptions?.Custom==null) {
                nameRule = FileNameRule.Ascending;
            }
            if (directory == null) { directory = string.Empty; }
            directory = Path.Combine(GetStoreDirectory(schemeName), directory);
            string fileName;
            switch (nameRule)
            {
                case FileNameRule.Ascending:
                    fileName = Path.Combine(directory, originName);
                    int index = 0;
                    var fileService = GetFileService();
                    while (await fileService.Exists(fileName))
                    {
                        fileName = Path.Combine(directory, $"{Path.GetFileNameWithoutExtension(originName)}({++index}){Path.GetExtension(originName)}");
                    }
                    break;
                case FileNameRule.Date:
                    fileName = Path.Combine(directory, string.Format(options?.RuleOptions?.Format ?? "{0:yyyyMMddHHmmss}", DateTime.Now) + Path.GetExtension(originName));
                    break;
                case FileNameRule.Custom:
                    fileName = Path.Combine(directory, options.RuleOptions.Custom(originName));
                    break;
                default:
                    fileName = Path.Combine(directory, originName);
                    break;
            }
            return fileName.Replace('\\', '/');

        }

        public FileValidateResult Validate(string schemeName, string fileName,long fileSize)
        {
            if (string.IsNullOrWhiteSpace(schemeName)) {
                return FileValidateResult.Successfully;
            }
            var scheme = GetScheme(schemeName);
            if (scheme.LimitedSize.HasValue && scheme.LimitedSize.Value < fileSize) {
                return FileValidateResult.Limited;
            }
            
            string ext = Path.GetExtension(fileName);
            return 
                   scheme.SupportExtensions==null 
                || scheme.SupportExtensions.Count()==0 
                || scheme.SupportExtensions.Any(e => string.Equals(ext, e, StringComparison.OrdinalIgnoreCase))
                ? FileValidateResult.Successfully : FileValidateResult.Invalid;
          

        }

        public IFileService GetFileService()
        {
            return serviceProvider.GetService<IFileService>();
        }

        public IResumableService GetResumableService()
        {
            return serviceProvider.GetService<IResumableService>();
        }

        public void Dispose()
        {
            serviceScope.Dispose();
        }

        public IEnumerable<FileServiceScheme> GetSchemes()
        {
            return options.Schemes;
        }
    }
}
