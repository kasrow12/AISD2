using System;
using ASD.Graphs;
using ASD.Graphs.Testing;
using System.Collections.Generic;

namespace ASD
{

    public abstract class Lab06TestCase : TestCase
    {
        //protected readonly DiGraph<int> graph;
        //protected readonly int daysNumber;
        private readonly bool checkRoute;
        protected readonly int expectedResult;

        protected readonly int w;
        protected readonly int l;
        protected readonly int[,] lilies;
        protected readonly int sila;
        protected readonly int start;
        protected readonly int max_skok;

        protected (int total, (int,int)[] route) result;

        //protected (bool res, int[] route) result;

        public Lab06TestCase(int w, int l, int[,]lilies, int sila, int start, int max_skok, int expectedRes, double timeLimit, string description, bool checkR) : base(timeLimit, null, description)
        { 
            this.w = w;
            this.l = l;
            this.lilies = lilies;
            this.sila = sila;
            this.start = start;
            this.max_skok = max_skok;
            this.expectedResult = expectedRes;
            this.checkRoute = checkR;
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            var (code, msg) = checkSolution(result.total, result.route);
            return (code, $"{msg} [{this.Description}]");
        }

        private (Result resultCode, string message) checkSolution(int returnedResult, (int x, int y)[] returnedRoute)
        {
            if (expectedResult != returnedResult)
            {
                return (Result.WrongResult, $"Zwrócono {returnedResult}, powinno być {expectedResult}");
            }

            if (!checkRoute)
            {
                return OkResult("OK");
            }

            //zwracamy zero jesli nie ma trasy
            if (returnedResult == 0)
            {
                if (returnedRoute != null)
                    return (Result.WrongResult, "Nie ma trasy, należało zwrócić null");
                return OkResult("OK");
            }
            if (returnedRoute == null)
            {
                return (Result.WrongResult, "Zwrócono null zamiast trasy");
            }
            if (returnedRoute.Length == 0)
            {
                return (Result.WrongResult, "Zwrócono pustą trasę");
            }
            return checkRouteValid(result.route);
        }

        protected abstract (Result resultCode, string message) checkRouteValid((int,int)[] returnedRoute);
        public (Result resultCode, string message) OkResult(string message) => (TimeLimit < PerformanceTime ? Result.LowEfficiency : Result.Success, $"{message} {PerformanceTime.ToString("#0.00")}s");

    }

    public class Stage1TestCase : Lab06TestCase
    {
        public Stage1TestCase(int w, int l, int[,]lilies, int sila, int start, int max_skok, int expectedRes, double timeLimit, string description, bool checkR) : base(w, l, lilies, sila, start, max_skok, expectedRes, timeLimit, description, checkR)
        {
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            result = ((Lab06)prototypeObject).Lab06_FindRoute(w, l, lilies, sila, start);
        }

        protected override (Result resultCode, string message) checkRouteValid((int,int)[] returnedRoute)
        {
            int cur_x = -1;
            int cur_y = start;
            foreach ((int x, int y) jump in returnedRoute)
            {
                cur_x += jump.x;
                cur_y += jump.y;
                if(cur_x == w)
                {
                    return OkResult("OK");
                }
                if(lilies[cur_x,cur_y] != 1)
                {
                    return (Result.WrongResult, "Skok nie kończy się na lilii");
                }
            }
            if(cur_x != w + 1)
            {
                return (Result.WrongResult, "Trasa nie kończy się na drugim brzegu");
            }
            return OkResult("OK");
        }
    }

