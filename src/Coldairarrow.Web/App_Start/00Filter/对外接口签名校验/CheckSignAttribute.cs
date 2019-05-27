using Coldairarrow.Business.Base_SysManage;
using Coldairarrow.Util;
using System.Web.Mvc;

namespace Coldairarrow.Web
{
    /*
==== 签名算法 ====

appId:AppAdmin

appSecret:oEt6sgjgPGeA5wFX

每个接口必须参数

| appId | string | 应用Id |
| time | string | 当前时间，格式为：2017-01-01 23:00:00 |
| sign| string | 签名,签名算法如下 |

签名算法示例：

令:

appId=xxx

appSecret=xxx

time=2017-01-01 23:00:00

1.对除签名外的所有请求参数按key做升序排列(字符串ASCII排序)

例如：有c=3,b=2,a=1 三个业务参数，另需要加上校验签名参数appId和time， 按key排序后为：a=1，appId=xxx，b=2，c=3，time=2017-01-01 23:00:00。

2 把参数名和参数值连接成字符串，得到拼装字符：a1appIdxxxb2c3time2017-01-01 23:00:00

3 用申请到的appSecret连接到接拼装字符串尾部，然后进行32位MD5加密，最后将到得MD5加密摘要转化成大写,即得到签名sign

示例：拼接字符串为a1appIdxxxb2c3time2017-01-01 23:00:00,appSecret为xxx,则sign=DBC4DB3A404576DB0D3D5F1F8547526B     
    */
    /// <summary>
    /// 校验签名
    /// </summary>
    public class CheckSignAttribute : FilterAttribute, IActionFilter
    {
        public ICheckSignBusiness _checkSignBusiness { get; set; }

        /// <summary>
        /// Action执行之前执行
        /// </summary>
        /// <param name="filterContext"></param>
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {

            //若为本地测试，则不需要校验
            if (GlobalSwitch.RunModel == RunModel.LocalTest)
            {
                return;
            }

            //判断是否需要签名
            bool needSign = !filterContext.ContainsAttribute<IgnoreSignAttribute>();

            //不需要签名
            if (!needSign)
                return;

            //需要签名
            var checkSignRes = _checkSignBusiness.IsSecurity(filterContext.HttpContext.ApplicationInstance.Context);
            if (!checkSignRes.Success)
            {
                filterContext.Result = new ContentResult() { Content = checkSignRes.ToJson() };
            }
        }

        /// <summary>
        /// Action执行完毕之后执行
        /// </summary>
        /// <param name="filterContext"></param>
        public void OnActionExecuted(ActionExecutedContext filterContext)
        {

        }
    }
}