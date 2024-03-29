﻿using Microsoft.AspNetCore.Http;
using Qiniu.Storage;
using Qiniu.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Ufangx.FileServices.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Qiniu
{
    internal static class Utils
    {
        static async Task<string> GetRootDir(IHttpContextAccessor contextAccessor,string basePath)
        {
            string root;
            var rootService = contextAccessor.HttpContext.RequestServices.GetService<IRootDirectory>();
            if (rootService == null || string.IsNullOrWhiteSpace(root = await rootService.GetRoot())) return basePath;
            return Path.Combine(basePath, root.Trim().Replace('\\', '/').TrimStart('/')).Replace('\\', '/');
        }

        public static async Task<string> GetSaveKey(this IHttpContextAccessor contextAccessor, string basePath, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("message", nameof(path));
            }
            var root =await GetRootDir(contextAccessor, basePath);
            return Path.Combine(root, path.TrimStart('/', '\\')).Trim().ToLower().Replace('\\', '/');
        }
        public static string GetToken(string savekey,string ak,string sk,string bucket,int expires=3600)
        {
            Mac mac = new Mac(ak, sk);
            PutPolicy putPolicy = new PutPolicy();
            // 如果需要设置为"覆盖"上传(如果云端已有同名文件则覆盖)，请使用 SCOPE = "BUCKET:KEY"
            putPolicy.Scope = $"{bucket}:{savekey}";
            //putPolicy.Scope = options.Bucket;
            // 上传策略有效期(对应于生成的凭证的有效期)          
            putPolicy.SetExpires(expires);
            // 上传到云端多少天后自动删除该文件，如果不设置（即保持默认默认）则不删除
            //putPolicy.DeleteAfterDays = 1;
            // 生成上传凭证，参见
            // https://developer.qiniu.com/kodo/manual/upload-token            
            string jstr = putPolicy.ToJsonString();
            var token = Auth.CreateUploadToken(mac, jstr);
            return token;
        }
    }
}
