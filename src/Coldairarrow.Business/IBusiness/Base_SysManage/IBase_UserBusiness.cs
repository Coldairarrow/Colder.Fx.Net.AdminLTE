using Coldairarrow.Entity.Base_SysManage;
using Coldairarrow.Util;
using System.Collections.Generic;

namespace Coldairarrow.Business.Base_SysManage
{
    public interface IBase_UserBusiness : IDependency
    {
        List<Base_UserDTO> GetDataList(string condition, string keyword, Pagination pagination);

        /// <summary>
        /// 获取指定的单条数据
        /// </summary>
        /// <param name="id">主键</param>
        /// <returns></returns>
        Base_User GetTheData(string id);

        void AddData(Base_User newData);

        /// <summary>
        /// 更新数据
        /// </summary>
        void UpdateData(Base_User theData);
        void SetUserRole(string userId, List<string> roleIds);

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="theData">删除的数据</param>
        void DeleteData(List<string> ids);

        /// <summary>
        /// 获取当前操作者信息
        /// </summary>
        /// <returns></returns>
        Base_UserDTO GetCurrentUser();

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <returns></returns>
        Base_UserDTO GetTheUser(string userId);

        List<string> GetUserRoleIds(string userId);

        /// <summary>
        /// 更改密码
        /// </summary>
        /// <param name="oldPwd">老密码</param>
        /// <param name="newPwd">新密码</param>
        AjaxResult ChangePwd(string oldPwd, string newPwd);

        /// <summary>
        /// 保存权限
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="permissions">权限值</param>
        void SavePermission(string userId, List<string> permissions);

        List<Base_User> BuildSelectResult(string selectedValueJson, string q, string textFiled, string valueField);
    }

    public class Base_UserDTO : Base_User
    {
        public string RoleNames { get => string.Join(",", RoleNameList); }

        public List<string> RoleIdList { get; set; }

        public List<string> RoleNameList { get; set; }

        public EnumType.RoleType RoleType
        {
            get
            {
                int type = 0;

                var values = typeof(EnumType.RoleType).GetEnumValues();
                foreach (var aValue in values)
                {
                    if (RoleNames.Contains(aValue.ToString()))
                        type += (int)aValue;
                }

                return (EnumType.RoleType)type;
            }
        }
    }
}