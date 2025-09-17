

using AlgoQuora;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;


namespace TestSortedList; 
class SortedListChecked<T> : SortedMultiList<T> where T : new() {
    public double tot_depth = 0;
    public int cnt_depth = 0;
    public int max_depth = 0;
    public bool AddUnique(T val) => _Add(val, true).added;
    public void SelfCheckRules() {
        void func(Node t, int d = 0) {
            if (is_nil(t)) return;
            func(t.left, d+1);
            func(t.right, d+1);
            check_violate(t);
            if (t.cnt != cnt_safe(t.left) + cnt_safe(t.right) + 1) {
                throw new Exception("bad cnt");
            }
            if (!is_nil(t.left)) {
                if (Compare(t.left.val, t.val) > 0) {
                    throw new Exception("bad order");
                }
            }
            if (!is_nil(t.right)) {
                if (Compare(t.right.val, t.val) < 0) {
                    throw new Exception("bad order");
                }
            }
            if(is_nil(t.left) && is_nil(t.right)) {
                tot_depth += d;
                cnt_depth++;
                max_depth = Math.Max(max_depth, d);
            }
        }
        func(root);
    }
}

class SortedList_Tester<T> where T : new() {
    public SortedListChecked<T> lst = new();
    List<T> checker = new();

    int Compare(T v1, T v2) => Comparer<T>.Default.Compare(v1, v2);

    public int Count => checker.Count;

    public void Add(T v, bool unique = false) {
        if (unique) lst.AddUnique(v);
        else lst.Add(v);
        for(int i = 0; i < checker.Count; i++) {
            int cmp = Compare(v, checker[i]);
            if (unique && cmp == 0) return;
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
        lst.SelfCheckRules();
        if (lst.Count != checker.Count) throw new Exception("bad count");
        if (!lst.SequenceEqual(checker)) throw new Exception("not equal");
    }


}

internal class Program {
    static void Test() {
        Random rnd = new(5);
        int next() { return rnd.Next(0, 100); }
        {
            SortedList_Tester<int> tester = new();
            for (int j = 0; j < 2000; j++) {
                tester.Add(next());
                //if (j % 100 == 0)
                tester.Check();
            }
            tester.Check();
            Console.WriteLine("may be ok");
        }
        {
            SortedList<int> a = new();
            SortedSet<int> test = new();
            for (int q = 0; q < 1000; q++) {
                var v = next();
                a.Add(v);
                test.Add(v);
            }
            if (!a.SequenceEqual(test)) throw new Exception("bad");
        }

        {
            SortedListChecked<int> a = new();
            for (int q = 0; q < 300000; q++) {
                a.Add(q);
            }
            a.SelfCheckRules();
            Console.WriteLine($"avr depth: {a.tot_depth / a.cnt_depth}, max={a.max_depth}");
        }

        {
            SortedList_Tester<int> tester = new();
            for (int j = 0; j < 1000; j++) {
                tester.Add(next(), ((next()&1) == 0));
                tester.Check();
                tester.Remove(next());
                tester.Check();
                if (tester.RemoveAllOf(next()) != 0) {
                    tester.Check();
                }
                tester.CountOf(next());
                if (tester.Count > 0) {
                    tester.Get(rnd.Next(0, tester.Count));
                    tester.Add(next());
                    tester.RemoveAt(rnd.Next(0, tester.Count));
                    tester.Check();
                }
                tester.Contains(next());
            }
        }

        {
            var r = new Random(343);
            SortedListChecked<int> a = new();
            for (int q = 0; q < 400000; q++) {
                a.AddUnique(q);
                a.AddUnique(r.Next(0, 300000));
            }
            a.SelfCheckRules();
            if (a.Count != 400000)
                throw new Exception("bad");

            Console.WriteLine($"avr depth: {a.tot_depth / a.cnt_depth}, max={a.max_depth}");
        }

        {
            SortedDictionary<int, int> dict = new();
            SortedMap<int, int> map = new();
            for (int j = 0; j < 10000; j++) {
                for (int u = 0; u < 2; u++) {
                    var k = next();
                    var v = next();
                    dict.TryAdd(k, v);
                    map.Add(k, v);
                }
                {
                    var k = next();
                    dict.Remove(k);
                    map.Remove(k);
                }
            }
            if (!dict.Select(x => x.Key).SequenceEqual(map.Select(x => x.Key))) {
                throw new Exception("bad");
            }
            if (!dict.Select(x => x.Value).SequenceEqual(map.Select(x => x.Value))) {
                throw new Exception("bad");
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
    Random rnd = new(848);
    int next() { return rnd.Next(0,N); }
    public void clear() {
        rnd = new Random(343);
    }

    [Benchmark]
    public void SortedList_Insert() {
        var r = new Random(343);
        SortedList<int> list = new();
        for (int i = 0; i < 400000; i++) {
            list.Add(i);
            list.Add(r.Next(0, 300000));
        }
        if (list.Count != 400000) throw new Exception("bad");
    }

    [Benchmark]
    public void SortedSet_Insert() {
        clear();
        SortedSet<int> list = new();
        for (int i = 0; i < 400000; i++) {
            list.Add(i);
            list.Add(next());
        }
        if (list.Count != 400000) throw new Exception("bad");
    }

    [Benchmark]
    public void SortedList_AddRemoveRnd() {
        clear();
        SortedList<int> list = new();
        for (int i = 0; i < N; i++) {
            list.Add(next());
            list.Remove(next());
        }
    }

    [Benchmark]
    public void SortedSet_AddRemoveRnd() {
        clear();
        SortedSet<int> list = new();
        for (int i = 0; i < N; i++) {
            list.Add(next());
            list.Remove(next());
        }
    }

    //[Benchmark]
    //public void SortedList_AddRemoveLast() {
    //    SortedList<int> list = new();
    //    for (int i = 0; i < N; i++) {
    //        list.Add(i);
    //    }
    //    for (int i = 0; i < N; i++) {
    //        list.Remove(i);
    //    }
    //}

    //[Benchmark]
    //public void SortedSet_AddRemoveLast() {
    //    SortedSet<int> list = new();
    //    for (int i = 0; i < N; i++) {
    //        list.Add(i);
    //    }
    //    for (int i = 0; i < N; i++) {
    //        list.Remove(i);
    //    }
    //}

    //[Benchmark]
    //public void SortedMap_AddRemoveRnd() {
    //    clear();
    //    SortedMap<int, int> list = new();
    //    for (int i = 0; i < N; i++) {
    //        list.Add(next(), next());
    //        list.Remove(next());
    //    }
    //}

    //[Benchmark]
    //public void SortedDictionary_AddRemoveRnd() {
    //    clear();
    //    SortedDictionary<int, int> list = new();
    //    for (int i = 0; i < N; i++) {
    //        list.TryAdd(next(), next());
    //        list.Remove(next());
    //    }
    //}

    //[Benchmark]
    //public void SortedMultiList_AddRemoveAll() {
    //    clear();
    //    SortedMultiList<int> list = new();
    //    for (int i = 0; i < N; i++) {
    //        list.Add(rnd.Next(0, N / 2));
    //        list.RemoveAllOf(rnd.Next(0, N / 2));
    //    }
    //}

}
