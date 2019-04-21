namespace Coldairarrow.DataRepository
{
    public interface IShardingRepository : IBaseRepository
    {
        #region 查询数据

        IShardingQueryable<T> GetIShardingQueryable<T>() where T : class, new();

        #endregion
    }
}
