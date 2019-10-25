using Coldairarrow.Entity.Base_SysManage;
using Coldairarrow.Util;
using NLog;
using NLog.Targets;
using System;

namespace Coldairarrow.Business
{
    public class BaseTarget : TargetWithLayout
    {
        public BaseTarget()
        {
            Name = "系统日志";
            Layout = LoggerConfig.Layout;
        }

        protected Base_SysLog GetBase_SysLogInfo(LogEventInfo logEventInfo)
        {
            string id = string.Empty;
            try
            {
                id = IdHelper.GetId();
            }
            catch
            {

            }
            if (id.IsNullOrEmpty())
                id = Guid.NewGuid().ToString();

            Base_SysLog newLog = new Base_SysLog
            {
                Id = id,
                Data = logEventInfo.Properties[LoggerConfig.Data] as string,
                Level = logEventInfo.Level.ToString(),
                LogContent = logEventInfo.Message,
                LogType = logEventInfo.Properties[LoggerConfig.LogType] as string,
                OpTime = logEventInfo.TimeStamp,
                OpUserName = logEventInfo.Properties[LoggerConfig.OpUserName] as string
            };

            return newLog;
        }
    }
}
