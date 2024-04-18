using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ASD.Graphs;
using ASD.Graphs.Testing;

namespace ASD
{
    internal class Lab08Stage1Case : Lab08StageBase
    {
        bool? buildingFound;

        public Lab08Stage1Case(int l, int h, int[,] pleasure, int? expectedResult, double timeLimit, string description)
            : base(l, h, pleasure, expectedResult, timeLimit, description)
        {
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            buildingFound = ((Lab08)prototypeObject).Stage1ExistsBuilding(l, h, pleasure);
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            if (buildingFound is null)
            {
                return (Result.NotPerformed, "Wewnętrzny błąd");
            }

            bool result = expectedResult.HasValue;
            if (result != buildingFound)
            {
                return (Result.WrongResult, buildingFound.Value ? "Nie istnieje budowla zadowalająca Kazika; zwrócono przeciwnie" : "Istnieje budowla zadowalająca Kazika; zwrócono przeciwnie");
            }

            return OkResult("OK");
        }
    }

    internal class Lab08Stage2Case : Lab08StageBase
    {
        protected int? buildingPleasure;
        protected (int x, int y)[] blockOrder;

        public Lab08Stage2Case(int l, int h, int[,] pleasure, int? expectedResult, double timeLimit, string description)
           : base(l, h, pleasure, expectedResult, timeLimit, description)
        {
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            buildingPleasure = ((Lab08)prototypeObject).Stage2GetOptimalBuilding(l, h, pleasure, out blockOrder);
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            if (buildingPleasure is null && expectedResult is null)
                return OkResult("OK");

            if (buildingPleasure is null)
                return (Result.WrongResult, "Istnieje budowla zadowalająca Kazika; zwrócono przeciwnie");
            if (expectedResult is null)
                return (Result.WrongResult, "Nie istnieje budowla zadowalająca Kazika; zwrócono przeciwnie");

            if (buildingPleasure.Value != expectedResult.Value)
                return (Result.WrongResult, $"Niepoprawna maksymalna wartość zadowolenia: zwrócono {buildingPleasure.Value}, powinno być {expectedResult.Value}");

            // check blocks
            int blockPleasure = 0;
            bool[,] blockPresent = new bool[l, h];
            foreach ((int x, int y) in blockOrder)
            {
                if (x < 0 || x >= l || y < 0 || y >= h)
                    return (Result.WrongResult, $"Zwrócona pozycja bloku ({x},{y}) znajduje się poza granicami obszaru budowy");

                if (y != 0)
                {
                    if (!blockPresent[x, y - 1])
                        return (Result.WrongResult, $"Próba ustawienia bloku na ({x},{y}), ale poniżej, na ({x},{y - 1}), nie ma żadnego bloku");
                    if (x >= l - 1 || !blockPresent[x + 1, y - 1])
                        return (Result.WrongResult, $"Próba ustawienia bloku na ({x},{y}), ale poniżej w prawo, na ({x + 1},{y - 1}) nie ma żadnego bloku");
                }
                blockPleasure += pleasure[x, y] - 1;
                blockPresent[x, y] = true;
            }
            if (blockPleasure != buildingPleasure.Value)
                return (Result.WrongResult, $"Suma zadowolenia z postawionych bloków ({blockPleasure}) jest inna niż zwrócone zadowolenie z budowli ({buildingPleasure.Value})");

            return OkResult("OK");
        }
    }

    internal abstract class Lab08StageBase : TestCase
    {
        protected readonly int l, h;
        protected readonly int[,] pleasure;
        protected readonly int? expectedResult;

        public Lab08StageBase(int l, int h, int[,] pleasure, int? expectedResult, double timeLimit, string description) : base(timeLimit, null, description)
        {
            this.l = l;
            this.h = h;
            this.pleasure = pleasure;
            this.expectedResult = expectedResult;
        }

        protected (Result resultCode, string message) OkResult(string message) => (TimeLimit < PerformanceTime ? Result.LowEfficiency : Result.Success, $"{message} {PerformanceTime.ToString("#0.00")}s");
    }

    class Lab08Tests : TestModule
    {
        public TestSet stage1Tests;
        public TestSet stage2Tests;

        public override void PrepareTestSets()
        {
            stage1Tests = new TestSet(new Lab08(), "Etap I: prace przedprojektowe");
            stage2Tests = new TestSet(new Lab08(), "Etap II: kompletny projekt");
            TestSets["Etap 1"] = stage1Tests;
            TestSets["Etap 2"] = stage2Tests;
            PrepareTests();
        }

