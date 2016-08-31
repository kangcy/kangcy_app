using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CommonTools;
using EGT_OTA.Helper;
using EGT_OTA.Models;

namespace EGT_OTA.Controllers.App
{
    /// <summary>
    /// 文章
    /// </summary>
    public class ArticleController : BaseController
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
            string Name = ZNRequest.GetString("Name");
            var query = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "Title", "Cover", "Views", "Goods", "Comments", "CreateDate", "Status").From<Article>();
            if (!string.IsNullOrWhiteSpace(Name))
            {
                query = query.And("Title").Like("%" + Name + "%");
            }
            var recordCount = query.GetRecordCount();
            var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1; //计算总页数
            var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Article>();
            var newlist = (from l in list
                           select new
                           {
                               ID = l.ID,
                               Title = l.Title,
                               Cover = GetFullUrl(l.Cover),
                               Views = l.Views,
                               Goods = l.Goods,
                               Comments = l.Comments,
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

        /// <summary>
        /// 文章编辑
        /// </summary>
        public ActionResult ArticleManage()
        {
            var status = false;
            var result = string.Empty;
            try
            {
                Article model = new Article();
                model.ID = ZNRequest.GetInt("ID");
                model.Title = ZNRequest.GetString("Title");
                model.Introduction = SqlFilter(ZNRequest.GetString("Introduction"));
                model.Cover = ZNRequest.GetString("Cover");
                model.TypeID = ZNRequest.GetInt("TypeID", 0);
                model.Views = 0;
                model.Goods = 0;
                model.Comments = 0;
                model.Status = 0;
                var user = ZNRequest.GetInt("UserID");
                if (model.ID == 0)
                {
                    model.CreateUserID = user;
                    model.CreateDate = DateTime.Now;
                    model.CreateIP = Tools.GetClientIP;
                    status = Tools.SafeInt(db.Add<Article>(model)) > 0;
                }
                else
                {
                    model.UpdateUserID = user;
                    model.UpdateDate = DateTime.Now;
                    model.UpdateIP = Tools.GetClientIP;
                    status = db.Update<Article>(model) > 0;
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message, ex);
                result = ex.Message;
            }
            return Json(new { status = status, result = result }, JsonRequestBehavior.AllowGet);
        }

        #region  APP请求

        /// <summary>
        /// 编辑
        /// </summary>
        public ActionResult Edit()
        {
            User user = GetUserInfo();
            if (user == null)
            {
                return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
            }

            var status = false;
            var result = string.Empty;
            try
            {
                Article model = new Article();
                model.ID = ZNRequest.GetInt("ID");
                model.Title = SqlFilter(ZNRequest.GetString("Title"));
                model.Introduction = SqlFilter(ZNRequest.GetString("Introduction"));
                model.Cover = ZNRequest.GetString("Cover");
                model.TypeID = ZNRequest.GetInt("TypeID", 0);
                model.MusicID = ZNRequest.GetInt("MusicID", 0);
                model.MusicUrl = ZNRequest.GetString("MusicUrl");
                model.Status = Enum_Status.Audit;
                model.Views = 0;
                model.Goods = 0;
                model.Keeps = 0;
                model.Comments = 0;
                model.IsRecommend = 0;
                if (model.ID == 0)
                {
                    model.CreateUserID = user.ID;
                    model.CreateDate = DateTime.Now;
                    model.CreateIP = Tools.GetClientIP;
                    status = Tools.SafeInt(db.Add<Article>(model)) > 0;
                }
                else
                {
                    model.UpdateUserID = user.ID;
                    model.UpdateDate = DateTime.Now;
                    model.UpdateIP = Tools.GetClientIP;
                    status = db.Update<Article>(model) > 0;
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message, ex);
                result = ex.Message;
            }
            return Json(new { status = status, result = result }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 详情
        /// </summary>
        [AllowAnyone]
        public ActionResult Detail()
        {
            User user = GetUserInfo();
            if (user == null)
            {
                return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
            }

            var result = false;
            var message = string.Empty;
            var id = ZNRequest.GetInt("ID");
            if (id == 0)
            {
                return Json(new { result = result, message = "参数异常" }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                Article model = db.Single<Article>(x => x.ID == id);
                if (model == null)
                {
                    return Json(new { result = result, message = "数据异常" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    new SubSonic.Query.Update<Article>(Repository.GetProvider()).Set("Views").EqualTo(model.Views + 1).Where<Article>(x => x.ID == model.ID).Execute();

                    //创建人
                    User createUser = db.Single<User>(x => x.ID == model.CreateUserID);
                    model.UserName = createUser == null ? "" : createUser.NickName;

                    //类型
                    ArticleType articleType = db.Single<ArticleType>(x => x.ID == model.TypeID);
                    model.TypeName = articleType == null ? "" : articleType.Name;

                    //音乐
                    if (model.MusicID > 0)
                    {
                        Music music = db.Single<Music>(x => x.ID == model.MusicID);
                        model.MusicUrl = music == null ? "" : music.FileUrl;
                    }

                    //设置
                    model.AutoMusic = user.AutoMusic;
                    model.ShareNick = user.ShareNick;

                    model.CreateDateText = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                    return Json(new { result = result, message = model }, JsonRequestBehavior.AllowGet);
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
        /// 点赞
        /// </summary>
        [AllowAnyone]
        public ActionResult Good()
        {
            User user = GetUserInfo();
            if (user == null)
            {
                return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
            }

            var result = false;
            var message = string.Empty;
            try
            {
                var id = ZNRequest.GetInt("ID");
                if (id == 0)
                {
                    return Json(new { result = result, message = "参数异常" }, JsonRequestBehavior.AllowGet);
                }
                Article model = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "Goods").From<Article>().Where<Article>(x => x.ID == id).ExecuteSingle<Article>();
                if (model == null)
                {
                    return Json(new { result = result, message = "数据异常" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    result = new SubSonic.Query.Update<Article>(Repository.GetProvider()).Set("Goods").EqualTo(model.Goods + 1).Where<Article>(x => x.ID == model.ID).Execute() > 0;
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
            var callback = ZNRequest.GetString("jsoncallback");
            try
            {
                var pager = new Pager();
                var query = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "Title", "TypeID", "Cover", "Views", "Keeps", "Comments", "CreateUserID", "CreateDate").From<Article>().Where<Article>(x => x.Status == Enum_Status.Approved);

                //创建人
                var CreateUserID = ZNRequest.GetInt("CreateUserID");
                if (CreateUserID > 0)
                {
                    query = query.And("CreateUserID").IsEqualTo(CreateUserID);
                }

                //文章类型
                var TypeID = ZNRequest.GetInt("TypeID");
                if (TypeID > 0)
                {
                    query = query.And("TypeID").IsEqualTo(TypeID);
                }
                var recordCount = query.GetRecordCount();
                var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1;
                var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Article>();
                var articletypes = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "Name").From<ArticleType>().ExecuteTypedList<ArticleType>();
                var users = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "NickName", "Avatar").From<User>().Where("ID").In(list.Select(x => x.CreateUserID).ToArray()).ExecuteTypedList<User>();
                var newlist = (from a in list
                               join u in users on a.CreateUserID equals u.ID
                               join t in articletypes on a.TypeID equals t.ID
                               select new
                               {
                                   NickName = u.NickName,
                                   Avatar = GetFullUrl(u.Avatar),
                                   ArticleID = a.ID,
                                   Title = a.Title,
                                   Cover = GetFullUrl(a.Cover),
                                   Views = a.Views,
                                   Goods = a.Goods,
                                   Comments = a.Comments,
                                   Keeps = a.Keeps,
                                   CreateDate = a.CreateDate.ToString("yyyy-MM-dd"),
                                   TypeName = t.Name
                               }).ToList();
                var result = new
                {
                    currpage = pager.Index,
                    records = recordCount,
                    totalpage = totalPage,
                    list = newlist
                };
                return Content(callback + "(" + Newtonsoft.Json.JsonConvert.SerializeObject(result) + ")");
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
