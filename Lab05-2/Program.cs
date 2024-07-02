using ASD.Graphs;

namespace ASD;

internal class WidePathTestCase : TestCase
{
    protected int end;
    protected int expected;
    protected DiGraph<int> G, G_copy;
    protected int maxWeight;
    protected List<int> result;
    protected int start;
    protected int[] weights;

    public WidePathTestCase(DiGraph<int> G, int start, int end, int expected, double timeLimit, string description)
        : base(timeLimit, null, description)
    {
        this.G = G;
        G_copy = (DiGraph<int>)G.Clone();
        this.start = start;
        this.end = end;
        this.expected = expected;
    }

    protected override void PerformTestCase(object prototypeObject)
    {
        result = ((Lab06)prototypeObject).WidePath(G, start, end);
    }


    protected override (Result resultCode, string message) VerifyTestCase(object settings)
    {
        if (!G.Equals(G_copy))
            return (Result.WrongResult, "Wejściowy graf się zmienił!");
        if (result == null)
            return (Result.WrongResult, "Brak rozwiązania!");
        if (result.Count == 0 && expected == -1)
            return (Result.Success,
                "OK, czas: " + PerformanceTime.ToString("F4") + " (limit: " + TimeLimit.ToString("F4") + ")");
        if (result.Count == 0)
            return (Result.WrongResult,
                "Nie znaleziono istniejącej ścieżki" + " w czasie: " + PerformanceTime.ToString("F4"));
        if (result.Count > 0 && expected == -1)
            return (Result.WrongResult,
                "Ścieżka nie istnieje, a zwrócono listę rozmiaru " + result.Count);
        if (result[0] != start)
            return (Result.WrongResult, "Zły wierzchołek początkowy");
        if (result[result.Count - 1] != end)
            return (Result.WrongResult, "Zły wierzchołek końcowy");
        int numericResult = WidestEdgeInPath(result);
        if (numericResult == int.MinValue)
            return (Result.WrongResult, "Podana ścieżka nie istnieje");
        if (numericResult < expected)
            return (Result.WrongResult, "Zbyt wąska ścieżka");
        if (numericResult > expected)
            return (Result.WrongResult,
                "Znaleziona ściażka lepsza od oczekiwanej. Zgłoś to prowadzącemu. Szerokość znalezionej ścieżki to " +
                numericResult);
        return (Result.Success,
            "OK, czas: " + PerformanceTime.ToString("F4") + " (limit: " + TimeLimit.ToString("F4") + ")");
    }

    protected int WidestEdgeInPath(List<int> verticesList)
    {
        int minWeight = int.MaxValue;
        for (int i = 0; i < verticesList.Count - 1; i++)
        {
            if (!G.HasEdge(verticesList[i], verticesList[i + 1]))
                return int.MinValue;
            if (G.GetEdgeWeight(verticesList[i], verticesList[i + 1]) < minWeight)
                minWeight = G.GetEdgeWeight(verticesList[i], verticesList[i + 1]);
        }

        return minWeight;
    }
}

internal class WeightWidePathTestCase : WidePathTestCase
{
    public WeightWidePathTestCase(DiGraph<int> G, int start, int end, int[] weights, int maxWeight, int expected,
        double timeLimit, string description)
        : base(G, start, end, expected, timeLimit, description)
    {
        this.weights = weights;
        this.maxWeight = maxWeight;
    }


    protected override void PerformTestCase(object prototypeObject)
    {
        result = ((Lab06)prototypeObject).WeightedWidePath(G, start, end, weights, maxWeight);
    }

