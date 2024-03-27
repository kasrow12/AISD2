using System.Text;

namespace ASD;

public class FindShortestPathTestCase : TestCase
{
    private readonly bool checkPath;
    private readonly int expectedResult;
    private readonly char[,] M;
    private readonly int t;
    private readonly bool withDynamites;
    private string path;

    private int result;

    public FindShortestPathTestCase(double timeLimit, Exception expectedException, string description, char[,] M,
        bool withDynamite, int t, int expectedResult, bool checkPath)
        : base(timeLimit, expectedException, description)
    {
        this.M = (char[,])M.Clone();
        withDynamites = withDynamite;
        this.t = t;
        this.expectedResult = expectedResult;
        this.checkPath = checkPath;
    }

    protected override void PerformTestCase(object prototypeObject)
    {
        var ap = (Maze)prototypeObject;
        result = ap.FindShortestPath((char[,])M.Clone(), withDynamites, out path, t);
    }

    protected override (Result resultCode, string message) VerifyTestCase(object settings)
    {
        string message = "";
        var resultCode = Result.NotPerformed;

        if (result != expectedResult)
        {
            message = string.Format("zwraca {0}, a nie {1}", result, expectedResult);
            resultCode = Result.WrongResult;
            return (resultCode, message);
        }

        if (checkPath)
        {
            if (expectedResult == -1 && !string.IsNullOrEmpty(path))
            {
                message = "Ścieżka powinna być pusta";
                resultCode = Result.WrongResult;
                return (resultCode, message);
            }

            if (expectedResult != -1 && string.IsNullOrEmpty(path))
            {
                message = "Ścieżka pusta";
                resultCode = Result.WrongResult;
                return (resultCode, message);
            }

            int resultFromPath = 0;
            var start = (0, 0);

            for (int i = 0; i < M.GetLength(0); i++)
            for (int j = 0; j < M.GetLength(1); j++)
            {
                if (M[i, j] == 'S')
                    start = (i, j);
            }

            var curr = start;
            for (int i = 0; i < path.Length; i++)
            {
                switch (path[i])
                {
                    case 'N':
                        curr.Item1--;
                        break;
                    case 'S':
                        curr.Item1++;
                        break;
                    case 'E':
                        curr.Item2++;
                        break;
                    case 'W':
                        curr.Item2--;
                        break;
                    default:
                        message = "Nieprawidłowy symbol w ścieżce";
                        resultCode = Result.WrongResult;
                        return (resultCode, message);
                }

                if (curr.Item1 >= M.GetLength(0) || curr.Item1 < 0 || curr.Item2 >= M.GetLength(1) || curr.Item2 < 0)
                {
                    message = "Ścieżka wychodzi poza labirynt";
                    resultCode = Result.WrongResult;
                    return (resultCode, message);
                }

                if (M[curr.Item1, curr.Item2] == 'X')
                {
                    resultFromPath += t;

                    if (!withDynamites)
                    {
                        message = "W wersji bez dynamitów ścieżka prowadzi przez ścianę";
                        resultCode = Result.WrongResult;
                        return (resultCode, message);
                    }
                }
                else
                {
                    resultFromPath += 1;
                }
            }

            if (M[curr.Item1, curr.Item2] != 'E' && expectedResult != -1)
            {
                message = "Ścieżka nie kończy się w punkcie E";
                resultCode = Result.WrongResult;
                return (resultCode, message);
            }

            if (result != resultFromPath && expectedResult != -1)
            {
                message = "Czas przejścia ścieżki jest różny od zwróconego rezultatu";
                resultCode = Result.WrongResult;
                return (resultCode, message);
            }
        }

        message = $"wynik OK (czas:{PerformanceTime,6:#0.000} jednostek)";
        resultCode = Result.Success;
        return (resultCode, message);
    }
}

public class FindShortestPathWithKDynamites : TestCase
{
    private readonly bool checkPath;
    private readonly int expectedResult;
    private readonly int k;
    private readonly char[,] M;
    private readonly int t;
    private string path;

    private int result;

    public FindShortestPathWithKDynamites(double timeLimit, Exception expectedException, string description, char[,] M,
        int k, int t, int expectedResult, bool checkPath)
        : base(timeLimit, expectedException, description)
    {
        this.M = (char[,])M.Clone();
        this.k = k;
        this.t = t;
        this.expectedResult = expectedResult;
        this.checkPath = checkPath;
    }

    protected override void PerformTestCase(object prototypeObject)
    {
        var ap = (Maze)prototypeObject;
        result = ap.FindShortestPathWithKDynamites((char[,])M.Clone(), k, out path, t);
    }

