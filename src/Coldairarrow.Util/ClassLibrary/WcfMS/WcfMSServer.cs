using Coldairarrow.Util.Wcf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

namespace Coldairarrow.Util.WcfMS
{
    /// <summary>
    /// 基于Wcf的微服务服务端
    /// </summary>
    public class WcfMSServer : WcfMSBase
    {
        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="config">配置信息</param>
        public WcfMSServer(WcfMSConfig config)
        {
            if (string.IsNullOrEmpty(config.ProjectName))
                throw new Exception("项目名不能为空！");

            InitParamter(config);

            _redisCache = new RedisCache(_redisConfig);
            _redisCache.SetCache(_configCacheKey, config);
            CheckOnline();

            HttpRuntime.Cache[Guid.NewGuid().ToString()] = this;
        }

        #endregion

        #region 私有成员

        private void CheckOnline()
        {
            TimerHelper.SetInterval(() =>
            {
                try
                {
                    List<string> serviceNames = GetAllServiceNames();
                    serviceNames?.ForEach(aServiceName =>
                    {
                        List<string> serviceUrls = GetServiceUrls(aServiceName);
                        serviceUrls?.ForEach(aServiceUrl =>
                        {
                            IBaseWcfMSService aService = GetService<IBaseWcfMSService>(aServiceUrl);
                            Task task = Task.Factory.StartNew(() =>
                            {
                                try
                                {
                                    bool isOnline = aService.IsOnline();
                                }
                                catch
                                {

                                }
                            });
                            if (!task.Wait(100))
                            {
                                RemoveServiceUrl(aServiceName, aServiceUrl);
                            }
                        });
                    });
                }
                catch
                {

                }
            }, new TimeSpan(0, 0, 1));
        }

        #endregion

        #region 外部接口

        /// <summary>
        /// 注册服务
        /// 注：自动获取一个可用端口
        /// </summary>
        /// <typeparam name="IService">服务接口</typeparam>
        /// <typeparam name="Service">服务实现</typeparam>
        public void RegisterService<IService, Service>() where Service : BaseWcfMSService, IBaseWcfMSService, IService where IService : IBaseWcfMSService
        {
            RegisterService<IService, Service>(IpHelper.GetFirstAvailablePort());
        }

        /// <summary>
        /// 注册服务
        /// 注：指定端口
        /// </summary>
        /// <typeparam name="IService">服务接口</typeparam>
        /// <typeparam name="Service">服务实现</typeparam>
        public void RegisterService<IService, Service>(int port) where Service : BaseWcfMSService, IBaseWcfMSService, IService where IService : IBaseWcfMSService
        {
            string ipAddress = string.IsNullOrEmpty(_ipAddress) ? IpHelper.GetLocalIp() : _ipAddress;
            string serviceName = typeof(IService).Name;
            string cacheKey = BuildCacheKey(serviceName);
            string serviceUrl = $"http://{ipAddress}:{port}/{serviceName}";

            WcfHost<IService, Service> newHost = null;
            if (!_openSecurity)
            {
                newHost = new WcfHost<IService, Service>(serviceUrl);
            }
            else
            {
                if (string.IsNullOrEmpty(_userName) || string.IsNullOrEmpty(_password))
                    newHost = new WcfHost<IService, Service>(serviceUrl, true);
                else
                    newHost = new WcfHost<IService, Service>(serviceUrl, true) { UserName = _userName, Password = _password };
            }
            bool succcess = newHost.StartHost();
            if (!succcess)
                throw new Exception($"注册服务:{serviceName}到:{serviceUrl}失败！");
            HttpRuntime.Cache[Guid.NewGuid().ToString()] = newHost;

            //更新服务地址缓存
            var urls = _redisCache.GetCache<List<string>>(cacheKey);
            if (urls == null)
                urls = new List<string>();
            if (!urls.Contains(serviceUrl))
                urls.Add(serviceUrl);
            _redisCache.SetCache(cacheKey, urls);

            //更新该项目所有服务
            string allServicesNamesCacheKey = BuildCacheKey("allServices");
            List<string> serviceNames = _redisCache.GetCache<List<string>>(allServicesNamesCacheKey);
            if (serviceNames == null)
                serviceNames = new List<string>();
            if (!serviceNames.Contains(serviceName))
                serviceNames.Add(serviceName);
            _redisCache.SetCache(allServicesNamesCacheKey, serviceNames);
        }

        #endregion
    }
}
