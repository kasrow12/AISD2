
using System;
using System.Linq;
using System.Text;
using ASD;
using System.Collections.Generic;

namespace ASD
{

    public abstract class Lab02TestCase : TestCase
    {
        protected readonly int n;
        protected readonly int m;
        protected readonly ((int di, int dj) step, int cost)[] moves;
        private bool checkStrings;
        private readonly bool expectedRes;
        private readonly int expectedCost;

        protected (bool returnedRes, int returnedCost, (int i, int j)[] returnedPath) result;

        protected Lab02TestCase(int n, int m, ((int di, int dj) step, int cost)[] moves, bool expectedRes, int expectedCost, bool checkStrings, int timeLimit, string description) : base(timeLimit, null, description)
        {
            this.n = n;
            this.m = m;
            this.moves = moves;
            this.expectedRes = expectedRes;
            this.expectedCost = expectedCost;
            this.checkStrings = checkStrings;
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            var (code, msg) = checkSolution();
            return (code, $"{msg} [{this.Description}]");
        }

        protected (Result resultCode, string message) checkSolution()
        {
            // sprawdzenie czy odpowiedź jest poprawna
            if (result.returnedRes != expectedRes)
                return (Result.WrongResult, $"Zwrócono ({result.returnedRes}), powinno być ({expectedRes})");

            // jak nie ma trasy to nic już nie sprawdzamy
            if (!result.returnedRes)
                return OkResult("OK");

            if (result.returnedCost != expectedCost)
                return (Result.WrongResult, $"Zwrócono za duży koszt, zwrócono ({result.returnedCost}), powinno być ({expectedCost})");

            if (result.returnedPath == null)
                return (Result.WrongResult, $"Odpowiedź poprawna, ale zwrócono null zamiast trasy");

            // odkomentować, żeby wypisywać zwracaną ścieżkę
            //int p = 0;
            //for (; p < result.returnedPath.Length - 1; ++p)
            //{
            //    Console.Write($"({result.returnedPath[p].i}, {result.returnedPath[p].j}) -> ");
            //}
            //Console.WriteLine($"({result.returnedPath[p].i}, {result.returnedPath[p].j})");

            // odkomentować, żeby wypisywać kroki
            //int s = 0;
            //for (; s < result.returnedPath.Length - 1; ++s)
            //{
            //    Console.Write($"({result.returnedPath[s + 1].i - result.returnedPath[s].i}, {result.returnedPath[s + 1].j - result.returnedPath[s].j}) ");
            //}


            // sprawdzenie czy podana trasa prowadzi do ostatniego wiersza i nie wychodzi poza planszę
            int i;
            (int i, int j) pos = (0, 0);
            if (result.returnedPath[0].i != 0 || result.returnedPath[0].j != 0)
                return (Result.WrongResult, $"Odpowiedź poprawna, ale zwrócona ścieżka nie zaczyna się w polu (0,0)");
            for (i = 1; i < result.returnedPath.Length; ++i)
            {
                pos = result.returnedPath[i];
                if (pos.i < 0 || pos.i >= n || pos.j < 0 || pos.j >= m)
                    return (Result.WrongResult, $"Odpowiedź poprawna, ale zwrócona ścieżka wychodzi poza planszę");
            }
            if (pos.i != n - 1)
                return (Result.WrongResult, $"Odpowiedź poprawna, ale zwrócona ścieżka nie kończy się w ostatnim wierszu");
            return CheckMoves();
        }
        // metoda sprawdzająca poprawność ruchów
        protected abstract (Result resultCode, string message) CheckMoves();
        public (Result resultCode, string message) OkResult(string message) => (TimeLimit < PerformanceTime ? Result.LowEfficiency : Result.Success, $"{message} {PerformanceTime.ToString("#0.00")}s");
    }

    public class Stage1TestCase : Lab02TestCase
    {
        public Stage1TestCase(int n, int m, ((int, int), int)[] moves, bool expectedRes, int expectedCost, bool checkStrings, int timeLimit, string description) : base(n, m, moves, expectedRes, expectedCost, checkStrings, timeLimit, description)
        { }

        protected override void PerformTestCase(object prototypeObject)
        {
            result = ((Lab02)prototypeObject).Lab02Stage1(n, m, moves);
        }

