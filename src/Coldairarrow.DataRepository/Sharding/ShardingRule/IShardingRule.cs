namespace Coldairarrow.DataRepository
{
    /// <summary>
    /// 分片规则接口
    /// </summary>
    public interface IShardingRule
    {
        string FindTable(object obj);
    }
}
