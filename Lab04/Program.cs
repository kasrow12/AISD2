using System;
using System.Collections.Generic;
using System.Linq;
using ASD.Graphs;
using ASD.Graphs.Testing;

namespace ASD
{
    public class Lab04Stage1TestCase : TestCase
    {
        DiGraph graph;
        int miastoStartowe;
        int K;

        int[] result;
        int[] expectedResult;

        public Lab04Stage1TestCase(DiGraph graph, int miastoStartowe, int K, int[] expectedResult, double timeLimit, string description) : base(timeLimit, null, description)
        {
            this.graph = graph;
            this.miastoStartowe = miastoStartowe;
            this.K = K;
            this.expectedResult = expectedResult;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            result = ((Lab04)prototypeObject).Lab04Stage1(graph, miastoStartowe, K);
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            var res = CheckSolution();
            return (res.resultCode, $"{res.message} [{Description}]");
        }

        private (Result resultCode, string message) CheckSolution()
        {
            if (result == null)
            {
                return (Result.WrongResult, "Zwrócono null zamiast tablicy numerów miast");
            }

            if (result.Length != expectedResult.Length)
            {
                return (Result.WrongResult, $"Zła liczba zwróconych numerów miast: otrzymano {result.Length}, oczekiwano {expectedResult.Length}");
            }

            foreach (var v in result)
            {
                if (v < 0 || v >= graph.VertexCount)
                {
                    return (Result.WrongResult, "W tablicy znajduje się numer spoza zakresu");
                }
            }

            for (int i = 1; i < result.Length; i++)
            {
                if (result[i] < result[i - 1])
                {
                    return (Result.WrongResult, "Tablica nie jest posortowana rosnąco");
                }
            }

            for (int i = 1; i < result.Length; i++)
            {
                if (result[i] == result[i - 1])
                {
                    return (Result.WrongResult, "Tablica zawiera powtarzające się elementy");
                }
            }

            for (int i = 0; i < result.Length; i++)
            {
                if (result[i] != expectedResult[i])
                {
                    return (Result.WrongResult, $"Miasto {result[i]} nie jest obecne w rozwiązaniu");
                }
            }

            return (PerformanceTime > TimeLimit ? Result.LowEfficiency : Result.Success, $"OK ({PerformanceTime.ToString("#0.00")}s)");
        }
    }

    public class Lab04Stage2TestCase : TestCase
    {
        DiGraph<int> graph;
        int miastoStartowe;
        int K;

        int[] result;
        int[] expectedResult;

        public Lab04Stage2TestCase(DiGraph<int> graph, int miastoStartowe, int K, int[] expectedResult, double timeLimit, string description) : base(timeLimit, null, description)
        {
            this.graph = graph;
            this.miastoStartowe = miastoStartowe;
            this.K = K;
            this.expectedResult = expectedResult;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            result = ((Lab04)prototypeObject).Lab04Stage2(graph, miastoStartowe, K);
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            var res = CheckSolution();
            return (res.resultCode, $"{res.message} [{Description}]");
        }

