using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CommonTools;
using EGT_OTA.Helper;
using EGT_OTA.Models;
using Newtonsoft.Json;

namespace EGT_OTA.Controllers
{
    /// <summary>
    /// 文章
    /// </summary>
    public class ArticleController : BaseController
    {
        /// <summary>
        /// 删除
        /// </summary>
        public ActionResult Delete()
        {
            try
            {
                User user = GetUserInfo();
                if (user == null)
                {
                    return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
                }
                var id = ZNRequest.GetInt("ArticleID");
                Article article = db.Single<Article>(x => x.ID == id);
                if (article == null)
                {
                    return Json(new { result = false, message = "当前文章不存在" }, JsonRequestBehavior.AllowGet);
                }
                if (article.CreateUserID != user.ID)
                {
                    return Json(new { result = false, message = "没有权限" }, JsonRequestBehavior.AllowGet);
                }
                article.Status = Enum_Status.DELETE;
                var result = db.Update<Article>(article) > 0;
                if (result)
                {
                    return Json(new { result = true, message = "成功" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 编辑
        /// </summary>
        public ActionResult Edit()
        {
            try
            {
                User user = GetUserInfo();
                if (user == null)
                {
                    return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
                }
                Article model = new Article();
                model.ID = ZNRequest.GetInt("ArticleID");
                if (model.ID > 0)
                {
                    model = db.Single<Article>(x => x.ID == model.ID);
                    if (model == null)
                    {
                        model = new Article();
                    }
                }
                model.Title = SqlFilter(ZNRequest.GetString("Title"));
                model.MusicID = ZNRequest.GetInt("MusicID", 0);
                model.MusicName = ZNRequest.GetString("MusicName");
                model.MusicUrl = ZNRequest.GetString("MusicUrl");
                model.UpdateUserID = user.ID;
                model.UpdateDate = DateTime.Now;
                model.UpdateIP = Tools.GetClientIP;
                var result = false;
                if (model.ID == 0)
                {
                    model.Cover = ZNRequest.GetString("Cover");
                    model.Status = Enum_Status.Audit;
                    model.Views = 0;
                    model.Goods = 0;
                    model.Keeps = 0;
                    model.Comments = 0;
                    model.Pays = 0;
                    model.Tag = Enum_ArticleTag.None;
                    model.TypeID = 10000;
                    model.TypeIDList = "-10000-";
                    model.ArticlePower = Enum_ArticlePower.Myself;
                    model.CreateUserID = user.ID;
                    model.CreateDate = DateTime.Now;
                    model.CreateIP = Tools.GetClientIP;
                    model.Number = ValidateCodeHelper.BuildCode(10);
                    model.ID = Tools.SafeInt(db.Add<Article>(model));
                    result = model.ID > 0;

                    //初始化文章段落
                    ArticlePart part = new ArticlePart();
                    if (result)
                    {
                        part.ArticleID = model.ID;
                        part.Types = 1;
                        part.Introduction = model.Cover;
                        part.SortID = 0;
                        part.ID = Tools.SafeInt(db.Add<ArticlePart>(part));
                        result = part.ID > 0;
                    }
                    if (result)
                    {
                        return Json(new { result = true, message = part }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    model.Status = Enum_Status.Approved;
                    result = db.Update<Article>(model) > 0;

                    var parts = ZNRequest.GetString("PartIDs");
                    if (!string.IsNullOrEmpty(parts))
                    {

                        var articlePart = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "SortID").From<ArticlePart>().ExecuteTypedList<ArticlePart>();

                        var ids = parts.Split(',');
                        ids.ToList().ForEach(x =>
                        {
                            var id = x.Split('-');
                            var partid = Tools.SafeInt(id[0]);
                            var index = Tools.SafeInt(id[1]);
                            new SubSonic.Query.Update<ArticlePart>(Repository.GetProvider()).Set("SortID").EqualTo(index).Where<ArticlePart>(y => y.ID == partid).Execute();
                        });
                    }
                }
                if (result)
                {
                    return Json(new { result = true, message = "成功" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 详情
        /// </summary>
        public ActionResult Detail()
        {
            try
            {
                var id = ZNRequest.GetInt("ArticleID");
                if (id == 0)
                {
                    return Json(new { result = false, message = "参数异常" }, JsonRequestBehavior.AllowGet);
                }
                Article model = db.Single<Article>(x => x.ID == id);
                if (model == null)
                {
                    return Json(new { result = false, message = "文章信息异常" }, JsonRequestBehavior.AllowGet);
                }

                if (model.Status == Enum_Status.DELETE)
                {
                    return Json(new { result = false, message = "当前文章已删除，请刷新重试" }, JsonRequestBehavior.AllowGet);
                }

                string password = ZNRequest.GetString("ArticlePassword");

                //浏览数
                new SubSonic.Query.Update<Article>(Repository.GetProvider()).Set("Views").EqualTo(model.Views + 1).Where<Article>(x => x.ID == model.ID).Execute();

                //创建人
                User createUser = db.Single<User>(x => x.ID == model.CreateUserID);
                if (createUser != null)
                {
                    model.NickName = createUser == null ? "" : createUser.NickName;
                    model.Avatar = createUser == null ? GetFullUrl(null) : GetFullUrl(createUser.Avatar);
                    model.AutoMusic = createUser.AutoMusic;
                    model.ShareNick = createUser.ShareNick;
                }
                //类型
                ArticleType articleType = GetArticleType().Single<ArticleType>(x => x.ID == model.TypeID);
                model.TypeName = articleType == null ? "" : articleType.Name;

                //音乐
                if (model.MusicID > 0)
                {
                    Music music = db.Single<Music>(x => x.ID == model.MusicID);
                    model.MusicUrl = music == null ? "" : music.FileUrl;
                    model.MusicName = music == null ? "" : music.Name;
                }

                //文章部分
                model.ArticlePart = new SubSonic.Query.Select(Repository.GetProvider()).From<ArticlePart>().Where<ArticlePart>(x => x.ArticleID == id).OrderAsc("SortID").ExecuteTypedList<ArticlePart>();

                model.CreateDateText = DateTime.Now.ToString("yyyy-MM-dd");
                model.ShareUrl = System.Configuration.ConfigurationManager.AppSettings["share_url"] + model.Number;

                return Json(new { result = true, message = model }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 编辑封面
        /// </summary>
        public ActionResult EditArticleCover()
        {
            try
            {
                User user = GetUserInfo();
                if (user == null)
                {
                    return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
                }
                var id = ZNRequest.GetInt("ArticleID");
                if (id == 0)
                {
                    return Json(new { result = false, message = "文章信息异常" }, JsonRequestBehavior.AllowGet);
                }
                var Cover = ZNRequest.GetString("Cover");
                var result = new SubSonic.Query.Update<Article>(Repository.GetProvider()).Set("Cover").EqualTo(Cover).Where<Article>(x => x.ID == id).Execute() > 0;
                if (result)
                {
                    return Json(new { result = true, message = "成功" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 编辑音乐
        /// </summary>
        public ActionResult EditArticleMusic()
        {
            try
            {
                User user = GetUserInfo();
                if (user == null)
                {
                    return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
                }
                var id = ZNRequest.GetInt("ArticleID");
                if (id == 0)
                {
                    return Json(new { result = false, message = "文章信息异常" }, JsonRequestBehavior.AllowGet);
                }
                var MusicID = ZNRequest.GetInt("MusicID");
                var MusicName = ZNRequest.GetString("MusicName");
                var MusicUrl = ZNRequest.GetString("MusicUrl");
                var result = new SubSonic.Query.Update<Article>(Repository.GetProvider()).Set("MusicID").EqualTo(MusicID).Set("MusicUrl").EqualTo(MusicUrl).Set("MusicName").EqualTo(MusicName).Where<Article>(x => x.ID == id).Execute() > 0;
                if (result)
                {
                    return Json(new { result = true, message = "成功" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 编辑权限
        /// </summary>
        public ActionResult EditArticlePower()
        {
            try
            {
                User user = GetUserInfo();
                if (user == null)
                {
                    return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
                }
                var id = ZNRequest.GetInt("ArticleID");
                if (id == 0)
                {
                    return Json(new { result = false, message = "参数异常" }, JsonRequestBehavior.AllowGet);
                }
                var ArticlePower = ZNRequest.GetInt("ArticlePower");
                var result = new SubSonic.Query.Update<Article>(Repository.GetProvider()).Set("ArticlePower").EqualTo(ArticlePower).Where<Article>(x => x.ID == id).Execute() > 0;
                if (result)
                {
                    return Json(new { result = true, message = "成功" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 编辑分类
        /// </summary>
        public ActionResult EditArticleType()
        {
            try
            {
                User user = GetUserInfo();
                if (user == null)
                {
                    return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
                }
                var id = ZNRequest.GetInt("ArticleID");
                if (id == 0)
                {
                    return Json(new { result = false, message = "参数异常" }, JsonRequestBehavior.AllowGet);
                }
                var TypeID = ZNRequest.GetInt("ArticleType");
                var articleType = GetArticleType().Single<ArticleType>(x => x.ID == TypeID);
                if (articleType == null)
                {
                    return Json(new { result = false, message = "不存在当前类型" }, JsonRequestBehavior.AllowGet);
                }
                var result = new SubSonic.Query.Update<Article>(Repository.GetProvider()).Set("TypeID").EqualTo(TypeID).Set("TypeIDList").EqualTo(articleType.ParentIDList).Where<Article>(x => x.ID == id).Execute() > 0;
                if (result)
                {
                    return Json(new { result = true, message = "成功" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 校验权限
        /// </summary>
        public ActionResult CheckPowerPwd()
        {
            try
            {
                var id = ZNRequest.GetInt("ArticleID");
                if (id == 0)
                {
                    return Json(new { result = false, message = "参数异常" }, JsonRequestBehavior.AllowGet);
                }
                var pwd = ZNRequest.GetInt("ArticlePowerPwd");
                var result = db.Exists<Article>(x => x.ID == id && x.ArticlePower == Enum_ArticlePower.Password && x.ArticlePowerPwd == pwd);
                if (result)
                {
                    return Json(new { result = true, message = "成功" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 列表
        /// </summary>
        public ActionResult All()
        {
            try
            {
                //创建人
                var pager = new Pager();
                var query = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "Title", "TypeID", "Cover", "Views", "Keeps", "Comments", "CreateUserID", "CreateDate").From<Article>().Where<Article>(x => x.ID > 0 && x.Status == Enum_Status.Approved);

                //昵称、轻墨号
                var title = ZNRequest.GetString("Title");
                if (!string.IsNullOrEmpty(title))
                {
                    query.And("Title").Like("%" + title + "%");
                }
                var CreateUserID = ZNRequest.GetInt("CreateUserID");
                if (CreateUserID > 0)
                {
                    query = query.And("CreateUserID").IsEqualTo(CreateUserID);
                }

                var CurrUserID = ZNRequest.GetInt("CurrUserID", 0);
                if (CreateUserID != CurrUserID || CreateUserID == 0)
                {
                    //查看公开或加密的文章
                    query = query.And("ArticlePower").In(new int[] { 1, 3 });
                }

                //文章类型
                var TypeID = ZNRequest.GetInt("TypeID");
                if (TypeID > 0)
                {
                    query = query.And("TypeIDList").Like("%-" + TypeID + "-%");
                }

                //搜索默认显示推荐文章
                var Source = ZNRequest.GetString("Source");
                if (!string.IsNullOrWhiteSpace(Source))
                {
                    query = query.And("Tag").IsEqualTo(Enum_ArticleTag.Recommend);
                }

                var recordCount = query.GetRecordCount();
                var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1;
                var list = query.Paged(pager.Index, pager.Size).OrderAsc("Tag").ExecuteTypedList<Article>();
                var articletypes = GetArticleType();
                var users = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "NickName", "Avatar").From<User>().Where("ID").In(list.Select(x => x.CreateUserID).ToArray()).ExecuteTypedList<User>();

                var array = list.Select(x => x.ID).ToArray();

                var parts = new SubSonic.Query.Select(Repository.GetProvider()).From<ArticlePart>().Where<ArticlePart>(x => x.Types == 1).And("ArticleID").In(array).OrderAsc("SortID").ExecuteTypedList<ArticlePart>();

                var newlist = (from a in list
                               join u in users on a.CreateUserID equals u.ID
                               select new
                               {
                                   NickName = u.NickName,
                                   Avatar = GetFullUrl(u.Avatar),
                                   ArticleID = a.ID,
                                   Title = a.Title,
                                   Views = a.Views,
                                   Goods = a.Goods,
                                   Comments = a.Comments,
                                   Keeps = a.Keeps,
                                   Pays = a.Pays,
                                   UserID = a.CreateUserID,
                                   CreateDate = FormatTime(a.CreateDate),
                                   TypeName = articletypes.Exists(x => x.ID == a.TypeID) ? articletypes.FirstOrDefault(x => x.ID == a.TypeID).Name : "",
                                   ArticlePart = parts.Where(x => x.ArticleID == a.ID).OrderBy(x => x.ID).Take(4).ToList(),
                                   ArticlePower = a.ArticlePower,
                                   Tag = a.Tag
                               }).ToList();
                var result = new
                {
                    currpage = pager.Index,
                    records = recordCount,
                    totalpage = totalPage,
                    list = newlist
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error(ex.Message);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
