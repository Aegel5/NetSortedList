using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AlgoQuora;

public class SortedList<T> : IEnumerable<T> where T:new() {
    protected class Node {
        public int cnt = 1;
        public T node = new();
        public Node left = nil;
        public Node right = nil;
        public uint prior = rand_prior();
    }
    static readonly Node nil = new Node { cnt = 0 };
    protected Node root = nil;
    static Random rnd = new();
    [MethodImpl(256)] protected bool is_nil(Node t) { return ReferenceEquals(t, nil); }
    [MethodImpl(256)] int cnt_safe(Node t) { return t.cnt; }
    [MethodImpl(256)] static uint rand_prior() => (uint)rnd.Next(int.MinValue, int.MaxValue);
    public int Count => cnt_safe(root);
    protected void push(Node tt) { }
    [MethodImpl(256)] protected void upd(Node tt) { tt.cnt = cnt_safe(tt.left) + cnt_safe(tt.right) + 1; }
    [MethodImpl(256)] protected int Compare(T v1, T v2) => Comparer<T>.Default.Compare(v1, v2);
    public T this[int key] {
        get => get_at(key).node;
        //set => get_at(key).node = value;
    }
    private Node get_at(int pos) {
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
        yield return t.node;
        foreach (var item in left_to_right(t.right))
            yield return item;
    }
    public IEnumerator<T> GetEnumerator() => left_to_right(root).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => left_to_right(root).GetEnumerator();
    public int BinarySearch_Last(Func<T, bool> check, int l = 0) => BinarySearch_First(x => !check(x), l) - 1;
    public int BinarySearch_First(Func<T, bool> check, int l = 0) {
        var t = root;
        int offset = 0;
        int res = Count; // последний верный ответ.
        while (!is_nil(t)) {
            push(t);
            var l_cnt = cnt_safe(t.left);
            if (l <= l_cnt// имеем ли право проверять текущий элемент?
                && check(t.node)) {  // обычный поиск в дереве. используем технику "последнего верного ответа"
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
    public int BinarySearch_More(T val, int from_index = 0) => BinarySearch_First(x => Compare(x, val) > 0, from_index);
    public int BinarySearch_MoreEq(T val, int from_index = 0) => BinarySearch_First(x => Compare(x, val) >= 0, from_index);
    public int BinarySearch_Less(T val, int from_index = 0) => BinarySearch_Last(x => Compare(x, val) < 0, from_index);
    public int BinarySearch_LessEq(T val, int from_index = 0) => BinarySearch_Last(x => Compare(x, val) <= 0, from_index);
    public bool Contains(T node) {
        var t = root;
        while (!is_nil(t)) {
            int cmp = Compare(node, t.node);
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
    public int CountOfElement(T node) => Contains(node) ? 1 : 0;
    protected bool _Add(T node, bool skip_if_equal = false) {
        bool added = false;
        Node func(Node t) {
            if (is_nil(t)) {
                added = true;
                return new Node() { node = node }; // no need to upd
            }
            push(t);
            int cmp = Compare(node, t.node);
            if (skip_if_equal && cmp == 0) return t; // no need to upd
            if (cmp <= 0) {
                var res = func(t.left);
                if (res != t.left) { // произошла смена
                    t.left = res;
                    if (t.prior < res.prior) {
                        t = rotate_left(t);
                    }
                }
            } else {
                var res = func(t.right);
                if (res != t.right) { // произошла смена
                    t.right = res;
                    if (t.prior < res.prior) {
                        t = rotate_right(t);
                    }
                }
            }
            upd(t);
            return t;
        }
        root = func(root);
        return added;
    }
    public bool Add(T node) => _Add(node, skip_if_equal: true);
    public bool Remove(T node) {
        bool found = false;
        void func(ref Node t) {
            if (is_nil(t)) return;
            push(t);
            int cmp = Compare(node, t.node);
            if (cmp == 0) {
                found = true;
                t = merge(t.left, t.right);
            } else if (cmp < 0) {
                func(ref t.left);
            } else {
                func(ref t.right);
            }
            upd(t); // требуется обновление cnt.
        }
        func(ref root);
        return found;
    }
    public void RemoveAt(int i) {
        void func(ref Node t, int key) {
            if (is_nil(t)) return;
            push(t);
            var cur = t.cnt - cnt_safe(t.right);
            if (key == cur) {
                t = merge(t.left, t.right);
            } else if (key < cur) {
                func(ref t.left, key);
            } else {
                func(ref t.right, key - cur);
            }
            upd(t); // требуется обновление cnt.
        }
        func(ref root, i + 1);
    }
    protected Node merge(Node left, Node right) {
        if (is_nil(left)) {
            return right;
        }
        if (is_nil(right)) {
            return left;
        }
        if (left.prior > right.prior) {
            push(left);
            left.right = merge(left.right, right);
            upd(left);
            return left;
        }
        push(right);
        right.left = merge(left, right.left);
        upd(right);
        return right;
    }
    Node rotate_right(Node t) {
        var res = t.right;
        t.right = res.left;
        res.left = t;
        upd(t);
        return res; // need to update
    }
    Node rotate_left(Node t) {
        var res = t.left;
        t.left = res.right;
        res.right = t;
        upd(t);
        return res; // need to update
    }
}

public class SortedMultiList<T> : SortedList<T> where T : new() {
    new public bool Add(T node) => _Add(node, skip_if_equal: false);
    new public int CountOfElement(T node) {
        var low = BinarySearch_Less(node);
        var high = BinarySearch_More(node);
        return high - low - 1;
    }
    public int RemoveAll(T node) {
        int cnt = 0;
        void del(ref Node t) {
            if (is_nil(t)) return;
            if (Compare(node, t.node) != 0) return;
            del_actual(ref t);
        }
        void del_actual(ref Node t) {
            cnt++;
            del(ref t.left);
            del(ref t.right);
            t = merge(t.left, t.right);
        }
        void func(ref Node t) {
            if (is_nil(t)) return;
            push(t);
            int cmp = Compare(node, t.node);
            if (cmp == 0) {
                del_actual(ref t);
            } else if (cmp < 0) {
                func(ref t.left);
            } else {
                func(ref t.right);
            }
            upd(t); // требуется обновление cnt.
        }
        func(ref root);
        return cnt;
    }
}




