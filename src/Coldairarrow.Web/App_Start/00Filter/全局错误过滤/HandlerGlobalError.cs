﻿using Coldairarrow.Business;
using Coldairarrow.Util;
using System.Text;
using System.Web.Mvc;

namespace Coldairarrow.Web
{
    /// <summary>
    /// 过滤处理全局错误信息
    /// </summary>
    public class HandlerGlobalError : HandleErrorAttribute, IDependency
    {
        ILogger _logger { get => AutofacHelper.GetService<ILogger>(); }

        /// <summary>
        /// 处理系统错误
        /// </summary>
        /// <param name="exContext">错误消息</param>
        public override void OnException(ExceptionContext exContext)
        {
            base.OnException(exContext);

            exContext.ExceptionHandled = true;
            exContext.HttpContext.Response.StatusCode = 200;

            var theEx = exContext.Exception;
            _logger.Error(theEx);

            AjaxResult res = new AjaxResult()
            {
                Success = false,
                Msg = theEx.Message
            };

            exContext.Result = new ContentResult() { Content = res.ToJson(), ContentEncoding = Encoding.UTF8, ContentType = "application/json" };
        }
    }
}