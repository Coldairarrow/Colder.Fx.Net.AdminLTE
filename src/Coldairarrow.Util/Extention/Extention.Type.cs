using System;

namespace Coldairarrow.Util
{
    public static partial class Extention
    {
        /// <summary>
        /// 转换为对应的Nullable'T'类型
        /// </summary>
        /// <param name="type">值类型</param>
        /// <returns></returns>
        public static Type ToNullableType(this Type type)
        {
            if (type.IsValueType)
            {
                string fullName = $"System.Nullable`1[[{type.FullName}, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]";

                return Type.GetType(fullName);
            }
            else
                return type;
        }
    }
}