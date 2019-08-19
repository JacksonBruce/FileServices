using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Html5Uploader
{
    public class ResumableInfo
    {
        public ResumableInfo(string FileName
            , string FileType
            , string StorePath
            , long FileSize
            , long BlobSize
            , long BlobCount
            , Guid? Key=null
            , long BlobIndex=0
            , DateTime? CreateDate=null)
        {
            this.StorePath = StorePath;
            this.Key = Key ?? Guid.NewGuid();
            this.FileName = FileName;
            this.FileType = FileType;
            this.FileSize = FileSize;
            this.BlobSize = BlobSize;
            this.BlobCount = BlobCount;
            this.BlobIndex = BlobIndex;
            this.CreateDate = CreateDate ?? DateTime.Now;
        }
        public Guid Key { get;protected set; }
        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; protected set; }
        /// <summary>
        /// 文件类型
        /// </summary>
        public string FileType { get; protected set; }
        /// <summary>
        /// 存储位置
        /// </summary>
        public string StorePath { get; set; }
        /// <summary>
        /// 文件大小
        /// </summary>
        public long FileSize { get; protected set; }
        /// <summary>
        /// 切片大小
        /// </summary>
        public long BlobSize { get; protected set; }
        /// <summary>
        /// 切片总数
        /// </summary>
        public long BlobCount { get; protected set; }
        /// <summary>
        /// 已经完成的切片索引
        /// </summary>
        public long BlobIndex { get; set; }
        /// <summary>
        /// 创建的时间
        /// </summary>
        public DateTime CreateDate { get; protected set; }
    }
}
