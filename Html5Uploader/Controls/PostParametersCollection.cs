using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Html5Uploader.Controls
{
    public class PostParametersCollection : List<PostParameter>
    {
        public PostParametersCollection()
            : base()
        { }
        public PostParameter this[string key]
        {
            get
            {
                return this.Find(p => string.Equals(key, p.Key, StringComparison.CurrentCultureIgnoreCase));
            }
            set
            {
                PostParameter par = this.Find(p => string.Equals(key, p.Key
                     , StringComparison.CurrentCultureIgnoreCase));
                par.Key = value.Key;
                par.Value = value.Value;
            }
        }
    }
    public class PostParameter
    {
        public PostParameter() { }
        public PostParameter(string k, string v)
        { Key = k; Value = v; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