        private (Result resultCode, string message) CheckSolution()
        {
            if (result == null)
            {
                return (Result.WrongResult, "Zwrócono null zamiast tablicy numerów miast");
            }

            if (result.Length != expectedResult.Length)
            {
                return (Result.WrongResult, $"Zła liczba zwróconych numerów miast: otrzymano {result.Length}, oczekiwano {expectedResult.Length}");
            }

            foreach (var v in result)
            {
                if (v < 0 || v >= graph.VertexCount)
                {
                    return (Result.WrongResult, "W tablicy znajduje się numer spoza zakresu");
                }
            }

            for (int i = 1; i < result.Length; i++)
            {
                if (result[i] < result[i - 1])
                {
                    return (Result.WrongResult, "Tablica nie jest posortowana rosnąco");
                }
            }

            for (int i = 1; i < result.Length; i++)
            {
                if (result[i] == result[i - 1])
                {
                    return (Result.WrongResult, "Tablica zawiera powtarzające się elementy");
                }
            }

            for (int i = 0; i < result.Length; i++)
            {
                if (result[i] != expectedResult[i])
                {
                    return (Result.WrongResult, $"Miasto {result[i]} nie powinno być obecne w rozwiązaniu");
                }
            }

            return (PerformanceTime > TimeLimit ? Result.LowEfficiency : Result.Success, $"OK ({PerformanceTime.ToString("#0.00")}s)");
        }

        private (Result resultCode, string message) Ok()
        {
            return (PerformanceTime > TimeLimit ? Result.LowEfficiency : Result.Success, $"OK ({PerformanceTime.ToString("#0.00")}s)");
        }
    }

    public class Lab04Tests : TestModule
    {
        TestSet stage1 = new TestSet(new Lab04(), "Etap 1: Pociag o kazdej pelnej godzinie");
        TestSet stage2 = new TestSet(new Lab04(), "Etap 2: Jedna krawedz - jeden pociag");
        public override void PrepareTestSets()
        {
            TestSets["Stage1"] = stage1;
            TestSets["Stage2"] = stage2;

            PrepareStages();
        }

        private void AddStage1(Lab04Stage1TestCase testCase)
        {
            stage1.TestCases.Add(testCase);
        }

        private void AddStage2(Lab04Stage2TestCase testCase)
        {
            stage2.TestCases.Add(testCase);
        }

        private void PrepareStages()
        {
            RandomGraphGenerator rGG = new RandomGraphGenerator(1234);

            // Etap 1, test 1, 2, 3
            int n = 7;
            DiGraph g1_1 = new DiGraph(n);
            g1_1.AddEdge(0, 1);
            g1_1.AddEdge(0, 2);
            g1_1.AddEdge(1, 2);
            g1_1.AddEdge(2, 3);
            g1_1.AddEdge(3, 1);
            g1_1.AddEdge(3, 4);
            g1_1.AddEdge(4, 5);

            AddStage1(new Lab04Stage1TestCase(g1_1, 0, 11, new int[] { 0, 1, 2, 3, 4 }, 1, "Przykład z zadania"));
            AddStage1(new Lab04Stage1TestCase(g1_1, 3, 10, new int[] { 1, 2, 3, 4, 5 }, 1, "Przykład z zadania, miastoStartowe=3, K=10"));
            AddStage1(new Lab04Stage1TestCase(g1_1, 6, 10, new int[] { 6 }, 0.1, "Przykład z zadania, miastoStartowe=6, K=20"));

            // Etap 1, test 4
            n = 6;
            DiGraph g1_4 = new DiGraph(n);
            g1_4.AddEdge(0, 1);
            g1_4.AddEdge(1, 2);
            g1_4.AddEdge(1, 3);
            g1_4.AddEdge(3, 4);
            g1_4.AddEdge(4, 2);
            g1_4.AddEdge(2, 5);
            AddStage1(new Lab04Stage1TestCase(g1_4, 0, 10, new int[] { 0, 1, 2, 3 }, 1, "Mały graf"));

            // Etap 1, test 5
            n = 1000;
            DiGraph g1_5 = new DiGraph(n);
            for (int i = 0; i <= 998; i++){
                g1_5.AddEdge(i, i+1);
            }
            for (int i = 4; i <= 998; i++){
                for (int j = i+1; j <= 999; j++){
                    g1_5.AddEdge(j, i);
                }
            }
            AddStage1(new Lab04Stage1TestCase(g1_5, 0, 11, new int[] { 0, 1, 2, 3 }, 1, "Duży graf, ale mało godzin"));

            // Etap 1, test 6
            n = 1000;
            DiGraph g1_6 = new DiGraph(n);
            for (int i = 0; i <= 998; i++){
                for (int j = i+1; j <= 999; j++){
                    g1_6.AddEdge(i, j);
                }
            }
            int[] tablicaWynikowa = new int[1000];
            for (int i = 0; i < tablicaWynikowa.Length; i++)
            {
                tablicaWynikowa[i] = i;
            }
            AddStage1(new Lab04Stage1TestCase(g1_6, 0, 9, tablicaWynikowa, 1, "Graf pełny - wrzędzie da się dostać w 1 godzine"));

            // Etap 1, test 7
            n = 1000;
            DiGraph g1_7 = new DiGraph(n);
            for (int i = 0; i <= 998; i++){
                for (int j = i+1; j <= 999; j++){
                    g1_7.AddEdge(j, i);
                }
            }
            AddStage1(new Lab04Stage1TestCase(g1_7, 0, 9, new int[] { 0 }, 1, "Graf pełny - nigdzie nie da się dostać"));

            // Etap 1, test 8
            n = 20;
            DiGraph g1_8 = rGG.DiGraph(n, 0.2);
            AddStage1(new Lab04Stage1TestCase(g1_8, 0, 11, new int[] { 0, 3, 17, 18, 19 }, 1, "Mały graf losowy"));

            // Etap 1, test 9
            n = 100;
            DiGraph g1_9 = rGG.DiGraph(n, 0.02);
            AddStage1(new Lab04Stage1TestCase(g1_9, 0, 11, new int[] { 0, 22, 26, 27, 34, 38, 47, 54, 71, 72, 73 }, 1, "Duży graf losowy"));



            // Etap 2, test 1 i 2
            n = 6;
            DiGraph<int> g2_1 = new DiGraph<int>(n);
            g2_1.AddEdge(0, 1, 10);
            g2_1.AddEdge(0, 2, 9);
            g2_1.AddEdge(1, 2, 9);
            g2_1.AddEdge(2, 3, 15);
            g2_1.AddEdge(1, 3, 12);
            g2_1.AddEdge(3, 4, 13);
            g2_1.AddEdge(4, 5, 14);
            AddStage2(new Lab04Stage2TestCase(g2_1, 0, 14, new int[] { 0, 1, 2, 3, 4 }, 1, "Przykład z zadania"));
            AddStage2(new Lab04Stage2TestCase(g2_1, 3, 15, new int[] { 3, 4, 5 }, 1, "Przykład z zadania, miastoStartowe=3, K=15"));

            // Etap 2, test 3
            n = 6;
            DiGraph<int> g2_3 = new DiGraph<int>(n);
            g2_3.AddEdge(0, 1, 8);
            g2_3.AddEdge(1, 2, 13);
            g2_3.AddEdge(1, 3, 9);
            g2_3.AddEdge(3, 4, 10);
            g2_3.AddEdge(4, 2, 11);
            g2_3.AddEdge(2, 5, 12);
            AddStage2(new Lab04Stage2TestCase(g2_3, 0, 13, new int[] { 0, 1, 2, 3, 4, 5 }, 1, "Mały graf"));

            // Etap 2, test 4 i 5
            n = 24;
            DiGraph<int> g2_4 = new DiGraph<int>(n);
            for (int i = 1; i <= 13; i++){
                g2_4.AddEdge(0, i, 21 - i);
                for (int j = 1; j < i; j++){
                    g2_4.AddEdge(i, j, 21 - i + 1);
                }
            }
            for (int i = 10; i < 20; i++){
                g2_4.AddEdge(1, i+4, i);
            }

            tablicaWynikowa = new int[24];
            for (int i = 0; i < tablicaWynikowa.Length; i++)
            {
                tablicaWynikowa[i] = i;
            }
            AddStage2(new Lab04Stage2TestCase(g2_4, 0, 20, tablicaWynikowa, 1, "Czeste powroty do jednego miasta"));
            tablicaWynikowa = new int[23];
            for (int i = 1; i < tablicaWynikowa.Length + 1; i++)
            {
                tablicaWynikowa[i-1] = i;
            }
            AddStage2(new Lab04Stage2TestCase(g2_4, 13, 20, tablicaWynikowa, 1, "Duzo krawedzi niepotrzebnych do przejrzenia"));

            // Etap 2, test 6
            n = 203;
            DiGraph<int> g2_6 = new DiGraph<int>(n);
            for (int i = 0; i < 100; i++){
                g2_6.AddEdge(i, i+1, 8+i);
                g2_6.AddEdge(i, 101, 208-i);
            }
            g2_6.AddEdge(100, 101, 108);
            for (int i = 0; i <= 100; i++){
                g2_6.AddEdge(101, 102+i, 109+i);
            }
            tablicaWynikowa = new int[203];
            for (int i = 0; i < tablicaWynikowa.Length; i++)
            {
                tablicaWynikowa[i] = i;
            }
            AddStage2(new Lab04Stage2TestCase(g2_6, 0, 210, tablicaWynikowa, 1, "Wielokrotne dojście do wierzchołka o wcześniejszych godzinach"));

            // Etap 2, test 7
            n = 2003;
            DiGraph<int> g2_7 = new DiGraph<int>(n);
            for (int i = 0; i < 1000; i++){
                g2_7.AddEdge(i, i+1, 8+i);
                g2_7.AddEdge(i, 1001, 2008-i);
            }
            g2_7.AddEdge(1000, 1001, 1008);
            for (int i = 0; i <= 1000; i++){
                g2_7.AddEdge(1001, 1002+i, 1009+i);
            }
            tablicaWynikowa = new int[2003];
            for (int i = 0; i < tablicaWynikowa.Length; i++)
            {
                tablicaWynikowa[i] = i;
            }
            AddStage2(new Lab04Stage2TestCase(g2_7, 0, 2010, tablicaWynikowa, 0.5, "Wielokrotne dojście do wierzchołka o wcześniejszych godzinach"));

            // Etap 2, test 8
            n = 2004;
            DiGraph<int> g2_8 = new DiGraph<int>(n);
            for (int i = 1; i <= 1001; i++){
                g2_8.AddEdge(0, i, i+7);
                g2_8.AddEdge(i, 1002, 2010-i);
                g2_8.AddEdge(1002, i+1002, i+1009);
            }
            tablicaWynikowa = new int[2004];
            for (int i = 0; i < tablicaWynikowa.Length; i++)
            {
                tablicaWynikowa[i] = i;
            }
            AddStage2(new Lab04Stage2TestCase(g2_8, 0, 2011, tablicaWynikowa, 0.5, "Wielokrotne dojście do wierzchołka o wcześniejszych godzinach"));

            // Etap 2, test 9
            n = 20;
            DiGraph g2_9_no_weights = rGG.DiGraph(n, 0.2);
            DiGraph<int> g2_9 = rGG.AssignWeights(g2_9_no_weights, 9, 11);
            AddStage2(new Lab04Stage2TestCase(g2_9, 0, 11, new int[] { 0, 1, 5, 15, 18 }, 1, "Mały graf losowy"));

            // Etap 2, test 10
            n = 100;
            DiGraph g2_10_no_weights = rGG.DiGraph(n, 0.05);
            DiGraph<int> g2_10 = rGG.AssignWeights(g2_10_no_weights, 9, 11);
            AddStage2(new Lab04Stage2TestCase(g2_10, 0, 11, new int[] { 0, 37, 39, 42, 81 }, 1, "Duży graf losowy"));
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var tests = new Lab04Tests();
            tests.PrepareTestSets();
            foreach (var ts in tests.TestSets)
            {
                ts.Value.PerformTests(false, true);
            }
        }
    }
}
