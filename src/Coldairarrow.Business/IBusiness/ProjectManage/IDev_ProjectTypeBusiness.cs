using Coldairarrow.Entity.ProjectManage;
using Coldairarrow.Util;
using System.Collections.Generic;

namespace Coldairarrow.Business.ProjectManage
{
    public interface IDev_ProjectTypeBusiness
    {
        List<Dev_ProjectType> GetDataList(Pagination pagination, string condition, string keyword);
        Dev_ProjectType GetTheData(string id);
        void AddData(Dev_ProjectType newData);
        void UpdateData(Dev_ProjectType theData);
        void DeleteData(List<string> ids);
    }
}