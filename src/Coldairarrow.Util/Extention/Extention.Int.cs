using System;

namespace Coldairarrow.Util
{
    public static partial class Extention
    {
        /// <summary>
        /// jsGetTime转为DateTime
        /// </summary>
        /// <param name="jsGetTime">js中Date.getTime()</param>
        /// <returns></returns>
        public static DateTime ToDateTime_From_JsGetTime(this long jsGetTime)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(jsGetTime + "0000");  //说明下，时间格式为13位后面补加4个"0"，如果时间格式为10位则后面补加7个"0",至于为什么我也不太清楚，也是仿照人家写的代码转换的
            TimeSpan toNow = new TimeSpan(lTime);
            DateTime dtResult = dtStart.Add(toNow); //得到转换后的时间

            return dtResult;
        }

        /// <summary>
        /// 将数字转为对应的字节数组
        /// </summary>
        /// <param name="num">数字</param>
        /// <returns></returns>
        public static byte[] ToBytes(this int num)
        {
            return BitConverter.GetBytes(num);
        }
    }
}
