using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ufangx.FileServices.Abstractions;
using Ufangx.FileServices.Models;

namespace Ufangx.FileServices.Services
{
    public class Uploader : IUploader
    {
        private readonly IFileService fileService;
        private readonly IFileServiceProvider serviceProvider;
        private readonly IHttpContextAccessor contextAccessor;

        public Uploader(IFileService fileService,IFileServiceProvider serviceProvider,IHttpContextAccessor contextAccessor)
        {
            this.fileService = fileService;
            this.serviceProvider = serviceProvider;
            this.contextAccessor = contextAccessor;
        }

        protected IFileService FileService => fileService;

        protected IFileServiceProvider ServiceProvider => serviceProvider;

        protected IHttpContextAccessor ContextAccessor => contextAccessor;
        protected async Task<string> GenerateFileName(FileServiceScheme scheme, string originName, string directory = null)
        {
            if (string.IsNullOrWhiteSpace(originName))
            {
                throw new ArgumentException("message", nameof(originName));
            }
            var fileNameRuleOptions = serviceProvider.GetNameRuleOptions();
            FileNameRule nameRule = fileNameRuleOptions?.Rule ?? FileNameRule.Ascending;
            if (nameRule == FileNameRule.Custom && fileNameRuleOptions?.Custom == null)
            {
                nameRule = FileNameRule.Ascending;
            }
            if (directory == null) { directory = string.Empty; }
            directory = Path.Combine(scheme?.StoreDirectory ?? string.Empty, directory).Replace('\\', '/');
            string fileName;
            switch (nameRule)
            {
                case FileNameRule.Ascending:
                    fileName = Path.Combine(directory, originName).Replace('\\', '/');
                    int index = 0;
                    while (await fileService.Exists(fileName))
                    {
                        fileName = Path.Combine(directory, $"{Path.GetFileNameWithoutExtension(originName)}({++index}){Path.GetExtension(originName)}").Replace('\\', '/');
                    }
                    break;
                case FileNameRule.Date:
                    fileName = Path.Combine(directory, string.Format(fileNameRuleOptions?.Format ?? "{0:yyyyMMddHHmmss}", DateTime.Now) + Path.GetExtension(originName)).Replace('\\', '/');
                    break;
                case FileNameRule.Custom:
                    fileName = Path.Combine(directory, fileNameRuleOptions.Custom(originName)).Replace('\\', '/');
                    break;
                default:
                    fileName = Path.Combine(directory, originName).Replace('\\', '/');
                    break;
            }
            return fileName.Replace('\\', '/');

        }
        protected async Task<string> GetStoreFileName(FileServiceScheme scheme, string originName, string dir, string fileName)
        {
            //如果客户端指定了
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                return Path.Combine(scheme?.StoreDirectory ?? string.Empty, dir, fileName).Replace('\\', '/');
            }
            //否则生成文件名称
            return await GenerateFileName(scheme, originName, dir);
        }
        public async Task<object> Handle(IFormFileCollection files, string schemeName = null)
        {
            List<object> results = new List<object>();
            foreach (var file in files)
            {
                var result = await Handle(file, schemeName);
                if (result != null){ results.Add(result); }
            }
            return results;
        }
        protected async Task<object> SchemeHandler(FileServiceScheme scheme, string path, string fileType, long fileSize)
        {
            if (scheme?.HandlerType == null) return path;
            if (!typeof(IFileHandler).IsAssignableFrom(scheme.HandlerType))
            {
                throw new Exception($"类型“{scheme.HandlerType.FullName}”没有实现“{typeof(IFileHandler).FullName}”接口");
            }
            var handler = contextAccessor.HttpContext.RequestServices.GetService(scheme.HandlerType) as IFileHandler;
            if (handler == null) return path;
            return await handler.Handler(path, fileType, fileSize);

        }
        async Task<object> Handle(IFormFile file, FileServiceScheme scheme, string dir, string name)
        {
            string path = await GetStoreFileName(scheme, file.FileName, dir, name);
            if (await fileService.Save(path, file.OpenReadStream()))
            {
                return await SchemeHandler(scheme, path, file.ContentType, file.Length);
            }
            return null;
        }
       protected FileServiceScheme GetScheme(string schemeName) {
            if (string.IsNullOrWhiteSpace(schemeName))
            {
                schemeName = serviceProvider.DefaultSchemeName;
            }
            if (string.IsNullOrWhiteSpace(schemeName))
            {
                return null;
            }
            return serviceProvider.GetScheme(schemeName);
        }
        public Task<object> Handle(IFormFile file, string schemeName = null, string dir = null, string name = null)
        => Handle(file, GetScheme(schemeName), dir, name);
      protected  FileValidateResult Validate(string fileName, long fileSize, FileServiceScheme scheme) {
            if (scheme == null) return FileValidateResult.Successfully;
            if (scheme.LimitedSize.HasValue && scheme.LimitedSize.Value < fileSize)
            {
                return FileValidateResult.Limited;
            }
            string ext = Path.GetExtension(fileName);
            return
                   scheme.SupportExtensions == null
                || scheme.SupportExtensions.Count() == 0
                || scheme.SupportExtensions.Any(e => string.Equals(ext, e, StringComparison.OrdinalIgnoreCase))
                ? FileValidateResult.Successfully : FileValidateResult.Invalid;
        }

        FileValidateResult Validate(IFormFile file, FileServiceScheme scheme)
        => Validate(file.FileName, file.Length, scheme);

        public FileValidateResult Validate(IFormFile file, string schemeName = null)
        => Validate(file, GetScheme(schemeName));
        public FileValidateResult Validate(IFormFileCollection files, string schemeName = null)
        {
            var scheme = GetScheme(schemeName);
            foreach (var file in files)
            {
                var result = Validate(file, scheme);
                if (result != FileValidateResult.Successfully) {
                    return result;
                }
            }
            return FileValidateResult.Successfully;
        }
    }
}
