using System;
using System.Collections.Generic;
using System.Text;

namespace Ufangx.FileServices.Models
{
    public class FileNameRuleOptions
    {
        public FileNameRule Rule { get; set; }
        public string Format { get; set; }
        public Func<string, string> Custom { get; set; }

    }
}
