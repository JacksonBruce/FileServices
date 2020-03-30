using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ufangx.FileServices.Models;

namespace Ufangx.FileServices.Abstractions
{
    public interface IFileServiceProvider
    {

        IEnumerable<string> AuthenticationSchemes { get; }
        /// <summary>
        /// 
        /// </summary>
        string DefaultSchemeName { get; }
        /// <summary>
        /// 生成文件名称
        /// </summary>
        /// <param name="originName"></param>
        /// <param name="directory"></param>
        /// <returns></returns>
        Task<string> GenerateFileName(string originName, string schemeName, string directory = null);
        FileValidateResult Validate(string schemeName, string fileName,long fileSize);
        IFileHandler GetHandler(string schemeName);
        string GetStoreDirectory(string schemeName);
        IFileService GetFileService();
        IResumableService GetResumableService();
        IEnumerable<FileServiceScheme> GetSchemes();


    }
}
