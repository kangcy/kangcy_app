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
    /// 意见反馈管理
    /// </summary>
    public class FeedBackController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 列表
        /// </summary>
        public ActionResult List()
        {
            var pager = new Pager();
            var query = new SubSonic.Query.Select(Repository.GetProvider()).From<FeedBack>();
            string Name = ZNRequest.GetString("Name");
            if (!string.IsNullOrWhiteSpace(Name))
            {
                var array = db.Find<User>(x => x.NickName.Contains(Name)).Select(x => x.ID).ToArray();
                if (array.Length > 0)
                {
                    query = query.And("CreateUserID").In(array);
                }
            }
            var recordCount = query.GetRecordCount();
            var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1;
            var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<FeedBack>();
            var users = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "NickName").From<User>().And("ID").In(list.Select(x => x.CreateUserID).ToArray()).ExecuteTypedList<User>();
            var newlist = (from l in list
                           select new
                           {
                               ID = l.ID,
                               NickName = users.Exists(x => x.ID == l.CreateUserID) ? users.FirstOrDefault(x => x.ID == l.CreateUserID).NickName : "",
                               Summary = l.Summary,
                               CreateDate = l.CreateDate.ToString("yyyy-MM-dd hh:mm:ss")
                           }).ToList();
            var result = new
            {
                page = pager.Index,
                records = recordCount,
                total = totalPage,
                rows = newlist
            };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 删除
        /// </summary>
        public ActionResult Delete()
        {
            var result = false;
            var message = string.Empty;
            if (!CurrentUser.HasPower("38-4"))
            {
                return Json(new { result = result, message = "您不是管理员或者没有管理的权限" }, JsonRequestBehavior.AllowGet);
            }

            var id = ZNRequest.GetInt("ids");
            try
            {
                if (id > 0)
                {
                    result = db.Delete<FeedBack>(id) > 0;
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message, ex);
                message = ex.Message;
            }
            return Json(new { result = result, message = message }, JsonRequestBehavior.AllowGet);
        }

        #region  APP请求

        /// <summary>
        /// 编辑
        /// </summary>
        [AllowAnyone]
        public ActionResult Edit()
        {
            var result = true;
            var message = string.Empty;
            var callback = ZNRequest.GetString("jsoncallback");
            try
            {
                User user = GetUserInfo();
                if (user == null)
                {
                    result = false;
                    message = "用户信息验证失败";
                }
                var summary = SqlFilter(ZNRequest.GetString("Summary"));
                if (string.IsNullOrWhiteSpace(summary))
                {
                    result = false;
                    message = "请填写反馈信息";
                }
                if (result)
                {
                    FeedBack model = new FeedBack();
                    model.Summary = summary;
                    model.CreateDate = DateTime.Now;
                    model.CreateUserID = user.ID;
                    model.CreateIP = Tools.GetClientIP;
                    result = Tools.SafeInt(db.Add<FeedBack>(model)) > 0;
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
                result = false;
                message = "添加失败";
            }
            return Content(callback + "(" + Newtonsoft.Json.JsonConvert.SerializeObject(new { result = result, message = message }) + ")");
        }

        #endregion
    }
}