    public class Stage2TestCase : Lab06TestCase
    {
        private readonly int v;
        public Stage2TestCase(int w, int l, int[,]lilies, int sila, int start, int max_skok, int v, int expectedRes, double timeLimit, string description, bool checkR) : base(w, l, lilies, sila, start, max_skok, expectedRes, timeLimit, description, checkR)
        {
            this.v = v;
        }
        protected override void PerformTestCase(object prototypeObject)
        {
            result = ((Lab06)prototypeObject).Lab06_FindRouteFlowing(w, l, lilies, sila, start, max_skok, v);
        }
        private int[,] Permute(int[,] rzeka, int t, int v)
        {
            int[,] ret = new int[w, l];
            int permutation_dist = t * v % l;
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < l; j++)
                {
                    ret[i, (j + permutation_dist) % l ] = rzeka[i,j];
                }
            }
            return ret;
        }
        protected override (Result resultCode, string message) checkRouteValid((int, int)[] returnedRoute)
        {
            if (returnedRoute.Length  > max_skok)
            {
                return (Result.WrongResult, "Zwrócono za długą trasę");
            }
            int cur_x = -1;
            int cur_y = start;
            int jump_count = 0;
            foreach ((int x, int y) jump in returnedRoute)
            {
                cur_x += jump.x;
                cur_y += jump.y;
                jump_count++;
                if (cur_x == -1) continue;
                if(cur_x == w)
                {
                    return OkResult("OK");
                }
                if(Permute(lilies, jump_count, v)[cur_x,cur_y] != 1)
                {
                    return (Result.WrongResult, "Skok nie kończy się na lilii");
                }
            }
            if(cur_x != w + 1)
            {
                return (Result.WrongResult, "Trasa nie kończy się na drugim brzegu");
            }
            return OkResult("OK");
        }
    }
    public class Lab06Tests : TestModule
    {
        TestSet Stage1 = new TestSet(prototypeObject: new Lab06(), description: "Etap 1, minimalna siła żabki do pokonania trasy", settings: true);
        TestSet Stage2 = new TestSet(prototypeObject: new Lab06(), description: "Etap 2, zwrócenie trasy z Etapu 1", settings: true);
        TestSet Stage3 = new TestSet(prototypeObject: new Lab06(), description: "Etap 3, minimalna siła żabki do pokonania trasy na płynącej rzece", settings: true);
        TestSet Stage4 = new TestSet(prototypeObject: new Lab06(), description: "Etap 4, zwrócenie trasy z Etapu 3", settings: true);

        public override void PrepareTestSets()
        {
            TestSets["Stage1"] = Stage1;
            TestSets["Stage2"] = Stage2;
            TestSets["Stage3"] = Stage3;
            TestSets["Stage4"] = Stage4;

            prepare();
        }

        private void addStage1(Lab06TestCase s1aTestCase)
        {
            Stage1.TestCases.Add(s1aTestCase);
        }

        private void addStage2(Lab06TestCase s1bTestCase)
        {
            Stage2.TestCases.Add(s1bTestCase);
        }

        private void addStage3(Lab06TestCase s1aTestCase)
        {
            Stage3.TestCases.Add(s1aTestCase);
        }

        private void addStage4(Lab06TestCase s1bTestCase)
        {
            Stage4.TestCases.Add(s1bTestCase);
        }

        private void prepare()
        {
            int w;
            int l;
            int sila;
            int start;
            int max_skok;
            int v;
            int[,] lilie;

            w = 3;
            l = 3;
            sila = 2;
            lilie = new int[3,3] { { 1,0,1}, { 0,1,0}, { 1,0,1}};
            max_skok = 6;
            start = 1;
            v = 0;

            addStage1(new Stage1TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, expectedRes: 7, timeLimit: 10, description: "Przykład z zadania", checkR: false));
            addStage2(new Stage1TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, expectedRes: 7, timeLimit: 10, description: "Przykład z zadania", checkR: true));
            addStage3(new Stage2TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, v: v, expectedRes: 7, timeLimit: 10, description: "Przykład z zadania", checkR: false));
            addStage4(new Stage2TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, v: v, expectedRes: 7, timeLimit: 10, description: "Przykład z zadania", checkR: true));


            w = 2;
            l = 2;
            sila = 10;
            lilie = new int[2, 2] { { 1, 0 }, { 0, 1 } };
            max_skok = 4;
            start = 0;
            v = 0;
            addStage1(new Stage1TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, expectedRes: 4, timeLimit: 10, description: "Przykład prosty", checkR: false));
            addStage2(new Stage1TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, expectedRes: 4, timeLimit: 10, description: "Przykład prosty", checkR: true));
            addStage3(new Stage2TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, v: v, expectedRes: 4, timeLimit: 10, description: "Przykład prosty", checkR: false));
            addStage4(new Stage2TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, v: v, expectedRes: 4, timeLimit: 10, description: "Przykład prosty", checkR: true));

            w = 4;
            l = 8;
            sila = 10;
            lilie = new int[4, 8] { { 1, 0, 1, 0, 1, 0, 1, 0 },
                                    { 1, 0, 0, 1, 0, 0, 1, 0 }, 
                                    { 0, 0, 1, 1, 1, 1, 1, 1 },
                                    { 1, 0, 1, 1, 0, 0, 1, 1 }};
            max_skok = 6;
            start = 2;
            v = 0;
            addStage1(new Stage1TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, expectedRes: 6, timeLimit: 10, description: "Przykład większy", checkR: false));
            addStage2(new Stage1TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, expectedRes: 6, timeLimit: 10, description: "Przykład większy", checkR: true));
            addStage3(new Stage2TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, v: v, expectedRes: 6, timeLimit: 10, description: "Przykład większy", checkR: false));
            addStage4(new Stage2TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, v: v, expectedRes: 6, timeLimit: 10, description: "Przykład większy", checkR: true));

            w = 10;
            l = 2;
            sila = 5;
            lilie = new int[10, 2] { { 1, 0 }, { 1, 0 }, { 1, 0 }, { 1, 0 }, { 1, 0 }, { 1, 0 }, { 1, 0 }, { 1, 0 }, { 1, 0 }, { 1, 0 } };
            max_skok = 9;
            start = 0;
            v = 0;

            addStage1(new Stage1TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, expectedRes: 11, timeLimit: 10, description: "Przykład wąska rzeka", checkR: false));
            addStage2(new Stage1TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, expectedRes: 11, timeLimit: 10, description: "Przykład wąska rzeka", checkR: true));
            addStage3(new Stage2TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, v: v, expectedRes: 15, timeLimit: 10, description: "Przykład długie skoki", checkR: false));
            addStage4(new Stage2TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, v: v, expectedRes: 15, timeLimit: 10, description: "Przykład długie skoki", checkR: true));

            w = 8;
            l = 11;
            sila = 4;
            max_skok = 14;
            lilie = new int[8, 11] { { 1,0,0,0,0,0,0,0,0,0,0},
                                      {1,0,0,0,1,0,1,0,1,0,1},
                                      {1,0,0,0,1,0,0,0,0,0,0},
                                      {1,0,1,0,1,0,0,0,0,0,1},
                                      {0,0,0,0,0,0,0,0,0,0,0},
                                      {0,0,0,0,0,0,0,0,0,0,1},
                                      {0,0,0,0,0,0,0,0,0,0,0},
                                      {0,0,0,0,0,0,0,0,0,0,1}
            };
            start = 0;
            v = 0;

            addStage1(new Stage1TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, expectedRes: 39, timeLimit: 40, description: "Przykład skomplikowany", checkR: false));
            addStage2(new Stage1TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, expectedRes: 39, timeLimit: 40, description: "Przykład skomplikowany", checkR: true));
            addStage3(new Stage2TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, v: v, expectedRes: 41, timeLimit: 50, description: "Przykład skomplikowany", checkR: false));
            addStage4(new Stage2TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, v: v, expectedRes: 41, timeLimit: 50, description: "Przykład skomplikowany", checkR: true));

            w = 4;
            l = 7;
            sila = 2;
            lilie = new int[4, 7] { { 1,0,1,0,1,0,1},
                                    { 1,0,0,1,0,0,1},
                                    { 1,0,0,0,1,0,0},
                                    { 1,0,0,0,0,1,0} } ;
            max_skok = 8;
            start = 0;
            v = 0;
            addStage1(new Stage1TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, expectedRes: 5, timeLimit: 10, description: "Przykład szybka rzeka", checkR: false));
            addStage2(new Stage1TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, expectedRes: 5, timeLimit: 10, description: "Przykład szybka rzeka", checkR: true));
            addStage3(new Stage2TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, v: v, expectedRes: 5, timeLimit: 10, description: "Przykład szybka rzeka", checkR: false));
            addStage4(new Stage2TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, v: v, expectedRes: 5, timeLimit: 10, description: "Przykład szybka rzeka", checkR: true));

            Random rng = new Random(777);
            w = 20;
            l = 20;
            sila = 7;
            lilie = new int[20, 20];
            for(int i=0; i<w; i++)
            {
                for(int j=0; j<l; j++)
                {
                    lilie[i,j] = rng.NextDouble() > 0.9 ? 1 : 0;
                }
            }
            max_skok = 80;
            start = 10;
            v = 0;
            addStage1(new Stage1TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, expectedRes: 0, timeLimit: 10, description: "Przykład rzadka losowa rzeka", checkR: false));
            addStage2(new Stage1TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, expectedRes: 0, timeLimit: 10, description: "Przykład rzadka losowa rzeka", checkR: true));
            addStage3(new Stage2TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, v: v, expectedRes: 0, timeLimit: 10, description: "Przykład rzadka losowa rzeka", checkR: false));
            addStage4(new Stage2TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, v: v, expectedRes: 0, timeLimit: 10, description: "Przykład rzadka losowa rzeka", checkR: true));

            w = 10;
            l = 10;
            sila = 3;
            lilie = new int[10, 10];
            for(int i=0; i<w; i++)
            {
                for(int j=0; j<l; j++)
                {
                    lilie[i,j] = rng.NextDouble() > 0.6 ? 1 : 0;
                }
            }
            max_skok = 13;
            start = 5;
            v = 0;
            addStage1(new Stage1TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, expectedRes: 17, timeLimit: 10, description: "Przykład gęsta losowa rzeka", checkR: false));
            addStage2(new Stage1TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, expectedRes: 17, timeLimit: 10, description: "Przykład gęsta losowa rzeka", checkR: true));
            addStage3(new Stage2TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, v: v, expectedRes: 17, timeLimit: 10, description: "Przykład gęsta losowa rzeka", checkR: false));
            addStage4(new Stage2TestCase(w: w, l: l, lilies: lilie, sila: sila, start: start, max_skok: max_skok, v: v, expectedRes: 17, timeLimit: 10, description: "Przykład gęsta losowa rzeka", checkR: true));
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var tests = new Lab06Tests();
            tests.PrepareTestSets();
            foreach (var ts in tests.TestSets)
            {
                ts.Value.PerformTests(verbose: true, checkTimeLimit: false);
            }
        }
    }
}