        protected override (Result resultCode, string message) CheckMoves()
        {
            (int i, int j) pos = (0, 0);
            for (int i = 1; i < result.returnedPath.Length; ++i)
            {
                (int di, int dj) move = (result.returnedPath[i].i - pos.i, result.returnedPath[i].j - pos.j);
                pos = result.returnedPath[i];
                bool ok = false;
                int k;
                for (k = 0; k < moves.Length; ++k)
                    if (moves[k].step.di == move.di && moves[k].step.dj == move.dj)
                    {
                        ok = true;
                        break;
                    }
                if (!ok)
                    return (Result.WrongResult, $"Odpowiedź poprawna, ale zwrócona ścieżka zawiera niedopuszczalny krok {move}");

            }
            return OkResult("OK");
        }
    }


    public class Stage2TestCase : Lab02TestCase
    {
        public Stage2TestCase(int n, int m, ((int, int), int)[] moves, bool expectedRes, int expectedCost, bool checkStrings, int timeLimit, string description) : base(n, m, moves, expectedRes, expectedCost, checkStrings, timeLimit, description)
        { }

        protected override void PerformTestCase(object prototypeObject)
        {
            result = ((Lab02)prototypeObject).Lab02Stage2(n, m, moves);
        }

        protected override (Result resultCode, string message) CheckMoves()
        {
            (int i, int j) pos = (0, 0);
            int[] counterList = new int[moves.Length];
            for (int i = 1; i < result.returnedPath.Length; ++i)
            {
                (int di, int dj) move = (result.returnedPath[i].i - pos.i, result.returnedPath[i].j - pos.j);
                pos = result.returnedPath[i];
                int k;
                bool ok = false;
                for (k = 0; k < moves.Length; ++k)
                    if (moves[k].step.di == move.di && moves[k].step.dj == move.dj)
                    {
                        ok = true;
                        ++counterList[k];
                        break;
                    }
                if (!ok)
                    return (Result.WrongResult, $"Odpowiedź poprawna, ale zwrócona ścieżka zawiera niedopuszczalny krok {move}");
            }
            for (int i = 0; i < counterList.Length; ++i)
                if (counterList[i] > 1)
                    return (Result.WrongResult, $"Odpowiedź poprawna, ale krok {moves[i].step} występuje więcej niż raz na zwróconej ścieżce");
            return OkResult("OK");
        }

    }

    public class Lab02Tests : TestModule
    {
        TestSet Stage1 = new TestSet(prototypeObject: new Lab02(), description: "Etap 1", settings: true);
        TestSet Stage2 = new TestSet(prototypeObject: new Lab02(), description: "Etap 2", settings: true);

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
            Random rand = new Random(1500190);
            ((int, int), int)[] moves = new ((int, int), int)[] { ((1, 1), 4), ((2, 2), 6), ((2, 1), 5), ((1, 0), 3), ((0, 1), 3) };
            // Przykład z zadania
            addStage1(new Stage1TestCase(n: 8, m: 6, moves: moves, expectedRes: true, expectedCost: 18, timeLimit: 1, description: "TRASA ISTNIEJE: przykład z zadania", checkStrings: false));
            addStage2(new Stage2TestCase(n: 8, m: 6, moves: moves, expectedRes: false, expectedCost: int.MaxValue, timeLimit: 1, description: "TRASA NIE ISTNIEJE: przykład z zadania", checkStrings: false));

            // Mała kwadratowa plansza
            moves = new ((int, int), int)[] { ((4, 5), 4), ((1, 3), 1), ((3, 2), 2), ((4, 6), 10), ((2, 0), 3) };
            addStage1(new Stage1TestCase(n: 10, m: 10, moves: moves, expectedRes: true, expectedCost: 6, timeLimit: 1, description: "TRASA ISTNIEJE: mała kwadratowa plansza", checkStrings: false));
            addStage2(new Stage2TestCase(n: 10, m: 10, moves: moves, expectedRes: true, expectedCost: 9, timeLimit: 1, description: "TRASA ISTNIEJE: mała kwadratowa plansza", checkStrings: false));

            // Mała wąska plansza
            moves = new ((int, int), int)[] { ((3, 1), 4), ((4, 1), 5), ((2, 0), 4), ((1, 0), 2) };
            addStage1(new Stage1TestCase(n: 10, m: 3, moves: moves, expectedRes: true, expectedCost: 12, timeLimit: 1, description: "TRASA ISTNIEJE: mała wąska plansza", checkStrings: false));
            addStage2(new Stage2TestCase(n: 10, m: 3, moves: moves, expectedRes: true, expectedCost: 13, timeLimit: 1, description: "TRASA ISTNIEJE: mała wąska plansza", checkStrings: false));

