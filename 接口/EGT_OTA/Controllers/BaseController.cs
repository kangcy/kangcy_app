using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Text;
using EGT_OTA.Models;
using CommonTools;
using SubSonic.Repository;
using System.Text.RegularExpressions;
using EGT_OTA.Helper;

namespace EGT_OTA.Controllers
{
    public class BaseController : Controller
    {
        protected readonly SimpleRepository db = Repository.GetRepo();

        //默认管理员账号
        protected readonly string Admin_Name = System.Web.Configuration.WebConfigurationManager.AppSettings["admin_name"];
        protected readonly string Admin_Password = System.Web.Configuration.WebConfigurationManager.AppSettings["admin_password"];
        protected readonly string Base_Url = System.Web.Configuration.WebConfigurationManager.AppSettings["base_url"];

        /// <summary>
        /// 分页基础类
        /// </summary>
        public class Pager
        {
            public int Index { get; set; }
            public int Size { get; set; }

            public Pager()
            {
                this.Index = ZNRequest.GetInt("page", 1);
                this.Size = ZNRequest.GetInt("rows", 15);
            }
        }

        /// <summary>
        /// 解码
        /// </summary>
        protected string UrlDecode(string msg)
        {
            if (string.IsNullOrEmpty(msg))
            {
                return string.Empty;
            }
            return System.Web.HttpContext.Current.Server.UrlDecode(msg);
        }

        /// <summary>
        /// 防注入
        /// </summary>
        protected string SqlFilter(string inputString, bool nohtml = true)
        {
            string SqlStr = @"and|or|exec|execute|insert|select|delete|update|alter|create|drop|count|\*|chr|char|asc|mid|substring|master|truncate|declare|xp_cmdshell|restore|backup|net +user|net +localgroup +administrators";
            try
            {
                if (!string.IsNullOrEmpty(inputString))
                {
                    inputString = UrlDecode(inputString);
                    if (nohtml)
                    {
                        inputString = Tools.NoHTML(inputString);
                    }
                    inputString = Regex.Replace(inputString, @"\b(" + SqlStr + @")\b", string.Empty, RegexOptions.IgnoreCase).Replace("&nbsp;", "");
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("SQL注入", ex);
            }
            return inputString;
        }

        /// <summary>
        /// 图片完整路径
        /// </summary>
        protected string GetFullUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return Base_Url + "Images/default.png";
            }
            if (url.ToLower().StartsWith("http"))
            {
                return url;
            }
            return Base_Url + url;
        }

        /// <summary>
        /// APP访问用户信息
        /// </summary>
        protected User GetUserInfo()
        {
            var id = ZNRequest.GetInt("ID");
            return db.Single<User>(x => x.ID == id);
        }

        /// <summary>
        /// 格式化时间显示
        /// </summary>
        protected string FormatTime(DateTime date)
        {
            var totalSeconds = Convert.ToInt32((DateTime.Now - date).TotalSeconds);
            var hour = (totalSeconds / 3600);
            var year = 24 * 365;
            if (hour > year)
            {
                return Convert.ToInt32(hour / year) + "年前";
            }
            else if (hour > 24)
            {
                return Convert.ToInt32(hour / 24) + "天前";
            }
            else if (hour > 0)
            {
                return Convert.ToInt32(hour) + "小时前";
            }
            else
            {
                var minute = totalSeconds / 60;
                if (minute > 0)
                {
                    return Convert.ToInt32(minute) + "分钟前";
                }
                else
                {
                    return totalSeconds + "秒前";
                }
            }
        }

        /// <summary>
        /// 音乐
        /// </summary>
        protected List<Music> GetMusic()
        {
            string str = string.Empty;
            string filePath = System.Web.HttpContext.Current.Server.MapPath("/Config/music.config");
            if (System.IO.File.Exists(filePath))
            {
                StreamReader sr = new StreamReader(filePath, Encoding.Default);
                str = sr.ReadToEnd();
                sr.Close();
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<Music>>(str);
        }

        /// <summary>
        /// 文章类型
        /// </summary>
        protected List<ArticleType> GetArticleType()
        {
            string str = string.Empty;
            string filePath = System.Web.HttpContext.Current.Server.MapPath("/Config/articletype.config");
            if (System.IO.File.Exists(filePath))
            {
                StreamReader sr = new StreamReader(filePath, Encoding.Default);
                str = sr.ReadToEnd();
                sr.Close();
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<ArticleType>>(str);
        }

        /// <summary>
        /// 文章模板
        /// </summary>
        protected List<ArticleTemp> GetArticleTemp()
        {
            string str = string.Empty;
            string filePath = System.Web.HttpContext.Current.Server.MapPath("/Config/articletemp.config");
            if (System.IO.File.Exists(filePath))
            {
                StreamReader sr = new StreamReader(filePath, Encoding.Default);
                str = sr.ReadToEnd();
                sr.Close();
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<ArticleTemp>>(str);
        }
    }
}
