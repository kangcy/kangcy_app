using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SubSonic.SqlGeneration.Schema;

namespace EGT_OTA.Models
{
    /// <summary>
    /// 用户信息
    /// </summary>
    [Serializable]
    public class UserInfo
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [SubSonicPrimaryKey]
        public int ID { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        [SubSonicStringLength(20), SubSonicNullString]
        public string RealName { get; set; }

        ///<summary>
        ///登陆账号
        ///</summary>
        [SubSonicStringLength(20), SubSonicNullString]
        public string UserName { get; set; }

        ///<summary>
        ///登陆密码
        ///</summary>
        [SubSonicStringLength(20), SubSonicNullString]
        public string Password { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public int Sex { get; set; }

        /// <summary>
        /// 年龄
        /// </summary>
        public int Age { get; set; }

        ///<summary>
        ///微信
        ///</summary>
        [SubSonicStringLength(100), SubSonicNullString]
        public string Weixin { get; set; }

        ///<summary>
        ///QQ
        ///</summary>
        [SubSonicStringLength(100), SubSonicNullString]
        public string QQ { get; set; }

        ///<summary>
        ///电子邮件
        ///</summary>
        [SubSonicNullString]
        public string Email { get; set; }

        /// <summary>
        /// 角色类型
        /// </summary>
        public int RoleID { get; set; }

        ///<summary>
        ///上次登录IP
        ///</summary>
        [SubSonicStringLength(32), SubSonicNullString]
        public string LastLoginIP { get; set; }

        /// <summary>
        /// 上次登录时间
        /// </summary>
        public DateTime LastLoginDate { get; set; }

        /// <summary>
        /// 登陆次数
        /// </summary>
        public int LoginTimes { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }

        [SubSonicIgnore]
        public string Power { get; set; }
    }
}
