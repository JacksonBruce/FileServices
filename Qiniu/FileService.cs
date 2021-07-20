using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Qiniu.Http;
using Qiniu.Storage;
using Qiniu.Util;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Ufangx.FileServices.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Qiniu
{
    public class FileService : IFileService
    {
        protected readonly FileServiceOptions options;
        protected readonly Mac mac;
        protected readonly Config config;
        protected readonly IHttpContextAccessor contextAccessor;

        public FileService(IOptions<FileServiceOptions> options, IHttpContextAccessor contextAccessor)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            this.options = options.Value;
            mac = new Mac(this.options.AccessKey, this.options.SecretKey);
            config = new Config();
            config.UseHttps = this.options.UseHttps;
            config.UseCdnDomains = this.options.UseCdnDomains;
            config.ChunkSize = this.options.ChunkUnit;
            if (this.options.PutThreshold.HasValue)
            {
                config.PutThreshold = this.options.PutThreshold.Value;
            }
            if (this.options.MaxRetryTimes.HasValue)
            {
                config.MaxRetryTimes = this.options.MaxRetryTimes.Value;
            }
            if (!string.IsNullOrWhiteSpace(this.options.Zone))
            {
                switch (this.options.Zone.ToLower())
                {
                    case "zonecneast":
                       config.Zone = Zone.ZoneCnEast;
                         //config.Zone = Zone.ZONE_CN_East;
                        break;
                    case "zonecnnorth":
                       config.Zone = Zone.ZoneCnNorth;
                         //config.Zone = Zone.ZONE_CN_North;
                        break;
                    case "zonecnsouth":
                        config.Zone = Zone.ZoneCnSouth;
                        //config.Zone = Zone.ZONE_CN_South;
                        break;
                    case "zoneusnorth":
                        config.Zone = Zone.ZoneUsNorth;
                        //config.Zone = Zone.ZONE_US_North;
                        break;
                    case "zoneassingapore":
                        config.Zone = Zone.ZoneAsSingapore;
                        //config.Zone = Zone.ZONE_AS_Singapore;
                        break;
                    default:
                        config.Zone = Newtonsoft.Json.JsonConvert.DeserializeObject<Zone>(this.options.Zone);
                        break;
                }
            }

            this.contextAccessor = contextAccessor;
        }

        public Task<bool> Append(string path, Stream stream, CancellationToken token = default)
        {
            //var key = GetSaveKey(path);
            //if (stream.CanSeek && stream.Position > 0) { stream.Position = 0; }
            //ResumableUploader resumableUploader = new ResumableUploader(config);
            //var result = await
            //    resumableUploader.UploadStream(stream, key, GetToken(key), null);
            //    //Task.FromResult(resumableUploader.UploadStream(stream, key, GetToken(key), null));
            //return result.Code == (int)HttpCode.OK;
            throw new NotSupportedException();
        }

        public async Task<bool> Append(string path, byte[] data, CancellationToken token = default)
        => await Append(path, new MemoryStream(data), token);

        public async Task<bool> Delete(string path, CancellationToken token = default)
       => (await GetBucketManager().Delete(options.Bucket,await GetSaveKey(path))).Code == (int)HttpCode.OK;
        //(await Task.FromResult(GetBucketManager().Delete(options.Bucket, GetSaveKey(path)))).Code == (int)HttpCode.OK;

        public async Task<bool> Exists(string path, CancellationToken token = default)
        {
            var result = await GetInfo(path);
            return result.Code == (int)HttpCode.OK; 
        }
        async Task<string> GetDownloadUrl(string path)
        {
            var key =await GetSaveKey(path);
            string baseUrl = options.Domain.Trim();
            baseUrl = baseUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || baseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ? baseUrl :
                "http://" + baseUrl;
            var privateUrl = DownloadManager.CreatePrivateUrl(mac, baseUrl, key, 3600);
          
            return privateUrl;
        }
        public async Task<byte[]> GetFileData(string path, CancellationToken token = default)
        {
            var url =await GetDownloadUrl(path);
            HttpClient client = new HttpClient();
            return await client.GetByteArrayAsync(url);

        }
        async Task<StatResult> GetInfo(string path) => await 
            GetBucketManager().Stat(options.Bucket,await GetSaveKey(path));
            //Task.FromResult(GetBucketManager().Stat(options.Bucket, GetSaveKey(path)));
        public async Task<DateTime> GetModifyDate(string path, CancellationToken token = default)
        {
            var result = await GetInfo(path);
            if (result.Code == (int)HttpCode.OK)
            {
                return new DateTime(result.Result.PutTime);
            }
            throw new Exception(result.Text);
        }
        public async Task<Stream> GetStream(string path, CancellationToken token = default)
        {
            var url =await GetDownloadUrl(path);
            HttpClient client = new HttpClient();
           return await client.GetStreamAsync(url);

         
        }
        BucketManager GetBucketManager() => new BucketManager(mac, config);
        public async Task Move(string sourceFileName, string destFileName)
        {
            BucketManager bucket = GetBucketManager();
            //var result = await Task.FromResult(bucket.Move(options.Bucket, GetSaveKey(sourceFileName), options.Bucket, GetSaveKey(destFileName)));
            var result = await bucket.Move(options.Bucket,await GetSaveKey(sourceFileName), options.Bucket,await GetSaveKey(destFileName));
            if (result.Code != (int)HttpCode.OK)
            {
                throw new Exception(result.Text);
            }
        }
        Task<string> GetSaveKey(string path)
        {
            return contextAccessor.GetSaveKey(options.BasePath, path);
        }
        string GetToken(string savekey)
        {
            return Utils.GetToken(savekey, options.AccessKey, options.SecretKey, options.Bucket);
            //Mac mac = new Mac(options.AccessKey, options.SecretKey);
            //PutPolicy putPolicy = new PutPolicy();
            //// 如果需要设置为"覆盖"上传(如果云端已有同名文件则覆盖)，请使用 SCOPE = "BUCKET:KEY"
            //putPolicy.Scope = $"{options.Bucket}:{savekey}";
            ////putPolicy.Scope = options.Bucket;
            //// 上传策略有效期(对应于生成的凭证的有效期)          
            //putPolicy.SetExpires(3600);
            //// 上传到云端多少天后自动删除该文件，如果不设置（即保持默认默认）则不删除
            ////putPolicy.DeleteAfterDays = 1;
            //// 生成上传凭证，参见
            //// https://developer.qiniu.com/kodo/manual/upload-token            
            //string jstr = putPolicy.ToJsonString();
            //var token = Auth.CreateUploadToken(mac, jstr);
            //return token;
        }
        UploadManager GetUploadManager()
        {
            return new UploadManager(config);
        }
        public async Task<bool> Save(string path, Stream stream, CancellationToken token = default)
        {
            if (stream == null || stream.Length == 0) return false;
            if (stream.CanSeek && stream.Position > 0) { stream.Position = 0; }
            byte[] data = new byte[stream.Length];
            await stream.ReadAsync(data, 0, data.Length);
            return await Save(path, data, token);
            //不能用UploadStream方法，因为方法里面会释放掉数据流，数据量在作用域内释放会导致很多问题
        }

        public async Task<bool> Save(string path, byte[] data, CancellationToken token = default)
        {
            var key =await GetSaveKey(path);
            var uploadManager = GetUploadManager();
            //var result = await Task.FromResult(uploadManager.UploadData(data, key, GetToken(key), null));
            var result = await uploadManager.UploadData(data, key, GetToken(key), null);
            return result.Code == (int)HttpCode.OK;
        }
    }
}