        void PrepareTests()
        {
            {
                // Test 1
                int h = 5, l = 5;
                int[,] pleasure = new int[l, h];
                pleasure[0, 4] = 1;
                pleasure[1, 1] = 2;
                pleasure[1, 2] = 3;
                pleasure[2, 1] = 4;
                pleasure[3, 3] = 99;
                int? expectedResult = 3;
                double timeLimit = 3;
                string desc = "Przykład 1 z treści zadania";
                stage1Tests.TestCases.Add(new Lab08Stage1Case(l, h, pleasure, expectedResult, timeLimit, desc));
                stage2Tests.TestCases.Add(new Lab08Stage2Case(l, h, pleasure, expectedResult, timeLimit, desc));
            }
            {
                // Test 2
                int h = 5, l = 5;
                int[,] pleasure = new int[l, h];
                pleasure[0, 4] = 1;
                pleasure[1, 1] = 2;
                pleasure[1, 2] = 2;
                pleasure[2, 1] = 2;
                pleasure[3, 3] = 99;
                int? expectedResult = null;
                double timeLimit = 2;
                string desc = "Przykład 2 z treści zadania";
                stage1Tests.TestCases.Add(new Lab08Stage1Case(l, h, pleasure, expectedResult, timeLimit, desc));
                stage2Tests.TestCases.Add(new Lab08Stage2Case(l, h, pleasure, expectedResult, timeLimit, desc));
            }
            {
                // Test 3
                int h = 5, l = 6;
                int[,] pleasure = new int[l, h];
                pleasure[0, 0] = 1;
                pleasure[1, 1] = 2;
                pleasure[1, 2] = 2;
                pleasure[2, 1] = 2;
                pleasure[2, 4] = 4;
                pleasure[3, 2] = 6;
                int? expectedResult = 1;
                double timeLimit = 1;
                string desc = "Przykład 3 z treści zadania";
                stage1Tests.TestCases.Add(new Lab08Stage1Case(l, h, pleasure, expectedResult, timeLimit, desc));
                stage2Tests.TestCases.Add(new Lab08Stage2Case(l, h, pleasure, expectedResult, timeLimit, desc));
            }
            {
                // Test 4
                int h = 9, l = 11;
                int[,] pleasure = new int[l, h];
                for (int i = 0; i < 6; ++i)
                    pleasure[i, i] = 3 + i % 2;
                for (int i = 6; i < l; ++i)
                    pleasure[i, 10 - i] = 3 + i % 2;
                int? expectedResult = 7;
                double timeLimit = 1;
                string desc = "Nieoptymalna piramidka";
                stage1Tests.TestCases.Add(new Lab08Stage1Case(l, h, pleasure, expectedResult, timeLimit, desc));
                stage2Tests.TestCases.Add(new Lab08Stage2Case(l, h, pleasure, expectedResult, timeLimit, desc));
            }
            {
                // Test 5
                int h = 9, l = 11;
                int[,] pleasure = new int[l, h];
                for (int i = 0; i < 5; ++i)
                    pleasure[i, i] = i + 2;
                for (int i = 5; i < l; ++i)
                    pleasure[i, 10 - i] = 11 - i;
                int? expectedResult = 5;
                double timeLimit = 1;
                string desc = "Optymalna piramidka";
                stage1Tests.TestCases.Add(new Lab08Stage1Case(l, h, pleasure, expectedResult, timeLimit, desc));
                stage2Tests.TestCases.Add(new Lab08Stage2Case(l, h, pleasure, expectedResult, timeLimit, desc));
            }
            {
                // Test 6
                int h = 100, l = 100;
                int[,] pleasure = new int[l, h];
                for (int i = 0; i < h; ++i)
                    for (int j = 0; j < h; ++j)
                        pleasure[i, j] = 1;
                int? expectedResult = null;
                double timeLimit = 3;
                string desc = "Zadowolenie ze wszystkich bloków równe 1";
                stage1Tests.TestCases.Add(new Lab08Stage1Case(l, h, pleasure, expectedResult, timeLimit, desc));
                stage2Tests.TestCases.Add(new Lab08Stage2Case(l, h, pleasure, expectedResult, timeLimit, desc));
            }
            {
                // Test 7
                int h = 50, l = 50;
                int[,] pleasure = new int[l, h];
                for (int i = 0; i < h; ++i)
                    for (int j = 0; j < h; ++j)
                        pleasure[i, j] = 2;
                int? expectedResult = 1275;
                double timeLimit = 4;
                string desc = "Chmielna 89";
                stage1Tests.TestCases.Add(new Lab08Stage1Case(l, h, pleasure, expectedResult, timeLimit, desc));
                stage2Tests.TestCases.Add(new Lab08Stage2Case(l, h, pleasure, expectedResult, timeLimit, desc));
            }
            {
                // Test 8
                int h = 10, l = 25;
                int[,] pleasure = new int[l, h];
                pleasure[0, 0] = 2;
                pleasure[3, 4] = 3;
                pleasure[5, 4] = 5;
                pleasure[4, 2] = 8;
                pleasure[11, 2] = 1;
                pleasure[12, 5] = 10;
                pleasure[15, 7] = 50;
                pleasure[20, 9] = 100;
                int? expectedResult = 17;
                double timeLimit = 1;
                string desc = "Kilka rozłącznych budowli";
                stage1Tests.TestCases.Add(new Lab08Stage1Case(l, h, pleasure, expectedResult, timeLimit, desc));
                stage2Tests.TestCases.Add(new Lab08Stage2Case(l, h, pleasure, expectedResult, timeLimit, desc));
            }
            {
                // Test 9
                PrepareRandomTest(100, 100, 20, 30, 88, 22, 1, "Test losowy 100x100, rozwiązanie istnieje");
            }
            {
                // Test 10
                PrepareRandomTest(100, 100, 20, 30, 12345, null, 1, "Test losowy 100x100, rozwiązanie nie istnieje");
            }
            {
                // Test 11
                int h = 3, l = 1000;
                int[,] pleasure = new int[l, h];
                for (int i = 0; i < l; i += 2)
                    pleasure[i, 1] = 4;
                int? expectedResult = 500;
                double timeLimit = 6;
                string desc = "Wielki Mur Chiński";
                stage1Tests.TestCases.Add(new Lab08Stage1Case(l, h, pleasure, expectedResult, timeLimit, desc));
                stage2Tests.TestCases.Add(new Lab08Stage2Case(l, h, pleasure, expectedResult, timeLimit, desc));
            }
            {
                // Test 12
                PrepareRandomTest(500, 100, 20, 50, 22200, null, 3, "Duży test losowy 500x100, mało dodatniego zadowolenia");
            }
            {
                // Test 13
                PrepareRandomTest(100, 500, 20, 50, 58407, null, 1, "Duży test losowy 100x500, mało dodatniego zadowolenia");
            }
            {
                // Test 14
                PrepareRandomTest(500, 100, 200, 50, -1, 185, 30, "Duży test losowy 500x100, sporo dodatniego zadowolenia");
            }
            {
                // Test 15
                PrepareRandomTest(100, 500, 200, 50, -1, 41, 3, "Duży test losowy 100x500, sporo dodatniego zadowolenia");
            }
        }

