using AlgoQuora.details;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AlgoQuora {

    namespace details {
        public struct MapRecord<TKey, TValue> {
            public TKey Key;
            public TValue Value;
        }
    }

    public class SortedMap<TKey, TValue> : _CartesianBase<MapRecord<TKey,TValue>> {

        protected IComparer<TKey> comparerKey;
        [MethodImpl(256)] protected int CompareKey(TKey v1, TKey v2) => comparerKey.Compare(v1, v2);
        public SortedMap() {
            comparerKey = Comparer<TKey>.Default;
            comparer = Comparer<MapRecord<TKey,TValue>>.Create((x, y) => comparerKey.Compare(x.Key, y.Key));
        }

        protected MapRecord<TKey, TValue> rec(TKey key) => new MapRecord<TKey, TValue> { Key = key };

        public bool Add(TKey key, TValue value) {
            return _Add(new MapRecord<TKey, TValue> { Key = key, Value = value }, skip_if_equal:true).added;
        }

        public ref TValue this[TKey key] => ref _Add(rec(key), skip_if_equal: true).node.val.Value;

        public bool Contains(TKey key) => _Contains(rec(key));

        public int CountOf(TKey key) => Contains(key) ? 1 : 0;

        public bool Remove(TKey key) => _Remove(rec(key));

        public MapRecord<TKey, TValue> ByIndex(Index i) => get_at(i).val;
        public ref TValue ByIndexValue(Index i) => ref get_at(i).val.Value;

        public BSResult More(TKey val, int l = 0, int r = int.MaxValue) => BinarySearch_First(x => CompareKey(x.Key, val) > 0, l, r);
        public BSResult MoreEq(TKey val, int l = 0, int r = int.MaxValue) => BinarySearch_First(x => CompareKey(x.Key, val) >= 0, l, r);
        public BSResult Less(TKey val, int l = 0, int r = int.MaxValue) => BinarySearch_Last(x => CompareKey(x.Key, val) < 0, l, r);
        public BSResult LessEq(TKey val, int l = 0, int r = int.MaxValue) => BinarySearch_Last(x => CompareKey(x.Key, val) <= 0, l, r);

    }
    public class SortedMultiMap<TKey, TValue> : SortedMap<TKey, TValue> {

        new public bool Add(TKey key, TValue value) {
            return _Add(new MapRecord<TKey, TValue> { Key = key, Value = value }, skip_if_equal: false).added;
        }

        public int RemoveAllOf(TKey key) => _RemoveAllOf(rec(key));

        new public int CountOf(TKey node) {
            var low = Less(node);
            var high = More(node);
            return high - low - 1;
        }

    }
}
