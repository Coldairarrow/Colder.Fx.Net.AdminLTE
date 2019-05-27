using Coldairarrow.Entity.Base_SysManage;
using Coldairarrow.Util;
using System.Collections.Generic;
using static Coldairarrow.Entity.Base_SysManage.EnumType;

namespace Coldairarrow.Business.Base_SysManage
{
    public interface IBase_SysRoleBusiness
    {
        #region 外部接口

        List<Base_SysRoleDTO> GetDataList(Pagination pagination, string roldId = null, string roleName = null);

        /// <summary>
        /// 获取指定的单条数据
        /// </summary>
        /// <param name="id">主键</param>
        /// <returns></returns>
        Base_SysRole GetTheData(string id);

        Base_SysRoleDTO GetTheInfo(string id);

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="newData">数据</param>
        void AddData(Base_SysRole newData);

        /// <summary>
        /// 更新数据
        /// </summary>
        void UpdateData(Base_SysRole theData);

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="theData">删除的数据</param>
        void DeleteData(List<string> ids);

        /// <summary>
        /// 保存权限
        /// </summary>
        /// <param name="roleId">角色Id</param>
        /// <param name="permissions">权限值</param>
        void SavePermission(string roleId, List<string> permissions);

        #endregion

        #region 私有成员

        #endregion

        #region 数据模型

        #endregion
    }

    public class Base_SysRoleDTO: Base_SysRole
    {
        public RoleType? RoleType { get => RoleName?.ToEnum<RoleType>(); }
    }
}