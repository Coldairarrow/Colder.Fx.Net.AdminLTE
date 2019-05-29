using Coldairarrow.Entity.ProjectManage;
using Coldairarrow.Util;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;

namespace Coldairarrow.Business.ProjectManage
{
    public class Dev_ProjectTypeBusiness : BaseBusiness<Dev_ProjectType>, IDev_ProjectTypeBusiness, IDependency
    {
        #region 外部接口

        public List<Dev_ProjectType> GetDataList(Pagination pagination, string condition, string keyword)
        {
            var q = GetIQueryable();
            //筛选
            if (!condition.IsNullOrEmpty() && !keyword.IsNullOrEmpty())
                q = q.Where($@"{condition}.Contains(@0)", keyword);

            return q.GetPagination(pagination).ToList();
        }

        public Dev_ProjectType GetTheData(string id)
        {
            return GetEntity(id);
        }

        public void AddData(Dev_ProjectType newData)
        {
            Insert(newData);
        }

        public void UpdateData(Dev_ProjectType theData)
        {
            Update(theData);
        }

        public void DeleteData(List<string> ids)
        {
            Delete(ids);
        }

        #endregion

        #region 私有成员

        #endregion

        #region 数据模型

        #endregion
    }
}