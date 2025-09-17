using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

namespace AlgoQuora {

    // https://wcipeg.com/wiki/Size_Balanced_Tree
    // https://github.com/THEFZNKHAN/balanced-tree-visualizer

    public class _CartesianBase<T> : IEnumerable<T> {
        protected class Node {
            public int cnt = 1;
            public T val;
            public Node? left;
            public Node? right;
        }
        protected IComparer<T> comparer = Comparer<T>.Default;
        protected Node? root;
        [MethodImpl(256)] protected bool is_nil([NotNullWhen(false)] Node t) { return t == null; }
        [MethodImpl(256)] protected int cnt_safe(Node t) { return t == null ? 0 : t.cnt; }
        public int Count => cnt_safe(root);
        public bool IsEmpty => Count == 0;
        [MethodImpl(256)] protected void push(Node t) { }
        [MethodImpl(256)] protected void upd(Node t) { }
        [MethodImpl(256)] protected int Compare(T v1, T v2) => comparer.Compare(v1, v2);
        [MethodImpl(256)] protected int ActualIndex(Index index) => index.IsFromEnd ? Count - index.Value : index.Value;
        [MethodImpl(256)] protected Node get_at(Index index) => get_at(ActualIndex(index));
        public _CartesianBase() {
            Clear();
        }
        protected Node get_at(int pos) {
            Debug.Assert(pos >= 0 && pos < Count);
            var t = root;
            while (true) {
                push(t);
                var cnt_left = cnt_safe(t.left);
                if (pos == cnt_left)
                    return t;
                if (pos < cnt_left)
                    t = t.left;
                else {
                    t = t.right;
                    pos -= cnt_left + 1;
                }
            }
        }
        IEnumerable<T> left_to_right(Node t) {
            if (is_nil(t)) yield break;
            push(t);
            foreach (var item in left_to_right(t.left))
                yield return item;
            yield return t.val;
            foreach (var item in left_to_right(t.right))
                yield return item;
        }
        public IEnumerator<T> GetEnumerator() => left_to_right(root).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => left_to_right(root).GetEnumerator();

        protected bool _Contains(T node) {
            var t = root;
            while (!is_nil(t)) {
                int cmp = Compare(node, t.val);
                if (cmp == 0) {
                    return true;
                } else if (cmp < 0) {
                    t = t.left;
                } else {
                    t = t.right;
                }
            }
            return false;
        }
        protected bool _Remove(T node) {
            bool found = false;
            void func(ref Node t) {
                if (is_nil(t)) return;
                push(t);
                int cmp = Compare(node, t.val);
                if (cmp == 0) {
                    found = true;
                    t = merge_check(t.left, t.right);
                    return;
                } else if (cmp < 0) {
                    func(ref t.left);
                } else {
                    func(ref t.right);
                }
                if (found) t.cnt--;
            }
            func(ref root);
            return found;
        }
        protected Node merge_check(Node left, Node right) {
            if (is_nil(left)) return right;
            if (is_nil(right)) return left;
            return merge_no_check(left, right);
        }
        protected Node merge_no_check(Node left, Node right) { // both must be not nil
            if (left.cnt > right.cnt) {
                push(left);
                left.cnt += right.cnt;
                left.right = is_nil(left.right) ? right : merge_no_check(left.right, right);
                return left;
            } else {
                push(right);
                right.cnt += left.cnt;
                right.left = is_nil(right.left) ? left : merge_no_check(left, right.left);
                return right;
            }
        }

        public void RemoveAt(Index index) {
            var i = ActualIndex(index);
            Debug.Assert(i >= 0 && i < Count);
            void func(ref Node t, int key) {
                if (is_nil(t)) return;
                push(t);
                var cur = t.cnt - cnt_safe(t.right);
                if (key == cur) {
                    t = merge_check(t.left, t.right);
                    return;
                } else if (key < cur) {
                    func(ref t.left, key);
                } else {
                    func(ref t.right, key - cur);
                }
                t.cnt--;
            }
            func(ref root, i + 1);
        }
        public struct BSResult {
            public int Index { get; init; }
            public bool Ok { get; init; }
            [MethodImpl(256)] public static implicit operator int(BSResult value) => value.Index;
            [MethodImpl(256)] public static implicit operator long(BSResult value) => value.Index;
            public BSResult(int i, bool ok) {
                Index = i;
                Ok = ok;
            }
        }
        int _First(Func<T, bool> check, int l = 0) {
            var t = root;
            int offset = 0;
            int res = Count; // последний верный ответ.
            while (!is_nil(t)) {
                push(t);
                var l_cnt = cnt_safe(t.left);
                if (l <= l_cnt// имеем ли право проверять текущий элемент?
                    && check(t.val)) {  // обычный поиск в дереве. используем технику "последнего верного ответа"
                    res = l_cnt + offset; // верный ответ
                    t = t.left; // идем влево, так как справа 100% уже понятно.
                } else {
                    // текущий элемент проверять запрещено. просто идем вправо.
                    offset += l_cnt + 1;
                    l -= offset;
                    t = t.right;
                }
            }
            return res;
        }
        public BSResult BinarySearch_Last(Func<T, bool> check, int l = 0, int r = int.MaxValue) {
            int i = _First(x => !check(x), l) - 1;
            if (i > r) i = r;
            return new(i, i >= l);
        }
        public BSResult BinarySearch_First(Func<T, bool> check, int l = 0, int r = int.MaxValue) {
            int i = _First(check, l);
            return new(i, i <= r);
        }
        public void Clear() {
            root = null;// new Node { cnt = 0 };
        }
        protected int _RemoveAllOf(T node) {
            int func(ref Node t) {
                if (is_nil(t)) return 0;
                push(t);
                int cmp = Compare(node, t.val);
                int cnt = 0;
                if (cmp == 0) {
                    cnt += func(ref t.left);
                    cnt += func(ref t.right);
                    t = merge_check(t.left, t.right);
                    return cnt + 1;
                } else if (cmp < 0) {
                    cnt = func(ref t.left);
                } else {
                    cnt = func(ref t.right);
                }
                t.cnt -= cnt;
                return cnt;
            }
            return func(ref root);
        }
        Node rotate_left(Node t, Node left) {
            var res_right = left.right;
            left.right = t;
            t.left = res_right;
            left.cnt = t.cnt;
            if (!is_nil(left.left)) t.cnt -= left.left.cnt;
            t.cnt--;
            return left;
        }
        Node rotate_right(Node t, Node right) {
            var res_left = right.left;
            right.left = t;
            t.right = res_left;
            right.cnt = t.cnt;
            if (!is_nil(right.right)) t.cnt -= right.right.cnt;
            t.cnt--;
            return right;
        }