    protected override (Result resultCode, string message) VerifyTestCase(object settings)
    {
        if (!G.Equals(G_copy))
            return (Result.WrongResult, "Wejściowy graf się zmienił!");
        if (result == null)
            return (Result.WrongResult, "Brak rozwiązania!");
        if (result.Count == 0 && expected == int.MinValue)
            return (Result.Success,
                "OK, czas: " + PerformanceTime.ToString("F4") + " (limit: " + TimeLimit.ToString("F4") + ")");
        if (result.Count == 0)
            return (Result.WrongResult,
                "Nie znaleziono istniejącej ścieżki, w czasie: " + PerformanceTime.ToString("F4"));
        if (result.Count > 0 && expected == int.MinValue)
            return (Result.WrongResult,
                "Ścieżka nie istnieje, a zwrócono listę rozmiaru " + result.Count + " w czasie: " +
                PerformanceTime.ToString("F4"));
        if (result[0] != start)
            return (Result.WrongResult, "Zły wierzchołek początkowy");
        if (result[result.Count - 1] != end)
            return (Result.WrongResult, "Zły wierzchołek końcowy");
        int numericResult = WeightedResult(result);
        if (numericResult == int.MinValue)
            return (Result.WrongResult, "Podana ścieżka nie istnieje" + numericResult);
        if (numericResult < expected)
            return (Result.WrongResult,
                "Zbyt wąska ścieżka (" + numericResult + " zamiast " + expected + ")");
        if (numericResult > expected)
            return (Result.WrongResult, "Popsuły się testy :o" + numericResult);
        return (Result.Success,
            "OK, czas: " + PerformanceTime.ToString("F4") + " (limit: " + TimeLimit.ToString("F4") + ")");
    }

    private int WeightedResult(List<int> verticesList)
    {
        int minWeight = int.MaxValue;
        int verticesWeightSum = 0;
        for (int i = 0; i < verticesList.Count - 1; i++)
        {
            verticesWeightSum += weights[verticesList[i]];
            if (!G.HasEdge(verticesList[i], verticesList[i + 1]))
                return int.MinValue;
            int weight = G.GetEdgeWeight(verticesList[i], verticesList[i + 1]);
            if (weight < minWeight)
                minWeight = weight;
        }

        return minWeight - verticesWeightSum;
    }
}

internal class Lab06TestModule : TestModule
{
    public override void PrepareTestSets()
    {
        TestSets["SmallWidePath"] = makeSmallWidePath();
        TestSets["BigWidePath"] = makeBigWidePath();
        TestSets["HomeWidePath"] = makeHomeWidePath();
        TestSets["SmallWeightedWidePath"] = makeSmallWeightWidePath();
        TestSets["BigWeightedWidePath"] = makeBigWeightWidePath();
        TestSets["HomeWeightWidePath"] = makeHomeWeightWidePath();
    }

    private TestSet makeSmallWidePath()
    {
        var set = new TestSet(new Lab06(), "Część I, testy laboratoryjne małe");
        {
            var path = new DiGraph<int>(7);
            path.AddEdge(0, 1, 10);
            path.AddEdge(1, 3, 8);
            path.AddEdge(0, 2, 9);
            path.AddEdge(2, 3, 9);
            set.TestCases.Add(new WidePathTestCase(
                path,
                0,
                3,
                9,
                1,
                "Prosty graf"));
        }
        {
            set.TestCases.Add(new WidePathTestCase(
                new DiGraph<int>(10),
                0,
                7,
                -1,
                1,
                "Same wierzcholki izolowane"));
        }
        {
            var star = new DiGraph<int>(6);
            star.AddEdge(0, 1, 4);
            star.AddEdge(0, 2, 2);
            star.AddEdge(0, 3, 7);
            star.AddEdge(0, 4, 3);
            star.AddEdge(0, 5, 6);
            star.AddEdge(1, 2, 2);
            star.AddEdge(1, 3, 7);
            star.AddEdge(1, 4, 3);
            star.AddEdge(1, 5, 9);
            star.AddEdge(2, 1, 2);
            star.AddEdge(2, 3, 7);
            star.AddEdge(2, 4, 3);
            star.AddEdge(2, 5, 9);
            star.AddEdge(3, 1, 2);
            star.AddEdge(3, 2, 7);
            star.AddEdge(3, 4, 3);
            star.AddEdge(3, 5, 9);
            set.TestCases.Add(new WidePathTestCase(
                star,
                0,
                5,
                7,
                1,
                "Gęsty graf z prostą ścieżką"));
        }
        {
            var path = new DiGraph<int>(7);
            path.AddEdge(0, 1, 3);
            path.AddEdge(1, 2, 5);
            path.AddEdge(2, 3, 3);
            path.AddEdge(3, 4, 4);
            path.AddEdge(4, 5, 6);
            path.AddEdge(5, 6, 3);
            set.TestCases.Add(new WidePathTestCase(
                path,
                0,
                6,
                3,
                1,
                "Ścieżka"));
        }
        {
            var path = new DiGraph<int>(8);
            path.AddEdge(0, 1, 1);
            path.AddEdge(0, 5, 10);
            path.AddEdge(0, 3, 3);
            path.AddEdge(2, 1, 5);
            path.AddEdge(3, 2, 7);
            path.AddEdge(3, 1, 4);
            path.AddEdge(4, 3, 8);
            path.AddEdge(4, 1, 3);
            path.AddEdge(5, 4, 9);
            path.AddEdge(5, 1, 2);

            set.TestCases.Add(new WidePathTestCase(
                path,
                0,
                1,
                5,
                1,
                "Pięciokąt ze środkiem z drogą po obwodzie"));
        }
        {
            var path = new DiGraph<int>(8);
            path.AddEdge(0, 1, 9);
            path.AddEdge(0, 5, 8);
            path.AddEdge(1, 2, 7);
            path.AddEdge(1, 3, 9);
            path.AddEdge(2, 3, 12);
            path.AddEdge(3, 4, 9);
            path.AddEdge(4, 5, 8);

            set.TestCases.Add(new WidePathTestCase(
                path,
                0,
                5,
                8,
                1,
                "Dwie ścieżki o równej szerokości"));
        }
        {
            var path = new DiGraph<int>(12);
            path.AddEdge(0, 1, 9);
            path.AddEdge(0, 5, 8);
            path.AddEdge(1, 2, 7);
            path.AddEdge(1, 3, 9);
            path.AddEdge(2, 3, 12);
            path.AddEdge(3, 4, 9);
            path.AddEdge(4, 5, 8);
            path.AddEdge(6, 7, 9);
            path.AddEdge(6, 11, 8);
            path.AddEdge(7, 8, 7);
            path.AddEdge(7, 9, 9);
            path.AddEdge(8, 9, 12);
            path.AddEdge(9, 10, 9);
            path.AddEdge(10, 11, 8);

            set.TestCases.Add(new WidePathTestCase(
                path,
                0,
                11,
                -1,
                1,
                "Graf bez ścieżki"));
        }
        return set;
    }

