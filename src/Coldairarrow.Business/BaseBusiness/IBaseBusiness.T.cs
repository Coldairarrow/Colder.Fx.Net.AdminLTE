using Coldairarrow.DataRepository;

namespace Coldairarrow.Business
{
    public interface IBaseBusiness<T> : IRepository<T> where T : class, new()
    {

    }
}
