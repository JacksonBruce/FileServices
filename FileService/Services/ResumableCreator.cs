using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ufangx.FileServices.Abstractions;
using Ufangx.FileServices.Models;

namespace Ufangx.FileServices.Services
{
    public class ResumableCreator : Uploader, IResumableCreator
    {
        private readonly IResumableInfoService resumableInfoService;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="resumableInfoService"></param>
        /// <param name="fileService"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="contextAccessor"></param>
        public ResumableCreator(IResumableInfoService resumableInfoService, IFileService fileService, IFileServiceProvider serviceProvider, IHttpContextAccessor contextAccessor) 
            : base(fileService, serviceProvider, contextAccessor)
        {
            this.resumableInfoService = resumableInfoService;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileSize"></param>
        /// <param name="fileType"></param>
        /// <param name="blobCount"></param>
        /// <param name="blobSize"></param>
        /// <param name="schemeName"></param>
        /// <param name="dir"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<IResumableInfo> Create(string fileName, long fileSize, string fileType, long blobCount, int blobSize, string schemeName = null,string dir=null,string name=null)
        {
            return await resumableInfoService.Create(await GetStoreFileName(GetScheme(schemeName), fileName, dir, name),
               fileName,
               fileSize,
               fileType,
               blobCount,
               blobSize);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<IResumableInfo> Get(string key)
        {
            return await resumableInfoService.Get(key);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileSize"></param>
        /// <param name="schemeName"></param>
        /// <returns></returns>
        public FileValidateResult Validate(string fileName, long fileSize, string schemeName)
        {
            return Validate(fileName, fileSize, GetScheme(schemeName));
        }
    }
}
