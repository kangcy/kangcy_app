using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EGT_OTA.Models
{
    [Serializable]
    public class AdminMenu
    {
        //{"ModuleId":"1","ParentId":"0","FullName":"项目管理","Icon":"widgets.png","Location":"",Button:"刷新,新增,编辑,删除"},

        /// <summary>
        /// ID
        /// </summary>
        public string ModuleId { get; set; }

        /// <summary>
        /// 父ID
        /// </summary>
        public string ParentId { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// 链接
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// 操作
        /// </summary>
        public string Button { get; set; }
    }

    [Serializable]
    public class Button
    {
        //{"ButtonId":"8","ModuleId":"base",ParentId:0,"Code":"lr-noaudit","FullName":"取消审核","Icon":"accept.png","Category":"1","JsEvent":"btn_noaudit()","Split":0}

        public string ButtonId { get; set; }

        public string ParentId { get; set; }

        public string Code { get; set; }

        public string FullName { get; set; }

        public string Icon { get; set; }

        public string Category { get; set; }

        public string JsEvent { get; set; }

        public string Split { get; set; }
    }
}
