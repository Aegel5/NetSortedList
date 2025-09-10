

using AlgoQuora;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using static System.Net.Mime.MediaTypeNames;

namespace TestSortedList; 
class SortedListChecked<T> : SortedMultiList<T> where T : new() {
    public void SelfCheck() {
        void check_tree(Node t) {
            if (is_nil(t)) return;
            check_tree(t.left);
            check_tree(t.right);
            if (t.cnt != cnt_safe(t.left) + cnt_safe(t.right) + 1) {
                throw new Exception("bad cnt");
            }
            if (!is_nil(t.left)) {
                if(t.prior < t.left.prior)
                    throw new Exception("bad left prior");
                if(Compare(t.left.val, t.val) > 0) {
                    throw new Exception("bad order");
                }
            }
            if (!is_nil(t.right)) {
                if(t.prior < t.right.prior)
                    throw new Exception("bad right prior");
                if (Compare(t.right.val, t.val) < 0) {
                    throw new Exception("bad order");
                }
            }
        }
        check_tree(root); 
    }
}

class SortedList_Tester<T> where T : new() {
    SortedListChecked<T> lst = new();
    List<T> checker = new();

    int Compare(T v1, T v2) => Comparer<T>.Default.Compare(v1, v2);

    public int Count => checker.Count;

    public void Add(T v) {
        lst.Add(v);
        for(int i = 0; i < checker.Count; i++) {
            int cmp = Compare(v, checker[i]);
            //if (cmp == 0) return;
            if(cmp < 0) {
                checker.Insert(i, v);
                return;
            }
        }
        checker.Add(v);
    }
    public void Remove(T v) {
        lst.Remove(v);
        for (int i = 0; i < checker.Count; i++) {
            int cmp = Compare(v, checker[i]);
            if (cmp == 0) {
                checker.RemoveAt(i);
                return;
            }
        }
    }
    public int RemoveAllOf(T v) {
        var cnt = lst.RemoveAllOf(v);
        var cnt2 = checker.RemoveAll(x => Compare(x, v) == 0);
        if (cnt != cnt2)
            throw new Exception("bad");
        return cnt;
    }
    public void Get(int i) {
        if (Compare(lst[i], checker[i]) != 0)
            throw new Exception("bad");

    }
    public void RemoveAt(int i) {
        lst.RemoveAt(i);
        checker.RemoveAt(i);
    }
    public int CountOf(T v) {
        var cnt = lst.CountOf(v);
        if(cnt > 1) {
            int k = 0;
        }
        var cnt2 = checker.Count(x => Compare(x, v) == 0);
        if (cnt != cnt2)
            throw new Exception("bad");
        return cnt;
    }
    public void Contains(T v) {
        if (lst.Contains(v) != checker.Contains(v)) throw new Exception("bad");
    }
    public void Check() {
        lst.SelfCheck();
        if (lst.Count != checker.Count) throw new Exception("bad count");
        if (!lst.SequenceEqual(checker)) throw new Exception("not equal");
    }

}

internal class Program {
    static void Test() {
        Random rnd = new();
        int next() { return rnd.Next(0, 100); }
        for (int i = 0; i < 100; i++) {
            SortedList_Tester<int> tester = new();
            for (int j = 0; j < 1000; j++) {
                tester.Add(next());
                tester.Check();
                tester.Remove(next());
                tester.Check();
                if(tester.RemoveAllOf(next()) != 0) {
                    tester.Check();
                }
                tester.CountOf(next());
                if(tester.Count > 0) {
                    tester.Get(rnd.Next(0, tester.Count));
                    tester.Add(next());
                    tester.RemoveAt(rnd.Next(0, tester.Count));
                    tester.Check();
                }
                tester.Contains(next());
            }
        }
        Console.WriteLine("Test OK");
    }
    static void Main(string[] args) {

        //Random rnd = new(848);
        //for (int j = 0; j < 100; j++) {
        //    SortedList<int> list = new();
        //    for (int i = 0; i < 900000; i++) {
        //        list.Add(rnd.Next(0, 900000 / 2));
        //    }
        //}
        //return;

        Test();
        BenchmarkRunner.Run(typeof(Program).Assembly, new Config());
    }
}

public class MyBenchmarks {
    int N = 300000;

    [Benchmark]
    public void SortedList_AddRemove() {
        Random rnd = new(848);
        SortedList<int> list = new();
        for (int i = 0; i < N; i++) {
            list.Add(rnd.Next(0, N));
            list.Remove(rnd.Next(0, N));
        }
    }

    [Benchmark]
    public void SortedSet_AddRemove() {
        Random rnd = new(848);
        SortedSet<int> list = new();
        for (int i = 0; i < N; i++) {
            list.Add(rnd.Next(0, N));
            list.Remove(rnd.Next(0, N));
        }
    }

    [Benchmark]
    public void SortedMultiList_AddRemoveAll() {
        Random rnd = new(848);
        SortedMultiList<int> list = new();
        for (int i = 0; i < N; i++) {
            list.Add(rnd.Next(0, N/2));
            list.RemoveAllOf(rnd.Next(0, N/2));
        }
    }



}