        static readonly double alp = new Random().NextDouble() * 0.1 + 2.35;
        [MethodImpl(256)] int alpha_weight(int v) => (int)Math.Ceiling(v *alp);

        protected void check_violate(Node t) {
            var a = cnt_safe(t.left);
            var b = cnt_safe(t.right);
            if (a > b) (a, b) = (b, a);
            if (b > alpha_weight(a) && b != 1) {
                //throw new Exception("tree violate rules");
            }
        }

        [MethodImpl(256)]
        void maintaince(ref Node t, Node res, bool left) {
            var cnt_other = t.cnt - res.cnt - 1;
            if (res.cnt <= alpha_weight(cnt_other)) return;
            if (left) {
                var res_left_cnt = cnt_safe(res.left);
                if (alpha_weight(res_left_cnt) >= res.cnt - res_left_cnt + cnt_other) {
                    t = rotate_left(t, res);
                    return;
                }
                res = rotate_right(res, res.right);
                t = rotate_left(t, res);
            } else {
                var res_right_cnt = cnt_safe(res.right);
                if (alpha_weight(res_right_cnt) >= res.cnt - res_right_cnt + cnt_other) {
                    t = rotate_right(t, res);
                    return;
                }
                res = rotate_left(res, res.left);
                t = rotate_right(t, res);
            }

        }
        protected (bool added, Node node) _Add(T val, bool skip_if_equal = false) {
            if (is_nil(root)) {
                root = new Node { val = val };
                return (true, root);
            }
            bool added = false;
            Node? result_node = null;
            Node func(Node t) {
                int cmp = Compare(val, t.val);
                if (skip_if_equal && cmp == 0) {
                    result_node = t;
                    return t;
                }
                if (cmp <= 0) {
                    if (t.left == null) {
                        added = true;
                        result_node = new Node { val = val };
                        t.left = result_node;
                        t.cnt++;
                    } else {
                        var res = func(t.left);
                        if (!added) return t;
                        t.left = res;
                        t.cnt++;
                        maintaince(ref t, res, true);
                    }
                } else {
                    if (t.right == null) {
                        added = true;
                        result_node = new Node { val = val };
                        t.right = result_node;
                        t.cnt++;
                    } else {
                        var res = func(t.right);
                        if (!added) return t;
                        t.right = res;
                        t.cnt++;
                        maintaince(ref t, res, false);
                    }
                }
                return t;
            }
            root = func(root);
            return (added, result_node);
        }
    }
    
    public class SortedList<T> : _CartesianBase<T> {

        public T this[Index key] {
            get => get_at(key).val;
            //set => get_at(key).node = value;
        }

        public bool Contains(T value) => _Contains(value);
        public int CountOf(T node) => Contains(node) ? 1 : 0;
        public bool Add(T node) => _Add(node, skip_if_equal: true).added;
        public bool Remove(T node) => _Remove(node);
        public BSResult More(T val, int l = 0, int r = int.MaxValue) => BinarySearch_First(x => Compare(x, val) > 0, l, r);
        public BSResult MoreEq(T val, int l = 0, int r = int.MaxValue) => BinarySearch_First(x => Compare(x, val) >= 0, l, r);
        public BSResult Less(T val, int l = 0, int r = int.MaxValue) => BinarySearch_Last(x => Compare(x, val) < 0, l, r);
        public BSResult LessEq(T val, int l = 0, int r = int.MaxValue) => BinarySearch_Last(x => Compare(x, val) <= 0, l, r);

    }

    public class SortedMultiList<T> : SortedList<T> {
        new public bool Add(T node) => _Add(node, skip_if_equal: false).added;
        new public int CountOf(T node) {
            var low = Less(node);
            var high = More(node);
            return high - low - 1;
        }

        public int RemoveAllOf(T val) => _RemoveAllOf(val);

    }

}