using System;
using System.ServiceProcess;

namespace Coldairarrow.Util.WindowsService
{
    partial class WindowsService : ServiceBase
    {
        #region 构造函数
        public WindowsService()
        {
            InitializeComponent();
        }

        #endregion

        #region 保护成员
        protected override void OnStart(string[] args)
        {
            HandleOnStart?.BeginInvoke(args,null,null);
        }

        protected override void OnStop()
        {
            HandleOnStop?.BeginInvoke(null,null);
        }

        #endregion

        #region 事件处理

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
