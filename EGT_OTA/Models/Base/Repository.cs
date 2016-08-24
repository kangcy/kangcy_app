using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SubSonic.SqlGeneration.Schema;
using SubSonic.Repository;
using SubSonic.DataProviders;
using System.Web;
using EGT_OTA.Models;

namespace EGT_OTA.Models
{
    /// <summary>
    /// 在Global文件中注册时使用
    /// </summary>
    public class Repository
    {
        public static SimpleRepository GetRepo()
        {
            var item = HttpContext.Current.Items["DefaultConnection"] as SimpleRepository;
            if (item == null)
            {
                var newItem = EGT_OTA.Models.Repository.GetRepo("DefaultConnection");
                HttpContext.Current.Items["DefaultConnection"] = newItem;
                return newItem;
            }
            return item;
        }

        public static SimpleRepository GetRepo(string db)
        {
            return new SimpleRepository(db, SimpleRepositoryOptions.Default);
        }

        public static SimpleRepository GetRepoByConn(string conn)
        {
            var idp = ProviderFactory.GetProvider(conn, "MySql.Data.MySqlClient");
            //var idp = ProviderFactory.GetProvider(conn, "System.Data.SqlClient");

            return new SimpleRepository(idp);
        }

        public static IDataProvider GetProvider(string connection = "DefaultConnection")
        {
            string database = System.Configuration.ConfigurationManager.ConnectionStrings[connection].ToString();
            return ProviderFactory.GetProvider(database, "MySql.Data.MySqlClient");
        }

        public static void UpdateDB(string connection = "DefaultConnection")
        {
            var repo = new SimpleRepository(GetProvider(connection), SimpleRepositoryOptions.RunMigrations);

            #region  用户、角色

            repo.Single<UserInfo>(x => x.ID == 0);//用户
            repo.Single<Role>(x => x.ID == 0);//角色

            #endregion

            #region  产品管理

            repo.Single<Country>(x => x.ID == 0);//国家
            repo.Single<Province>(x => x.ID == 0);//省份
            repo.Single<City>(x => x.ID == 0);//城市

            #endregion

            #region 前台

            repo.Single<User>(x => x.ID == 0);//用户
            repo.Single<Article>(x => x.ID == 0);//文章
            repo.Single<ArticleType>(x => x.ID == 0);//文章类型
            repo.Single<Comment>(x => x.ID == 0);//评论
            repo.Single<Music>(x => x.ID == 0);//音乐
            repo.Single<Video>(x => x.ID == 0);//视频
            repo.Single<Fan>(x => x.ID == 0);//视频

            #endregion
        }
    }
}