    private TestSet makeBigWidePath()
    {
        var set = new TestSet(new Lab06(), "Część I, testy laboratoryjne duże");
        {
            int n = 20000;
            var Gr = new DiGraph<int>(n);
            var r = new Random(13);

            for (int i = 0; i < n; i++)
            for (int j = 0; j < 2; j++)
            {
                int dest = r.Next(n);
                if (dest != i)
                    Gr.AddEdge(i, dest, r.Next(1, 100));
            }

            set.TestCases.Add(new WidePathTestCase(
                Gr,
                0,
                1000,
                36,
                3,
                "Rzadki losowy graf"));
        }
        {
            int n = 20000;
            var Gr = new DiGraph<int>(n);
            var r = new Random(13);

            for (int i = 0; i < n; i++)
            for (int j = 0; j < 20; j++)
            {
                int dest = r.Next(n);
                if (dest != i)
                    Gr.AddEdge(i, dest, r.Next(1, 100));
            }

            set.TestCases.Add(new WidePathTestCase(
                Gr,
                0,
                1000,
                92,
                20,
                "Gęstszy graf losowy"));
        }
        {
            int n = 20000;
            var Gr = new DiGraph<int>(n);
            var r = new Random(13);

            for (int i = 0; i < n; i++)
            for (int j = 0; j < 60; j++)
            {
                int dest = r.Next(n - 1);
                if (dest != i)
                    Gr.AddEdge(i, dest, r.Next(1, 100));
            }

            set.TestCases.Add(new WidePathTestCase(
                Gr,
                0,
                19999,
                -1,
                20,
                "Jeszcze gęstszy graf losowy z wierzchołkiem izolowanym"));
        }
        return set;
    }

