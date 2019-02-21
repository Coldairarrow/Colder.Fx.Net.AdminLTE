using System;
using System.Collections.Generic;

namespace Coldairarrow.Util.WcfMS
{
    /// <summary>
    /// 基于Wcf的微服务客户端
    /// </summary>
    public class WcfMSClient : WcfMSBase
    {
        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="projectName">项目名</param>
        /// <param name="redisConfig">redis配置</param>
        public WcfMSClient(string projectName, string redisConfig = "localhost:6379")
        {
            if (string.IsNullOrEmpty(projectName))
                throw new Exception("项目名不能为空！");

            _projectName = projectName;
            _redisCache = new RedisCache(redisConfig);
            WcfMSConfig config = _redisCache.GetCache<WcfMSConfig>(_configCacheKey);
            InitParamter(config);
        }

        #endregion

        #region 外部接口

        /// <summary>
        /// 获取服务
        /// </summary>
        /// <typeparam name="IService">服务接口</typeparam>
        /// <returns></returns>
        public IService GetService<IService>() where IService : IBaseWcfMSService
        {
            string serviceName = typeof(IService).Name;
            string cacheKey = BuildCacheKey(serviceName);
            List<string> urls = _redisCache.GetCache<List<string>>(cacheKey);
            if (urls == null || urls?.Count == 0)
                throw new Exception("服务未注册！");
            int index = _random.Next(0, urls.Count);
            string serviceUrl = urls[index];

            return GetService<IService>(serviceUrl);
        }

        #endregion
    }
}
