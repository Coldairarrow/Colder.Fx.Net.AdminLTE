using System.IO;
using System.Text;

namespace Coldairarrow.Util
{
    public static partial class Extention
    {
        /// <summary>
        /// 将流Stream转为byte数组
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] ReadToBytes(this Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);

            return bytes;
        }

        /// <summary>
        /// 将流读为字符串
        /// 注：使用默认编码
        /// </summary>
        /// <param name="stream">流</param>
        /// <returns></returns>
        public static string ReadToString(this Stream stream)
        {
            return ReadToString(stream, Encoding.Default);
        }

        /// <summary>
        /// 将流读为字符串
        /// 注：使用指定编码
        /// </summary>
        /// <param name="stream">流</param>
        /// <param name="encoding">指定编码</param>
        /// <returns></returns>
        public static string ReadToString(this Stream stream, Encoding encoding)
        {
            string resStr = string.Empty;
            stream.Position = 0;
            var streamReader = new StreamReader(stream, encoding);
            resStr = streamReader.ReadToEnd();
            stream.Position = 0;

            return resStr;
        }
    }
}
