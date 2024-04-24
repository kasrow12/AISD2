
using System;
using System.Threading;
using ASD.Graphs;
using ASD.Graphs.Testing;

class Lab10
{

    private static Graph[] cliq_test;
    private static Graph<int>[,] izo_test;
    private static int[] cliq_res;
    private static bool[] izo_res;

    private static Graph[] cliq_test2;
    private static Graph<int>[,] izo_test2;
    private static int[] cliq_res2;
    private static bool[] izo_res2;

    private static double speedFactor;

    public static int Fib(int n)
    {
        if (n < 2)
            return n;
        return Fib(n - 1) + Fib(n - 2);
    }

    public static void Main()
    {
        int[] clique, map;
        bool izo, res;
        int n;
        Graph g, h;

        DateTime t1 = DateTime.Now;
        Fib(39);
        DateTime t2 = DateTime.Now;
        speedFactor = (t2 - t1).TotalMilliseconds / 1000;
        Console.WriteLine($"SpeedFactor = {speedFactor}");

        PrepareTests();
        PrepareTests2();

        Console.WriteLine();
        Console.WriteLine("Clique Tests");
        for (int i = 0; i < cliq_test.Length; ++i)
        {
            Console.Write($"Test {i + 1}:  ");
            g = (Graph)cliq_test[i].Clone();
            n = cliq_test[i].MaxClique(out clique);
            Console.WriteLine("{0}", n == cliq_res[i] && CliqueTest(cliq_test[i], n, clique) && cliq_test[i].Equals(g) ? "Passed" : "Fail" + $"(Answer = {n}, expected = {cliq_res[i]})");
        }

        Console.WriteLine();
        Console.WriteLine("Clique Tests - efficiency");
        for (int i = 0; i < cliq_test2.Length; ++i)
        {
            n = 0;
            clique = null;
            Thread thr = new Thread(() => { n = cliq_test2[i].MaxClique(out clique); });
            Console.Write($"Test {i + 1}:  ");
            g = (Graph)cliq_test2[i].Clone();
            thr.Start();
            if (!thr.Join((int)(speedFactor * 4000)))  // powinno wystarczyc 2000 
            {
                thr.Abort();
                Console.WriteLine("Timeout");
            }
            else
                Console.WriteLine("{0}", n == cliq_res2[i] && CliqueTest(cliq_test2[i], n, clique) && cliq_test2[i].Equals(g) ? "Passed" : "Fail" + $"(Answer = {n}, expected = {cliq_res[i]})");
        }

        Console.WriteLine();
        Console.WriteLine("Isomorpism Tests");
        for (int i = 0; i < izo_test.GetLength(0); ++i)
        {
            Console.Write($"Test {i + 1}:  ");
            g = (Graph<int>)izo_test[i, 0].Clone();
            h = (Graph<int>)izo_test[i, 1].Clone();
            izo = izo_test[i, 0].IsomorphismTest(izo_test[i, 1], out map);
            res = (izo ? izo_res[i] && map != null && CheckIsomorphism(izo_test[i, 1], izo_test[i, 0], map) == izo_res[i] : !izo_res[i] && map == null)
                    && izo_test[i, 0].Equals(g) && izo_test[i, 1].Equals(h);
            Console.WriteLine("{0}", res ? "Passed" : "Fail");
        }

        Console.WriteLine();
        Console.WriteLine("Isomorpism Tests - efficiency");
        for (int i = 0; i < izo_test2.GetLength(0); ++i)
        {
            map = null;
            izo = !izo_res2[i];
            Thread thr = new Thread(() => { izo = izo_test2[i, 0].IsomorphismTest(izo_test2[i, 1], out map); });
            Console.Write($"Test {i + 1}:  ");
            g = (Graph<int>)izo_test2[i, 0].Clone();
            h = (Graph<int>)izo_test2[i, 1].Clone();
            thr.Start();
            if (!thr.Join((int)(speedFactor * 4000)))  // powinno wystarczyc 2000 
            {
                thr.Abort();
                Console.WriteLine("Timeout");
            }
            else
            {
                res = (izo ? izo_res2[i] && map != null && CheckIsomorphism(izo_test2[i, 1], izo_test2[i, 0], map) == izo_res2[i] : !izo_res2[i] && map == null)
                        && izo_test2[i, 0].Equals(g) && izo_test2[i, 1].Equals(h);
                Console.WriteLine("{0}", res ? "Passed" : "Fail");
            }
        }

        Console.WriteLine();
    }

