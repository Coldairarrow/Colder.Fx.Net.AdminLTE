using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Coldairarrow.Util.Wcf
{
    /// <summary>
    /// Wcf客户端
    /// </summary>
    public static class WcfClient
    {
        #region 外部接口

        /// <summary>
        /// 获取服务
        /// 注：速度最快,无安全校验
        /// </summary>
        /// <typeparam name="T">服务接口</typeparam>
        /// <param name="serviceUrl">服务地址</param>
        /// <returns></returns>
        public static T GetService<T>(string serviceUrl)
        {
            return GetService<T>(serviceUrl, false, null, null);
        }

        /// <summary>
        /// 获取服务
        /// 注：若开启安全校验则使用默认账号密码
        /// </summary>
        /// <typeparam name="T">服务接口</typeparam>
        /// <param name="serviceUrl">服务地址</param>
        /// <param name="openSecurity">是否开启安全校验</param>
        /// <returns></returns>
        public static T GetService<T>(string serviceUrl,bool openSecurity)
        {
            return GetService<T>(serviceUrl, openSecurity, null, null);
        }

        /// <summary>
        /// 获取服务
        /// 注：自定义是否开启安全校验并指定账号密码
        /// </summary>
        /// <typeparam name="T">服务接口</typeparam>
        /// <param name="serviceUrl">服务地址</param>
        /// <param name="openSecurity">是否开启安全校验</param>
        /// <param name="userName">账号</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        public static T GetService<T>(string serviceUrl, bool openSecurity, string userName,string password)
        {
            var binding = GetBinding(openSecurity);

            EndpointAddress address = new EndpointAddress(serviceUrl);
            ChannelFactory<T> factory = new ChannelFactory<T>(binding, address);

            if (openSecurity)
            {
                // 以下设置可以忽略客户端对服务器证书的信任验证
                factory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
                // 设置用户名和密码
                if (string.IsNullOrEmpty(userName))
                    userName = WcfHelper.UserName;
                if (string.IsNullOrEmpty(password))
                    password = WcfHelper.Password;

                factory.Credentials.UserName.UserName = userName;
                factory.Credentials.UserName.Password = password;
            }

            return factory.CreateChannel();
        }

        #endregion

        #region 私有成员

        private static Binding GetBinding(bool openSecurity)
        {
            return WcfHelper.GetDefaultBinding(openSecurity);
        }

        #endregion
    }
}
