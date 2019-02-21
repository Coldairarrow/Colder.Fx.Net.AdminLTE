using System;
using System.ServiceModel;
using System.Text;

namespace Coldairarrow.Util.Wcf
{
    static class WcfHelper
    {
        #region 参数配置

        public static string UserName { get; set; } = "DefaultWcfUserName";
        public static string Password { get; set; } = "5d02d7cd745b04de1b21209f327a8d08";

        #endregion

        #region 外部接口

        /// <summary>
        /// 获取默认的绑定类型
        /// 注：默认忽略安全校验,速度最快
        /// </summary>
        /// <returns></returns>
        public static WSHttpBinding GetDefaultBinding()
        {
            return GetDefaultBinding(false);
        }

        /// <summary>
        /// 获取默认的绑定类型
        /// 注：是否开启安全校验,若开启安全校验,则会影响一定性能
        /// </summary>
        /// <param name="openSecurity">是否开启安全校验</param>
        /// <returns></returns>
        public static WSHttpBinding GetDefaultBinding(bool openSecurity)
        {
            //设置默认绑定
            WSHttpBinding defaultWSHttpBinding = new WSHttpBinding
            {
                CloseTimeout = TimeSpan.Parse("00:01:00"),
                OpenTimeout = TimeSpan.Parse("00:01:00"),
                ReceiveTimeout = TimeSpan.Parse("00:10:00"),
                SendTimeout = TimeSpan.Parse("00:01:00"),
                BypassProxyOnLocal = false,
                TransactionFlow = false,
                HostNameComparisonMode = HostNameComparisonMode.StrongWildcard,
                MaxBufferPoolSize = int.MaxValue,
                MaxReceivedMessageSize = int.MaxValue,
                MessageEncoding = WSMessageEncoding.Text,
                TextEncoding = Encoding.UTF8,
                UseDefaultWebProxy = true
            };

            //数据长度
            defaultWSHttpBinding.ReaderQuotas.MaxDepth = int.MaxValue;
            defaultWSHttpBinding.ReaderQuotas.MaxStringContentLength = int.MaxValue;
            defaultWSHttpBinding.ReaderQuotas.MaxArrayLength = int.MaxValue;
            defaultWSHttpBinding.ReaderQuotas.MaxBytesPerRead = int.MaxValue;
            defaultWSHttpBinding.ReaderQuotas.MaxNameTableCharCount = int.MaxValue;

            defaultWSHttpBinding.ReliableSession.Ordered = true;
            defaultWSHttpBinding.ReliableSession.InactivityTimeout = TimeSpan.Parse("00:10:00");
            defaultWSHttpBinding.ReliableSession.Enabled = false;

            //安全协议
            if (openSecurity)
            {
                defaultWSHttpBinding.Security.Mode = SecurityMode.Message;
                defaultWSHttpBinding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
            }
            else
            {
                defaultWSHttpBinding.Security.Mode = SecurityMode.None;
                defaultWSHttpBinding.Security.Message.ClientCredentialType = MessageCredentialType.None;
            }
            defaultWSHttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            defaultWSHttpBinding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;

            return defaultWSHttpBinding;
        }

        #endregion
    }
}
