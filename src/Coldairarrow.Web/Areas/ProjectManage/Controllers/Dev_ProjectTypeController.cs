using Coldairarrow.Business.ProjectManage;
using Coldairarrow.Entity.ProjectManage;
using Coldairarrow.Util;
using System.Web.Mvc;

namespace Coldairarrow.Web.Areas.ProjectManage.Controllers
{
    public class Dev_ProjectTypeController : BaseMvcController
    {
        #region DI

        public Dev_ProjectTypeController(IDev_ProjectTypeBusiness dev_ProjectTypeBus)
        {
            _dev_ProjectTypeBus = dev_ProjectTypeBus;
        }
        IDev_ProjectTypeBusiness _dev_ProjectTypeBus { get; }

        #endregion

        #region 视图功能

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Form(string id)
        {
            var theData = id.IsNullOrEmpty() ? new Dev_ProjectType() : _dev_ProjectTypeBus.GetTheData(id);

            return View(theData);
        }

        #endregion

        #region 获取数据

        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <param name="pagination">分页参数</param>
        /// <param name="condition">查询类型</param>
        /// <param name="keyword">关键字</param>
        /// <returns></returns>
        public ActionResult GetDataList(Pagination pagination, string condition, string keyword)
        {
            var dataList = _dev_ProjectTypeBus.GetDataList(pagination, condition, keyword);

            return DataTable_Bootstrap(dataList, pagination);
        }

        #endregion

        #region 提交数据

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="theData">保存的数据</param>
        public ActionResult SaveData(Dev_ProjectType theData)
        {
            if (theData.Id.IsNullOrEmpty())
            {
                theData.Id = IdHelper.GetId();

                _dev_ProjectTypeBus.AddData(theData);
            }
            else
            {
                _dev_ProjectTypeBus.UpdateData(theData);
            }

            return Success();
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="theData">删除的数据</param>
        public ActionResult DeleteData(string ids)
        {
            _dev_ProjectTypeBus.DeleteData(ids.ToList<string>());

            return Success("删除成功！");
        }

        #endregion
    }
}