    protected override (Result resultCode, string message) VerifyTestCase(object settings)
    {
        string message = "";
        var resultCode = Result.NotPerformed;
        if (result != expectedResult)
        {
            message = string.Format("zwraca {0}, a nie {1}", result, expectedResult);
            resultCode = Result.WrongResult;
            return (resultCode, message);
        }

        if (checkPath)
        {
            if (expectedResult == -1 && !string.IsNullOrEmpty(path))
            {
                message = "Ścieżka powinna być pusta";
                resultCode = Result.WrongResult;
                return (resultCode, message);
            }

            if (expectedResult != -1 && string.IsNullOrEmpty(path))
            {
                message = "Ścieżka pusta";
                resultCode = Result.WrongResult;
                return (resultCode, message);
            }

            int resultFromPath = 0;
            int usedDynamits = 0;
            var start = (0, 0);

            for (int i = 0; i < M.GetLength(0); i++)
            for (int j = 0; j < M.GetLength(1); j++)
            {
                if (M[i, j] == 'S')
                    start = (i, j);
            }

            var curr = start;
            for (int i = 0; i < path.Length; i++)
            {
                switch (path[i])
                {
                    case 'N':
                        curr.Item1--;
                        break;
                    case 'S':
                        curr.Item1++;
                        break;
                    case 'E':
                        curr.Item2++;
                        break;
                    case 'W':
                        curr.Item2--;
                        break;
                    default:
                        message = "Nieprawidłowy symbol w ścieżce";
                        resultCode = Result.WrongResult;
                        return (resultCode, message);
                }

                if (curr.Item1 >= M.GetLength(0) || curr.Item1 < 0 || curr.Item2 >= M.GetLength(1) || curr.Item2 < 0)
                {
                    message = "Ścieżka wychodzi poza labirynt";
                    resultCode = Result.WrongResult;
                    return (resultCode, message);
                }

                if (M[curr.Item1, curr.Item2] == 'X')
                {
                    resultFromPath += t;
                    usedDynamits++;

                    if (usedDynamits > k)
                    {
                        message = "Przekroczono liczbę dostępnych dynamitów";
                        resultCode = Result.WrongResult;
                        return (resultCode, message);
                    }
                }
                else
                {
                    resultFromPath += 1;
                }
            }

            if (M[curr.Item1, curr.Item2] != 'E' && expectedResult != -1)
            {
                message = "Ścieżka nie kończy się w punkcie E";
                resultCode = Result.WrongResult;
                return (resultCode, message);
            }

            if (result != resultFromPath && expectedResult != -1)
            {
                message = "Czas przejścia ścieżki jest różny od zwróconego rezultatu";
                resultCode = Result.WrongResult;
                return (resultCode, message);
            }
        }

        message = $"wynik OK (czas:{PerformanceTime,6:#0.000} jednostek)";
        resultCode = Result.Success;
        return (resultCode, message);
    }
}

internal class Lab06TestModule : TestModule
{
    private readonly bool checkPath;

    public Lab06TestModule()
    {
        checkPath = true;
    }

    public Lab06TestModule(bool checkPath)
    {
        this.checkPath = checkPath;
    }

    private char[,] mazeStringToCharArray(string m)
    {
        string[] rows = m.Split('\n');

        char[,] result = new char[rows.Length, rows[0].Length];
        for (int i = 0; i < result.GetLength(0); i++)
        for (int j = 0; j < result.GetLength(1); j++)
            result[i, j] = rows[i][j];

        return result;
    }

