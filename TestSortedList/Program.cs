

using AlgoQuora;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using static System.Net.Mime.MediaTypeNames;

namespace TestSortedList; 
class SortedListChecked<T> : SortedList<T> where T : new() {
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
                if(Compare(t.left.node, t.node) > 0) {
                    throw new Exception("bad order");
                }
            }
            if (!is_nil(t.right)) {
                if(t.prior < t.right.prior)
                    throw new Exception("bad right prior");
                if (Compare(t.right.node, t.node) < 0) {
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

    public void Add(T v) {
        lst.Add(v);
        for(int i = 0; i < checker.Count; i++) {
            int cmp = Comparer<T>.Default.Compare(v, checker[i]);
            if (cmp == 0) return;
            if(cmp < 0) {
                checker.Insert(i, v);
                return;
            }
        }
        checker.Add(v);
    }
    public void Check() {
        lst.SelfCheck();
        if (lst.Count != checker.Count) throw new Exception("bad count");
        if (!lst.SequenceEqual(checker)) throw new Exception("not equal");
    }

}

internal class Program {
    static void Test() {
        Random rnd = new(111);
        for (int i = 0; i < 100; i++) {
            SortedList_Tester<int> tester = new();
            for (int j = 0; j < 1000; j++) {
                tester.Add(rnd.Next(0, 100));
                tester.Check();
            }
        }
        Console.WriteLine("Test OK");
    }
    static void Main(string[] args) {
        Test();
        BenchmarkRunner.Run(typeof(Program).Assembly, new Config());
    }
}

public class MyBenchmarks {
    int N = 300000;

    [Benchmark]
    public void SortedList_Add() {
        Random rnd = new(88);
        SortedList<int> list = new();
        for (int i = 0; i < N; i++) {
            list.Add(rnd.Next(0, N / 2));
        }
    }

    [Benchmark]
    public void SortedSet_Add() {
        Random rnd = new(88);
        SortedSet<int> list = new();
        for (int i = 0; i < N; i++) {
            list.Add(rnd.Next(0, N / 2));
        }
    }


}
