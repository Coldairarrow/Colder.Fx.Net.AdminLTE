using Coldairarrow.Util;

namespace Coldairarrow.Business
{
    public class TestBusiness : BaseBusiness<object>, ITestBusiness, IDependency
    {
        public string GetValue()
        {
            return "Test";
        }
    }
}
