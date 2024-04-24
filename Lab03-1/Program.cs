using System;
using ASD.Graphs;
using ASD.Graphs.Testing;

namespace ASD
{

    class Lab03Main
    {

        const int ReverseTestSize = 5;
        const int BipartiteTestSize = 5;
        const int KruskalTestSize = 5;
        const int AcyclicTestSize = 5;

        static bool maskExceptions = false;

        static void Main(string[] args)
        {
            Console.WriteLine("\nPart 1 - Reverse");
            TestReverse();
            Console.WriteLine("\nPart 2 - Bipartite");
            TestBipartite();
            Console.WriteLine("\nPart 3 - Kruskal");
            TestKruskal();
            Console.WriteLine("\nPart 4 - Acyclic");
            TestAcyclic();
            Console.WriteLine();
        }

        private static void TestReverse()
        {
            var rgg = new RandomGraphGenerator(12345);
            DiGraph[] g = new DiGraph[ReverseTestSize];
            DiGraph r, gg;
            g[0] = rgg.DiGraph(10, 0.7);
            g[1] = rgg.DiGraph(100, 0.1);
            rgg.SetRepresentation(new MatrixGraphRepresentation());
            g[2] = rgg.DiGraph(10, 0.5);
            g[3] = rgg.DiGraph(30, 0.1);
            g[4] = new DiGraph(50000);

            for (int i = 0; i < ReverseTestSize; ++i)
            {
                Console.Write($"  Test {i} - ");
                gg = (DiGraph)g[i].Clone();
                try
                {
                    r = (new Lab03GraphFunctions()).Lab03Reverse(g[i]);
                    if (r == null)
                    {
                        Console.WriteLine("Failed : null returned");
                        continue;
                    }
                    if (r.Representation.GetType() != g[i].Representation.GetType())
                    {
                        Console.WriteLine("Failed : invalid graph representation");
                        continue;
                    }
                    if (!g[i].Equals(gg))
                    {
                        Console.WriteLine("Failed : graph was destroyed");
                        continue;
                    }
                    bool ok = true;
                    if (r.VertexCount != gg.VertexCount)
                        ok = false;
                    if (ok)
                        for (int v = 0; v < r.VertexCount; v++)
                        {
                            foreach (int u in r.OutNeighbors(v))
                                if (!gg.HasEdge(u, v))
                                    ok = false;
                            foreach (int u in gg.OutNeighbors(v))
                                if (!r.HasEdge(u, v))
                                    ok = false;
                        }
                    if (!ok)
                    {
                        Console.WriteLine("Failed : bad result");
                        continue;
                    }
                    Console.WriteLine("Passed");
                }
                catch (System.Exception e) when (maskExceptions)
                {
                    Console.WriteLine($"Failed : {e.GetType()} : {e.Message}");
                }
            }
        }

        private static void TestBipartite()
        {
            var rgg = new RandomGraphGenerator(12345);
            Graph[] g = new Graph[BipartiteTestSize];
            bool?[] res = { true, false, false, false, true };
            Graph gg;
            bool r;
            int[] part;
            g[0] = rgg.BipartiteGraph(4, 3, 0.4);
            g[1] = rgg.Graph(100, 0.1);
            g[2] = rgg.Graph(10, 0.5);
            g[3] = GraphExamples.Cycle(50001);
            g[4] = new Graph(50000);

            for (int i = 0; i < BipartiteTestSize; ++i)
            {
                Console.Write($"  Test {i} - ");
                gg = (Graph)g[i].Clone();
                try
                {
                    r = (new Lab03GraphFunctions()).Lab03IsBipartite(g[i], out part);
                    if (!g[i].Equals(gg))
                    {
                        Console.WriteLine("Failed : graph was destroyed");
                        continue;
                    }
                    if (r != res[i])
                    {
                        Console.WriteLine("Failed : bad result");
                        continue;
                    }
                    if (r && !IsProperPartition(g[i], part))
                    {
                        Console.WriteLine("Failed : invalid partition");
                        continue;
                    }
                    if (!r && part != null)
                    {
                        Console.WriteLine("Failed : part==null expected");
                        continue;
                    }
                    Console.WriteLine("Passed");
                }
                catch (System.Exception e) when (maskExceptions)
                {
                    Console.WriteLine($"Failed : {e.GetType()} : {e.Message}");
                }
            }
        }

