using System;
using System.Web;
using System.Web.Caching;

namespace Coldairarrow.Util
{
    /// <summary>
    /// 系统缓存帮助类
    /// </summary>
    public class SystemCache : ICache
    {
        public object GetCache(string key)
        {
            return HttpRuntime.Cache[key];
        }

        public T GetCache<T>(string key) where T : class
        {
            return (T)HttpRuntime.Cache[key];
        }

        public bool ContainsKey(string key)
        {
            return GetCache(key) != null;
        }

        public void RemoveCache(string key)
        {
            HttpRuntime.Cache.Remove(key);
        }

        public void SetKeyExpire(string key, TimeSpan expire)
        {
            object value = GetCache(key);
            SetCache(key, value, expire);
        }

        public void SetCache(string key, object value)
        {
            _SetCache(key, value, null, null);
        }

        public void SetCache(string key, object value, TimeSpan timeout)
        {
            _SetCache(key, value, timeout, ExpireType.Absolute);
        }

        public void SetCache(string key, object value, TimeSpan timeout, ExpireType expireType)
        {
            _SetCache(key, value, timeout, expireType);
        }

        private void _SetCache(string key, object value, TimeSpan? timeout, ExpireType? expireType)
        {
            if (timeout == null)
                HttpRuntime.Cache[key] = value;
            else
            {
                if (expireType == ExpireType.Absolute)
                {
                    DateTime endTime = DateTime.Now.AddTicks(timeout.Value.Ticks);
                    HttpRuntime.Cache.Insert(key, value, null, endTime, Cache.NoSlidingExpiration);
                }
                else
                {
                    HttpRuntime.Cache.Insert(key, value, null, Cache.NoAbsoluteExpiration, timeout.Value);
                }
            }
        }
    }
}