    private TestSet makeHomeWidePath()
    {
        var set = new TestSet(new Lab06(), "Część I, testy domowe");
        {
            int n = 20000;
            var Gr = new DiGraph<int>(n);
            var r = new Random(13);

            for (int i = 1; i < n; i++)
            for (int j = 0; j < 10; j++)
            {
                int dest = r.Next(1, n - 1);
                if (dest != i)
                    Gr.AddEdge(i, dest, 10);
            }

            set.TestCases.Add(new WidePathTestCase(
                Gr,
                0,
                19999,
                -1,
                2,
                "Duży losowy graf z z izolowanym wierzchołkiem początkowym"));
        }
        {
            int n = 20000;
            var Gr = new DiGraph<int>(n);
            var r = new Random(13);

            for (int i = 1; i < n; i++)
            for (int j = 0; j < 10; j++)
            {
                int dest = r.Next(1, n - 1);
                if (dest != i)
                    Gr.AddEdge(i, dest, 10);
            }

            Gr.AddEdge(0, 19999, 1);

            set.TestCases.Add(new WidePathTestCase(
                Gr,
                0,
                19999,
                1,
                2,
                "Duży losowy graf z bezpośrednim optymalnym połączeniem"));
        }
        {
            int n = 1500;
            var Gr = new DiGraph<int>(n);

            for (int i = 0; i < n / 3; i++)
            {
                Gr.AddEdge(i, i + 500, 510);
                Gr.AddEdge(i + 500, i + 1, 1);
                Gr.AddEdge(i, i + 1000, 510 - i);
                Gr.AddEdge(i + 1000, i + 1, 510 - i);
            }

            set.TestCases.Add(new WidePathTestCase(
                Gr,
                0,
                499,
                12,
                2,
                "Wiele połączonych szeregowo cykli długości 4, gdzie w każdym cyklu bardziej obiecująca ścieżka okazuje się gorsza"));
        }
        {
            int n = 1000;
            var Gr = new DiGraph<int>(n);
            var r = new Random(13);

            for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                if (i != j)
                    Gr.AddEdge(i, j, r.Next(20));

            set.TestCases.Add(new WidePathTestCase(
                Gr,
                0,
                999,
                19,
                2,
                "Graf pełny"));
        }
        {
            int n = 1000;
            var Gr = new DiGraph<int>(n);
            var r = new Random(13);

            for (int i = 0; i < n / 2; i++)
            for (int j = i + n / 2; j < n; j++)
            {
                {
                    Gr.AddEdge(i, j, r.Next(200));
                    Gr.AddEdge(j, i, r.Next(200));
                }
            }

            set.TestCases.Add(new WidePathTestCase(
                Gr,
                0,
                999,
                199,
                2,
                "Duży pełny graf dwudzielny"));
        }
        {
            int n = 1000;
            var Gr = new DiGraph<int>(n);
            var r = new Random(13);

            for (int i = 0; i < n / 2; i++)
            for (int j = 0; j < n / 2; j++)
            {
                {
                    if (i != j)
                    {
                        Gr.AddEdge(i, j, r.Next(10, 200));
                        Gr.AddEdge(i + n / 2, j + n / 2, r.Next(10, 200));
                    }
                }
            }

            Gr.AddEdge(0, n / 2, 1);

            set.TestCases.Add(new WidePathTestCase(
                Gr,
                0,
                999,
                1,
                2,
                "Dwa duże grafy pełne połączone wąskim mostem"));
        }
        {
            int n = 1000;
            var Gr = new DiGraph<int>(n, new MatrixGraphRepresentation());
            var r = new Random(13);

            for (int i = 5; i < n; i++)
            for (int j = 5; j < n; j++)
                if (i != j)
                    Gr.AddEdge(i, j, r.Next(10, 200));

            for (int i = 0; i < 5; i++)
            for (int j = 0; j < 5; j++)
                if (i != j)
                    Gr.AddEdge(i, j, r.Next(10, 200));

            Gr.AddEdge(0, 5, 1);

            set.TestCases.Add(new WidePathTestCase(
                Gr,
                0,
                999,
                1,
                2,
                "Mały graf pełny i duży graf pełny połączone wąskim mostem"));
        }
        {
            int n = 2000;
            var Gr = new DiGraph<int>(n);
            var r = new Random(13);

            for (int i = 5; i < n; i++)
            for (int j = 5; j < n; j++)
                if (i != j)
                    Gr.AddEdge(i, j, r.Next(100, 200));

            Gr.AddEdge(0, 1, 199);
            Gr.AddEdge(1, 4, 199);
            Gr.AddEdge(0, 2, 10);
            Gr.AddEdge(2, 3, 10);
            Gr.AddEdge(3, 1999, 10);
            Gr.AddEdge(4, 5, 1);


            set.TestCases.Add(new WidePathTestCase(
                Gr,
                0,
                1999,
                10,
                2,
                "Duży graf pełny z wysokimi wagami, do którego można się dostać wąskim połączeniem oraz szersza od wąskiego połączenia ścieżka idąca od początku do końca"));
        }
        {
            int n = 10000;
            var Gr = new DiGraph<int>(n);
            var r = new Random(13);

            for (int i = 0; i < n; i++)
            for (int j = 0; j < 10; j++)
            {
                {
                    if (i != j)
                    {
                        int rv = r.Next(1000);
                        if (rv != i)
                            Gr.AddEdge(i, rv, r.Next(20));
                    }
                }
            }

            set.TestCases.Add(new WidePathTestCase(
                Gr,
                r.Next(n),
                r.Next(n),
                17,
                2,
                "Losowy graf z losowym początkiem i końcem"));
        }
        {
            int n = 1000;
            var Gr = new DiGraph<int>(n);
            var r = new Random(13);

            for (int i = 0; i < n; i++)
            for (int j = 0; j < 10; j++)
            {
                {
                    if (i != j)
                    {
                        int rv = r.Next(1000);
                        if (i != rv)
                            Gr.AddEdge(i, rv, r.Next(0, 2));
                    }
                }
            }

            set.TestCases.Add(new WidePathTestCase(
                Gr,
                300,
                100,
                1,
                2,
                "Losowy graf z wagami z wąskiego zakresu"));
        }

        return set;
    }