        void PrepareRandomTest(int l, int h, int specialBlocksCount, int maxPleasureIncl, int seed, int? expectedResult, double timeLimit, string desc)
            => PrepareRandomTest(l, h, specialBlocksCount, maxPleasureIncl, seed, expectedResult, timeLimit, timeLimit, desc);

        void PrepareRandomTest(int l, int h, int specialBlocksCount, int maxPleasureIncl, int seed, int? expectedResult, double timeLimit1, double timeLimit2, string desc)
            => PrepareRandomTest(l, h, specialBlocksCount, maxPleasureIncl, seed, expectedResult, timeLimit1, timeLimit2, desc, stage1Tests, stage2Tests);

        void PrepareRandomTest(int l, int h, int specialBlocksCount, int maxPleasureIncl, int seed, int? expectedResult, double timeLimit1, double timeLimit2, string desc, TestSet ts1, TestSet ts2)
        {
            Random random = new Random(seed);
            int[,] pleasure = new int[l, h];
            for (int i = 0; i < specialBlocksCount; ++i)
            {
                int x = random.Next(l);
                int y = random.Next(h);
                pleasure[x, y] = random.Next(1, maxPleasureIncl + 1);
            }
            ts1.TestCases.Add(new Lab08Stage1Case(l, h, pleasure, expectedResult, timeLimit1, desc));
            ts2.TestCases.Add(new Lab08Stage2Case(l, h, pleasure, expectedResult, timeLimit2, desc));
        }
    }

    class Program
    {
        public static void Main(string[] args)
        {
            var tests = new Lab08Tests();
            tests.PrepareTestSets();
            foreach (var ts in tests.TestSets)
            {
                ts.Value.PerformTests(verbose: true, checkTimeLimit: false);
            }
        }
    }
}