            moves = new ((int, int), int)[] { ((3, 1), 4), ((4, 1), 5), ((2, 1), 4), ((1, 1), 2) };
            addStage1(new Stage1TestCase(n: 10, m: 3, moves: moves, expectedRes: false, expectedCost: int.MaxValue, timeLimit: 1, description: "TRASA NIE ISTNIEJE: mała wąska plansza", checkStrings: false));
            addStage2(new Stage2TestCase(n: 10, m: 3, moves: moves, expectedRes: false, expectedCost: int.MaxValue, timeLimit: 1, description: "TRASA NIE ISTNIEJE: mała wąska plansza", checkStrings: false));

            // Duża kwadratowa plansza
            moves = new ((int, int), int)[] { ((170, 200), 140), ((170, 300), 140), ((50, 50), 42), ((70, 30), 58), ((1, 0), 1), ((200, 10), 210), ((100, 1), 100), ((8, 0), 7), ((70, 70), 60), ((90, 90), 80) };
            addStage1(new Stage1TestCase(n: 1000, m: 1000, moves: moves, expectedRes: true, expectedCost: 826, timeLimit: 1, description: "TRASA ISTNIEJE: duża kwadratowa plansza", checkStrings: false));
            addStage2(new Stage2TestCase(n: 1000, m: 1000, moves: moves, expectedRes: false, expectedCost: int.MaxValue, timeLimit: 5, description: "TRASA  NIE ISTNIEJE: duża kwadratowa plansza", checkStrings: false));

            moves = new ((int, int), int)[] { ((170, 200), 140), ((170, 300), 150), ((50, 50), 42), ((50, 150), 47), ((70, 30), 58), ((70, 90), 62), ((1, 0), 1), ((200, 10), 210), ((100, 10), 100), ((100, 100), 120), ((8, 0), 7), ((70, 70), 60), ((190, 90), 180) };
            addStage1(new Stage1TestCase(n: 1000, m: 1000, moves: moves, expectedRes: true, expectedCost: 826, timeLimit: 1, description: "TRASA ISTNIEJE: duża kwadratowa plansza", checkStrings: false));
            addStage2(new Stage2TestCase(n: 1000, m: 1000, moves: moves, expectedRes: true, expectedCost: 910, timeLimit: 10, description: "TRASA ISTNIEJE: duża kwadratowa plansza", checkStrings: false));

            // Testy losowe
            moves = RandomMoves(1000, 1000, rand);
            addStage1(new Stage1TestCase(n: 1000, m: 1000, moves: moves, expectedRes: true, expectedCost: 512, timeLimit: 10, description: "TRASA ISTNIEJE: test losowy, plansza 1000x1000", checkStrings: false));
            addStage2(new Stage2TestCase(n: 1000, m: 1000, moves: moves, expectedRes: true, expectedCost: 762, timeLimit: 40, description: "TRASA ISTNIEJE: test losowy, plansza 1000x1000", checkStrings: false));

            moves = RandomMoves(300, 3000, rand);
            addStage1(new Stage1TestCase(n: 300, m: 3000, moves: moves, expectedRes: true, expectedCost: 185, timeLimit: 1, description: "TRASA ISTNIEJE: test losowy, plansza 300x3000", checkStrings: false));
            addStage2(new Stage2TestCase(n: 300, m: 3000, moves: moves, expectedRes: false, expectedCost: int.MaxValue, timeLimit: 5, description: "TRASA NIE ISTNIEJE: test losowy, plansza 300x3000", checkStrings: false));

            moves = RandomMoves(100, 10, rand);
            addStage1(new Stage1TestCase(n: 100, m: 10, moves: moves, expectedRes: false, expectedCost: int.MaxValue, timeLimit: 1, description: "TRASA NIE ISTNIEJE: test losowy, plansza 100x10", checkStrings: false));
            addStage2(new Stage2TestCase(n: 100, m: 10, moves: moves, expectedRes: false, expectedCost: int.MaxValue, timeLimit: 1, description: "TRASA NIE ISTNIEJE: test losowy, plansza 100x10", checkStrings: false));

        }
        private ((int, int), int)[] RandomMoves(int n, int m, Random rand)
        {
            ((int, int), int)[] moves = new ((int, int), int)[rand.Next((int)n / 100, (int)n / 25)];
            for (int i = 0; i < moves.Length; ++i)
            {
                (int di, int dj) step = (rand.Next(1, (int)n / 10), rand.Next(1, (int)n / 10));
                int cost = rand.Next(Math.Max((int)step.di / 2, 1), step.di * 2);
                moves[i] = (step, cost);
            }
            return moves;
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
