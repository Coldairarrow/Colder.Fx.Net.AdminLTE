using Coldairarrow.Business.Base_SysManage;
using Coldairarrow.Entity.Base_SysManage;
using Coldairarrow.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Coldairarrow.Web
{
    public class Base_UserController : BaseMvcController
    {
        Base_UserBusiness _base_UserBusiness = new Base_UserBusiness();

        #region 视图功能

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Form(string id)
        {
            var theData = id.IsNullOrEmpty() ? new Base_User() : _base_UserBusiness.GetTheData(id);

            return View(theData);
        }

        public ActionResult ChangePwdForm()
        {
            return View();
        }

        public ActionResult PermissionForm(string userId)
        {
            ViewData["userId"] = userId;

            return View();
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
            var dataList = _base_UserBusiness.GetDataList(condition, keyword, pagination);

            return Content(pagination.BuildTableResult_BootstrapTable(dataList).ToJson());
        }

        public ActionResult GetDataList_NoPagin(string q)
        {
            List<SelectResponseModel> resList = new List<SelectResponseModel>();
            var query = q.ToObject<SelectQueryModel>();
            Pagination pagination = new Pagination()
            {
                PageRows = 5
            };
            var where = LinqHelper.True<Base_User>();
            List<Base_User> selected = new List<Base_User>();
            if (query.Selected.Count > 0)
            {
                resList = _base_UserBusiness
                    .GetIQueryable()
                    .Where(x => query.Selected.Contains(x.UserId))
                    .Select(x => new SelectResponseModel
                    {
                        text=x.RealName,
                        value=x.UserId,
                        selected=true
                    }).ToList();
            }
            if (!query.Keyword.IsNullOrEmpty())
            {
                where = where.And(x => x.RealName.Contains(query.Keyword)&&!query.Selected.Contains(x.UserId));
            }
            var keywordList = _base_UserBusiness
                .GetIQueryable().Where(where)
                .GetPagination(pagination)
                .Select(x => new SelectResponseModel
                {
                    text = x.RealName,
                    value = x.UserId,
                    selected = false
                }).ToList();

            return Content(resList.Concat(keywordList).ToJson());
        }

        class SelectQueryModel
        {
            public List<string> Selected { get; set; }
            public string Keyword { get; set; }
        }

        class SelectResponseModel
        {
            public string text { get; set; }
            public string value { get; set; }
            public bool selected { get; set; }
        }

        #endregion

        #region 提交数据

        public ActionResult SaveData(Base_User theData, string Pwd, string RoleIdList)
        {
            if (!Pwd.IsNullOrEmpty())
                theData.Password = Pwd.ToMD5String();

            if (theData.Id.IsNullOrEmpty())
            {
                theData.Id = Guid.NewGuid().ToSequentialGuid();
                theData.UserId = Guid.NewGuid().ToSequentialGuid();

                _base_UserBusiness.AddData(theData);
            }
            else
            {
                _base_UserBusiness.UpdateData(theData);
            }

            //角色设置
            if (!RoleIdList.IsNullOrEmpty())
            {
                _base_UserBusiness.SetUserRole(theData.UserId, RoleIdList.ToList<string>());
            }

            return Success();
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="theData">删除的数据</param>
        public ActionResult DeleteData(string ids)
        {
            _base_UserBusiness.DeleteData(ids.ToList<string>());

            return Success("删除成功！");
        }

        /// <summary>
        /// 更改密码
        /// </summary>
        /// <param name="oldPwd">老密码</param>
        /// <param name="newPwd">新密码</param>
        public ActionResult ChangePwd(string oldPwd, string newPwd)
        {
            var res = _base_UserBusiness.ChangePwd(oldPwd, newPwd);

            return Content(res.ToJson());
        }

        /// <summary>
        /// 保存权限
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="permissions">权限</param>
        /// <returns></returns>
        public ActionResult SavePermission(string userId, string permissions)
        {
            PermissionManage.SetUserPermission(userId, permissions.ToList<string>());

            return Success();
        }

        #endregion
    }
}