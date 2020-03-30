using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ufangx.FileServices.Abstractions;

namespace Ufangx.FileServices.Models
{
    public class FileServiceScheme
    {
        /// <summary>
        /// 文件服务方案
        /// </summary>
        /// <param name="name"></param>
        public FileServiceScheme(string name) {
            Name = name;
        }
        /// <summary>
        /// 方案名称
        /// </summary>
        public string Name { get;}
        /// <summary>
        /// 存储目录
        /// </summary>
        public string StoreDirectory { get; set; }
        /// <summary>
        /// 支持的文件扩展名
        /// </summary>
        public IEnumerable<string> SupportExtensions { get; set; } = Enumerable.Empty<string>();
        /// <summary>
        /// 文件大小的最大限制
        /// </summary>
        public long? LimitedSize { get; set; }
        /// <summary>
        /// 处理类型
        /// </summary>
        public Type HandlerType { get; set; }
          
    }
}
