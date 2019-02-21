using System;
using System.Collections;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;

namespace Coldairarrow.Util.WindowsService
{
    /// <summary>
    /// Windows服务容器，提供服务安装、卸载等操作
    /// </summary>
    public class WindowsServiceContainer
    {
        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="args">从控制台传过来的参数</param>
        public WindowsServiceContainer(string serviceName, string[] args)
        {
            _serviceName = serviceName;
            _args = args;
            _service = new WindowsService();
        }

        #endregion

        #region 私有成员
        string[] _args;
        WindowsService _service;
        string _serviceName;
        string _displayName;
        string _description;
        private void InitSerice()
        {
            _service.HandleOnStart = HandleOnStart == null ? null : new Action<string[]>(HandleOnStart);
            _service.HandleOnStop = HandleOnStop == null ? null : new Action(HandleOnStop);
        }
        private bool IsServiceExisted()
        {
            return ServiceController.GetServices().ToList().Exists(x => x.ServiceName == _serviceName);
        }
        private ServiceControllerStatus ServiceStatus { get => GetService().Status; }
        private TransactedInstaller GetInstaller()
        {
            try
            {
                TransactedInstaller installer = new TransactedInstaller();
                installer.Installers.Add(new ServiceProcessInstaller
                {
                    Account = ServiceAccount.LocalSystem
                });
                installer.Installers.Add(new ServiceInstaller
                {
                    ServiceName = _serviceName,
                    DisplayName = _displayName,
                    Description = _description,
                    ServicesDependedOn = ServicesDependedOn,
                    StartType = StartType
                });
                installer.Context = new InstallContext();
                installer.Context.Parameters["assemblypath"] = "\"" + Assembly.GetEntryAssembly().Location + "\" /service";

                return installer;
            }
            catch (Exception ex)
            {
                HandleException?.Invoke(ex);
                return null;
            }
        }
        private ServiceController GetService()
        {
            try
            {
                if (!IsServiceExisted())
                {
                    HandleLog?.Invoke("服务不存在，无法启动！");
                    return null;
                }
                else
                {
                    return new ServiceController(_serviceName);
                }
            }
            catch (Exception ex)
            {
                HandleException?.Invoke(ex);
                return null;
            }
        }
        private void StartService()
        {
            try
            {
                ServiceController service = GetService();
                if (service.Status != ServiceControllerStatus.Running &&
                    service.Status != ServiceControllerStatus.StartPending)
                {
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 5));
                    HandleLog?.Invoke("服务启动完成******");
                }
            }
            catch (Exception ex)
            {
                if (ex is System.ServiceProcess.TimeoutException)
                    HandleLog?.Invoke($@"服务5秒内未启动，服务启动超时，启动失败！");

                HandleException?.Invoke(ex);
            }
        }
        private void StopService()
        {
            GetService().Stop();
        }
        private void InstallService()
        {
            GetInstaller().Install(new Hashtable());
        }
        private void UnInstallService()
        {
            try
            {
                ServiceControllerStatus status = ServiceStatus;
                if (status != ServiceControllerStatus.StopPending && status != ServiceControllerStatus.Stopped)
                    StopService();
                GetService().WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 5));
                GetInstaller().Uninstall(null);
            }
            catch (Exception ex)
            {
                if (ex is System.ServiceProcess.TimeoutException)
                    HandleLog?.Invoke($@"服务5秒内未停止，服务停止超时，停止失败,卸载失败！");
                HandleException?.Invoke(ex);
            }
        }
        private Action<string> _handleLog { get; set; }

        #endregion

        #region 外部接口

        /// <summary>
        /// 服务名
        /// </summary>
        public string ServiceName { get => _serviceName; set => _serviceName = value; }

        /// <summary>
        /// 显示名
        /// </summary>
        public string DisplayName { get => string.IsNullOrEmpty(_displayName) ? _serviceName : _displayName; set => _displayName = value; }

        /// <summary>
        /// 服务描述信息
        /// </summary>
        public string Description { get => string.IsNullOrEmpty(_description) ? _serviceName : _description; set => _description = value; }

        /// <summary>
        /// 启动服务必须的前置服务,默认为空
        /// </summary>
        public string[] ServicesDependedOn { get; set; } = new string[0];

        /// <summary>
        /// 服务启动类型,默认为自动
        /// </summary>
        public ServiceStartMode StartType { get; set; } = ServiceStartMode.Automatic;

        /// <summary>
        /// 开始运行
        /// </summary>
        public void Start()
        {
            InitSerice();

            try
            {
                if (_args.Length > 0)
                {
                    ServiceBase.Run(_service);
                }
                else
                {
                    while (true)
                    {
                        try
                        {
                            Console.WriteLine("\n请选择你要执行的操作——1：自动部署服务，2：卸载服务，3：退出");
                            Console.WriteLine("————————————————————");
                            ConsoleKey key = Console.ReadKey().Key;

                            if (key == ConsoleKey.NumPad1 || key == ConsoleKey.D1)
                            {
                                if (IsServiceExisted())
                                {
                                    Console.WriteLine("\n检测到服务已存在，进行原服务卸载......");
                                    UnInstallService();
                                    Console.WriteLine("原服务卸载成功，进行安装新服务......");
                                    InstallService();
                                    Console.WriteLine("新服务安装成功，进行启动服务......");
                                }
                                else
                                {
                                    Console.WriteLine("\n监测到该服务不存在，进行安装此服务......");
                                    InstallService();
                                    Console.WriteLine("服务安装成功，进行启动服务......");
                                }

                                StartService();
                            }
                            else if (key == ConsoleKey.NumPad2 || key == ConsoleKey.D2)
                            {
                                if (IsServiceExisted())
                                {
                                    Console.WriteLine("\n检测到服务已存在，进行服务卸载......");
                                    UnInstallService();
                                    Console.WriteLine("服务卸载成功......");
                                }
                                else
                                    Console.WriteLine("\n该服务未安装，无法进行卸载......");
                            }
                            else if (key == ConsoleKey.NumPad3 || key == ConsoleKey.D3)
                            {
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            HandleException?.BeginInvoke(ex, null, null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException?.Invoke(ex);
            }
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 异常处理
        /// </summary>
        public Action<Exception> HandleException { get; set; }

        /// <summary>
        /// 日志处理
        /// </summary>
        public Action<string> HandleLog
        {
            get
            {
                return _handleLog ?? new Action<string>(log =>
                {
                    Console.WriteLine(log);
                });
            }
            set
            {
                _handleLog = value;
            }
        }

        /// <summary>
        /// 当服务启动时执行操作
        /// </summary>
        public Action<string[]> HandleOnStart { get; set; }

        /// <summary>
        /// 当服务停止时执行的操作
        /// </summary>
        public Action HandleOnStop { get; set; }

        #endregion
    }
}
