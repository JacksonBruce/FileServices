using System;
using System.Collections.Generic;
using System.Text;

namespace Ufangx.FileServices.Models
{
    public enum FileValidateResult
    {
        /// <summary>
        /// 验证成功
        /// </summary>
        Successfully,
        /// <summary>
        /// 文件大小受限制
        /// </summary>
        Limited,
        /// <summary>
        /// 文件类型是无效的
        /// </summary>
        Invalid
    }
}
