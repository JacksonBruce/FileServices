using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ufangx.FileServices.Abstractions;
using Ufangx.FileServices.Models;

namespace Ufangx.FileServices.Local
{
    public class LocalResumableService : LocalFileService, IResumableService
    {
        private readonly IResumableInfoService resumableInfoService;
        public LocalResumableService(IResumableInfoService resumableInfoService, IOptions<LocalFileOption> option, IHttpContextAccessor httpContextAccessor) :base(option, httpContextAccessor) {
            this.resumableInfoService = resumableInfoService;
        }
        FileStream GetFileStream(string path) {
            return new FileStream(path, FileMode.Create, FileAccess.Write);
        }
        string GetTempDir(string path, string key)
        {
            return Path.Combine(Path.GetDirectoryName(path), $"_tmp{key}").Replace('\\','/');
        }
        bool CheckFiles(string dir, long count) {
            //Console.WriteLine("正在检查文件。。。");
            //Stopwatch sw = Stopwatch.StartNew();
            for (long i = 0; i < count; i++)
            {
                string path = Path.Combine(dir, $"{i}").Replace('\\', '/');
                if (!File.Exists(path)) { return false; }
            }
            //sw.Stop();
            //Console.WriteLine($"检查{count}个文件，用时：{sw.Elapsed.TotalMilliseconds}毫秒");
            return true;
        }
        async Task<byte[]> GetBlob(string fileName, CancellationToken token = default)
        {
#if netstandard20
            return await Task.FromResult(File.ReadAllBytes(fileName));
#else
            return await File.ReadAllBytesAsync(fileName, token);     
#endif
        }
        async void MakeFile(string dir,string path, long count, CancellationToken token = default) {
            try
            {
                //Console.WriteLine($"开始创建“{path}”文件。。。。");
                //Stopwatch sw = Stopwatch.StartNew();
                using (var fs = GetFileStream(path))
                {
                    for (long i = 0; i < count; i++)
                    { 
                        var blob = await GetBlob(Path.Combine(dir, $"{i}").Replace('\\','/'), token);
                        await fs.WriteAsync(blob, 0, blob.Length, token);
                    }
                    await fs.FlushAsync(token);
                    fs.Close();
                }
                //Directory.Delete(dir, true);
                //sw.Stop();
                //Console.WriteLine($"用时：{sw.Elapsed.TotalMilliseconds}毫秒，创建“{path}”文件。");
            }
            catch(Exception ex) {
                if (File.Exists(path)) { File.Delete(path); }
                //Console.WriteLine("error:" + ex.Message);
            }
            finally
            {
                Directory.Delete(dir, true);
            }



        }
        public async Task<bool> SaveBlob(Blob blob, Func<IResumableInfo,bool, Task> finished = null, CancellationToken token = default)
        {
            if (blob is null)
            {
                throw new ArgumentNullException(nameof(blob));
            }

            var info = await resumableInfoService.Get(blob.ResumableKey);
            if (info == null) {
                throw new Exception($"无效的{nameof(blob.ResumableKey)}");
            }
            var p =await physicalPath(info.StoreName);
            string tempdir = GetTempDir(p,info.Key);
            var tmp = Path.Combine(tempdir, $"{blob.BlobIndex}").Replace('\\','/');
            if (CreateDirIfNonexistence(tmp))
            {
                var stream = blob.Data;
                using (var fs = GetFileStream(tmp))
                {
                    int len = 10485760;
                    byte[] buffer = new byte[len];
                    int readed;
                    while ((readed = await stream.ReadAsync(buffer, 0, len, token)) > 0)
                    {
                        await fs.WriteAsync(buffer, 0, Math.Min(readed, len), token);
                        await fs.FlushAsync(token);
                    }
                    fs.Close();
                }
                if (blob.BlobIndex>= info.BlobCount-1)
                {
                    bool ok = false;
                    if (CheckFiles(tempdir, info.BlobCount)) {
                
                        MakeFile(tempdir, p, info.BlobCount);
                        //Console.WriteLine("后台线程创建文件。。。");
                        ok = true;
                    }
                    if (finished != null) { await finished(info,ok); }
                    await resumableInfoService.Delete(info);
                    return ok;
                }
                else {
                    info.BlobIndex++;
                    await resumableInfoService.Update(info);
                    return true;
                }
             
            }

            return false;




        }

        public async Task<bool> DeleteBlobs(string key, CancellationToken token = default)
        {
            var info = await resumableInfoService.Get(key);
            if (info == null)
            {
                throw new Exception($"无效的{nameof(key)}");
            }
         
            if (await resumableInfoService.Delete(info)) {
                string tempdir = GetTempDir(await physicalPath(info.StoreName), info.Key); 
                try
                {
                    Directory.Delete(tempdir, true);
                    return true;
                }
                catch { 
                  //删除文件失败写回续传信息
                    await resumableInfoService.Update(info);
                    throw;
                }
             
            }
            return false;

        }
    }
}