        private static void TestKruskal()
        {
            var rgg = new RandomGraphGenerator(12345);
            Graph<int>[] g = new Graph<int>[KruskalTestSize];
            Graph<int> r, gg;
            int[] res = { -133, -76248, 9, 20462, 0};
            int mstwr;
            g[0] = rgg.WeightedGraph(5, 0.7, -99, 99);
            g[1] = rgg.WeightedGraph(100, 0.1, -999, 999);
            g[2] = rgg.WeightedGraph(10, 0.5, 1, 1);
            g[3] = rgg.AssignWeights(GraphExamples.Cycle(50000), -99, 99);
            g[4] = new Graph<int>(50000);

            for (int i = 0; i < KruskalTestSize; ++i)
            {
                Console.Write($"  Test {i} - ");
                gg = (Graph<int>)g[i].Clone();
                try
                {
                    r = (new Lab03GraphFunctions()).Lab03Kruskal(g[i], out mstwr);
                    if (r == null)
                    {
                        Console.WriteLine("Failed : null returned");
                        continue;
                    }
                    if (r.Directed)
                    {
                        Console.WriteLine("Failed : returned graph is directed");
                        continue;
                    }
                    if (r.GetType() != g[i].GetType())
                    {
                        Console.WriteLine("Failed : invalid graph representation");
                        continue;
                    }
                    if (!g[i].Equals(gg))
                    {
                        Console.WriteLine("Failed : graph was destroyed");
                        continue;
                    }
                    if (mstwr != res[i])
                    {
                        Console.WriteLine($"Failed : bad result (expected = {res[i]}, actual = {mstwr})");
                        continue;
                    }
                    if(!IsSubtree(g[i], r, mstwr))
                    {
                        Console.WriteLine($"Failed : wrong spanning tree");
                        continue;
                    }
                    Console.WriteLine("Passed");
                }
                catch (System.Exception e) when (maskExceptions)
                {
                    Console.WriteLine($"Failed : {e.GetType()} : {e.Message}");
                }
            }
        }

        private static void TestAcyclic()
        {
            var rgg = new RandomGraphGenerator(12345);
            Graph[] g = new Graph[AcyclicTestSize];
            bool?[] res = { true, false, false, false, true };
            Graph gg;
            bool r;
            g[0] = rgg.Tree(7);
            g[1] = rgg.Graph(100, 0.1);
            g[2] = rgg.Graph(10, 0.5);
            g[3] = GraphExamples.Cycle(50000);
            g[4] = new Graph(50000);

            for (int i = 0; i < AcyclicTestSize; ++i)
            {
                Console.Write($"  Test {i} - ");
                gg = (Graph) g[i].Clone();
                try
                {
                    r = (new Lab03GraphFunctions()).Lab03IsUndirectedAcyclic(g[i]);
                    if (!g[i].Equals(gg))
                    {
                        Console.WriteLine("Failed : graph was destroyed");
                        continue;
                    }
                    if (r != res[i])
                    {
                        Console.WriteLine("Failed : bad result");
                        continue;
                    }
                    Console.WriteLine("Passed");
                }
                catch (System.Exception e) when (maskExceptions)
                {
                    Console.WriteLine($"Failed : {e.GetType()} : {e.Message}");
                }
            }
        }

        private static bool IsSubtree(Graph<int> g, Graph<int> t, int mstwr)
        {
            if (t == null)
                return false;
            int edges = 0;
            if (g.VertexCount != t.VertexCount) return false;
            for (int v = 0; v < t.VertexCount; ++v)
                foreach (int u in t.OutNeighbors(v))
                {
                    if (u < v)
                    {
                        edges++;
                        mstwr -= g.GetEdgeWeight(v, u);
                    }
                    if (!g.HasEdge(v, u))
                        return false;
                }
            if (mstwr != 0)
                return false;
            return edges <= g.VertexCount - 1;
        }

        private static bool IsProperPartition(Graph g, int[] part)
        {
            if (part == null || part.Length != g.VertexCount) return false;
            for (int v = 0; v < g.VertexCount; ++v)
                if (part[v] != 1 && part[v] != 2)
                    return false;
            for (int v = 0; v < g.VertexCount; ++v)
                foreach (int u in g.OutNeighbors(v))
                    if (part[u] == part[v])
                        return false;
            return true;
        }

    }  // class Lab03

}
