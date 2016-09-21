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
        /// 编辑
        /// </summary>
        [AllowAnyone]
        public ActionResult Edit()
        {
            var callback = ZNRequest.GetString("jsoncallback");
            try
            {
                User user = GetUserInfo();
                if (user == null)
                {
                    return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
                }
                ArticlePart model = new ArticlePart();
                model.ID = ZNRequest.GetInt("ID");
                if (model.ID == 0)
                {
                    model.ArticleID = ZNRequest.GetInt("ArticleID", 0);
                    model.Types = ZNRequest.GetInt("Types", 0);
                    if (model.ArticleID == 0)
                    {
                        return Content(callback + "(" + JsonConvert.SerializeObject(new { result = false, message = "文章信息异常" }) + ")");
                    }
                    if (model.Types == 0)
                    {
                        return Content(callback + "(" + JsonConvert.SerializeObject(new { result = false, message = "段落信息异常" }) + ")");
                    }
                }
                model.Introduction = SqlFilter(ZNRequest.GetString("Introduction"), false);
                var newId = model.ID;
                var result = false;
                if (model.ID == 0)
                {
                    newId = Tools.SafeInt(db.Add<ArticlePart>(model));
                    result = newId > 0;
                }
                else
                {
                    result = db.Update<ArticlePart>(model) > 0;
                }
                return Content(callback + "(" + JsonConvert.SerializeObject(new { result = true, message = newId }) + ")");
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
            }
            return Content(callback + "(" + JsonConvert.SerializeObject(new { result = false, message = "失败" }) + ")");
        }

        /// <summary>
        /// 删除
        /// </summary>
        [AllowAnyone]
        public ActionResult Delete()
        {
            var callback = ZNRequest.GetString("jsoncallback");
            try
            {
                User user = GetUserInfo();
                if (user == null)
                {
                    return Content(callback + "(" + JsonConvert.SerializeObject(new { result = false, message = "用户信息验证失败" }) + ")");
                }
                var id = ZNRequest.GetInt("ID");
                var model = db.Single<Keep>(x => x.ID == id);
                if (model != null)
                {
                    var result = db.Delete<Keep>(id) > 0;
                    if (result)
                    {
                        return Content(callback + "(" + JsonConvert.SerializeObject(new { result = true, message = "成功" }) + ")");
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
            }
            return Content(callback + "(" + JsonConvert.SerializeObject(new { result = false, message = "失败" }) + ")");
        }

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
