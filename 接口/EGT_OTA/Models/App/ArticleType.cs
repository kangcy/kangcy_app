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
    /// 文章类型
    /// </summary>
    [Serializable]
    public class ArticleType
    {
        public ArticleType()
        {

        }

        public ArticleType(int currId, int parentId, string parentIdList, string name, string summary, string cover, int status = 1, int number = 0, int sortId = 0)
        {
            this.Name = name;
            this.Summary = summary;
            this.Cover = cover;
            this.CurrID = currId;
            this.ParentID = parentId;
            this.ParentIDList = parentIdList;
            this.Status = status;
            this.Number = number;
            this.SortID = sortId;
        }

        /// <summary>
        /// ID
        /// </summary>
        [SubSonicPrimaryKey]
        public int ID { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [SubSonicStringLength(100), SubSonicNullString]
        public string Name { get; set; }

        /// <summary>
        /// 简介
        /// </summary>
        [SubSonicStringLength(255), SubSonicNullString]
        public string Summary { get; set; }

        /// <summary>
        /// 封面
        /// </summary>
        [SubSonicStringLength(255), SubSonicNullString]
        public string Cover { get; set; }

        /// <summary>
        /// 当前索引值
        /// </summary>
        public int CurrID { get; set; }

        /// <summary>
        /// 父节点
        /// </summary>
        public int ParentID { get; set; }

        /// <summary>
        /// 父节点ID集合
        /// </summary>
        [SubSonicStringLength(50), SubSonicNullString]
        public string ParentIDList { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 订阅人数
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// 排序值
        /// </summary>
        public int SortID { get; set; }
    }
}