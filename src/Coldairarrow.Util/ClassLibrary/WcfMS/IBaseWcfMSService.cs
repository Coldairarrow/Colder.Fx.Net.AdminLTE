using System.ServiceModel;

namespace Coldairarrow.Util.WcfMS
{
    /// <summary>
    /// 服务接口基类
    /// </summary>
    [ServiceContract]
    public interface IBaseWcfMSService
    {
        [OperationContract]
        bool IsOnline();
    }
}
