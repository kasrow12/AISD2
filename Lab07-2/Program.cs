using ASD.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASD
{
    class TestCase
    {
        private Graph<double> _graph;
        private int _edgeConnectivity;
        private string _name;
        private IEnumerable<Tuple<int, int, int>> _stCutTests;

        public TestCase(string name, Graph<double> graph, int edgeConnectivity, IEnumerable<Tuple<int, int, int>> stCutTests)
        {
            _name = name;
            _graph = graph;
            _edgeConnectivity = edgeConnectivity;
            _stCutTests = stCutTests;
        }

        public void RunMinCutTest()
        {
            Console.WriteLine("    Test {0}", _name);
            foreach (var test in _stCutTests)
            {
                Console.Write("\tPrzekrój: {0}-{1}", test.Item1, test.Item2);

                Edge<double>[] cut;
                var cutValue = _graph.MinCut(test.Item1, test.Item2, out cut);
                if (cutValue != test.Item3)
                {
                    Console.WriteLine("\tBłędna waga zbioru rozcinającego");
                }
                else
                {
                    if (VerifyCut(cut, cutValue, test.Item1, test.Item2))
                    {
                        Console.WriteLine("\tOK");
                    }
                }

                //                Console.WriteLine();
            }

            Console.WriteLine();
        }

        public void RunEdgeConnectivityTest()
        {
            Console.WriteLine("    Test {0}", _name);
            Edge<double>[] cutingSet;
            var result = _graph.EdgeConnectivity(out cutingSet);
            Console.WriteLine(result == _edgeConnectivity ? "\tOK" : "\tBłędna spójność krawędziowa");
            //            Console.WriteLine();
        }

        public void RunEdgeConnectivityWithCutTest()
        {
            Console.WriteLine("    Test {0}", _name);
            Edge<double>[] cut;
            var result = _graph.EdgeConnectivity(out cut);
            if (result != _edgeConnectivity)
            {
                Console.WriteLine("\tBłędna spójność krawędziowa");
            }
            else
            {
                if (VerifyCut(cut, result))
                {
                    Console.WriteLine("\tOK");
                }
            }
            //            Console.WriteLine();
        }

        private bool VerifyCut(IEnumerable<Edge<double>> cutingSet, double value, int s = 0, int? t = null)
        {
            if (cutingSet.Sum(e => e.Weight) != value)
            {
                Console.WriteLine($"\tWaga zboru rozcinającego nie zgadza się z deklarowaną {value} {cutingSet.Sum(e => e.Weight)}");
                return false;
            }

            var cutGraph = (Graph<double>)_graph.Clone();
            foreach (var e in cutingSet)
            {
                cutGraph.RemoveEdge(e.From, e.To);
            }

            HashSet<int> visetedVertices = new HashSet<int>();
            visetedVertices.Add(s);
            foreach (var e in cutGraph.DFS().SearchFrom(s))
                visetedVertices.Add(e.To);

            if (t.HasValue)
            {
                if (visetedVertices.Contains(t.Value))
                {
                    Console.WriteLine("\tDany zbiór nie rozcina s od t");
                    return false;
                }

                return true;
            }

            if (visetedVertices.Count == _graph.VertexCount)
            {
                Console.WriteLine("\tDany zbiór nie rozcina grafu");
                return false;
            }

            return true;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            List<TestCase> tests = new List<TestCase>();

            Graph<double> testGraph = new Graph<double>(2, new MatrixGraphRepresentation());

            testGraph.AddEdge(0, 1, 1);

            tests.Add(
                new TestCase(
                    "K2",
                    testGraph,
                    1,
                    new[]
                    {
                        new Tuple<int, int, int>(0, 1, 1),
                        new Tuple<int, int, int>(1, 0, 1),
                    }));

            testGraph = new Graph<double>(2);

            testGraph.AddEdge(0, 1, 5);

            tests.Add(
                new TestCase(
                    "K2 ważone",
                    testGraph,
                    5,
                    new[]
                    {
                        new Tuple<int, int, int>(0, 1, 5),
                        new Tuple<int, int, int>(1, 0, 5),
                    }));


            testGraph = new Graph<double>(4);

            testGraph.AddEdge(0, 1, 1);
            testGraph.AddEdge(0, 2, 1);
            testGraph.AddEdge(0, 3, 1);

            testGraph.AddEdge(1, 2, 1);
            testGraph.AddEdge(1, 3, 1);

            testGraph.AddEdge(2, 3, 1);

            tests.Add(
                new TestCase(
                    "K4",
                    testGraph,
                    3,
                    new[]
                    {
                        new Tuple<int, int, int>(0, 1, 3),
                        new Tuple<int, int, int>(1, 0, 3),
                        new Tuple<int, int, int>(0, 2, 3),
                        new Tuple<int, int, int>(2, 0, 3),
                        new Tuple<int, int, int>(1, 2, 3),
                        new Tuple<int, int, int>(2, 3, 3),
                    }));


            testGraph = new Graph<double>(4);

            testGraph.AddEdge(0, 1, 2);
            testGraph.AddEdge(0, 2, 3);
            testGraph.AddEdge(0, 3, 4);

            testGraph.AddEdge(1, 2, 5);
            testGraph.AddEdge(1, 3, 6);

            testGraph.AddEdge(2, 3, 7);

            tests.Add(
                new TestCase(
                    "K4 ważone",
                    testGraph,
                    9,
                    new[]
                    {
                        new Tuple<int, int, int>(0, 1, 9),
                        new Tuple<int, int, int>(1, 0, 9),
                        new Tuple<int, int, int>(0, 2, 9),
                        new Tuple<int, int, int>(1, 2, 13),
                        new Tuple<int, int, int>(2, 1, 13),
                        new Tuple<int, int, int>(2, 3, 15),
                        new Tuple<int, int, int>(3, 2, 15),
                    }));


            testGraph = new Graph<double>(10);

            testGraph.AddEdge(0, 1, 1);
            testGraph.AddEdge(0, 3, 1);
            testGraph.AddEdge(0, 2, 1);
            testGraph.AddEdge(1, 4, 1);
            testGraph.AddEdge(2, 4, 1);
            testGraph.AddEdge(3, 4, 1);

            testGraph.AddEdge(4, 5, 1);

            testGraph.AddEdge(5, 6, 1);
            testGraph.AddEdge(5, 7, 1);
            testGraph.AddEdge(5, 8, 1);
            testGraph.AddEdge(6, 9, 1);
            testGraph.AddEdge(7, 9, 1);
            testGraph.AddEdge(8, 9, 1);

            tests.Add(
                new TestCase(
                    "Duży jednospójny graf",
                    testGraph,
                    1,
                    new[]
                    {
                        new Tuple<int, int, int>(0, 8, 1),
                        new Tuple<int, int, int>(0, 1, 2),
                        new Tuple<int, int, int>(1, 0, 2),
                        new Tuple<int, int, int>(0, 5, 1),
                        new Tuple<int, int, int>(5, 0, 1),
                        new Tuple<int, int, int>(0, 3, 2),
                        new Tuple<int, int, int>(3, 0, 2)
                    }));

            testGraph = new Graph<double>(10);

            testGraph.AddEdge(0, 1, 2);
            testGraph.AddEdge(0, 3, 1);
            testGraph.AddEdge(0, 2, 1);
            testGraph.AddEdge(1, 4, 4);
            testGraph.AddEdge(2, 4, 3);
            testGraph.AddEdge(3, 4, 5);

            testGraph.AddEdge(4, 5, 5);

            testGraph.AddEdge(5, 6, 2);
            testGraph.AddEdge(5, 7, 3);
            testGraph.AddEdge(5, 8, 4);
            testGraph.AddEdge(6, 9, 2);
            testGraph.AddEdge(7, 9, 3);
            testGraph.AddEdge(8, 9, 4);

            tests.Add(
                new TestCase(
                    "Duży jednospójny graf ważony",
                    testGraph,
                    4,
                    new[]
                    {
                        new Tuple<int, int, int>(0, 8, 4),
                        new Tuple<int, int, int>(0, 1, 4),
                        new Tuple<int, int, int>(1, 0, 4),
                        new Tuple<int, int, int>(0, 5, 4),
                        new Tuple<int, int, int>(5, 0, 4),
                        new Tuple<int, int, int>(8, 9, 8),
                        new Tuple<int, int, int>(9, 8, 8),
                    }));


            Console.WriteLine("\nMinimalny przekrój\n");
            foreach (var test in tests)
            {
                test.RunMinCutTest();
            }

            Console.WriteLine("\nSpójność krawędziowa\n");
            foreach (var test in tests)
            {
                test.RunEdgeConnectivityTest();
            }

            Console.WriteLine("\nSpójność krawędziowa ze zbiorem rozcinającym\n");
            foreach (var test in tests)
            {
                test.RunEdgeConnectivityWithCutTest();
            }

            Console.WriteLine();
        }

    }

}