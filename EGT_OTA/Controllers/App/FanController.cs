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
    /// 关注、粉丝管理
    /// </summary>
    public class FanController : BaseController
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
            var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Fan>();
            string Name = ZNRequest.GetString("Name");
            if (!string.IsNullOrWhiteSpace(Name))
            {
                var array = db.Find<User>(x => x.UserName == Name).Select(x => x.ID).ToArray();
                if (array.Length > 0)
                {
                    query = query.And("CreateUserID").In(array);
                }
            }
            string FanName = ZNRequest.GetString("FanName");
            if (!string.IsNullOrWhiteSpace(FanName))
            {
                var array = db.Find<User>(x => x.UserName == FanName).Select(x => x.ID).ToArray();
                if (array.Length > 0)
                {
                    query = query.And("UserID").In(array);
                }
            }
            var recordCount = query.GetRecordCount();
            var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1;
            var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Fan>();
            var userArray = list.Select(x => x.CreateUserID).ToList();
            userArray.AddRange(list.Select(x => x.UserID).ToList());
            var users = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "UserName").From<User>().And("ID").In(userArray.ToArray()).ExecuteTypedList<User>();
            var newlist = (from l in list
                           select new
                           {
                               ID = l.ID,
                               Name = users.Exists(x => x.ID == l.CreateUserID) ? users.FirstOrDefault(x => x.ID == l.CreateUserID).UserName : "",
                               FanName = users.Exists(x => x.ID == l.UserID) ? users.FirstOrDefault(x => x.ID == l.UserID).UserName : "",
                               CreateDate = l.CreateDate.ToString("yyyy-MM-dd hh:mm:ss"),
                               Status = EnumBase.GetDescription(typeof(Enum_Status), l.Status)
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

        #region  APP请求

        /// <summary>
        /// 编辑
        /// </summary>
        [AllowAnyone]
        public ActionResult Manage()
        {
            User user = GetUserInfo();
            if (user == null)
            {
                return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
            }

            var result = false;
            var message = string.Empty;
            var userID = ZNRequest.GetInt("UserID");
            Fan model = db.Single<Fan>(x => x.CreateUserID == user.ID && x.UserID == userID);
            if (model == null)
            {
                model = new Fan();
            }
            model.UserID = userID;
            model.Status = Enum_Status.Approved;
            model.UpdateDate = DateTime.Now;
            model.UpdateUserID = user.ID;
            model.UpdateIP = Tools.GetClientIP;
            try
            {
                if (model.ID == 0)
                {
                    model.CreateDate = DateTime.Now;
                    model.CreateUserID = user.ID;
                    model.CreateIP = Tools.GetClientIP;
                    result = Tools.SafeInt(db.Add<Fan>(model)) > 0;
                }
                else
                {
                    result = db.Update<Fan>(model) > 0;
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message, ex);
                message = ex.Message;
            }
            return Json(new { result = result, message = message }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 删除
        /// </summary>
        [AllowAnyone]
        public ActionResult Delete()
        {
            User user = GetUserInfo();
            if (user == null)
            {
                return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
            }

            var result = false;
            var message = string.Empty;
            var id = ZNRequest.GetInt("ids");
            var model = db.Single<Fan>(x => x.ID == id);
            try
            {
                if (model != null)
                {
                    model.Status = Enum_Status.DELETE;
                    result = db.Update<Fan>(model) > 0;
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message, ex);
                message = ex.Message;
            }
            return Json(new { result = result, message = message }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 列表
        /// </summary>
        [AllowAnyone]
        public ActionResult All()
        {
            var pager = new Pager();
            var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Comment>().Where<Fan>(x => x.Status == Enum_Status.Approved);

            //关注人
            var FromUserID = ZNRequest.GetInt("FromUserID");
            if (FromUserID > 0)
            {
                query = query.And("FromUserID").IsEqualTo(FromUserID);
            }

            //被关注人
            var ToUserID = ZNRequest.GetInt("ToUserID");
            if (ToUserID > 0)
            {
                query = query.And("ToUserID").IsEqualTo(ToUserID);
            }
            var recordCount = query.GetRecordCount();
            var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1;
            var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Fan>();
            var array = new List<int>();
            var from = list.Select(x => x.FromUserID).ToList();
            var to = list.Select(x => x.ToUserID).ToList();
            if (FromUserID > 0)
            {
                array = to;
            }
            if (ToUserID > 0)
            {
                array = from;
            }
            var users = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "NickName", "Avatar").From<User>().Where<User>(x => x.Status == Enum_Status.Approved).And("ID").In(array.ToArray()).ExecuteTypedList<User>();
            var newlist = (from l in list
                           join u in users on l.CreateUserID equals u.ID
                           select new
                           {
                               Summary = l.Summary,
                               CreateDate = l.CreateDate.ToString("yyyy-MM-dd"),
                               NickName = u.NickName,
                               Avatar = GetFullUrl(u.Avatar),
                               ArticleID = a.ID,
                               Title = a.Title
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


        #endregion
    }
}
