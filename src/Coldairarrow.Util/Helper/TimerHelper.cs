using System;
using System.Threading;
using System.Web;

namespace Coldairarrow.Util
{
    /// <summary>
    /// 时间帮助类
    /// </summary>
    public class TimerHelper
    {
        /// <summary>
        /// 设置一个时间间隔的循环操作
        /// </summary>
        /// <param name="action">执行的操作</param>
        /// <param name="timeSpan">时间间隔</param>
        public static Timer SetInterval(Action action, TimeSpan timeSpan)
        {
            Timer threadTimer = new Timer((state =>
            {
                action.Invoke();
            }), null, 0, (long)timeSpan.TotalMilliseconds);
            HttpRuntime.Cache[Guid.NewGuid().ToString()] = threadTimer;

            return threadTimer;
        }

        /// <summary>
        /// 设置一个时间间隔的循环操作
        /// </summary>
        /// <param name="action">执行的操作</param>
        /// <param name="timeSpan">时间间隔</param>
        /// <param name="dely">延迟一段时间后再开始循环</param>
        public static Timer SetInterval(Action action, TimeSpan timeSpan, TimeSpan dely)
        {
            Timer threadTimer = new Timer((state =>
            {
                action.Invoke();
            }), null, dely, timeSpan);
            HttpRuntime.Cache[Guid.NewGuid().ToString()] = threadTimer;

            return threadTimer;
        }

        /// <summary>
        /// 一段时间后执行操作
        /// </summary>
        /// <param name="action">需要执行的操作</param>
        /// <param name="dely">延时时间</param>
        public static void SetTimeout(Action action,TimeSpan dely)
        {
            if (dely == TimeSpan.Zero)
                action();
            else
            {
                Timer timer = null;
                action += () =>
                {
                    timer.Dispose();
                };
                timer = SetInterval(action, new TimeSpan(0, 0, 1), dely);
            }
        }
    }
}
