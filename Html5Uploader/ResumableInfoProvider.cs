using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Html5Uploader
{
    public abstract class ResumableInfoProvider:IDisposable
    {
        public abstract IEnumerable<ResumableInfo> ResumableInfos { get; }
        public abstract ResumableInfo GetResumableInfo(Guid key);
        public abstract ResumableInfo GetResumableInfo(string fileName, string fileType, long fileSize, long blobSize, long blobCount);
        public abstract void SaveResumableInfo(ResumableInfo info);
        public abstract void DeleteResumableInfo(Guid key);
        public abstract void Dispose();
    }
}
