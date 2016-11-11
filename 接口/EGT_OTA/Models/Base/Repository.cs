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

            repo.Single<User>(x => x.ID == 0);//用户
            repo.Single<Article>(x => x.ID == 0);//文章
            repo.Single<ArticlePart>(x => x.ID == 0);//文章部分
            repo.Single<Comment>(x => x.ID == 0);//评论
            repo.Single<Music>(x => x.ID == 0);//音乐
            repo.Single<MusicType>(x => x.ID == 0);//音乐类型
            repo.Single<Video>(x => x.ID == 0);//视频
            repo.Single<Fan>(x => x.ID == 0);//关注
            repo.Single<Keep>(x => x.ID == 0);//收藏
            repo.Single<Zan>(x => x.ID == 0);//点赞
            repo.Single<Pay>(x => x.ID == 0);//打赏
            repo.Single<FeedBack>(x => x.ID == 0);//意见反馈
            repo.Single<Help>(x => x.ID == 0);//帮助中心
            repo.Single<UserLogin>(x => x.ID == 0);//登录方式

            if (!repo.Exists<MusicType>(x => x.ID > 0))
            {
                InitMusicType(repo);
            }

            if (!repo.Exists<Music>(x => x.ID > 0))
            {
                InitMusic(repo);
            }
        }

        /// <summary>
        /// 文章类型初始化
        /// </summary>
        protected static void InitArticleType(SimpleRepository repo)
        {
            var list = new List<ArticleType>();

            //其它
            list.Add(new ArticleType(10000, 0, "-0-10000-", "其它", "", "http://www.dcloud.io/hellomui/images/1.jpg", 1, 10000));

            //女神
            list.Add(new ArticleType(1, 0, "-0-1-", "女神", "", "http://www.dcloud.io/hellomui/images/1.jpg"));
            list.Add(new ArticleType(10, 1, "-0-1-10-", "清新", "", "http://www.dcloud.io/hellomui/images/2.jpg"));
            list.Add(new ArticleType(11, 1, "-0-1-11-", "文艺", "", "http://www.dcloud.io/hellomui/images/3.jpg"));
            list.Add(new ArticleType(12, 1, "-0-1-12-", "森系", "", "http://www.dcloud.io/hellomui/images/4.jpg"));
            list.Add(new ArticleType(13, 1, "-0-1-13-", "少女", "", "http://www.dcloud.io/hellomui/images/1.jpg"));
            list.Add(new ArticleType(14, 1, "-0-1-14-", "日系", "", "http://www.dcloud.io/hellomui/images/2.jpg"));

            //萌宠
            list.Add(new ArticleType(2, 0, "-0-2-", "萌宠", "", "http://www.dcloud.io/hellomui/images/1.jpg"));
            list.Add(new ArticleType(20, 2, "-0-2-20-", "猫", "", "http://www.dcloud.io/hellomui/images/2.jpg"));
            list.Add(new ArticleType(21, 2, "-0-2-21-", "狗", "", "http://www.dcloud.io/hellomui/images/3.jpg"));
            list.Add(new ArticleType(22, 2, "-0-2-22-", "兔子", "", "http://www.dcloud.io/hellomui/images/4.jpg"));
            list.Add(new ArticleType(23, 2, "-0-2-23-", "宠物", "", "http://www.dcloud.io/hellomui/images/1.jpg"));

            //旅行
            list.Add(new ArticleType(3, 0, "-0-3-", "旅行", "", "http://www.dcloud.io/hellomui/images/1.jpg"));
            list.Add(new ArticleType(30, 3, "-0-3-30-", "旅行微攻略", "", "http://www.dcloud.io/hellomui/images/2.jpg"));
            list.Add(new ArticleType(31, 3, "-0-3-31-", "美宿", "", "http://www.dcloud.io/hellomui/images/3.jpg"));
            list.Add(new ArticleType(32, 3, "-0-3-32-", "土耳其", "", "http://www.dcloud.io/hellomui/images/4.jpg"));
            list.Add(new ArticleType(33, 3, "-0-3-33-", "澳大利亚", "", "http://www.dcloud.io/hellomui/images/1.jpg"));
            list.Add(new ArticleType(34, 3, "-0-3-34-", "斯里兰卡", "", "http://www.dcloud.io/hellomui/images/2.jpg"));
            list.Add(new ArticleType(35, 3, "-0-3-35-", "毛里求斯", "", "http://www.dcloud.io/hellomui/images/3.jpg"));
            list.Add(new ArticleType(36, 3, "-0-3-36-", "新西兰", "", "http://www.dcloud.io/hellomui/images/4.jpg"));

            //摄影
            list.Add(new ArticleType(4, 0, "-0-4-", "摄影", "", "http://www.dcloud.io/hellomui/images/1.jpg"));
            list.Add(new ArticleType(40, 4, "-0-4-40-", "手机摄影", "", "http://www.dcloud.io/hellomui/images/2.jpg"));
            list.Add(new ArticleType(41, 4, "-0-4-41-", "胶片", "", "http://www.dcloud.io/hellomui/images/3.jpg"));
            list.Add(new ArticleType(42, 4, "-0-4-42-", "人物", "", "http://www.dcloud.io/hellomui/images/4.jpg"));
            list.Add(new ArticleType(43, 4, "-0-4-43-", "风光", "", "http://www.dcloud.io/hellomui/images/1.jpg"));
            list.Add(new ArticleType(44, 4, "-0-4-44-", "黑白", "", "http://www.dcloud.io/hellomui/images/2.jpg"));

            //男神
            list.Add(new ArticleType(5, 0, "-0-5-", "男神", "", "http://www.dcloud.io/hellomui/images/1.jpg"));
            list.Add(new ArticleType(50, 5, "-0-5-50-", "男神", "", "http://www.dcloud.io/hellomui/images/2.jpg"));
            list.Add(new ArticleType(51, 5, "-0-5-51-", "帅哥", "", "http://www.dcloud.io/hellomui/images/3.jpg"));
            list.Add(new ArticleType(52, 5, "-0-5-52-", "男朋友", "", "http://www.dcloud.io/hellomui/images/4.jpg"));
            list.Add(new ArticleType(53, 5, "-0-5-53-", "男生", "", "http://www.dcloud.io/hellomui/images/1.jpg"));
            list.Add(new ArticleType(54, 5, "-0-5-54-", "暖男", "", "http://www.dcloud.io/hellomui/images/2.jpg"));

            repo.AddMany<ArticleType>(list);
        }

        /// <summary>
        /// 音乐类型初始化
        /// </summary>
        protected static void InitMusicType(SimpleRepository repo)
        {
            var list = new List<MusicType>();
            list.Add(new MusicType(0, "默认", 0));
            list.Add(new MusicType(1, "曾经的非主流", 1));
            list.Add(new MusicType(2, "动漫ACG", 2));
            list.Add(new MusicType(3, "恶搞", 3));
            list.Add(new MusicType(4, "古风", 4));
            list.Add(new MusicType(5, "怀旧", 5));
            list.Add(new MusicType(6, "欢快", 6));
            list.Add(new MusicType(7, "浪漫", 7));
            list.Add(new MusicType(8, "另类", 8));
            list.Add(new MusicType(9, "燃曲", 9));
            list.Add(new MusicType(10, "伤感", 10));
            list.Add(new MusicType(11, "粤语", 11));
            repo.AddMany<MusicType>(list);
        }

        /// <summary>
        /// 音乐初始化
        /// </summary>
        protected static void InitMusic(SimpleRepository repo)
        {
            var list = new List<Music>();

            //默认
            list.Add(new Music(0, "无背景音乐", "", "http://www.dcloud.io/hellomui/images/1.jpg", ""));

            //欢快
            list.Add(new Music(1, "土耳其进行曲", "", "http://www.dcloud.io/hellomui/images/1.jpg", ""));
            list.Add(new Music(1, "斗牛士进行曲", "", "http://www.dcloud.io/hellomui/images/2.jpg", ""));
            list.Add(new Music(1, "菊次郎的夏天", "", "http://www.dcloud.io/hellomui/images/3.jpg", ""));
            list.Add(new Music(1, "春之声圆舞曲", "", "http://www.dcloud.io/hellomui/images/4.jpg", ""));
            list.Add(new Music(1, "雨中漫步", "", "http://www.dcloud.io/hellomui/images/1.jpg", ""));

            //优美
            list.Add(new Music(2, "清晨", "", "http://www.dcloud.io/hellomui/images/1.jpg", ""));
            list.Add(new Music(2, "初雪", "", "http://www.dcloud.io/hellomui/images/2.jpg", ""));
            list.Add(new Music(2, "卡农", "", "http://www.dcloud.io/hellomui/images/3.jpg", ""));

            //浪漫
            list.Add(new Music(3, "月亮代表我的心", "", "http://www.dcloud.io/hellomui/images/1.jpg", ""));
            list.Add(new Music(3, "梦中的婚礼", "", "http://www.dcloud.io/hellomui/images/2.jpg", ""));
            list.Add(new Music(3, "爱的协奏曲", "", "http://www.dcloud.io/hellomui/images/3.jpg", ""));
            list.Add(new Music(3, "爱的罗曼史", "", "http://www.dcloud.io/hellomui/images/4.jpg", ""));

            //激情
            list.Add(new Music(4, "克罗地亚狂想曲", "", "http://www.dcloud.io/hellomui/images/1.jpg", ""));

            //古韵
            list.Add(new Music(5, "高山流水", "", "http://www.dcloud.io/hellomui/images/1.jpg", ""));
            list.Add(new Music(5, "梁祝", "", "http://www.dcloud.io/hellomui/images/2.jpg", ""));
            list.Add(new Music(5, "茉莉花", "", "http://www.dcloud.io/hellomui/images/3.jpg", ""));
            list.Add(new Music(5, "一帘幽梦", "", "http://www.dcloud.io/hellomui/images/4.jpg", ""));

            //情境
            list.Add(new Music(6, "生日快乐", "", "http://www.dcloud.io/hellomui/images/1.jpg", ""));
            list.Add(new Music(6, "摇篮曲", "", "http://www.dcloud.io/hellomui/images/2.jpg", ""));
            list.Add(new Music(6, "婚礼进行曲", "", "http://www.dcloud.io/hellomui/images/3.jpg", ""));

            //忧伤
            list.Add(new Music(7, "再见警察", "", "http://www.dcloud.io/hellomui/images/1.jpg", ""));

            //年代
            list.Add(new Music(8, "莫斯科郊外的晚上", "", "http://www.dcloud.io/hellomui/images/1.jpg", ""));

            repo.AddMany<Music>(list);
        }
    }
}
