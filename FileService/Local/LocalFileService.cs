using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ufangx.FileServices.Abstractions;

namespace Ufangx.FileServices.Local
{
    public class LocalFileService : IFileService
    {
        private readonly LocalFileOption option;

        public LocalFileService(IOptions<LocalFileOption> option) {

            this.option = option.Value; // option??new LocalFileOption();
            if (string.IsNullOrWhiteSpace(this.option.StorageRootDir)) {
                this.option.StorageRootDir = AppContext.BaseDirectory;
            }
        }
       protected string physicalPath(string path) {

            return Path.Combine(option.StorageRootDir, path.Trim().Replace('\\', '/').TrimStart('/'));
        }
        protected bool CreateDirIfNonexistence(string path) {
         
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }
            return true;
        }
        public async Task<bool> Delete(string path, CancellationToken token = default(CancellationToken))
        { 
            string p = physicalPath(path);
            if (File.Exists(p))
            {
                File.Delete(p);
            }
            return await Task.FromResult(true);
         
        }

        public async Task<bool> Exists(string path, CancellationToken token = default(CancellationToken))
        {
            return await Task.FromResult(File.Exists(physicalPath(path)));
        }

        public async Task<Stream> GetStream(string path, CancellationToken token = default(CancellationToken))
        {
            var p = physicalPath(path);
            if (!File.Exists(p)) return null;
            return await Task.FromResult(new FileStream(p, FileMode.Open, FileAccess.Read,FileShare.ReadWrite| FileShare.Delete));

        }

        public async Task<byte[]> GetFileData(string path, CancellationToken token = default(CancellationToken))
        {
            var p = physicalPath(path);
            if (!File.Exists(p)) return null;
#if netstandard20
   return await Task.FromResult(File.ReadAllBytes(p));
#else
        return await File.ReadAllBytesAsync(p, token);     
#endif
          
        }

        public async Task<bool> Save(string path, Stream stream, CancellationToken token = default(CancellationToken))
        {
            var p = physicalPath(path);
            if (CreateDirIfNonexistence(p))
            {
                if (stream.CanSeek && stream.Position > 0) { stream.Position = 0; }
                using (var fs = new FileStream(p, FileMode.Create))
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
                
              
                return true;
            }
            return false;
        }

        public async Task<bool> Save(string path, byte[] data, CancellationToken token = default(CancellationToken))
        {
            var p = physicalPath(path);
            if (CreateDirIfNonexistence(p))
            {
#if netstandard20
                File.WriteAllBytes(p, data);
                await Task.Yield();
#else
                await File.WriteAllBytesAsync(p, data, token);

#endif
                return true;
            }
            return false;
        }
        public async Task Move(string sourceFileName,string destFileName) {
            sourceFileName = physicalPath(sourceFileName);
            destFileName = physicalPath(destFileName);
            File.Move(sourceFileName, destFileName);
            await Task.CompletedTask;
        }
        public async Task<DateTime> GetModifyDate(string path, CancellationToken token = default(CancellationToken))
        {
            return await Task.FromResult(File.GetLastWriteTime(physicalPath(path)));
        }

        public async Task<bool> Append(string path, Stream stream, CancellationToken token = default(CancellationToken))
        {
            var p = physicalPath(path);
            if (CreateDirIfNonexistence(p))
            {
                if (stream.CanSeek && stream.Position > 0) { stream.Position = 0; }
                using (var fs = new FileStream(p, FileMode.Append,FileAccess.Write,FileShare.Read))
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
                return true;
            }

            return false;
        }

        public async Task<bool> Append(string path, byte[] data, CancellationToken token = default(CancellationToken))
        {
            var p = physicalPath(path);
            if (CreateDirIfNonexistence(p))
            {
                using (var fs = new FileStream(p, FileMode.Append, FileAccess.Write, FileShare.Read))
                {
                    await fs.WriteAsync(data, 0, data.Length, token);
                    await fs.FlushAsync(token);
                    fs.Close();
                }
                return true;
            }

            return false;
        }
    }
}
