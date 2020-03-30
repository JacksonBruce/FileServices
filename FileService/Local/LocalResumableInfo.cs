using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Ufangx.FileServices.Abstractions;

namespace Ufangx.FileServices.Local
{
    public class LocalResumableInfo : IResumableInfo
    {
        public string Key { get; set; }
        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 文件类型
        /// </summary>
        public string FileType { get; set; }
        /// <summary>
        /// 存储位置
        /// </summary>
        public string StoreName { get; set; }
        /// <summary>
        /// 文件大小
        /// </summary>
        public long FileSize { get; set; }
        /// <summary>
        /// 切片大小
        /// </summary>
        public long BlobSize { get; set; }
        /// <summary>
        /// 切片总数
        /// </summary>
        public long BlobCount { get; set; }
        /// <summary>
        /// 已经完成的切片索引
        /// </summary>
        public long BlobIndex { get; set; }
        /// <summary>
        /// 创建的时间
        /// </summary>
        public DateTime CreateDate { get; set; }



    }
}
