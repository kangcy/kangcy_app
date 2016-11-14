using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CommonTools;
using EGT_OTA.Models;

namespace EGT_OTA.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            var from = ZNRequest.GetString("from");//来源

            var number = ZNRequest.GetString("Number");

            if (string.IsNullOrWhiteSpace(number))
            {
                return Json(new { result = false, message = "参数异常" }, JsonRequestBehavior.AllowGet);
            }
            Article model = db.Single<Article>(x => x.Number == number);
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

            //音乐
            if (model.MusicID > 0)
            {
                Music music = db.Single<Music>(x => x.ID == model.MusicID);
                model.MusicUrl = music == null ? "" : music.FileUrl;
                model.MusicName = music == null ? "" : music.Name;
            }

            //文章部分
            model.ArticlePart = new SubSonic.Query.Select(Repository.GetProvider()).From<ArticlePart>().Where<ArticlePart>(x => x.ArticleID == model.ID).OrderAsc("SortID").ExecuteTypedList<ArticlePart>();

            model.CreateDateText = DateTime.Now.ToString("yyyy-MM-dd");

            return Json(new { result = true, message = model }, JsonRequestBehavior.AllowGet);
        }
    }
}
