using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Qiniu.Http;
using Qiniu.Storage;
using Qiniu.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ufangx.FileServices.Abstractions;
using Ufangx.FileServices.Models;

namespace Qiniu
{
    public class ResumableService : FileService, IResumableService
    {
        private readonly IResumableInfoService resumableInfoService;
        private readonly ILogger<ResumableService> logger;
        private readonly HttpManager _httpManager;

        public ResumableService(IResumableInfoService resumableInfoService,ILogger<ResumableService> logger, IOptions<FileServiceOptions> options, IHttpContextAccessor contextAccessor) : base(options, contextAccessor)
        {
            this.resumableInfoService = resumableInfoService;
            this.logger = logger;
            _httpManager = new HttpManager();
        }

        public async Task<bool> DeleteBlobs(string key, CancellationToken token = default)
        {
            var info = await resumableInfoService.Get(key);
            if (info == null)
            {
                throw new Exception($"无效的{nameof(key)}");
            }             
            return await resumableInfoService.Delete(info);
        }

        public async Task<bool> SaveBlob(Blob blob, Func<IResumableInfo,bool, Task> finished = null, CancellationToken token = default)
        {
            if (blob is null)
            {
                throw new ArgumentNullException(nameof(blob));
            }

            var info = (ResumableInfo)await resumableInfoService.Get(blob.ResumableKey);
            if (info == null)
            {
                throw new Exception($"无效的{nameof(blob.ResumableKey)}");
            }

            //get upload host
            var ak = UpToken.GetAccessKeyFromUpToken(info.UploadToken);
            var bucket = UpToken.GetBucketFromUpToken(info.UploadToken);
            if (ak == null || bucket == null)
            {
             
                return false;
            }
            int leng = (int)blob.Data.Length;
            byte[] blockBuffer = new byte[leng];
            await blob.Data.ReadAsync(blockBuffer, 0, leng);
            var uploadHost = await config.UpHost(ak, bucket);
            var url = $"{uploadHost}/mkblk/{leng}";
            var upTokenStr = $"UpToken {info.UploadToken}";
         
            var result = await _httpManager.PostDataAsync(url, blockBuffer, token: upTokenStr);
            if (result.Code == (int)HttpCode.OK)
            {
                var rc = JsonConvert.DeserializeObject<ResumeContext>(result.Text);
                if (rc.Crc32 > 0)
                {
                    var crc1 = rc.Crc32;
                    var crc2 = Crc32.CheckSumSlice(blockBuffer,0,leng);
                    if (crc1 != crc2)
                    {
                        result.RefCode = (int)HttpCode.USER_NEED_RETRY;
                        result.RefText += $" CRC32: remote={crc1}, local={crc2}\n";
                        logger.LogError(result.RefText);
                    }
                    else
                    {
                        //write the mkblk context
                        info.Contexts.Add(rc.Ctx);
                        info.ExpiredAt = rc.ExpiredAt;
                        if (blob.BlobIndex >= info.BlobCount - 1)
                        {
                            bool ok = false;
                            try
                            {
                           
                                if (await MakeFile(info))
                                {
                                    ok = true;
                                }
                            }
                            finally
                            {
                                await resumableInfoService.Delete(info);
                            }
                            if (finished != null) { await finished(info, ok); }
                            return ok;
                        }
                        else
                        {
                            info.BlobIndex++;
                            await resumableInfoService.Update(info);     
                            return true;
                        }
                  
                    }
                }
                else
                {
                    result.RefText += $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.ffff}] JSON Decode Error: text = {result.Text}";
                    result.RefCode = (int)HttpCode.USER_NEED_RETRY;
                }
            }
            else
            {
                result.RefCode = (int)HttpCode.USER_NEED_RETRY;
            }
            if (result.Code != (int)HttpCode.OK) { 
                logger.LogError($"提交数据块失败:{result.Text}，返回code:{result.Code}");
            }

            return false;
        }
        private async Task<bool> MakeFile(ResumableInfo info)
        {
            string key = await contextAccessor.GetSaveKey(options.BasePath, info.StoreName);
            string fileName = key;
            long size = info.FileSize;
            string upToken = info.UploadToken;
            IEnumerable<string> contexts = info.Contexts;

            var result = new HttpResult();

            try
            {
                var fnameStr = "fname";
                var mimeTypeStr = "";
                var keyStr = "";
                var paramStr = "";
                //check file name
                if (!string.IsNullOrEmpty(fileName))
                {
                    fnameStr = $"/fname/{Base64.UrlSafeBase64Encode(fileName)}";
                }

                //check mime type
                if (!string.IsNullOrEmpty(info.FileType))
                {
                    mimeTypeStr = $"/mimeType/{Base64.UrlSafeBase64Encode(info.FileType)}";
                }

                //check key
                if (!string.IsNullOrEmpty(key))
                {
                    keyStr = $"/key/{Base64.UrlSafeBase64Encode(key)}";
                }

                //check extra params
                //if (putExtra.Params != null && putExtra.Params.Count > 0)
                //{
                //    var sb = new StringBuilder();
                //    foreach (var kvp in putExtra.Params)
                //    {
                //        var k = kvp.Key;
                //        var v = kvp.Value;
                //        if (k.StartsWith("x:") && !string.IsNullOrEmpty(v))
                //        {
                //            sb.Append($"/{k}/{v}");
                //        }
                //    }

                //    paramStr = sb.ToString();
                //}

                //get upload host
                var ak = UpToken.GetAccessKeyFromUpToken(upToken);
                var bucket = UpToken.GetBucketFromUpToken(upToken);
                if (ak == null || bucket == null)
                {
                    logger.LogError($"Token“{upToken}”是无效的");
                    return false;
                }

                var uploadHost = await config.UpHost(ak, bucket);

                var url = $"{uploadHost}/mkfile/{size}{mimeTypeStr}{fnameStr}{keyStr}{paramStr}";
                var body = string.Join(",", contexts);
                var upTokenStr = $"UpToken {upToken}";
                result = await _httpManager.PostTextAsync(url, body, upTokenStr);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "创建文件失败");
                //var sb = new StringBuilder();
                //sb.Append($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.ffff}] mkfile Error: ");
                //var e = ex;
                //while (e != null)
                //{
                //    sb.Append(e.Message + " ");
                //    e = e.InnerException;
                //}

                //sb.AppendLine();

                //if (ex is QiniuException qex)
                //{
                //    result.Code = qex.HttpResult.Code;
                //    result.RefCode = qex.HttpResult.Code;
                //    result.Text = qex.HttpResult.Text;
                //    result.RefText += sb.ToString();
                //}
                //else
                //{
                //    result.RefCode = (int)HttpCode.USER_UNDEF;
                //    result.RefText += sb.ToString();
                //}
            }

            return result.Code == (int)HttpCode.OK;
        }

    }
}
