using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SubSonic.SqlGeneration.Schema;

namespace EGT_OTA.Models
{
    /// <summary>
    /// 用户角色表
    /// </summary>
    [Serializable]
    public class Role : BaseModel
    {
        ///<summary>
        ///角色ID
        ///</summary>
        public int ID { get; set; }

        ///<summary>
        ///角色名称
        ///</summary>
        [SubSonicStringLength(50), SubSonicNullString]
        public string Name { get; set; }

        /// <summary>
        /// 权限
        /// </summary>
        [SubSonicStringLength(1000), SubSonicNullString]
        public string Auth { get; set; }
    }
}
