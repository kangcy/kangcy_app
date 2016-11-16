using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EGT_OTA.Models
{
    /// <summary>
    /// 文章标签枚举
    /// </summary>
    public class Enum_ArticleTag : EnumBase
    {
        /// <summary>
        /// 系统
        /// </summary>
        [EnumAttribute("系统")]
        public const int System = 1;

        /// <summary>
        /// 荐
        /// </summary>
        [EnumAttribute("荐")]
        public const int Recommend = 2;

        /// <summary>
        /// 精
        /// </summary>
        [EnumAttribute("精")]
        public const int Wonderful = 3;

        /// <summary>
        /// 无
        /// </summary>
        [EnumAttribute("无")]
        public const int None = 4;
    }
}
