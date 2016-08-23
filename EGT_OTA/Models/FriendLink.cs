using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SubSonic.SqlGeneration.Schema;

namespace EGT_OTA.Models
{
    /// <summary>
    /// 友情链接（图片格式）
    /// </summary>
    [Serializable]
    public class FriendLink
    {
        public FriendLink() { }

        #region 公有属性

        ///<summary>
        ///链接ID
        ///</summary>
        [SubSonicPrimaryKey]
        public int ID { get; set; }

        ///<summary>
        ///名称
        ///</summary>
        [SubSonicNullString]
        public string Title { get; set; }

        ///<summary>
        ///链接地址
        ///</summary>
        [SubSonicNullString]
        public string LinkUrl { get; set; }

        ///<summary>
        ///图片地址
        ///</summary>
        [SubSonicNullString]
        public string ImageUrl { get; set; }

        #endregion
    }
}
