using System;
using System.Collections.Generic;
using System.Text;
using Ufangx.FileServices.Abstractions;

namespace Ufangx.FileServices.Models
{
    public class FileServiceOptions
    {
        private readonly IList<FileServiceScheme> _schemes = new List<FileServiceScheme>();
        private readonly IList<string> _authenticationSchemes = new List<string>();
        public string DefaultScheme { get; set; }
        public IEnumerable<FileServiceScheme> Schemes => _schemes;
        public IEnumerable<string> AuthenticationSchemes => _authenticationSchemes;
        public IDictionary<string, FileServiceScheme> SchemeMap { get; } = new Dictionary<string, FileServiceScheme>(StringComparer.Ordinal);
        /// <summary>
        /// 命名规则设置
        /// </summary>
        public FileNameRuleOptions RuleOptions { get; set; } = new FileNameRuleOptions() { Rule = FileNameRule.Ascending };
        /// <summary>
        /// 添加文件服务方案
        /// </summary>
        /// <param name="name"></param>
        /// <param name="configureBuilder"></param>
        /// <returns></returns>
        public FileServiceOptions AddScheme(string name, Action<FileServiceScheme> configureBuilder)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (configureBuilder == null)
            {
                throw new ArgumentNullException(nameof(configureBuilder));
            }
            if (SchemeMap.ContainsKey(name))
            {
                throw new InvalidOperationException("方案名称已经存在：" + name);
            }
            var scheme = new FileServiceScheme(name);
            configureBuilder(scheme);
            _schemes.Add(scheme);
            SchemeMap[name] = scheme;
            return this;
        }
        /// <summary>
        /// 添加文件服务方案
        /// </summary>
        /// <typeparam name="THandler">方案处理程序类型</typeparam>
        /// <param name="name">方案名称</param>
        /// <param name="storeDirectory">文件存储目录</param>
        /// <param name="supportExtensions">支持的扩展名称</param>
        /// <returns></returns>
        public FileServiceOptions AddScheme<THandler>(string name, string storeDirectory = null, IEnumerable<string> supportExtensions = null, long? LimitedSize = null)
            where THandler : class, IFileHandler
            => AddScheme(name, opt =>
            {
                opt.HandlerType = typeof(THandler);
                opt.StoreDirectory = storeDirectory;
                if (supportExtensions != null)
                {
                    opt.SupportExtensions = supportExtensions;
                }
                if (LimitedSize != null)
                {
                    opt.LimitedSize = LimitedSize;
                }
            });
        

        /// <summary>
        /// 添加认证方案
        /// </summary>
        /// <param name="scheme"></param>
        /// <returns></returns>
        public FileServiceOptions AddAuthenticationScheme(string scheme) {
            if (_authenticationSchemes.Contains(scheme)) {
                throw new Exception($"认证方案已经存在：{scheme}");
            }
            _authenticationSchemes.Add(scheme);
            return this;
        }
        /// <summary>
        /// 添加认证方案
        /// </summary>
        /// <param name="schemes"></param>
        /// <returns></returns>
        public FileServiceOptions AddAuthenticationSchemes(IEnumerable<string> schemes)
        {
            foreach (var scheme in schemes) AddAuthenticationScheme(scheme);
            return this;
        }
    }
}
