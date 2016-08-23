/************************************************************************************ 
 * Copyright (c) 2016 安讯科技（南京）有限公司 版权所有 All Rights Reserved.
 * 文件名：  EGT_OTA.Models.App.User 
 * 版本号：  V1.0.0.0 
 * 创建人： 康春阳
 * 电子邮箱：kangcy@axon.com.cn 
 * 创建时间：2016/7/29 11:03:10 
 * 描述    :
 * =====================================================================
 * 修改时间：2016/7/29 11:03:10 
 * 修改人  ：  
 * 版本号  ：V1.0.0.0 
 * 描述    ：
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SubSonic.SqlGeneration.Schema;

namespace EGT_OTA.Models.App
{
    /// <summary>
    /// 用户信息
    /// </summary>
    [Serializable]
    public class User
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [SubSonicPrimaryKey]
        public int ID { get; set; }

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

        ///<summary>
        ///用户头像
        ///</summary>
        [SubSonicStringLength(100), SubSonicNullString]
        public string Avatar { get; set; }

        ///<summary>
        ///个性签名
        ///</summary>
        [SubSonicStringLength(255), SubSonicNullString]
        public string Signature { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public int Sex { get; set; }

        ///<summary>
        ///电子邮件
        ///</summary>
        [SubSonicNullString]
        public string Email { get; set; }

        /// <summary>
        /// 邮箱认证
        /// </summary>
        public int IsEmail { get; set; }

        ///<summary>
        ///绑定手机
        ///</summary>
        [SubSonicStringLength(13), SubSonicNullString]
        public string Phone { get; set; }

        /// <summary>
        /// 号码认证
        /// </summary>
        public int IsPhone { get; set; }

        ///<summary>
        ///绑定微信
        ///</summary>
        [SubSonicStringLength(50), SubSonicNullString]
        public string WeiXin { get; set; }

        /// <summary>
        /// 收藏数
        /// </summary>
        public int Keeps { get; set; }

        /// <summary>
        /// 关注数
        /// </summary>
        public int Follows { get; set; }

        /// <summary>
        /// 粉丝数
        /// </summary>
        public int Fans { get; set; }

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
    }
}