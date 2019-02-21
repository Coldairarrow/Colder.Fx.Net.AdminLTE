using System;
using System.IdentityModel.Selectors;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Coldairarrow.Util.Wcf
{
    /// <summary>
    /// Wcf服务代码控制类（必须开启管理员权限）
    /// </summary>
    /// <typeparam name="IService">服务接口</typeparam>
    /// <typeparam name="Service">服务处理</typeparam>
    public class WcfHost<IService, Service> : IWcfHost where Service : IService
    {
        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceUrl">服务地址</param>
        /// <param name="openSecurity">是否开启安全校验</param>
        /// <param name="wsHttpBinding">自定义绑定</param>
        public WcfHost(string serviceUrl = "http://127.0.0.1:14725", bool openSecurity = false, WSHttpBinding wsHttpBinding = null)
        {
            _serviceUrl = serviceUrl;
            OpenSecurity = openSecurity;
            if (wsHttpBinding == null)
                _wsHttpBinding = OpenSecurity ? WcfHelper.GetDefaultBinding(true) : WcfHelper.GetDefaultBinding(false);
            else
                _wsHttpBinding = wsHttpBinding;
            InitService();
        }

        #endregion

        #region 私有成员

        private ServiceHost _serviceHost { get; set; }
        private WSHttpBinding _wsHttpBinding { get; set; }
        private string _serviceUrl { get; set; }
        private WSHttpBinding GetDefaultBinding()
        {
            return WcfHelper.GetDefaultBinding();
        }
        private string _userName { get; set; }
        private string _password { get; set; }
        private void InitService()
        {
            _serviceHost = new ServiceHost(typeof(Service));
            _serviceHost.AddServiceEndpoint(typeof(IService), _wsHttpBinding, _serviceUrl);

            if (OpenSecurity)
            {
                //加密传输
                _serviceHost.Credentials.ServiceCertificate.SetCertificate("CN=localhost", StoreLocation.LocalMachine, StoreName.My);
                ////账号密码验证
                _serviceHost.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = System.ServiceModel.Security.UserNamePasswordValidationMode.Custom;
                _serviceHost.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = new CustUsernamepwdValidator(UserName, Password);
            }
        }

        #endregion

        #region 外部接口

        /// <summary>
        /// 开始Wcf服务
        /// </summary>
        public bool StartHost()
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    if (HandleHostOpened != null)
                        _serviceHost.Opened += new EventHandler(HandleHostOpened);

                    if (_serviceHost.State != CommunicationState.Opened)
                    {
                        _serviceHost.Open();
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    HandleException?.Invoke(ex);
                    return false;
                }
            }).Result;
        }

        /// <summary>
        /// 开启安全校验
        /// </summary>
        public bool OpenSecurity { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get => string.IsNullOrEmpty(_userName) ? WcfHelper.UserName : _userName; set => _userName = value; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get => string.IsNullOrEmpty(_password) ? WcfHelper.Password : _password; set => _password = value; }

        #endregion

        #region 事件处理

        /// <summary>
        /// 当Wcf服务开启后执行
        /// </summary>
        public Action<object, EventArgs> HandleHostOpened { get; set; }

        /// <summary>
        /// 异常处理
        /// </summary>
        public Action<Exception> HandleException { get; set; }

        #endregion

        #region 其它

        /// <summary>
        /// 自定义账号密码校验
        /// </summary>
        private class CustUsernamepwdValidator : UserNamePasswordValidator
        {
            public CustUsernamepwdValidator(string userName, string password)
            {
                _userName = userName;
                _password = password;
            }
            private string _userName { get; }
            private string _password { get; }
            public override void Validate(string userName, string password)
            {
                if (userName != _userName || password != _password)
                {
                    throw new Exception("用户名或密码错误。");
                }
            }
        }

        #endregion
    }
}
