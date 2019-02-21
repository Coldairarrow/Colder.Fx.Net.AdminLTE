using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic;

namespace Coldairarrow.Util
{
    public static partial class Extention
    {
        /// <summary>
        /// 数据替换
        /// 注：支持一次性替换多个，支持所有可迭代类型，KeyValuePair键值对中Key为需要替换的数据，Value为替换后数据
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="source">数据源</param>
        /// <param name="parttern">替换模式</param>
        /// <returns></returns>
        public static IEnumerable<T> Replace<T>(this IEnumerable<T> source, params KeyValuePair<IEnumerable<T>, IEnumerable<T>>[] parttern)
        {
            List<T> resList = new List<T>();
            List<T> sourceList = source.ToList();
            for (int i = 0; i < sourceList.Count; i++)
            {
                bool replaced = false;
                parttern.ForEach(aMatch =>
                {
                    var oldvalue = aMatch.Key.ToList();
                    var newvalue = aMatch.Value.ToList();

                    bool needReplace = true;
                    for(int j = 0; j < oldvalue.Count; j++)
                    {
                        if (!sourceList[i + j].Equals(oldvalue[j]))
                            needReplace = false;
                    }
                    if (needReplace)
                    {
                        resList.AddRange(newvalue);
                        i = i + oldvalue.Count - 1;
                        replaced = true;
                    }
                });
                if (!replaced)
                    resList.Add(sourceList[i]);
            }

            return resList;
        }

        /// <summary>
        /// 复制序列中的数据
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="iEnumberable">原数据</param>
        /// <param name="startIndex">原数据开始复制的起始位置</param>
        /// <param name="length">需要复制的数据长度</param>
        /// <returns></returns>
        public static IEnumerable<T> Copy<T>(this IEnumerable<T> iEnumberable, int startIndex, int length)
        {
            var sourceArray = iEnumberable.ToArray();
            T[] newArray = new T[length];
            Array.Copy(sourceArray, startIndex, newArray, 0, length);

            return newArray;
        }

        /// <summary>
        /// 序列连接对象
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="iEnumberable">原序列</param>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static IEnumerable<T> Concat<T>(this IEnumerable<T> iEnumberable, T obj)
        {
            return iEnumberable.Concat(new T[] { obj });
        }

        /// <summary>
        /// 给IEnumerable拓展ForEach方法
        /// </summary>
        /// <typeparam name="T">模型类</typeparam>
        /// <param name="iEnumberable">数据源</param>
        /// <param name="func">方法</param>
        public static void ForEach<T>(this IEnumerable<T> iEnumberable, Action<T> func)
        {
            foreach (var item in iEnumberable)
            {
                func(item);
            }
        }

        /// <summary>
        /// 给IEnumerable拓展ForEach方法
        /// </summary>
        /// <typeparam name="T">模型类</typeparam>
        /// <param name="iEnumberable">数据源</param>
        /// <param name="func">方法</param>
        public static void ForEach<T>(this IEnumerable<T> iEnumberable, Action<T, int> func)
        {
            var array = iEnumberable.ToArray();
            for (int i = 0; i < array.Count(); i++)
            {
                func(array[i], i);
            }
        }

        /// <summary>
        /// IEnumerable转换为List'T'
        /// </summary>
        /// <typeparam name="T">参数</typeparam>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static List<T> CastToList<T>(this IEnumerable source)
        {
            return new List<T>(source.Cast<T>());
        }

        /// <summary>
        /// 将IEnumerable'T'转为对应的DataTable
        /// </summary>
        /// <typeparam name="T">数据模型</typeparam>
        /// <param name="iEnumberable">数据源</param>
        /// <returns>DataTable</returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> iEnumberable)
        {
            return iEnumberable.ToJson().ToDataTable();
        }

        /// <summary>
        /// 获取分页数据
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="iEnumberable">数据源</param>
        /// <param name="pagination">分页参数</param>
        /// <returns></returns>
        public static IEnumerable<T> GetPagination<T>(this IEnumerable<T> iEnumberable, Pagination pagination)
        {
            return iEnumberable.OrderBy($@"{pagination.SortField} {pagination.SortType}").Skip((pagination.page - 1) * pagination.rows).Take(pagination.rows).ToList();
        }
    }
}
