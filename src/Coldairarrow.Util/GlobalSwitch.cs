using System;

namespace Coldairarrow.Util
{
    /// <summary>
    /// 全局控制
    /// </summary>
    public static class GlobalSwitch
    {
        #region 构造函数

        static GlobalSwitch()
        {
#if !DEBUG
            RunModel = RunModel.Publish;
#endif
        }

        #endregion

        #region 参数

        /// <summary>
        /// 项目名
        /// </summary>
        public static string ProjectName { get; } = "Colder.Fx.Net.AdminLTE";

        /// <summary>
        /// 网站根地址
        /// </summary>
        public static string WebRootUrl { get; set; } = "http://localhost:9599";

        #endregion

        #region 运行

        /// <summary>
        /// 运行模式
        /// </summary>
        public static RunModel RunModel { get; } = RunModel.Publish;

        #endregion

        #region 数据库相关

        /// <summary>
        /// 默认数据库类型
        /// </summary>
        public static DatabaseType DatabaseType { get; } = DatabaseType.SqlServer;

        /// <summary>
        /// 默认数据库连接名
        /// </summary>
        public static string DefaultDbConName { get; } = "BaseDb";

        /// <summary>
        /// 默认实体命名空间
        /// </summary>
        public static string DefaultEntityNamespace { get; } = "Coldairarrow.Entity";

        #endregion

        #region 缓存相关

        /// <summary>
        /// 默认缓存
        /// </summary>
        public static CacheType CacheType { get; } = CacheType.SystemCache;

        /// <summary>
        /// Redis配置字符串
        /// </summary>
        public static string RedisConfig { get; } = "localhost:6379";

        /// <summary>
        /// 是否开启Redis缓存
        /// </summary>
        public static bool OpenRedisCache { get; } = false;

        #endregion

        #region 日志相关

        /// <summary>
        /// 日志记录方式
        /// </summary>
        public static LoggerType LoggerType { get; set; } = LoggerType.RDBMS;

        /// <summary>
        /// ElasticSearch服务器配置
        /// </summary>
        public static Uri[] ElasticSearchNodes { get; set; } = new Uri[] { new Uri("http://localhost:9200/") };

        #endregion
    }

    /// <summary>
    /// 运行模式
    /// </summary>
    public enum RunModel
    {
        /// <summary>
        /// 本地测试模式
        /// 注:默认Admin账户,不需要登录.只有使用该模式才会有快速开发功能
        /// </summary>
        LocalTest,

        /// <summary>
        /// 发布模式
        /// </summary>
        Publish
    }

    /// <summary>
    /// 默认缓存类型
    /// </summary>
    public enum CacheType
    {
        /// <summary>
        /// 系统缓存
        /// </summary>
        SystemCache,

        /// <summary>
        /// Redis缓存
        /// </summary>
        RedisCache
    }
}