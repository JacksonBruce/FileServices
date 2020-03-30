using System;

namespace Ufangx.FileServices.Abstractions
{
    /// <summary>
    /// 续传信息
    /// </summary>
    public interface IResumableInfo
    {
        /// <summary>
        /// 数据块总数
        /// </summary>
        long BlobCount { get; }
        /// <summary>
        /// 已经处理的数据块总数
        /// </summary>
        long BlobIndex { get; set; }
        /// <summary>
        /// 数据块的大小
        /// </summary>
        long BlobSize { get; }
        /// <summary>
        /// 创建时间
        /// </summary>
        DateTime CreateDate { get; }
        /// <summary>
        /// 文件名称
        /// </summary>
        string FileName { get; }
        /// <summary>
        /// 文件大小
        /// </summary>
        long FileSize { get; }
        /// <summary>
        /// 文件类型
        /// </summary>
        string FileType { get; }
        /// <summary>
        /// 唯一键
        /// </summary>
        string Key { get; }
        /// <summary>
        /// 存储文件名
        /// </summary>
        string StoreName { get; }
    }
}