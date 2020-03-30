using System;
using System.Collections.Generic;
using System.Text;

namespace Ufangx.FileServices.Models
{
    /// <summary>
    /// 文件命名规则
    /// </summary>
    public enum FileNameRule
    {
        /// <summary>
        /// 保持原名，如果存在同名则在原名后递增数量
        /// </summary>
        Ascending,
        /// <summary>
        /// 保持原名，如果存在同名则覆盖
        /// </summary>
        Keep,
        /// <summary>
        /// 用日期命名
        /// </summary>
        Date,
        /// <summary>
        /// 自定义
        /// </summary>
        Custom
    }
}
