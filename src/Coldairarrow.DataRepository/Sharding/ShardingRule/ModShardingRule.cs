using Coldairarrow.Util;

namespace Coldairarrow.DataRepository
{
    /// <summary>
    /// 取模分片规则
    /// 说明:根据某字段的HASH,然后取模后得到表名后缀
    /// 举例:Base_User_0,Base_User为抽象表名,_0为后缀
    /// 警告:使用简单,但是难以扩容!!!
    /// </summary>
    /// <seealso cref="Coldairarrow.DataRepository.IShardingRule" />
    public class ModShardingRule : IShardingRule
    {
        public ModShardingRule(string absTableName, string keyField, int mod)
        {
            _absTableName = absTableName;
            _keyField = keyField;
            _mod = mod;
        }
        private string _absTableName { get; }
        private string _keyField { get; }
        private int _mod { get; }
        public string FindTable(object obj)
        {
            return $"{_absTableName}_{obj.GetPropertyValue(_keyField).GetHashCode() % _mod}";
        }
    }
}
