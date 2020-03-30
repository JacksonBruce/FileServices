using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qiniu.Storage;
using Qiniu.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ufangx.FileServices.Abstractions;
using Ufangx.FileServices.Caching;

namespace Qiniu 
{
    public class QiniuResumableInfoService : CacheResumableInfoService<ResumableInfo>
    {
        private readonly FileServiceOptions options;

        public QiniuResumableInfoService(IOptions<FileServiceOptions> options, IDistributedCache cache, ILogger<CacheResumableInfoService<ResumableInfo>> logger, IHttpContextAccessor contextAccessor) : base(cache, logger, contextAccessor)
        {
            this.options = options.Value;
        }
     
        public override async Task<IResumableInfo> Create(string storeName, string fileName, long fileSize, string fileType, long blobCount, int blobSize)
        {
            var info = (ResumableInfo)(await base.Create(storeName, fileName, fileSize, fileType, blobCount, blobSize));
            if (info.ExpiredAt!=0 && UnixTimestamp.IsContextExpired(info.ExpiredAt))
            {
                await Delete(info);
                info = (ResumableInfo)(await base.Create(storeName, fileName, fileSize, fileType, blobCount, blobSize));
            }
            if (string.IsNullOrWhiteSpace(info.UploadToken)) {
                info.UploadToken = Utils.GetToken(Utils.GetSaveKey(options.BasePath, info.StoreName), options.AccessKey, options.SecretKey, options.Bucket);
                await Update(info);
            }
            return info;
        }
    }
}
