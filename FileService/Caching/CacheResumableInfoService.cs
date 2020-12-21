using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Ufangx.FileServices.Abstractions;

namespace Ufangx.FileServices.Caching
{
    public class CacheResumableInfoService<TResumableInfo> : IResumableInfoService where TResumableInfo : IResumableInfo
    {
        protected readonly IDistributedCache cache;
        protected readonly ILogger<CacheResumableInfoService<TResumableInfo>> logger;
        protected readonly IHttpContextAccessor contextAccessor;

        public CacheResumableInfoService(IDistributedCache cache,ILogger<CacheResumableInfoService<TResumableInfo>> logger, IHttpContextAccessor contextAccessor)
        {
            this.cache = cache;
            this.logger = logger;
            this.contextAccessor = contextAccessor;
        }

        public virtual async Task<IResumableInfo> Create(string storeName, string fileName, long fileSize, string fileType, long blobCount, int blobSize)
        {
            if (string.IsNullOrWhiteSpace(storeName))
            {
                throw new ArgumentException("message", nameof(storeName));
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("message", nameof(fileName));
            }

            string cleanKey = $"StoreName={Path.GetDirectoryName(storeName).Replace('\\','/').ToLower()}&FileName={fileName.ToLower()}&FileType={fileType?.ToLower()}&FileSize={fileSize}&BlobSize={blobSize}&BlobCount={blobCount}&user={contextAccessor?.HttpContext?.User?.Identity?.Name?.ToLower()}";
            byte[] data;
            using (var md5 = MD5.Create())
            {
                data = md5.ComputeHash(Encoding.UTF8.GetBytes(cleanKey));
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString("X2"));
            }
            string key = sb.ToString();

            var info = await Get(key);
            if (info != null) return info;

            var json = JsonConvert.SerializeObject(new { StoreName = storeName, Key = key, FileType = fileType, FileSize = fileSize, FileName = fileName, CreateDate = DateTime.Now, BlobSize = blobSize, BlobIndex = 0, BlobCount = blobCount });

            return await Save(key, json) ? JsonConvert.DeserializeObject<TResumableInfo>(json) : default(TResumableInfo);
        }

        public virtual async Task<bool> Delete(IResumableInfo resumable)
        {
            try
            {
                var cachekey = GetCacheKey(resumable.Key);
                await cache.RemoveAsync(cachekey);
                return true;
            }
            catch (Exception ex) {
                logger.LogError(ex, "移除缓存失败");
            }
            return false;
        }

        public virtual async Task<IResumableInfo> Get(string key)
        {
            var data = await cache.GetStringAsync(GetCacheKey(key));
            if (data == null) return default(TResumableInfo);
            var info = JsonConvert.DeserializeObject<TResumableInfo>(data);
            return info;
        }
        protected virtual async Task<bool> Save(string key, object info) {
            try
            {
                string data;
                if (info is string json)
                {
                    data = json;
                }
                else {
                    data = JsonConvert.SerializeObject(info);
                }
                await cache.SetStringAsync(GetCacheKey(key), data);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "更新续传信息失败");
            }
            return false;
        }
        public async Task<bool> Update(IResumableInfo resumable)
        => await Save(resumable.Key, resumable);

        protected virtual string GetCacheKey(string key) {
            return $"{GetType().FullName}&key={key}&user={contextAccessor?.HttpContext?.User?.Identity?.Name}";
        }

    }
}
