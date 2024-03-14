using ASD.Graphs;
using ASD.Graphs.Testing;

namespace ASD;

public class Lab04Stage1TestCase : TestCase
{
    private readonly int[] expectedResult;
    private readonly DiGraph<int> graph;
    private readonly int start;

    private int[] result;

    public Lab04Stage1TestCase(DiGraph<int> graph, int start, int[] expectedResult, double timeLimit,
        string description) : base(timeLimit, null, description)
    {
        this.graph = graph;
        this.start = start;
        this.expectedResult = expectedResult;
    }

    protected override void PerformTestCase(object prototypeObject)
    {
        result = ((Lab04)prototypeObject).Lab04Stage1(graph, start);
    }

    protected override (Result resultCode, string message) VerifyTestCase(object settings)
    {
        var res = CheckSolution();
        return (res.resultCode, $"{res.message} [{Description}]");
    }

    private (Result resultCode, string message) CheckSolution()
    {
        if (result == null) return (Result.WrongResult, "Zwrócono null zamiast tablicy numerów grup");

        if (result.Length != expectedResult.Length)
            return (Result.WrongResult,
                $"Zła liczba zwróconych numerów grup: otrzymano {result.Length}, oczekiwano {expectedResult.Length}");

        foreach (int v in result)
        {
            if (v < 0 || v >= graph.VertexCount)
                return (Result.WrongResult, "W tablicy znajduje się numer spoza zakresu");
        }

        for (int i = 1; i < result.Length; i++)
        {
            if (result[i] < result[i - 1])
                return (Result.WrongResult, "Tablica nie jest posortowana rosnąco");
        }

        for (int i = 1; i < result.Length; i++)
        {
            if (result[i] == result[i - 1])
                return (Result.WrongResult, "Tablica zawiera powtarzające się elementy");
        }

        for (int i = 0; i < result.Length; i++)
        {
            if (result[i] != expectedResult[i])
                return (Result.WrongResult, $"Numer grupy {result[i]} nie jest obecny w rozwiązaniu");
        }

        return (PerformanceTime > TimeLimit ? Result.LowEfficiency : Result.Success,
            $"OK ({PerformanceTime.ToString("#0.00")}s)");
    }
}

public class Lab04Stage2TestCase : TestCase
{
    private readonly bool expectedResult;
    private readonly int[] goals;
    private readonly DiGraph<int> graph;
    private readonly int[] starts;

    private bool result;
    private int[] route;

    public Lab04Stage2TestCase(DiGraph<int> graph, int[] starts, int[] goals, bool expectedResult, double timeLimit,
        string description) : base(timeLimit, null, description)
    {
        this.graph = graph;
        this.starts = starts;
        this.goals = goals;
        this.expectedResult = expectedResult;
    }

    protected override void PerformTestCase(object prototypeObject)
    {
        (result, route) = ((Lab04)prototypeObject).Lab04Stage2(graph, starts, goals);
    }

    protected override (Result resultCode, string message) VerifyTestCase(object settings)
    {
        var res = CheckSolution();
        return (res.resultCode, $"{res.message} [{Description}]");
    }

    private (Result resultCode, string message) CheckSolution()
    {
        if (result != expectedResult)
            return (Result.WrongResult, $"Zły wynik: otrzymano {result}, oczekiwano {expectedResult}");

        if (!result)
        {
            if (route != null) return (Result.WrongResult, "Zwrócono tablicę, a oczekiwano null");
            return Ok();
        }

        if (route == null) return (Result.WrongResult, "Brak zwróconej tablicy numerów grup");

        if (route.Length == 0) return (Result.WrongResult, "Zwrócono pustą tablicę");

        foreach (int v in route)
        {
            if (v < 0 || v >= graph.VertexCount)
                return (Result.WrongResult, "W tablicy znajduje się numer spoza zakresu");
        }

        bool beginsInStarts = false;
        foreach (int i in starts)
        {
            if (i == route[0])
                beginsInStarts = true;
        }

        if (!beginsInStarts)
            return (Result.WrongResult,
                $"Tablica zaczyna się wartością {route[0]}, która nie jest wartością początkową");

        int prev = -1;

        for (int i = 1; i < route.Length; ++i)
        {
            int w = -2;
            try
            {
                w = graph.GetEdgeWeight(route[i - 1], route[i]);
            }
            catch (Exception)
            {
                return (Result.WrongResult,
                    $"Między kolejnymi numerami w tablicy ({route[i - 1]}, {route[i]}) nie ma krawędzi");
            }

            if (w != prev)
                return (Result.WrongResult,
                    $"Żeby przejść z {route[i - 1]} do {route[i]}, należy " +
                    (w == -1 ? "być nowym członkiem" : $"być wcześniej w {w}, a nie w {prev}"));
            prev = route[i - 1];
        }

        bool endsInGoal = false;
        foreach (int i in goals)
        {
            if (i == route[route.Length - 1])
                endsInGoal = true;
        }

        if (!endsInGoal)
            return (Result.WrongResult,
                $"Tablica kończy się wartością {route[route.Length - 1]}, która nie jest wartością końcową");
        return Ok();
    }

