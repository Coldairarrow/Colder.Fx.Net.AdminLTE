namespace Coldairarrow.Util.WcfMS
{
    /// <summary>
    /// 服务基类
    /// </summary>
    public class BaseWcfMSService : IBaseWcfMSService
    {
        /// <summary>
        /// 是否在线
        /// </summary>
        /// <returns></returns>
        public bool IsOnline()
        {
            return true;
        }
    }
}