    public static void PrepareTests()
    {
        var rgg = new RandomGraphGenerator(123);

        cliq_test = new Graph[5];
        izo_test = new Graph<int>[4, 2];

        cliq_res = new int[] { 4, 20, 19, 19, 11 };
        izo_res = new bool[] { true, false, true, false };

        if (cliq_test.Length != cliq_res.Length || izo_test.GetLongLength(0) != izo_res.Length)
            throw new ApplicationException("Zle zddefiniowane testy");

        cliq_test[0] = new Graph(8, new MatrixGraphRepresentation());
        cliq_test[0].AddEdge(0, 4);
        cliq_test[0].AddEdge(0, 7);
        cliq_test[0].AddEdge(1, 2);
        cliq_test[0].AddEdge(1, 3);
        cliq_test[0].AddEdge(1, 5);
        cliq_test[0].AddEdge(1, 6);
        cliq_test[0].AddEdge(2, 5);
        cliq_test[0].AddEdge(2, 6);
        cliq_test[0].AddEdge(3, 4);
        cliq_test[0].AddEdge(3, 7);
        cliq_test[0].AddEdge(4, 7);
        cliq_test[0].AddEdge(5, 6);

        cliq_test[1] = new Graph(20, new MatrixGraphRepresentation());
        for (int i = 0; i < cliq_test[1].VertexCount; ++i)
            for (int j = i + 1; j < cliq_test[1].VertexCount; ++j)
                cliq_test[1].AddEdge(i, j);

        cliq_test[2] = (Graph)cliq_test[1].Clone();
        cliq_test[2].RemoveEdge(0, 1);

        cliq_test[3] = (Graph)cliq_test[2].Clone();
        cliq_test[3].RemoveEdge(0, 2);

        cliq_test[4] = rgg.Graph(50, 0.7);

        izo_test[0, 0] = rgg.AssignWeights(cliq_test[0], 0, 0);
        izo_test[0, 1] = rgg.Permute(izo_test[0, 0]);

        izo_test[1, 0] = (Graph<int>)izo_test[0, 0].Clone();
        izo_test[1, 1] = (Graph<int>)izo_test[0, 1].Clone();
        izo_test[1, 0].SetEdgeWeight(2, 5, 3);

        //rgg.SetSeed(1234);
        izo_test[2, 0] = rgg.WeightedGraph(50, 0.95, 1, 999);
        izo_test[2, 1] = (Graph<int>)izo_test[2, 0].Clone();
        izo_test[2, 1] = rgg.Permute(izo_test[2, 1]);

        izo_test[3, 0] = rgg.WeightedGraph(300, 0.09, 1, 3);
        izo_test[3, 1] = rgg.WeightedGraph(300, 0.09, 1, 3);
    }

    public static void PrepareTests2()
    {
        int n;
        var rgg = new RandomGraphGenerator(124);

        cliq_test2 = new Graph[3];
        izo_test2 = new Graph<int>[2, 2];

        cliq_res2 = new int[] { 4, 4, 5 };
        izo_res2 = new bool[] { false, true };

        if (cliq_test2.Length != cliq_res2.Length || izo_test2.GetLongLength(0) != izo_res2.Length)
            throw new ApplicationException("Zle zddefiniowane testy");

        //rgg.SetSeed(123);
        cliq_test2[0] = rgg.Graph(500, 0.03);
        //rgg.SetSeed(125);
        cliq_test2[1] = rgg.Graph(300, 0.06);

        n = 500;
        cliq_test2[2] = new Graph(n);
        for (int i = 0; i < n; ++i)
            for (int j = 1; j <= 4; ++j)
                cliq_test2[2].AddEdge(i, (i + j) % n);

        n = 24;
        izo_test2[0, 0] = new Graph<int>(n);
        for (int i = 0; i < n; ++i)
            for (int j = 0; j < n; ++j)
                if (i != j)
                    izo_test2[0, 0].AddEdge(i, j);
        izo_test2[0, 1] = (Graph<int>)izo_test2[0, 0].Clone();
        for (int i = 0; i < n; ++i)
            izo_test2[0, 0].RemoveEdge(i, (i + 1) % n);
        for (int i = 0; i < n; ++i)
            izo_test2[0, 1].RemoveEdge(i, (i + 2) % n);

        //rgg.SetSeed(1234);
        izo_test2[1, 0] = rgg.WeightedGraph(250, 0.95, 1, 999);
        izo_test2[1, 1] = rgg.Permute(izo_test2[1, 0]);
    }

    public static bool CliqueTest(Graph g, int cn, int[] cl)
    {
        if (cl == null || cn != cl.Length) return false;
        for (int i = 0; i < cn; ++i)
            for (int j = i + 1; j < cn; ++j)
                if (!g.HasEdge(cl[i], cl[j]))
                    return false;
        return true;
    }


    public static bool CheckIsomorphism(Graph<int> g, Graph<int> h, int[] map)
    {
        if (map.GetLength(0) != g.VertexCount || h.VertexCount != g.VertexCount)
            return false;
        int[] revMap = new int[map.GetLength(0)];
        for (int i = 0; i < map.GetLength(0); i++)
            revMap[map[i]] = i;
        foreach (Edge<int> e in g.DFS().SearchAll())
            if (!h.HasEdge(map[e.From], map[e.To]) || g.GetEdgeWeight(e.From, e.To) != h.GetEdgeWeight(map[e.From], map[e.To]))
                return false;
        foreach (Edge<int> e in h.DFS().SearchAll())
            if (!g.HasEdge(revMap[e.From], revMap[e.To]) || g.GetEdgeWeight(revMap[e.From], revMap[e.To]) != h.GetEdgeWeight(e.From, e.To))
                return false;
        return true;
    }

}
