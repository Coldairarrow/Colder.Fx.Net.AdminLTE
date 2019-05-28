using Coldairarrow.Business.Base_SysManage;
using Coldairarrow.Entity.Base_SysManage;
using Coldairarrow.Util;
using System;
using System.Web.Mvc;

namespace Coldairarrow.Web.Areas.Base_SysManage.Controllers
{
    public class Base_DatabaseLinkController : BaseMvcController
    {
        public Base_DatabaseLinkController(IBase_DatabaseLinkBusiness dbLinkBus)
        {
            _dbLinkBus = dbLinkBus;
        }
        IBase_DatabaseLinkBusiness _dbLinkBus { get; }

        #region 视图功能

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Form(string id)
        {
            var theData = id.IsNullOrEmpty() ? new Base_DatabaseLink() : _dbLinkBus.GetTheData(id);

            return View(theData);
        }

        #endregion

        #region 获取数据

        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <param name="condition">查询类型</param>
        /// <param name="keyword">关键字</param>
        /// <returns></returns>
        public ActionResult GetDataList(string condition, string keyword, Pagination pagination)
        {
            var dataList = _dbLinkBus.GetDataList(condition, keyword, pagination);

            return Content(pagination.BuildTableResult_DataGrid(dataList).ToJson());
        }

        #endregion

        #region 提交数据

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="theData">保存的数据</param>
        public ActionResult SaveData(Base_DatabaseLink theData)
        {
            if(theData.Id.IsNullOrEmpty())
            {
                theData.Id = IdHelper.GetId();

                _dbLinkBus.AddData(theData);
            }
            else
            {
                _dbLinkBus.UpdateData(theData);
            }

            return Success();
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="theData">删除的数据</param>
        public ActionResult DeleteData(string ids)
        {
            _dbLinkBus.DeleteData(ids.ToList<string>());

            return Success("删除成功！");
        }

        #endregion
    }
}