using Coldairarrow.Business.Base_SysManage;
using Coldairarrow.Util;
using System.Linq;

namespace Coldairarrow.Business.Cache
{
    public class Base_UserDTOCache : BaseCache<Base_UserDTO>, IBase_UserDTOCache
    {
        public IBase_UserBusiness _sysUserBus { get; set; }
        protected override string _moduleKey => "Base_UserModel";
        protected override Base_UserDTO GetDbData(string key)
        {
            if (key.IsNullOrEmpty())
                return null;

            return _sysUserBus.GetDataList("UserId", key, new Pagination()).FirstOrDefault();
        }
    }
}
