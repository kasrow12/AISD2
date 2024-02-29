using System;
using System.Linq;
using System.Text;
using ASD;
using System.Collections.Generic;

namespace Lab02
{

    public abstract class Lab02TestCase : TestCase
    {
        protected readonly int n;
        protected readonly int m;
        protected readonly (int, int)[] obstacles;
        private bool checkStrings;
        private readonly bool expectedVal;

        protected (bool returnedVal, string returnedPath) result;

        protected Lab02TestCase(int n, int m, (int, int)[] obstacles, bool expectedVal, bool checkStrings, double timeLimit, string description) : base(timeLimit, null, description)
        {
            this.n = n;
            this.m = m;
            this.obstacles = obstacles;
            this.expectedVal = expectedVal;
            this.checkStrings = checkStrings;
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            var (code, msg) = checkSolution();
            return (code, $"{msg} [{this.Description}]");
        }

        protected (Result resultCode, string message) checkSolution()
        {
            // odkomentować, żeby wypisywać zwracany wzorzec
            // Console.WriteLine(result.returnedPath);

            // sprawdzenie czy odpowiedź jest poprawna
            if (result.returnedVal != expectedVal)
                return (Result.WrongResult, $"Zwrócono ({result.returnedVal}), powinno być ({expectedVal})");

            // jak nie ma trasy to nic już nie sprawdzamy
            if (!result.returnedVal)
                return OkResult("OK");

            if (result.returnedPath == null)
                return (Result.WrongResult, $"Odpowiedź poprawna, ale zwrócono null zamiast trasy");

            // sprawdzenie czy trasa składa się tylko z dozwolnych znaków
            for (int i = 0; i < result.returnedPath.Length; ++i)
                if (result.returnedPath[i] != 'D' && result.returnedPath[i] != 'R')
                    return (Result.WrongResult, $"Zwrócona trasa ({result.returnedPath}), zawiera niedozwolone znaki");

            // sprawdzenie czy podana trasa prowadzi do punktu docelowego i nie wchodzi na przeszkody
            int x = 0;
            int y = 0;
            bool[,] obstaclesTable = new bool[n, m];
            for (int i = 0; i < obstacles.Length; ++i)
                obstaclesTable[obstacles[i].Item1, obstacles[i].Item2] = true;
            for (int i = 0; i < result.returnedPath.Length; ++i)
            {
                if (result.returnedPath[i] == 'D')
                    ++x;
                else
                    ++y;
                if (x >= n || y >=m)
                    return (Result.WrongResult, $"Zwrócona trasa ({result.returnedPath}) wychodzi poza planszę");
                if (obstaclesTable[x, y])
                    return (Result.WrongResult, $"Zwrócona trasa ({result.returnedPath}) wchodzi na przeszkodę w punkcie ({x}, {y})");
            }
            if (x != n-1 || y != m-1)
                return (Result.WrongResult, $"Zwrócona trasa ({result.returnedPath}) nie kończy się w punkcie docelowym");

            // sprawdzenie czy trasa realizuje wzorzec dla etapu 2
            return pathMatch();
        }

        protected abstract (Result resultCode, string message) pathMatch();
        public (Result resultCode, string message) OkResult(string message) => (TimeLimit < PerformanceTime ? Result.LowEfficiency : Result.Success, $"{message} {PerformanceTime.ToString("#0.00")}s");
    }

    public class Stage1TestCase : Lab02TestCase
    {
        public Stage1TestCase(int n, int m, (int, int)[] obstacles, bool expectedVal, bool checkStrings, double timeLimit, string description) : base(n, m, obstacles, expectedVal, checkStrings, timeLimit, description)
        { }

        protected override void PerformTestCase(object prototypeObject)
        {
            result = ((PatternMatching)prototypeObject).Lab02Stage1(n, m, obstacles);
        }

        protected override (Result resultCode, string message) pathMatch()
        {
            return OkResult("OK");
        }
    }

    public class Stage2TestCase : Lab02TestCase
    {
        private readonly string pattern;
        public Stage2TestCase(int n, int m, string pattern, (int, int)[] obstacles, bool expectedVal, bool checkStrings, double timeLimit, string description) : base(n, m, obstacles, expectedVal, checkStrings, timeLimit, description)
        {
            this.pattern = pattern;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            result = ((PatternMatching)prototypeObject).Lab02Stage2(n, m, pattern, obstacles);
        }

        protected override (Result resultCode, string message) pathMatch()
        {
            int n = result.returnedPath.Length;
            int m = pattern.Length;

            bool[,] table = new bool[n + 1, m+1];
            table[0, 0] = true;

            for (int j = 1; j <= m; j++)
                if (pattern[j - 1] == '*')
                    table[0, j] = table[0, j - 1];

            for (int i = 1; i <= n; ++i)
                for (int j = 1; j <= m; ++j)
                {
                    if (pattern[j-1] == '*')
                        table[i, j] = table[i, j - 1] || table[i - 1, j];
                    if (pattern[j - 1] == '?' || result.returnedPath[i - 1] == pattern[j - 1])
                        table[i, j] = table[i - 1, j - 1];
                }

            if (table[n, m])
                return OkResult("OK");
            return (Result.WrongResult, $"Zwrócona trasa ({result}) nie realizuje wzorca ({pattern})");
        }
    }



