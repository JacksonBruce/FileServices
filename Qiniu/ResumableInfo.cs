using System;
using System.Collections.Generic;
using System.Text;
using Ufangx.FileServices.Abstractions;

namespace Qiniu
{
    public class ResumableInfo : IResumableInfo
    {
        public long BlobCount { get; set; }

        public long BlobIndex { get; set; }

        public long BlobSize { get; set; }

        public DateTime CreateDate { get; set; }

        public string FileName { get; set; }

        public long FileSize { get; set; }

        public string FileType { get; set; }

        public string Key { get; set; }

        public string StoreName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IList<string> Contexts { get; set; } = new List<string>();
        /// <summary>
        /// 上传票据
        /// </summary>
        public string UploadToken { get; set; }
        public long ExpiredAt { get; set; }
    }
}
