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
    [Authorization]
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
        /// 当前操作用户对象
        /// </summary>
        protected CurrentUser CurrentUser
        {
            get
            {
                return System.Web.HttpContext.Current.User as CurrentUser;
            }
        }

        /// <summary>
        /// 防注入
        /// </summary>
        protected string SqlFilter(string inputString)
        {
            string SqlStr = @"and|or|exec|execute|insert|select|delete|update|alter|create|drop|count|\*|chr|char|asc|mid|substring|master|truncate|declare|xp_cmdshell|restore|backup|net +user|net +localgroup +administrators";
            try
            {
                if (!string.IsNullOrEmpty(inputString))
                {
                    inputString = Regex.Replace(inputString, @"\b(" + SqlStr + @")\b", string.Empty, RegexOptions.IgnoreCase).Replace("&nbsp;", "");
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("SQL注入", ex);
            }
            return inputString;
        }

        #region  地市联动

        /// <summary>
        /// 国家下拉
        /// </summary>
        /// <param name="head">是否添加默认项</param>
        /// <param name="index">当前选中项</param>
        protected string CountrySelect(bool head, int index)
        {
            StringBuilder sbr = new StringBuilder();
            var list = db.All<Country>().ToList();
            if (head)
            {
                sbr.Append("<option value='0'>请选择国家</option>");
            }
            list.ForEach(x =>
            {
                sbr.AppendFormat("<option value='" + x.ID + "' {0}>" + x.Name + "</option>", x.ID == index ? "selected=\"selected\"" : "");
            });
            return sbr.ToString();
        }

        /// <summary>
        /// 省份下拉
        /// </summary>
        /// <param name="head">是否添加默认项</param>
        /// <param name="index">当前选中项</param>
        /// <param name="countryId">国家ID</param>
        protected string ProvinceSelect(bool head, int index, int countryId)
        {
            StringBuilder sbr = new StringBuilder();
            var list = db.Find<Province>(x => x.CountryID == countryId).ToList();
            if (head)
            {
                sbr.Append("<option value='0'>请选择省份</option>");
            }
            list.ForEach(x =>
            {
                sbr.AppendFormat("<option value='" + x.ID + "' {0}>" + x.Name + "</option>", x.ID == index ? "selected=\"selected\"" : "");
            });
            return sbr.ToString();
        }

        /// <summary>
        /// 城市下拉
        /// </summary>
        /// <param name="head">是否添加默认项</param>
        /// <param name="index">当前选中项</param>
        /// <param name="countryId">国家ID</param>
        /// <param name="countryId">省份ID</param>
        protected string CitySelect(bool head, int index, int countryId, int provinceId = 0)
        {
            StringBuilder sbr = new StringBuilder();
            var list = new List<City>();
            if (countryId > 0 || provinceId > 0)
            {
                list = db.All<City>().ToList();
            }
            if (countryId > 0)
            {
                list = list.FindAll(x => x.CountryID == countryId);
            }
            if (provinceId > 0)
            {
                list = list.FindAll(x => x.ProvinceID == provinceId);
            }
            if (head)
            {
                sbr.Append("<option value='0'>请选择城市</option>");
            }
            list.ForEach(x =>
            {
                sbr.AppendFormat("<option value='" + x.ID + "' {0}>" + x.Name + "</option>", x.ID == index ? "selected=\"selected\"" : "");
            });
            return sbr.ToString();
        }

        #endregion

        /// <summary>
        /// 状态下拉
        /// </summary>
        /// <param name="head">是否添加默认项</param>
        /// <param name="index">当前选中项</param>
        protected string StatusSelect(bool head, int index)
        {
            StringBuilder sbr = new StringBuilder();
            Dictionary<int, string> dic = EnumBase.GetDictionary(typeof(Enum_Status));
            if (head)
            {
                sbr.Append("<option value='0'>请选择审核状态</option>");
            }
            foreach (int key in dic.Keys)
            {
                sbr.AppendFormat("<option value='" + key + "' {0}>" + dic[key] + "</option>", key == index ? "selected=\"selected\"" : "");
            }
            return sbr.ToString();
        }

        /// <summary>
        /// 启用下拉
        /// </summary>
        /// <param name="head">是否添加默认项</param>
        /// <param name="index">当前选中项</param>
        protected string UsedSelect(bool head, int index)
        {
            StringBuilder sbr = new StringBuilder();
            Dictionary<int, string> dic = EnumBase.GetDictionary(typeof(Enum_Used));
            if (head)
            {
                sbr.Append("<option value='0'>请选择启用状态</option>");
            }
            foreach (int key in dic.Keys)
            {
                sbr.AppendFormat("<option value='" + key + "' {0}>" + dic[key] + "</option>", key == index ? "selected=\"selected\"" : "");
            }
            return sbr.ToString();
        }

        /// <summary>
        /// 性别下拉
        /// </summary>
        /// <param name="head">是否添加默认项</param>
        /// <param name="index">当前选中项</param>
        protected string SexSelect(bool head, int index)
        {
            StringBuilder sbr = new StringBuilder();
            Dictionary<int, string> dic = EnumBase.GetDictionary(typeof(Enum_Sex));
            if (head)
            {
                sbr.Append("<option value='0'>请选择性别</option>");
            }
            foreach (int key in dic.Keys)
            {
                sbr.AppendFormat("<option value='" + key + "' {0}>" + dic[key] + "</option>", key == index ? "selected=\"selected\"" : "");
            }
            return sbr.ToString();
        }

        /// <summary>
        /// 角色下拉
        /// </summary>
        /// <param name="head">是否添加默认项</param>
        /// <param name="index">当前选中项</param>
        protected string RoleSelect(bool head, int index)
        {
            StringBuilder sbr = new StringBuilder();
            var roles = db.All<Role>().ToList();
            if (head)
            {
                sbr.Append("<option value='0'>请选择角色</option>");
            }
            roles.ForEach(x =>
            {
                sbr.AppendFormat("<option value='" + x.ID + "' {0}>" + x.Name + "</option>", x.ID == index ? "selected=\"selected\"" : "");
            });
            return sbr.ToString();
        }

        /// <summary>
        /// 菜单
        /// </summary>
        protected string MenuStr()
        {
            string str = string.Empty;
            string filePath = System.Web.HttpContext.Current.Server.MapPath("~/Config/adminmenu.json");
            if (System.IO.File.Exists(filePath))
            {
                StreamReader sr = new StreamReader(filePath, Encoding.Default);
                str = sr.ReadToEnd();
                sr.Close();
            }
            return str;
        }

        /// <summary>
        /// 按钮
        /// </summary>
        protected string ButtonStr()
        {
            string str = string.Empty;
            string filePath = System.Web.HttpContext.Current.Server.MapPath("~/Config/button.json");
            if (System.IO.File.Exists(filePath))
            {
                StreamReader sr = new StreamReader(filePath, Encoding.Default);
                str = sr.ReadToEnd();
                sr.Close();
            }
            return str;
        }

        /// <summary>
        /// 地市联动
        /// </summary>
        public ActionResult Province()
        {
            var countryId = ZNRequest.GetInt("countryId", 0);
            return Content(ProvinceSelect(true, 0, countryId));
        }

        /// <summary>
        /// 地市联动
        /// </summary>
        public ActionResult City()
        {
            var countryId = ZNRequest.GetInt("countryId", 0);
            var provinceId = ZNRequest.GetInt("provinceId", 0);
            return Content(CitySelect(true, 0, countryId, provinceId));
        }

        /// <summary>
        /// 文章类型下拉
        /// </summary>
        /// <param name="head">是否添加默认项</param>
        /// <param name="index">当前选中项</param>
        protected string ArticleTypeSelect(bool head, int index)
        {
            StringBuilder sbr = new StringBuilder();
            var list = db.Find<ArticleType>(x => x.Status == Enum_Status.Approved).ToList();
            if (head)
            {
                sbr.Append("<option value='0' pid='-0-'>请选择文章类型</option>");
            }
            list.FindAll(x => x.ParentID == 0).ForEach(x =>
            {
                sbr.AppendFormat("<option value='" + x.ID + "' pid='-0-' {0}>" + x.Name + "</option>", x.ID == index ? "selected=\"selected\"" : "");
                BuildChildArticleType(index, x, x.Name, list, sbr);
            });
            return sbr.ToString();
        }

        protected void BuildChildArticleType(int index, ArticleType type, string parentName, List<ArticleType> list, StringBuilder sbr)
        {
            List<ArticleType> l = list.FindAll(x => x.ParentID == type.ID);
            foreach (ArticleType x in l)
            {
                sbr.AppendFormat("<option value='" + x.ID + "' pid='" + x.ParentIDList + x.ID + "-" + "' {0}>" + parentName + " - " + x.Name + "</option>", x.ID == index ? "selected=\"selected\"" : "");
                BuildChildArticleType(index, x, parentName + " - " + x.Name, list, sbr);
            }
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
            var username = ZNRequest.GetString("UserName");
            var password = DesEncryptHelper.Encrypt(ZNRequest.GetString("Password"));
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }
            return db.Single<User>(x => x.UserName == username && x.Password == password);
        }
    }
}
