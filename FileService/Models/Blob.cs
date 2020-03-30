using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ufangx.FileServices.Models
{
    public class Blob
    {
        public Stream Data { get; set; } 
        public string ResumableKey { get; set; } 
        public long BlobIndex { get; set; }
    }
}
