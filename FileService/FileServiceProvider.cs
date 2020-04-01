using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using Ufangx.FileServices.Abstractions;
using Ufangx.FileServices.Models;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.DependencyInjection;

namespace Ufangx.FileServices
{
    public class FileServiceProvider : IFileServiceProvider
    {
        private readonly FileServiceOptions options;

        public FileServiceProvider(IOptions<FileServiceOptions> options)
        {
            this.options = options.Value;
        }

        public IEnumerable<string> AuthenticationSchemes => options.AuthenticationSchemes;

        public string DefaultSchemeName => options.DefaultScheme;

        public FileServiceScheme GetScheme(string name) {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("message", nameof(name));
            }
            var scheme = options.SchemeMap[name];
            if (scheme == null)
            {
                throw new Exception($"无效的文件服务方案名称“{name}”");
            }
            return scheme;
        }

        public FileNameRuleOptions GetNameRuleOptions() => options.RuleOptions;
        public IEnumerable<FileServiceScheme> GetSchemes()
        {
            return options.Schemes;
        }
         
    }
}
