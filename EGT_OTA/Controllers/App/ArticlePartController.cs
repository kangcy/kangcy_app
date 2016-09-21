using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EGT_OTA.Models;
using System.IO;
using Newtonsoft.Json;
using CommonTools;
using EGT_OTA.Helper;
using System.Web.Security;
using Newtonsoft.Json.Linq;
using System.Text;

namespace EGT_OTA.Controllers
{
    /// <summary>
    /// 文章部分管理
    /// </summary>
    public class ArticlePartController : BaseController
    {
        #region  APP请求

        /// <summary>
        /// 列表
        /// </summary>
        [AllowAnyone]
        public ActionResult All()
        {
            var callback = ZNRequest.GetString("jsoncallback");
            try
            {
                var pager = new Pager();
                var query = new SubSonic.Query.Select(Repository.GetProvider()).From<ArticlePart>();
                var ArticleID = ZNRequest.GetInt("ArticleID");
                if (ArticleID == 0)
                {
                    return Content(callback + "()");
                }
                else
                {
                    query = query.Where<ArticlePart>(x => x.ArticleID == ArticleID);
                }
                var recordCount = query.GetRecordCount();
                var list = query.OrderDesc("ID").ExecuteTypedList<ArticlePart>();
                var result = new
                {
                    records = recordCount,
                    list = list
                };
                var message = callback + "(" + Newtonsoft.Json.JsonConvert.SerializeObject(result) + ")";
                return Content(message);
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
                return Content(callback + "()");
            }
        }

        #endregion
    }
}
