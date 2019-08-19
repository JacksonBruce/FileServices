using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Html5Uploader
{
    public class FileUploadComplete
    {

        public FileUploadComplete(string FilePath, long Size, string Message, Exception error = null)
        { 
            this.FilePath = FilePath;
            this.Message = Message;
            this.Error = error;
            this.Size = Size;
        }
        public FileUploadComplete(string FilePath,long Size, bool Sliced = false, long BlobIndex = 0, long BlobCount = 1, string Message = null, Exception error = null)
            : this(FilePath, Size, Message, error)
        {
            this.Sliced = Sliced;
            this.BlobCount = BlobCount;
            this.BlobIndex = BlobIndex;
        }
        public long Size { get; private set; }
        public bool Sliced { get; private set; }
        public long BlobIndex { get; private set; }
        public long BlobCount { get; private set; }
        public string FilePath { get; private set; }
        string _fileName;
        public string FileName
        {
            get
            {
                if (_fileName == null && !string.IsNullOrWhiteSpace(FilePath))
                {
                    _fileName = Path.GetFileName(FilePath);
                }
                return _fileName;
            }
        }
        public Exception Error { get;private set; }
        public string Message { get;set; }
        public object Context { get; set; }
    }
}