    private (Result resultCode, string message) Ok()
    {
        return (PerformanceTime > TimeLimit ? Result.LowEfficiency : Result.Success,
            $"OK ({PerformanceTime.ToString("#0.00")}s)");
    }
}

public class Lab04Tests : TestModule
{
    private readonly TestSet stage1 = new(new Lab04(), "Etap 1: Droga do doskonałości");
    private readonly TestSet stage2 = new(new Lab04(), "Etap 2: Rozwijanie zainteresowań");

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
        int n = 6;
        var g1 = new DiGraph<int>(n);
        g1.AddEdge(0, 2, -1);
        g1.AddEdge(0, 3, -1);
        g1.AddEdge(0, 4, 1);
        g1.AddEdge(0, 5, 3);
        g1.AddEdge(1, 0, 4);
        g1.AddEdge(1, 3, 2);
        g1.AddEdge(2, 1, 0);
        g1.AddEdge(2, 0, 3);
        g1.AddEdge(3, 2, 1);
        g1.AddEdge(3, 0, 0);
        g1.AddEdge(4, 3, -1);
        g1.AddEdge(4, 5, 0);
        g1.AddEdge(5, 0, 3);

        AddStage1(new Lab04Stage1TestCase(g1, 0, new[] { 0, 1, 2, 3, 5 }, 1, "Przykład z zadania"));
        AddStage1(new Lab04Stage1TestCase(g1, 4, new[] { 3, 4 }, 1, "Przykład z zadania, inny wierzchołek startowy"));
        AddStage1(new Lab04Stage1TestCase(g1, 1, new[] { 1 }, 1, "Przykład z zadania, nigdzie nie można dojść"));
        AddStage2(new Lab04Stage2TestCase(g1, new[] { 0, 4 }, new[] { 1, 5 }, true, 1,
            "Przykład z zadania, jest możliwość"));
        AddStage2(new Lab04Stage2TestCase(g1, new[] { 1, 2, 3, 4, 5 }, new[] { 0 }, false, 1,
            "Przykład z zadania, brak możliwości"));

        n = 5;
        var g6 = new DiGraph<int>(n);
        g6.AddEdge(0, 1, -1);
        g6.AddEdge(0, 4, 0);
        g6.AddEdge(4, 3, 0);
        g6.AddEdge(1, 2, 0);
        g6.AddEdge(3, 2, 4);
        g6.AddEdge(4, 2, -1);
        g6.AddEdge(2, 4, 3);
        g6.AddEdge(4, 0, 2);
        AddStage1(new Lab04Stage1TestCase(g6, 0, new[] { 0, 1, 2 }, 1, "Mały graf z jedną nieosiągalną ścieżką"));
        AddStage2(new Lab04Stage2TestCase(g6, new[] { 0, 1, 2 }, new[] { 3, 4 }, false, 1,
            "Mały graf z jedną nieosiągalną ścieżką"));

        n = 10;
        var g3 = new DiGraph<int>(n);
        AddStage1(new Lab04Stage1TestCase(g3, 3, new[] { 3 }, 1, "Graf bez krawędzi"));
        AddStage2(new Lab04Stage2TestCase(g3, new[] { 0, 1, 2, 3, 4 }, new[] { 5, 6, 7, 8, 9 }, false, 1,
            "Graf bez krawędzi"));
        AddStage2(new Lab04Stage2TestCase(g3, new[] { 5, 6, 7 }, new[] { 7, 8, 9 }, true, 1,
            "Graf bez krawędzi, wspólna grupa"));

        n = 13;
        var g7 = new DiGraph<int>(n);
        g7.AddEdge(0, 1, -1);
        g7.AddEdge(0, 2, -1);
        g7.AddEdge(0, 3, -1);
        g7.AddEdge(0, 12, 1);
        g7.AddEdge(1, 4, 0);
        g7.AddEdge(1, 3, -1);
        g7.AddEdge(2, 5, 0);
        g7.AddEdge(2, 6, 0);
        g7.AddEdge(2, 7, 0);
        g7.AddEdge(3, 7, 0);
        g7.AddEdge(3, 8, 0);
        g7.AddEdge(3, 9, 0);
        g7.AddEdge(3, 12, 1);
        g7.AddEdge(4, 10, 1);
        g7.AddEdge(4, 12, -1);
        g7.AddEdge(8, 11, 1);
        g7.AddEdge(8, 12, 7);
        g7.AddEdge(10, 6, 4);
        g7.AddEdge(6, 1, 10);
        g7.AddEdge(7, 2, 10);
        g7.AddEdge(1, 8, 6);
        AddStage1(new Lab04Stage1TestCase(g7, 0, Enumerable.Range(0, 12).ToArray(), 1,
            "Drzewo + ścieżka + dodatkowy wierzchołek"));
        AddStage2(new Lab04Stage2TestCase(g7, new[] { 0, 1, 2 }, new[] { 12 }, true, 1,
            "Drzewo + ścieżka + dodatkowy wierzchołek"));