    public class Lab02Tests : TestModule
    {
        TestSet Stage1 = new TestSet(prototypeObject: new PatternMatching(), description: "Etap 1", settings: true);
        TestSet Stage2 = new TestSet(prototypeObject: new PatternMatching(), description: "Etap 2", settings: true);

        public override void PrepareTestSets()
        {
            TestSets["Stage1"] = Stage1;
            TestSets["Stage2"] = Stage2;

            prepare();

        }

        private void addStage1(Stage1TestCase s1TestCase)
        {
            Stage1.TestCases.Add(s1TestCase);
        }

        private void addStage2(Stage2TestCase s2TestCase)
        {
            Stage2.TestCases.Add(s2TestCase);
        }


        private void prepare()
        {
            (int, int)[] obstacles = new (int, int)[]{ (2,3), (4,0)};
            // Przykłady z zadania
            addStage1(new Stage1TestCase(n: 5, m: 6, obstacles: obstacles, expectedVal: true, timeLimit: 1, description: "TRASA ISTNIEJE: przykład z zadania", checkStrings: false));
            addStage2(new Stage2TestCase(n: 5, m: 6, pattern: "D*D?RD", obstacles: obstacles, expectedVal: true, timeLimit:1, description: "TRASA ISTNIEJE: przykład z zadania", checkStrings: false));

            // Testy do etapu 1
            // Trasy istnieją
            addStage1(new Stage1TestCase(n: 1, m: 1, obstacles: new (int, int)[] { }, expectedVal: true, timeLimit: 1, description: "TRASA ISTNIEJE: robot w punkcie docelowym", checkStrings: false));
            addStage1(new Stage1TestCase(n: 10, m: 10, obstacles: new (int, int)[] { }, expectedVal: true, timeLimit: 1, description: "TRASA ISTNIEJE: brak przeszkód", checkStrings: false));
            obstacles = new (int, int)[] { (0, 2), (1, 0), (1, 4), (2, 1), (2, 2), (2, 4), (3, 5), (4, 3), (4, 6), (5, 4) };
            addStage1(new Stage1TestCase(n: 6, m: 7, obstacles: obstacles, expectedVal: true, timeLimit: 1, description: "TRASA ISTNIEJE: tylko jedna możliwa", checkStrings: false));
            obstacles = new (int, int)[] { (0, 2), (1, 0), (1, 4), (2, 1), (2, 2), (2, 4), (3, 5), (4, 3), (4, 6), (5, 4), (6, 6), (7, 7), (8, 8) };
            addStage1(new Stage1TestCase(n: 10, m: 11, obstacles: obstacles, expectedVal: true, timeLimit: 1, description: "TRASA ISTNIEJE: dużo przeszkód", checkStrings: false));
            obstacles = new (int, int)[36];
            for (int i = 2; i < 20; ++i)
                obstacles[i - 2] = (i, i - 2);
            for (int i = 2; i < 20; ++i)
                obstacles[i + 16] = (i-2, i);
            addStage1(new Stage1TestCase(n: 20, m: 20, obstacles: obstacles, expectedVal: true, timeLimit: 1, description: "TRASA ISTNIEJE: przeszkody ustawione schodkowo", checkStrings: false));
            obstacles = new (int, int)[19];
            for (int i = 0; i < 19; ++i)
                obstacles[i] = (9, i);
            addStage1(new Stage1TestCase(n: 20, m: 20, obstacles: obstacles, expectedVal: true, timeLimit: 1, description: "TRASA ISTNIEJE: prawie cały rząd przeszkód", checkStrings: false));

            // Trasy nie istnieją
            addStage1(new Stage1TestCase(n: 10, m: 10, obstacles: new (int, int)[] { (9, 9) }, expectedVal: false, timeLimit: 1, description: "TRASA NIE ISTNIEJE: przeszkoda w punkcie docelowym", checkStrings: false));
            obstacles = new (int, int)[] { (0, 2), (1, 0), (1, 4), (2, 1), (2, 2), (2, 4), (3, 4), (3, 5), (4, 3), (4, 6), (5, 4) };
            addStage1(new Stage1TestCase(n: 6, m: 7, obstacles: obstacles, expectedVal: false, timeLimit: 1, description: "TRASA NIE ISTNIEJE: dużo przeszkód", checkStrings: false));
            obstacles = new (int, int)[20];
            for (int i = 0; i < 10; ++i)
                obstacles[i] = (9, i);
            for (int i = 0; i < 10; ++i)
                obstacles[i+10] = (10, 10+i);
            addStage1(new Stage1TestCase(n: 20, m: 20, obstacles: obstacles, expectedVal: false, timeLimit: 1, description: "TRASA NIE ISTNIEJE: dwa nachodzące na siebie rzędy przeszkód", checkStrings: false));
            addStage1(new Stage1TestCase(n: 6, m: 9, obstacles: new (int, int)[] { (0, 1), (1, 1), (2, 1), (3, 1), (2, 3), (3, 3), (4, 3), (5, 3), (0,5), (1, 5), (2, 5), (3, 5),  (3, 6), (3, 7), (4, 7), (5, 7)}, expectedVal: false, timeLimit: 1, description: "TRASA NIE ISTNIEJE: dużo przeszkód", checkStrings: false));
            // Testy do etapu 2
            // Trasy istnieją
            addStage2(new Stage2TestCase(n: 1, m: 1, pattern: "*", obstacles: new (int, int)[] { }, expectedVal: true, timeLimit: 1, description: "TRASA ISTNIEJE: robot w punkcie docelowym, wzorzec *", checkStrings: false));
            obstacles = new (int, int)[] { (0, 2), (1, 0), (1, 4), (2, 1), (2, 2), (2, 4), (3, 5), (4, 3), (4, 6), (5, 4) };
            addStage2(new Stage2TestCase(n: 6, m: 7, pattern: "R*D?", obstacles: obstacles, expectedVal: true, timeLimit: 1, description: "TRASA ISTNIEJE: tylko jedna możliwa trasa zgodna ze wzorcem", checkStrings: false));
            addStage2(new Stage2TestCase(n: 4, m: 4, pattern: "D?R*", obstacles: new (int, int)[] { }, expectedVal: true, timeLimit: 1, description: "TRASA ISTNIEJE: brak przeszkód, wzorzec D?R*", checkStrings: false));
            addStage2(new Stage2TestCase(n: 20, m: 20, pattern: "D?DR*RDR?*R", obstacles: new (int, int)[] { }, expectedVal: true, timeLimit: 1, description: "TRASA ISTNIEJE: brak przeszkód, wzorzec D?DR*RDR?*R", checkStrings: false));
            addStage2(new Stage2TestCase(n: 10, m: 10, pattern: "D?DR*RDR?", obstacles: new (int, int)[] {(2,0)}, expectedVal: true, timeLimit: 1, description: "TRASA ISTNIEJE: przeszkoda na (2,0), wzorzec D?DR*RDR?", checkStrings: false));
            obstacles = new (int, int)[] { (0, 2), (1, 0), (1, 4), (2, 1), (2, 2), (2, 4), (3, 5), (4, 3), (4, 6), (5, 4), (6, 6), (7, 7), (8, 8) };
            addStage2(new Stage2TestCase(n: 10, m: 11, pattern: "RDR?D*DR", obstacles: obstacles, expectedVal: true, timeLimit: 1, description: "TRASA ISTNIEJE: dużo przeszkód", checkStrings: false));
            obstacles = new (int, int)[36];
            for (int i = 2; i < 20; ++i)
                obstacles[i - 2] = (i, i - 2);
            for (int i = 2; i < 20; ++i)
                obstacles[i + 16] = (i - 2, i);
            addStage2(new Stage2TestCase(n: 20, m: 20, pattern: "RDR*RD*RD?", obstacles: obstacles, expectedVal: true, timeLimit: 1, description: "TRASA ISTNIEJE: przeszkody ustawione schodkowo", checkStrings: false));
            // Trasy nie istnieją
            addStage2(new Stage2TestCase(n: 1, m: 1, pattern: "D", obstacles: new (int, int)[] { }, expectedVal: false, timeLimit: 1, description: "TRASA NIE ISTNIEJE: robot w punkcie docelowym, wzorzec D", checkStrings: false));
            addStage2(new Stage2TestCase(n: 9, m: 9, pattern: "RRRRRRRRRR", obstacles: new (int, int)[] { }, expectedVal: false, timeLimit: 1, description: "TRASA NIE ISTNIEJE: brak przeszkód, wzorzec wyprowadza poza planszę", checkStrings: false));
            obstacles = new (int, int)[] { (0, 2), (1, 0), (1, 4), (2, 1), (2, 2), (2, 4), (3, 5), (4, 3), (4, 6), (5, 4) };
            addStage2(new Stage2TestCase(n: 6, m: 7, pattern: "R*R?", obstacles: obstacles, expectedVal: false, timeLimit: 1, description: "TRASA NIE ISTNIEJE: tylko jedna możliwa trasa, niezgodna ze wzorcem", checkStrings: false));
            obstacles = new (int, int)[] { (0, 2), (1, 0), (1, 4), (2, 1), (2, 2), (2, 4), (3, 5), (4, 3), (4, 6), (5, 4), (6, 6), (7, 7), (8, 8) };
            addStage2(new Stage2TestCase(n: 10, m: 11, pattern: "RDR?DR*DR", obstacles: obstacles, expectedVal: false, timeLimit: 1, description: "TRASA NIE ISTNIEJE: dużo przeszkód", checkStrings: false));

        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            var tests = new Lab02Tests();
            tests.PrepareTestSets();
            foreach (var ts in tests.TestSets)
            {
                ts.Value.PerformTests(verbose: true, checkTimeLimit: false);
            }
        }
    }
}
