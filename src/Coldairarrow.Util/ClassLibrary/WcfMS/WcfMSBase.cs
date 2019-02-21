using Coldairarrow.Util.Wcf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Coldairarrow.Util.WcfMS
{
    /// <summary>
    /// 基于Wcf的微服务抽象基类
    /// </summary>
    public abstract class WcfMSBase
    {
        #region 私有成员

        protected string _projectName { get; set; }
        public string _redisConfig { get; set; } = "localhost:6379";
        public string _ipAddress { get; set; }
        public bool _openSecurity { get; set; } = false;
        public string _userName { get; set; }
        public string _password { get; set; }
        protected string _cacheKey { get => $"{_projectName}_WcfMS"; }
        protected string _configCacheKey { get => $"{_cacheKey}_config"; }
        protected RedisCache _redisCache { get; set; }
        protected ConcurrentDictionary<string, IBaseWcfMSService> _services { get; } = new ConcurrentDictionary<string, IBaseWcfMSService>();
        protected string BuildCacheKey(string key)
        {
            return $"{_cacheKey}_{key}";
        }
        protected Random _random { get; } = new Random();
        protected void InitParamter(WcfMSConfig config)
        {
            _projectName = config.ProjectName;
            _ipAddress = config.IpAddress;
            _openSecurity = config.OpenSecurity;
            _redisConfig = config.RedisConfig;
            _userName = config.UserName;
            _password = config.Password;
        }

        /// <summary>
        /// 获取服务
        /// </summary>
        /// <param name="serviceUrl">服务地址</param>
        /// <typeparam name="IService">服务接口</typeparam>
        /// <returns></returns>
        protected IService GetService<IService>(string serviceUrl) where IService : IBaseWcfMSService
        {
            IBaseWcfMSService theService = null;
            if (!_services.ContainsKey(serviceUrl))
            {
                theService = WcfClient.GetService<IService>(serviceUrl, _openSecurity, _userName, _password);
                _services[serviceUrl] = theService;
            }
            else
                theService = _services[serviceUrl];

            return (IService)theService;
        }

        /// <summary>
        /// 获取所有服务名
        /// </summary>
        /// <returns></returns>
        protected List<string> GetAllServiceNames()
        {
            string allServicesNamesCacheKey = BuildCacheKey("allServices");
            List<string> serviceNames = _redisCache.GetCache<List<string>>(allServicesNamesCacheKey);

            return serviceNames;
        }

        /// <summary>
        /// 获取服务名下的所有服务地址
        /// </summary>
        /// <param name="serviceName">服务名</param>
        /// <returns></returns>
        protected List<string> GetServiceUrls(string serviceName)
        {
            string cacheKey = BuildCacheKey(serviceName);
            var serviceUrls = _redisCache.GetCache<List<string>>(cacheKey);

            return serviceUrls;
        }

        /// <summary>
        /// 移除服务地址
        /// </summary>
        /// <param name="serviceName">服务名</param>
        /// <param name="serviceUrl">服务地址</param>
        protected void RemoveServiceUrl(string serviceName, string serviceUrl)
        {
            string cacheKey = BuildCacheKey(serviceName);
            List<string> serviceUrls = GetServiceUrls(serviceName);
            if (serviceUrls != null)
            {
                serviceUrls.Remove(serviceUrl);
                _redisCache.SetCache(cacheKey, serviceUrls);
            }
        }

        #endregion
    }
}