        n = 20;
        var g2 = new DiGraph<int>(n);
        for (int i = 0; i < n; i += 2)
        {
            g2.AddEdge(i, Mod(i - 1, n), -1);
            g2.AddEdge(i, Mod(i + 1, n), -1);
            g2.AddEdge(Mod(i - 1, n), i, i);
            g2.AddEdge(Mod(i + 1, n), i, i);
            for (int j = 2; j < n - 1; j += 2)
            {
                g2.AddEdge(i, Mod(i + j + 1, n), Mod(i - j + (j >= n / 2 ? -1 : 1), n));
                g2.AddEdge(Mod(i + j + 1, n), i, i);
            }
        }

        AddStage1(new Lab04Stage1TestCase(g2, 10, new[] { 1, 3, 5, 7, 9, 10, 11, 13, 15, 17, 19 }, 1, "Nawroty"));
        AddStage2(new Lab04Stage2TestCase(g2, new[] { 0, 1 }, new[] { 8, 11 }, true, 1, "Nawroty"));

        n = 37;
        var g9 = new DiGraph<int>(n);
        g9.AddEdge(36, 0, -1);
        g9.AddEdge(0, 2, 36);
        for (int i = 0; i < n; ++i)
        {
            g9.AddEdge(i, Mod(i + 1, n - 1), Mod(i - 1, n - 1));
            g9.AddEdge(Mod(i + 1, n - 1), Mod(i + 2, n - 1), i);
            if (i != 0) g9.AddEdge(i, Mod(i + 2, n - 1), Mod(i - 2, n - 1));
        }

        AddStage1(new Lab04Stage1TestCase(g9, 36, Enumerable.Range(0, 19).Select(x => 2 * x).ToArray(), 1,
            "Dwa cykle"));
        AddStage2(new Lab04Stage2TestCase(g9, Enumerable.Range(0, 19).Select(x => 2 * x).ToArray(),
            Enumerable.Range(0, 18).Select(x => 2 * x + 1).ToArray(), false, 1, "Dwa cykle"));

        n = 100;
        var g5 = new DiGraph<int>(n);
        for (int i = 0; i < n - 2; i++) g5.AddEdge(i, i + 1, i - 1);
        AddStage1(new Lab04Stage1TestCase(g5, 0, Enumerable.Range(0, n - 1).ToArray(), 1, "Ścieżka"));
        AddStage2(new Lab04Stage2TestCase(g5, new[] { 1, 2, 3, 4, 5 }, new[] { n - 1, n - 2, n - 3, n - 4 }, false, 1,
            "Ścieżka, zaczynamy w grupach bez wyjścia"));

        n = 200;
        var g8 = new DiGraph<int>(2 * n);
        var random = new Random(123456);
        for (int i = 0; i < n; i++)
        {
            for (int j = n; j < 2 * n; j++)
            {
                int ijw = random.Next(n, 2 * n + 1), jiw = random.Next(-1, n);
                if (ijw == 2 * n) ijw = -1;
                g8.AddEdge(i, j, ijw);
                g8.AddEdge(j, i, jiw);
            }
        }

        AddStage1(new Lab04Stage1TestCase(g8, 60, new[] { 53, 60, 99, 139, 224, 240, 283, 399 }, 1,
            "Duży graf dwudzielny z losowymi wagami"));
        AddStage2(new Lab04Stage2TestCase(g8, new[] { 1, 40, 300, 23, 58, 237 }, new[] { 5, 200, 29, 63, 321, 222, 0 },
            false, 1, "Duży graf dwudzielny z losowymi wagami"));

        n = 500;
        var rgg = new RandomGraphGenerator(420);
        var g4 = rgg.AssignWeights(rgg.DiGraph(n, 1), -1, n - 1);
        AddStage1(new Lab04Stage1TestCase(g4, 69, new[] { 69, 244, 371, 378, 416 }, 4,
            "Duży graf pełny z losowymi wagami"));
        AddStage2(new Lab04Stage2TestCase(g4, new[] { 0, 1, 2, 3 }, new[] { n - 1, n - 2, n - 3, n - 4 }, true, 4,
            "Duży graf pełny z losowymi wagami"));
    }

    private int Mod(int n, int m)
    {
        int mod = n % m;
        return mod < 0 ? m + mod : mod;
    }
}

internal class Program
{
    private static void Main(string[] args)
    {
        var tests = new Lab04Tests();
        tests.PrepareTestSets();
        foreach (var ts in tests.TestSets) ts.Value.PerformTests(true);
    }
}