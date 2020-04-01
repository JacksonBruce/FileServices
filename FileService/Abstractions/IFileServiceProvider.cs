using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ufangx.FileServices.Models;

namespace Ufangx.FileServices.Abstractions
{
    public interface IFileServiceProvider
    {

        IEnumerable<string> AuthenticationSchemes { get; }
        /// <summary>
        /// 
        /// </summary>
        string DefaultSchemeName { get; }
        FileServiceScheme GetScheme(string schemeName);
        IEnumerable<FileServiceScheme> GetSchemes();

        FileNameRuleOptions GetNameRuleOptions();


    }
}
