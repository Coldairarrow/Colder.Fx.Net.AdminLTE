using Coldairarrow.Util;
using System.Collections.Generic;

namespace Coldairarrow.Web
{
    /// <summary>
    /// 系统菜单管理
    /// </summary>
    public interface ISystemMenuManage : IDependency
    {
        #region 外部接口

        /// <summary>
        /// 获取系统所有菜单
        /// </summary>
        /// <returns></returns>
        List<Menu> GetAllSysMenu();

        /// <summary>
        /// 获取用户菜单
        /// </summary>
        /// <returns></returns>
        List<Menu> GetOperatorMenu();

        #endregion
    }

    #region 数据模型

    public class Menu
    {
        public string id { get => url ?? GuidHelper.GenerateKey(); }
        public string text { get; set; }
        public string icon { get; set; }
        public string url { get => SystemMenuManage.GetUrl(_url); set => _url = value; }
        public string _url { get; set; }
        public string Permission { get; set; }
        public bool IsShow { get; set; } = true;
        public string targetType { get; } = "iframe-tab";
        public bool isHeader { get; } = false;
        public List<Menu> children { get; set; }
    }

    #endregion
}