/************************************************************************************ 
 * Copyright (c) 2015 安讯科技（南京）有限公司 版权所有 All Rights Reserved.
 * 文件名：  EGT_OTA.Models.Travel.ScenicSpot 
 * 版本号：  V1.0.0.0 
 * 创建人： 康春阳
 * 电子邮箱：kangcy@axon.com.cn 
 * 创建时间：2015/12/14 12:42:18 
 * 描述    :
 * =====================================================================
 * 修改时间：2015/12/14 12:42:18 
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
    /// 城市
    /// </summary>
    [Serializable]
    public class City : BaseModel
    {
        ///<summary>
        ///ID
        ///</summary>
        [SubSonicPrimaryKey]
        public int ID { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [SubSonicStringLength(225), SubSonicNullString]
        public string Name { get; set; }

        /// <summary>
        /// 国家ID
        /// </summary>
        public int CountryID { get; set; }

        /// <summary>
        /// 省份ID
        /// </summary>
        public int ProvinceID { get; set; }
    }
}