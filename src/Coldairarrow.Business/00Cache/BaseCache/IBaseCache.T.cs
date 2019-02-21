namespace Coldairarrow.Business.Cache
{
    public interface IBaseCache<T> where T : class
    {
        T GetCache(string idKey);
        void UpdateCache(string idKey);
    }
}