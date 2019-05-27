using AutoMapper;
using Coldairarrow.Business.Cache;
using Coldairarrow.Entity.Base_SysManage;
using Coldairarrow.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;

namespace Coldairarrow.Business.Base_SysManage
{
    public class Base_UserBusiness : BaseBusiness<Base_User>, IBase_UserBusiness, IDependency
    {
        #region DI

        public Base_UserBusiness(IBase_UserDTOCache sysUserCache, IOperator @operator, IPermissionManage permissionManage)
        {
            _sysUserCache = sysUserCache;
            _operator = @operator;
            _permissionManage = permissionManage;
        }
        IBase_UserDTOCache _sysUserCache { get; }
        IOperator _operator { get; }
        IPermissionManage _permissionManage { get; }

        #endregion

        public override EnumType.LogType LogType => EnumType.LogType.系统用户管理;

        #region 外部接口

        public List<Base_UserDTO> GetDataList(Pagination pagination, string userId = null, string keyword = null)
        {
            var where = LinqHelper.True<Base_User>();
            if (!userId.IsNullOrEmpty())
                where = where.And(x => x.Id == userId);
            if (!keyword.IsNullOrEmpty())
            {
                where = where.And(x =>
                    x.UserName.Contains(keyword)
                    || x.RealName.Contains(keyword));
            }

            var list = GetIQueryable().Where(where).GetPagination(pagination).ToList()
                .Select(x => Mapper.Map<Base_UserDTO>(x))
                .ToList();

            SetProperty(list);

            return list;

            void SetProperty(List<Base_UserDTO> users)
            {
                //补充用户角色属性
                List<string> userIds = users.Select(x => x.Id).ToList();
                var userRoles = (from a in Service.GetIQueryable<Base_UserRoleMap>()
                                 join b in Service.GetIQueryable<Base_SysRole>() on a.RoleId equals b.Id
                                 where userIds.Contains(a.UserId)
                                 select new
                                 {
                                     a.UserId,
                                     RoleId= b.Id,
                                     b.RoleName
                                 }).ToList();
                users.ForEach(aUser =>
                {
                    var roleList = userRoles.Where(x => x.UserId == aUser.Id);
                    aUser.RoleIdList = roleList.Select(x => x.RoleId).ToList();
                    aUser.RoleNameList = roleList.Select(x => x.RoleName).ToList();
                });
            }
        }

        /// <summary>
        /// 获取指定的单条数据
        /// </summary>
        /// <param name="id">主键</param>
        /// <returns></returns>
        public Base_User GetTheData(string id)
        {
            return GetEntity(id);
        }

        [DataAddLog(EnumType.LogType.系统用户管理, "用户", "RealName")]
        [DataRepeatValidate(
            new string[] { "UserName" },
            new string[] { "用户名" })]
        public void AddData(Base_User newData)
        {
            Insert(newData);
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        [DataEditLog(EnumType.LogType.系统用户管理, "用户", "RealName")]
        [DataRepeatValidate(
            new string[] { "UserName" },
            new string[] { "用户名" })]
        public void UpdateData(Base_User theData)
        {
            if (theData.Id == "Admin" && _operator.UserId != theData.Id)
                throw new Exception("禁止更改超级管理员！");

            Update(theData);
            _sysUserCache.UpdateCache(theData.Id);
        }

        [DataDeleteLog(EnumType.LogType.系统用户管理, "用户", "RealName")]
        public void DeleteData(List<string> ids)
        {
            var adminUser = GetTheUser("Admin");
            if (ids.Contains(adminUser.Id))
                throw new Exception("超级管理员是内置账号,禁止删除！");
            var userIds = GetIQueryable().Where(x => ids.Contains(x.Id)).Select(x => x.Id).ToList();

            Delete(ids);
            _sysUserCache.UpdateCache(userIds);
        }

        public void SetUserRole(string userId, List<string> roleIds)
        {
            Service.Delete_Sql<Base_UserRoleMap>(x => x.UserId == userId);
            var insertList = roleIds.Select(x => new Base_UserRoleMap
            {
                Id = GuidHelper.GenerateKey(),
                UserId = userId,
                RoleId = x
            }).ToList();

            Service.Insert(insertList);
            _sysUserCache.UpdateCache(userId);
            _permissionManage.UpdateUserPermissionCache(userId);
        }

        public Base_UserDTO GetTheUser(string userId)
        {
            return _sysUserCache.GetCache(userId);
        }

        public List<string> GetUserRoleIds(string userId)
        {
            return GetTheUser(userId).RoleIdList;
        }

        /// <summary>
        /// 更改密码
        /// </summary>
        /// <param name="oldPwd">老密码</param>
        /// <param name="newPwd">新密码</param>
        public AjaxResult ChangePwd(string oldPwd, string newPwd)
        {
            AjaxResult res = new AjaxResult() { Success = true };
            string userId = _operator.UserId;
            oldPwd = oldPwd.ToMD5String();
            newPwd = newPwd.ToMD5String();
            var theUser = GetIQueryable().Where(x => x.Id == userId && x.Password == oldPwd).FirstOrDefault();
            if (theUser == null)
            {
                res.Success = false;
                res.Msg = "原密码不正确！";
            }
            else
            {
                theUser.Password = newPwd;
                Update(theUser);
            }

            _sysUserCache.UpdateCache(userId);

            return res;
        }

        /// <summary>
        /// 保存权限
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="permissions">权限值</param>
        public void SavePermission(string userId, List<string> permissions)
        {
            Service.Delete_Sql<Base_PermissionUser>(x => x.UserId == userId);
            var roleIdList = Service.GetIQueryable<Base_UserRoleMap>().Where(x => x.UserId == userId).Select(x => x.RoleId).ToList();
            var existsPermissions = Service.GetIQueryable<Base_PermissionRole>()
                .Where(x => roleIdList.Contains(x.RoleId) && permissions.Contains(x.PermissionValue))
                .GroupBy(x => x.PermissionValue)
                .Select(x => x.Key)
                .ToList();
            permissions.RemoveAll(x => existsPermissions.Contains(x));

            List<Base_PermissionUser> insertList = new List<Base_PermissionUser>();
            permissions.ForEach(newPermission =>
            {
                insertList.Add(new Base_PermissionUser
                {
                    Id = Guid.NewGuid().ToSequentialGuid(),
                    UserId = userId,
                    PermissionValue = newPermission
                });
            });

            Service.Insert(insertList);
        }

        #endregion

        #region 私有成员

        #endregion

        #region 数据模型

        #endregion
    }
}