/************************************************************************************ 
 * Copyright (c) 2016 安讯科技（南京）有限公司 版权所有 All Rights Reserved.
 * 文件名：  EGT_OTA.Models.App.Article 
 * 版本号：  V1.0.0.0 
 * 创建人： 康春阳
 * 电子邮箱：kangcy@axon.com.cn 
 * 创建时间：2016/7/29 15:08:56 
 * 描述    :
 * =====================================================================
 * 修改时间：2016/7/29 15:08:56 
 * 修改人  ：  
 * 版本号  ：V1.0.0.0 
 * 描述    ：
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SubSonic.SqlGeneration.Schema;

namespace EGT_OTA.Models
{
    /// <summary>
    /// 文章
    /// </summary>
    [Serializable]
    public class Article : BaseModel
    {
        /// <summary>
        /// 文章ID
        /// </summary>
        [SubSonicPrimaryKey]
        public int ID { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        [SubSonicStringLength(100), SubSonicNullString]
        public string Title { get; set; }

        /// <summary>
        /// 详细
        /// </summary>
        [SubSonicLongString, SubSonicNullString]
        public string Introduction { get; set; }

        /// <summary>
        /// 封面
        /// </summary>
        [SubSonicStringLength(255), SubSonicNullString]
        public string Cover { get; set; }

        /// <summary>
        /// 音乐
        /// </summary>
        public int MusicID { get; set; }

        /// <summary>
        /// 音乐外链
        /// </summary>
        public string MusicUrl { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public int TypeID { get; set; }

        /// <summary>
        /// 浏览量
        /// </summary>
        public int Views { get; set; }

        /// <summary>
        /// 点赞数
        /// </summary>
        public int Goods { get; set; }

        /// <summary>
        /// 收藏数
        /// </summary>
        public int Keeps { get; set; }

        /// <summary>
        /// 评论数
        /// </summary>
        public int Comments { get; set; }

        /// <summary>
        /// 是否推荐
        /// </summary>
        public int IsRecommend { get; set; }

        #region 扩展

        /// <summary>
        /// 创建人
        /// </summary>
        [SubSonicIgnore]
        public string UserName { get; set; }

        /// <summary>
        /// 文章类型
        /// </summary>
        [SubSonicIgnore]
        public string TypeName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SubSonicIgnore]
        public string CreateDateText { get; set; }

        #endregion
    }
}