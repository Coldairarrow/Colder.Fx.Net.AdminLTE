using Coldairarrow.Entity.Base_SysManage;
using Coldairarrow.Util;
using System.Collections.Generic;

namespace Coldairarrow.Business.Base_SysManage
{
    public interface IBase_SysRoleBusiness : IDependency
    {
        #region 外部接口

        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <param name="condition">查询类型</param>
        /// <param name="keyword">关键字</param>
        /// <returns></returns>
        List<Base_SysRole> GetDataList(string condition, string keyword, Pagination pagination);

        /// <summary>
        /// 获取指定的单条数据
        /// </summary>
        /// <param name="id">主键</param>
        /// <returns></returns>
        Base_SysRole GetTheData(string id);

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
}