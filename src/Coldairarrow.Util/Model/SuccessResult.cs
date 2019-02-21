namespace Coldairarrow.Util
{
    public class SuccessResult:AjaxResult
    {
        public SuccessResult(object data=null)
        {
            Data = data;
            Success = true;
        }
    }
}