    private TestSet makeSmallWeightWidePath()
    {
        var set = new TestSet(new Lab06(), "Część II, testy laboratoryjne małe");
        {
            int[] weights = { 0, 3, 12, 2, 1, 1 };
            set.TestCases.Add(new WeightWidePathTestCase(
                new DiGraph<int>(6),
                0,
                2,
                weights,
                20,
                int.MinValue,
                1,
                "Same wierzcholki izolowane"));
        }
        {
            var star = new DiGraph<int>(6);
            star.AddEdge(0, 1, 5);
            star.AddEdge(0, 2, 20);
            star.AddEdge(1, 3, 5);
            star.AddEdge(2, 3, 20);
            star.AddEdge(3, 4, 5);
            star.AddEdge(3, 5, 20);

            int[] weights = { 0, 3, 12, 2, 0, 0 };
            set.TestCases.Add(new WeightWidePathTestCase(
                star,
                0,
                4,
                weights,
                20,
                0, // tutaj na pewno 0?
                2,
                "Rozdział po złączeniu wersja 1"));
        }

        {
            var star = new DiGraph<int>(6);
            star.AddEdge(0, 1, 5);
            star.AddEdge(0, 2, 20);
            star.AddEdge(1, 3, 5);
            star.AddEdge(2, 3, 20);
            star.AddEdge(3, 4, 5);
            star.AddEdge(3, 5, 20);

            int[] weights = { 0, 3, 12, 2, 0, 0 };
            set.TestCases.Add(new WeightWidePathTestCase(
                star,
                0,
                5,
                weights,
                20,
                6,
                1,
                "Rozdział po złączeniu wersja 2"));
        }

        {
            var star = new DiGraph<int>(8);
            star.AddEdge(0, 1, 5);
            star.AddEdge(0, 2, 20);
            star.AddEdge(1, 3, 5);
            star.AddEdge(2, 3, 20);
            star.AddEdge(3, 4, 5);
            star.AddEdge(3, 5, 20);
            star.AddEdge(6, 3, 2);
            star.AddEdge(0, 6, 3);
            star.AddEdge(3, 7, 2);

            int[] weights = { 0, 3, 12, 2, 0, 0, 1, 0 };
            set.TestCases.Add(new WeightWidePathTestCase(
                star,
                0,
                7,
                weights,
                20,
                -1,
                1,
                "Trzy ścieżki prowadzące do celu wersja 1"));
        }
        {
            var star = new DiGraph<int>(8);
            star.AddEdge(0, 1, 5);
            star.AddEdge(0, 2, 20);
            star.AddEdge(1, 3, 5);
            star.AddEdge(2, 3, 20);
            star.AddEdge(3, 4, 5);
            star.AddEdge(3, 5, 20);
            star.AddEdge(6, 3, 2);
            star.AddEdge(0, 6, 3);
            star.AddEdge(3, 7, 2);

            int[] weights = { 0, 3, 12, 2, 0, 0, 1, 0 };
            set.TestCases.Add(new WeightWidePathTestCase(
                star,
                0,
                4,
                weights,
                20,
                0,
                1,
                "Trzy ścieżki prowadzące do celu wersja 2"));
        }
        {
            var star = new DiGraph<int>(19);
            star.AddEdge(0, 1, 10);
            star.AddEdge(1, 2, 9);
            star.AddEdge(2, 3, 8);
            star.AddEdge(3, 4, 7);
            star.AddEdge(4, 5, 7);
            star.AddEdge(5, 6, 6);
            star.AddEdge(6, 7, 5);
            star.AddEdge(7, 8, 4);
            star.AddEdge(8, 9, 3);
            star.AddEdge(0, 18, 4);
            star.AddEdge(18, 17, 4);
            star.AddEdge(17, 16, 4);
            star.AddEdge(16, 15, 4);
            star.AddEdge(15, 14, 4);
            star.AddEdge(14, 13, 4);
            star.AddEdge(13, 12, 4);
            star.AddEdge(12, 10, 4);
            star.AddEdge(10, 9, 4);
            int[] weights = { 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            set.TestCases.Add(new WeightWidePathTestCase(
                star,
                0,
                9,
                weights,
                10,
                -4,
                1,
                "Dwie równie długie ścieżki, jedna z nich się zwężą"));
        }
        {
            var star = new DiGraph<int>(19);
            star.AddEdge(0, 1, 10);
            star.AddEdge(1, 2, 9);
            star.AddEdge(2, 3, 8);
            star.AddEdge(3, 4, 7);
            star.AddEdge(4, 5, 7);
            star.AddEdge(5, 6, 6);
            star.AddEdge(6, 7, 5);
            star.AddEdge(7, 8, 4);
            star.AddEdge(8, 9, 3);
            star.AddEdge(0, 11, 10);
            star.AddEdge(11, 12, 9);
            star.AddEdge(12, 13, 8);
            star.AddEdge(13, 14, 7);
            star.AddEdge(14, 15, 7);
            star.AddEdge(15, 16, 6);
            star.AddEdge(16, 17, 5);
            star.AddEdge(17, 18, 4);
            star.AddEdge(18, 9, 3);
            int[] weights = { 0, 13, 10, 5, 3, 17, 13, 11, 1, 0, 0, 20, 19, 11, 4, 4, 4, 4, 8 };
            set.TestCases.Add(new WeightWidePathTestCase(
                star,
                0,
                9,
                weights,
                10,
                -70,
                1,
                "Dwie równie szerokie ścieżki, jedna ma większe wagi od drugiej"));
        }
        return set;
    }

    private TestSet makeBigWeightWidePath()
    {
        var set = new TestSet(new Lab06(), "Część II, testy laboratoryjne duże");
        {
            int n = 20000;
            var Gr = new DiGraph<int>(n);
            var r = new Random(16);
            int[] weights = new int[n];
            for (int i = 0; i < n; i++)
            {
                weights[i] = r.Next(100);
                for (int j = 0; j < 2; j++)
                {
                    int dest = r.Next(n);
                    if (dest != i)
                        Gr.AddEdge(i, dest, r.Next(1, 10));
                }
            }

            set.TestCases.Add(new WeightWidePathTestCase(
                Gr,
                0,
                19000,
                expected: -638,
                weights: weights,
                maxWeight: 10,
                timeLimit: 2,
                description: "Rzadki losowy graf"));
        }
        {
            int n = 20000;
            var Gr = new DiGraph<int>(n);
            var r = new Random(16);
            int[] weights = new int[n];
            for (int i = 0; i < n; i++)
            {
                weights[i] = r.Next(10);
                for (int j = 0; j < 2; j++)
                {
                    int dest = r.Next(n);
                    if (dest != i)
                        Gr.AddEdge(i, dest, r.Next(1, 100));
                }
            }

            set.TestCases.Add(new WeightWidePathTestCase(
                Gr,
                0,
                19000,
                expected: -52,
                weights: weights,
                maxWeight: 100,
                timeLimit: 20,
                description: "Rzadki losowy graf, duże wagi"));
        }
        {
            int n = 10000;
            var Gr = new DiGraph<int>(n);
            var r = new Random(13);
            int[] weights = new int[n];
            for (int i = 0; i < n; i++)
            {
                weights[i] = r.Next(10);
                for (int j = 0; j < 100; j++)
                {
                    int dest = r.Next(n);
                    if (dest != i)
                        Gr.AddEdge(i, dest, r.Next(1, 5));
                }
            }

            set.TestCases.Add(new WeightWidePathTestCase(
                Gr,
                0,
                1000,
                expected: -1,
                weights: weights,
                maxWeight: 5,
                timeLimit: 20,
                description: "Gęsty losowy graf, małe wagi"));
        }
        return set;
    }

    private TestSet makeHomeWeightWidePath()
    {
        var set = new TestSet(new Lab06(), "Część II, testy domowe");
        {
            int n = 20000;
            var Gr = new DiGraph<int>(n);
            var r = new Random(16);
            int[] weights = new int[n];
            for (int i = 0; i < n; i++)
            {
                weights[i] = r.Next(100);
                for (int j = 0; j < 2; j++)
                {
                    int dest = r.Next(n);
                    if (dest != i)
                        Gr.AddEdge(i, dest, 10);
                }
            }

            set.TestCases.Add(new WeightWidePathTestCase(
                Gr,
                0,
                19999,
                expected: -485,
                weights: weights,
                maxWeight: 10,
                timeLimit: 2,
                description: "Rzadki losowy z równymi wagami krawędzi"));
        }
        {
            int n = 20000;
            var Gr = new DiGraph<int>(n);
            var r = new Random(16);
            int[] weights = new int[n];
            for (int i = 0; i < n; i++)
            {
                weights[i] = r.Next(100);
                for (int j = 0; j < 2; j++)
                {
                    int dest = r.Next(n);
                    if (dest != i)
                        Gr.AddEdge(i, dest, 1000);
                }
            }

            set.TestCases.Add(new WeightWidePathTestCase(
                Gr,
                0,
                19999,
                expected: 505,
                weights: weights,
                maxWeight: 1000,
                timeLimit: 2,
                description: "Rzadki losowy z równymi bardzo dużymi wagami krawędzi"));
        }
        {
            int n = 1000;
            var Gr = new DiGraph<int>(n);
            var r = new Random(16);
            int[] weights = new int[n];
            for (int i = 0; i < n; i++)
            {
                weights[i] = r.Next(100);
                for (int j = 0; j < n; j++)
                {
                    if (i != j)
                        Gr.AddEdge(i, j, r.Next(10));
                }
            }

            set.TestCases.Add(new WeightWidePathTestCase(
                Gr,
                0,
                999,
                expected: 0,
                weights: weights,
                maxWeight: 10,
                timeLimit: 30,
                description: "Graf pełny"));
        }
        {
            int n = 20000;
            var Gr = new DiGraph<int>(n);
            int[] weights = new int[n];
            for (int i = 2; i < n - 1; i++)
            {
                weights[i] = 1;
                Gr.AddEdge(i, i + 1, 100);
            }

            weights[1] = 19709;
            Gr.AddEdge(0, 2, 100);
            Gr.AddEdge(0, 1, 101);
            Gr.AddEdge(1, 19999, 1);

            set.TestCases.Add(new WeightWidePathTestCase(
                Gr,
                0,
                19999,
                expected: -19708,
                weights: weights,
                maxWeight: 101,
                timeLimit: 2,
                description: "Bardzo długa ścieżka gorsza od wąskiej ścieżki długości 3"));
        }
        {
            int n = 20000;
            var Gr = new DiGraph<int>(n);
            int[] weights = new int[n];
            for (int i = 2; i < n - 1; i++)
            {
                weights[i] = 1;
                Gr.AddEdge(i, i + 1, 100);
            }

            weights[1] = 19999;
            Gr.AddEdge(0, 2, 100);
            Gr.AddEdge(0, 1, 101);
            Gr.AddEdge(1, 19999, 1);

            set.TestCases.Add(new WeightWidePathTestCase(
                Gr,
                0,
                19999,
                expected: -19897,
                weights: weights,
                maxWeight: 101,
                timeLimit: 2,
                description: "Bardzo długa ścieżka lepsza od ścieżki długości 3"));
        }
        {
            int n = 100;
            var Gr = new DiGraph<int>(n);
            var r = new Random(16);
            int[] weights = new int[n];
            for (int i = 0; i < n; i++)
            {
                weights[i] = r.Next(10);
                for (int j = 0; j < n; j++)
                {
                    int w = r.Next(3000);
                    if (i != j)
                        Gr.AddEdge(i, j, w);
                }
            }

            set.TestCases.Add(new WeightWidePathTestCase(
                Gr,
                0,
                99,
                expected: 2934,
                weights: weights,
                maxWeight: 3000,
                timeLimit: 50,
                description: "Graf pełny o 1000 wierzchołkach i nawyższej wadze krawędzi mniejszej niż 3001"));
        }
        {
            int n = 20000;
            var Gr = new DiGraph<int>(n);
            var r = new Random(16);
            int[] weights = new int[n];
            for (int i = 1; i < n - 1; i++)
            {
                weights[i] = r.Next(100);
                for (int j = 0; j < 20; j++)
                {
                    int rv = r.Next(2, 19998);
                    int rw = r.Next(100);
                    if (i != rv)
                        Gr.AddEdge(i, rv, rw);
                }
            }

            Gr.AddEdge(0, 19999, 1);

            set.TestCases.Add(new WeightWidePathTestCase(
                Gr,
                0,
                19999,
                expected: 1,
                weights: weights,
                maxWeight: 100,
                timeLimit: 5,
                description: "Dosyć gęsty graf, dla którego jedyną ścieżką jest optymalne połączenie."));
        }
        {
            int n = 20000;
            var Gr = new DiGraph<int>(n);
            var r = new Random(16);
            int[] weights = new int[n];
            for (int i = 0; i < n; i++)
            {
                weights[i] = r.Next(100);
                for (int j = 0; j < 2; j++)
                {
                    int dest = r.Next(n);
                    if (dest != i)
                        Gr.AddEdge(i, dest, 2);
                }
            }

            set.TestCases.Add(new WeightWidePathTestCase(
                Gr,
                300,
                200,
                expected: -613,
                weights: weights,
                maxWeight: 2,
                timeLimit: 2,
                description: "Rzadki losowy z krawędziamy wag z małego zakresu"));
        }
        {
            int n = 20000;
            var Gr = new DiGraph<int>(n);
            var r = new Random(16);
            int[] weights = new int[n];
            for (int i = 0; i < n; i++)
            {
                weights[i] = r.Next(100);
                for (int j = 0; j < 2; j++)
                {
                    int dest = r.Next(n);
                    if (dest != i)
                        Gr.AddEdge(i, dest, 10);
                }
            }

            set.TestCases.Add(new WeightWidePathTestCase(
                Gr,
                r.Next(n),
                r.Next(n),
                expected: -434,
                weights: weights,
                maxWeight: 10,
                timeLimit: 2,
                description: "Rzadki losowy graf z losowym początkiem i końcem"));
        }
        return set;
    }

    private double scoreWidePath()
    {
        bool labok = TestSets["SmallWidePath"].PassedCount + TestSets["BigWidePath"].PassedCount
                     == TestSets["SmallWidePath"].TestCases.Count + TestSets["BigWidePath"].TestCases.Count;
        if (!labok) return -1;

        if (TestSets["HomeWidePath"].PassedCount == TestSets["HomeWidePath"].TestCases.Count)
            return 1;
        int timeouts = TestSets["HomeWidePath"].TimeoutsCount;
        if (timeouts <= 1 &&
            timeouts + TestSets["HomeWidePath"].PassedCount == TestSets["HomeWidePath"].TestCases.Count)
            return 0.5;
        return 0;
    }

    private double scoreWeightedWidePath()
    {
        bool labok = TestSets["SmallWeightedWidePath"].PassedCount + TestSets["BigWeightedWidePath"].PassedCount
                     == TestSets["SmallWeightedWidePath"].TestCases.Count +
                     TestSets["BigWeightedWidePath"].TestCases.Count;
        if (!labok) return -1.5;

        if (TestSets["HomeWeightWidePath"].PassedCount == TestSets["HomeWeightWidePath"].TestCases.Count)
            return 1.5;
        int timeouts = TestSets["HomeWeightWidePath"].TimeoutsCount;
        if (timeouts <= 1 && timeouts + TestSets["HomeWeightWidePath"].PassedCount ==
            TestSets["HomeWeightWidePath"].TestCases.Count)
            return 0.5;
        return 0;
    }


    public override double ScoreResult()
    {
        return scoreWidePath() + scoreWeightedWidePath();
    }
}

internal class Program
{
    private static void Main(string[] args)
    {
        var Lab06test = new Lab06TestModule();
        Lab06test.PrepareTestSets();
        foreach (var ts in Lab06test.TestSets)
            ts.Value.PerformTests(verbose: true, checkTimeLimit: false);
    }
}