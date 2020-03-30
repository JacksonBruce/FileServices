using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ufangx.FileServices.Abstractions
{
    public interface IFileService
    {
        /// <summary>
        /// 获取文件流
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<Stream> GetStream(string path, CancellationToken token = default(CancellationToken));
        /// <summary>
        /// 获取文件数据
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<byte[]> GetFileData(string path, CancellationToken token = default(CancellationToken));
        /// <summary>
        /// 获取文件更新日期
        /// </summary>
        /// <param name="path"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<DateTime> GetModifyDate(string path, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<bool> Delete(string path, CancellationToken token = default(CancellationToken));
        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        Task<bool> Save(string path, Stream stream, CancellationToken token = default(CancellationToken));
        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<bool> Save(string path, byte[] data, CancellationToken token = default(CancellationToken));
        /// <summary>
        /// 保存文件，如果文件已经存在将追加数据
        /// </summary>
        /// <param name="path"></param>
        /// <param name="stream"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<bool> Append(string path, Stream stream, CancellationToken token = default(CancellationToken));
        /// <summary>
        /// 保存文件，如果文件已经存在将追加数据
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<bool> Append(string path, byte[] data, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// 文件是否存在
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<bool> Exists(string path, CancellationToken token = default(CancellationToken));
        /// <summary>
        /// 移动文件
        /// </summary>
        /// <param name="sourceFileName"></param>
        /// <param name="destFileName"></param>
        /// <returns></returns>
        Task Move(string sourceFileName, string destFileName);



    }
}
