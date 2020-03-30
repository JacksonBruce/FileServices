using Qiniu.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Qiniu
{
    public class FileServiceOptions
    {
        public void CopyFrom(FileServiceOptions options)
        {
            AccessKey = options.AccessKey;
            SecretKey = options.SecretKey;
            Bucket = options.Bucket;
            BasePath = options.BasePath;
            Domain = options.Domain;
            ChunkUnit = options.ChunkUnit;
            Zone = options.Zone;
            UseCdnDomains = options.UseCdnDomains;
            UseHttps = options.UseHttps;
            PutThreshold = options.PutThreshold;
            MaxRetryTimes = options.MaxRetryTimes;


        }
        /// <summary>
        ///  Access Key 
        /// </summary>
        public string AccessKey { get; set; }
        /// <summary>
        /// Secret Key
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        /// 空间名称
        /// </summary>
        public string Bucket { get; set; }

        /// <summary>
        /// 基本目录
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// 域名
        /// </summary>
        public string Domain { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ChunkUnit ChunkUnit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Zone { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool UseHttps { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool UseCdnDomains { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? PutThreshold { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? MaxRetryTimes { get; set; }
    }
}
