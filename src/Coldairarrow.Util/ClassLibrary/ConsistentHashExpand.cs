using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Coldairarrow.Util
{
    public class ConsistentHashExpand<T>
    {
        public ConsistentHashExpand()
        {
            _space = (uint)(uint.MaxValue / _replicate);
        }
        SortedDictionary<uint, T> _circle { get; set; } = new SortedDictionary<uint, T>();
        uint _replicate = 100;    //default _replicate count
        uint[] ayKeys = null;    //cache the ordered keys for better performance
        private uint _space { get; }
        //it's better you override the GetHashCode() of T.
        //we will use GetHashCode() to identify different node.
        public void Init(IEnumerable<T> nodes)
        {
            Init(nodes, _replicate);
        }

        public void Init(IEnumerable<T> nodes, uint replicate)
        {
            _replicate = replicate;

            foreach (T node in nodes)
            {
                this.Add(node, false);
            }
            ayKeys = _circle.Keys.ToArray();
        }

        public void Add(T node)
        {
            Add(node, true);
        }

        private void Add(T node, bool updateKeyArray)
        {
            uint hash = BetterHash(node.GetHashCode().ToString());
            _circle[hash] = node;
            for (uint i = 0; i < _replicate - 1; i++)
            {
                hash = NextIndex(hash, _space);
                _circle[hash] = node;
            }

            if (updateKeyArray)
            {
                ayKeys = _circle.Keys.ToArray();
            }
        }

        public void Remove(T node)
        {
            for (uint i = 0; i < _replicate; i++)
            {
                uint hash = BetterHash(node.GetHashCode().ToString() + i);
                if (!_circle.Remove(hash))
                {
                    throw new Exception("can not remove a node that not added");
                }
            }
            ayKeys = _circle.Keys.ToArray();
        }

        //we keep this function just for performance compare
        private T GetNode_slow(String key)
        {
            uint hash = BetterHash(key);
            if (_circle.ContainsKey(hash))
            {
                return _circle[hash];
            }

            uint first = _circle.Keys.FirstOrDefault(h => h >= hash);
            if (first == new uint())
            {
                first = ayKeys[0];
            }
            T node = _circle[first];
            return node;
        }

        //return the index of first item that >= val.
        //if not exist, return 0;
        //ay should be ordered array.
        uint First_ge(uint[] ay, uint val)
        {
            uint begin = 0;
            uint end = (uint)(ay.Length - 1);

            if (ay[end] < val || ay[0] > val)
            {
                return 0;
            }

            uint mid = begin;
            while (end - begin > 1)
            {
                mid = (end + begin) / 2;
                if (ay[mid] >= val)
                {
                    end = mid;
                }
                else
                {
                    begin = mid;
                }
            }

            if (ay[begin] > val || ay[end] < val)
            {
                throw new Exception("should not happen");
            }

            return end;
        }

        public T GetNode(String key)
        {
            //return GetNode_slow(key);

            uint hash = BetterHash(key);

            uint first = First_ge(ayKeys, hash);

            //uint diff = circle.Keys[first] - hash;

            return _circle[ayKeys[first]];
        }

        //default String.GetHashCode() can't well spread strings like "1", "2", "3"
        public static uint BetterHash(String key)
        {
            uint hash = MurmurHash2.Hash(Encoding.UTF8.GetBytes(key));
            return hash;
        }

        private uint NextIndex(uint index, uint addNum)
        {
            long sum = index + addNum;
            if (sum <= uint.MaxValue)
                return (uint)sum;
            else
                return (uint)(sum - uint.MaxValue);
        }
    }
}