    public override void PrepareTestSets()
    {
        string m1 = "XSOOOOO\n"
                    + "XXXXXOX\n"
                    + "OOOOOOO\n"
                    + "OXXXXXX\n"
                    + "OOOEXOO";

        string m2 = "SOOOOXX\n"
                    + "OXXOXXX\n"
                    + "OXXOXOO\n"
                    + "OOOOXXO\n"
                    + "OXXXOOE";

        string m3 = "SE";

        string m4 = "OEOOOOO\n"
                    + "OOOOOOO\n"
                    + "OOOOOOO\n"
                    + "OOOOOSO\n"
                    + "OOOOOOO";

        string m5 = "OOOOOOOOOOOOOOOOOOOOO\n"
                    + "OXXXXXXXXXXXXXXXXXXXO\n"
                    + "OXOOOOOOOOOOOOOOOOOXO\n"
                    + "OXOXXXXXXXXXXXXXXXOXO\n"
                    + "OXOXOOOOOOOOOOOOOXOXO\n"
                    + "OXOXOXXXXXXXXXXXOXOXO\n"
                    + "OXOXOXEOOOOOOOOXOXOXO\n"
                    + "OXOXOXXXXXXXXXOXOXOXO\n"
                    + "OXOXOOOOOOOOOOOXOXOXO\n"
                    + "OXOXXXXXXXXXXXXXOXOXO\n"
                    + "OXOOOOOOOOOOOOOOOXOXO\n"
                    + "OXXXXXXXXXXXXXXXXXOXO\n"
                    + "OOOOOOOOOOOOOOOOOOOXS";

        string m6 = "OOOOOOOOOOOOOOOOOOOOS\n"
                    + "OXXXXXXXXXXXXXXXXXXXX\n"
                    + "OOOOOOOOOOOOOOOOOOOOO\n"
                    + "XXXXXXXXXXXXXXXXXXXXO\n"
                    + "OOOOOOOOOOOOOOOOOOOOO\n"
                    + "OXXXXXXXXXXXXXXXXXXXX\n"
                    + "OOOOOOOOOOOOOOOOOOOOO\n"
                    + "XXXXXXXXXXXXXXXXXXXXO\n"
                    + "OOOOOOOOOOOOOOOOOOOOO\n"
                    + "OXXXXXXXXXXXXXXXXXXXX\n"
                    + "OOOOOOOOOOOOOOOOOOOOE\n"
                    + "XXXXXXXXXXXXXXXXXXXXO";

        string m7 = "XOOOOXXXOOOOXXXEOOOX\n"
                    + "XOXXOOOOOXXOXXXOXXOX\n"
                    + "XOXXOXXXOXXOOOOOXXOX\n"
                    + "XOOOOXXXOOOOXXXOOOOO\n"
                    + "XXXXXXXXXXXXXXXXXXXO\n"
                    + "XXXXXXXXXXOXXXXXXXXO\n"
                    + "XOOOOXXXOOOOXXXOOOOO\n"
                    + "XOXXOOOOOXXOXXXOXXOX\n"
                    + "XOXXOXXXOXXOOOOOXXOX\n"
                    + "XOSOOXXXOOOOXXXOOOOX";

        //losowy duży test
        var rand = new Random(777);
        var m8SB = new StringBuilder();
        int n = 200;
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i == 1 && j == 1)
                    m8SB.Append('S');
                else if (i == n - 2 && j == n - 2)
                    m8SB.Append('E');
                else if (rand.NextDouble() < 0.4)
                    m8SB.Append('X');
                else
                    m8SB.Append('O');
            }

            m8SB.Append('\n');
        }

        m8SB.Remove(m8SB.Length - 1, 1);

        string m8 = m8SB.ToString();

        string[] mazes = { m1, m2, m3, m4, m5, m6, m7, m8 };

        //--- Wersja I ---
        TestSets["v1Tests"] = new TestSet(new Maze(), "Wersja I", null, false);
        int[] v1ExpectedValues = { 16, -1, 1, 7, 152, 130, 34, -1 };
        for (int i = 0; i < mazes.Length; i++)
        {
            TestSets["v1Tests"].TestCases.Add(new FindShortestPathTestCase(20, null, "",
                mazeStringToCharArray(mazes[i]), false, 0, v1ExpectedValues[i], checkPath));
        }

        //--- Wersja II ---
        TestSets["v2Tests"] = new TestSet(new Maze(), "Wersja II", null, false);
        int[] v2ExpectedValues = { 10, 13, 1, 7, 29, 22, 23, 488 };
        int[] v2tValues = { 3, 4, 5, 3, 4, 5, 2, 10, 3 };
        for (int i = 0; i < mazes.Length; i++)
        {
            TestSets["v2Tests"].TestCases.Add(new FindShortestPathTestCase(20, null, "",
                mazeStringToCharArray(mazes[i]), true, v2tValues[i], v2ExpectedValues[i], checkPath));
        }

        //--- Wersja III ---
        TestSets["v3Tests"] = new TestSet(new Maze(), "Wersja III", null, false);
        int[] v3ExpectedValues = { 10, 12, 1, 7, 96, 94, 27, -1 };
        int[] v3tValues = { 3, 3, 4, 4, 5, 5, 6, 6, 8 };
        for (int i = 0; i < mazes.Length; i++)
        {
            TestSets["v3Tests"].TestCases.Add(new FindShortestPathWithKDynamites(20, null, "",
                mazeStringToCharArray(mazes[i]), 1, v3tValues[i], v3ExpectedValues[i], checkPath));
        }

        //--- Wersja IV ---
        TestSets["v4Tests"] = new TestSet(new Maze(), "Wersja IV", null, false);
        int[] v4ExpectedValues = { 12, -1, 1, 7, 56, 52, 34, 440 };
        int[] v4tValues = { 5, 5, 3, 3, 5, 2, 3, 4 };
        int[] dynamites = { 2, 0, 2, 5, 2, 2, 0, 8 };
        for (int i = 0; i < mazes.Length; i++)
        {
            TestSets["v4Tests"].TestCases.Add(new FindShortestPathWithKDynamites(20, null, "",
                mazeStringToCharArray(mazes[i]), dynamites[i], v4tValues[i], v4ExpectedValues[i], checkPath));
        }
    }
}

internal class Lab06
{
    private static void Main(string[] args)
    {
        bool checkPath = true;

        var lab06test = new Lab06TestModule(checkPath);
        lab06test.PrepareTestSets();

        foreach (var ts in lab06test.TestSets) ts.Value.PerformTests(verbose: true, checkTimeLimit: false);
    }
}