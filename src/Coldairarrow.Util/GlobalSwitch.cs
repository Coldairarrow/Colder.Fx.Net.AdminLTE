﻿using System;

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
        public static readonly string ProjectName = "Colder.Fx.Net.AdminLTE";

        /// <summary>
        /// 网站根地址
        /// </summary>
        public static string WebRootUrl
        {
            get
            {
                if (RunModel == RunModel.LocalTest)
                    return DebugWebRootUrl;
                else
                    return PublishWebRootUrl;
            }
        }

        public const string PublishWebRootUrl = "http://localhost:9599";
        public const string DebugWebRootUrl = "http://localhost:9599";

        #endregion

        #region 运行

        /// <summary>
        /// 运行模式
        /// </summary>
        public static readonly RunModel RunModel = RunModel.LocalTest;

        #endregion

        #region 数据库相关

        /// <summary>
        /// 默认数据库类型
        /// </summary>
        public static readonly DatabaseType DatabaseType = DatabaseType.SqlServer;

        /// <summary>
        /// 默认数据库连接名
        /// </summary>
        public static readonly string DefaultDbConName = "BaseDb";

        /// <summary>
        /// 默认实体命名空间
        /// </summary>
        public static readonly string DefaultEntityNamespace = "Coldairarrow.Entity";

        #endregion

        #region 缓存相关

        /// <summary>
        /// 默认缓存
        /// </summary>
        public static readonly CacheType CacheType = CacheType.SystemCache;

        /// <summary>
        /// Redis配置字符串
        /// </summary>
        public static readonly string RedisConfig = "localhost:6379";

        /// <summary>
        /// 是否开启Redis缓存
        /// </summary>
        public static readonly bool OpenRedisCache = false;

        #endregion

        #region 日志相关

        /// <summary>
        /// 日志记录方式
        /// </summary>
        public static readonly LoggerType LoggerType = LoggerType.RDBMS;

        /// <summary>
        /// ElasticSearch服务器配置
        /// </summary>
        public static readonly Uri[] ElasticSearchNodes = new Uri[] { new Uri("http://localhost:9200/") };

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