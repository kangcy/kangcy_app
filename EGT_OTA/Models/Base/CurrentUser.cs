using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using EGT_OTA.Models;
using System.Web;
using CommonTools;
using SubSonic.Repository;
using EGT_OTA.Helper;

namespace EGT_OTA.Models
{
    /// <summary>
    /// 系统当前用户对象类
    /// </summary>
    public class CurrentUser : IPrincipal
    {
        SimpleRepository db = Repository.GetRepo();
        static int timeSpan = 60; //弹性过期时间（分钟）

        int currentUserID = 0;
        UserInfo user = null;
        IIdentity identity = null;

        /// <summary>
        /// 获取当前用户的实例(游客身份的用户)
        /// </summary>
        /// <param name="userID">游客的ID，若为Guid.Empty，将返回新的Guid</param>
        /// <returns>当前用户的实例</returns>
        public static CurrentUser GetCurrentUser(int userID)
        {
            return GetCurrentUser(new GuestIdentity(), userID);
        }

        /// <summary>
        /// 获取当前用户的实例
        /// </summary>
        /// <param name="_identity"></param>
        /// <param name="userID"></param>
        /// <returns>当前用户的实例</returns>
        public static CurrentUser GetCurrentUser(IIdentity _identity, int userID)
        {
            return GetCurrentUser(_identity, userID, false);
        }

        /// <summary>
        /// 获取当前用户的实例
        /// </summary>
        /// <param name="_identity">身份认证标识</param>
        /// <param name="userID">当前用户的ID</param>
        /// <param name="removeCache">是否清除缓存</param>
        /// <returns>当前用户的实例</returns>
        public static CurrentUser GetCurrentUser(IIdentity _identity, int userID, bool removeCache)
        {
            string cacheKey = String.Format(ConstKeys.CACHEKEY_CURRENTUSER, userID);
            if (removeCache)
            {
                CacheHelper.Remove(cacheKey);
            }
            if (!removeCache && CacheHelper.GetCache(cacheKey) != null)
            {
                //从缓存中获取
                return CacheHelper.GetCache(cacheKey) as CurrentUser;
            }
            //若缓存已过期，实例化新实例并添加到缓存中
            CurrentUser currentUser = new CurrentUser(_identity, userID);
            CacheHelper.Insert(cacheKey, currentUser, TimeSpan.FromMinutes(timeSpan));
            return currentUser;
        }

        #region 构造函数

        private CurrentUser(int userID)
        {
            this.currentUserID = userID;
            GetCurrentUser();
        }

        private CurrentUser(IIdentity _identity, int userID)
            : this(userID)
        {
            identity = _identity;
        }

        #endregion

        #region 公共属性

        /// <summary>
        /// 当前用户的对象实例
        /// </summary>
        public UserInfo User
        {
            get
            {
                return user;
            }
        }

        /// <summary>
        /// 判断一个用户是否已经登录
        /// </summary>
        public bool IsAuthenticated
        {
            get;
            set;
        }

        /// <summary>
        /// 是否超级管理员
        /// </summary>
        public bool IsSuperAdminstrator
        {
            get;
            set;
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 判断一个用户是否属于某个角色
        /// </summary>
        /// <param name="roleCode">待判断的角色编号</param>
        /// <returns>当前用户是否属于待判断的角色</returns>
        public bool IsInRole(int roleID)
        {
            if (user == null)
            {
                return false;
            }
            if (this.IsSuperAdminstrator)
            {
                return true;
            }
            return user.RoleID == roleID;
        }

        /// <summary>
        /// 判断一个用户是否属于某个角色
        /// </summary>
        /// <param name="roleCode">待判断的角色编号</param>
        /// <returns>当前用户是否属于待判断的角色</returns>
        public bool IsInRole(string roleCode)
        {
            if (user == null)
            {
                return false;
            }
            if (this.IsSuperAdminstrator)
            {
                return true;
            }
            return true;
        }

        /// <summary>
        /// 清除当前用户的缓存
        /// </summary>
        public void RemoveCache()
        {
            string cacheKey = String.Format(ConstKeys.CACHEKEY_CURRENTUSER, currentUserID);
            CacheHelper.Remove(cacheKey);
        }

        /// <summary>
        /// 清除指定用户ID的缓存
        /// </summary>
        public void RemoveCache(Guid userID)
        {
            string cacheKey = String.Format(ConstKeys.CACHEKEY_CURRENTUSER, userID);
            CacheHelper.Remove(cacheKey);
        }

        /// <summary>
        /// 是否包含某权限
        /// </summary>
        /// <param name="power">权限组合</param>
        public bool HasPower(string power)
        {
            if (user == null)
            {
                return false;
            }
            if (this.IsSuperAdminstrator)
            {
                return true;
            }
            if (string.IsNullOrWhiteSpace(user.Power) || string.IsNullOrWhiteSpace(power))
            {
                return false;
            }
            user.Power = "," + user.Power + ",";
            return user.Power.Contains("," + power + ",");
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 根据请求上下文（Context）获取当前用户的实例
        /// </summary>
        /// <param name="context"></param>
        /// <returns>系统用户对象，若用户未登录，则返回游客身份的用户对象</returns>
        private void GetCurrentUser()
        {
            this.IsAuthenticated = false;
            this.IsSuperAdminstrator = false;
            if (currentUserID > 0)
            {
                //实例化当前登录用户
                user = db.Single<UserInfo>(x => x.ID == currentUserID);
            }
            if (currentUserID == -1)
            {
                user = new UserInfo();
                user.ID = -1;
                user.UserName = System.Web.Configuration.WebConfigurationManager.AppSettings["admin_name"];
                user.Password = System.Web.Configuration.WebConfigurationManager.AppSettings["admin_password"];
                user.RoleID = -1;
                this.IsSuperAdminstrator = true;
            }
            if (user != null)
            {
                this.IsAuthenticated = true;

                //获取用户权限集合
                if (user.RoleID <= 0)
                {
                    user.Power = string.Empty;
                }
                else
                {
                    Role role = db.Single<Role>(x => x.ID == user.RoleID);
                    if (role != null)
                    {
                        user.Power = role.Auth;
                    }
                    if (string.IsNullOrEmpty(user.Power))
                    {
                        user.Power = string.Empty;
                    }
                }
            }
            else
            {
                identity = new GuestIdentity();
                user = new UserInfo();
                user.ID = 0;
                user.UserName = "游客";
            }
        }

        #endregion

        /// <summary>
        /// 当前用户身份标识
        /// </summary>
        public IIdentity Identity
        {
            get
            {
                return identity;
            }
        }
    }

    /// <summary>
    /// 表示一个未通过身份验证的游客的用户标识（无法继承此类）
    /// </summary>
    public sealed class GuestIdentity : IIdentity
    {
        public string AuthenticationType
        {
            get { return "GuestIdentity"; }
        }

        public bool IsAuthenticated
        {
            get { return false; }
        }

        public string Name
        {
            get { return "Guest"; }
        }
    }
}
