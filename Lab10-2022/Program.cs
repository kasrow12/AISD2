using ASD;
using ASD.Graphs;
using ASD.Graphs.Testing;
using System;
using System.Linq;
using static ASD.TestCase;

namespace Lab10
{
    class Lab10Main
    {
        public static void Main(string[] args)
        {
            EggDeliveryTests tests = new EggDeliveryTests();
            tests.PrepareTestSets();
            foreach (var test in tests.TestSets)
            {
                test.Value.PerformTests(checkTimeLimit: false, verbose: true);
            }
        }
    }

    public enum TestCheck
    {
        ExistenceOnly,
        Full
    }

    public class EggDeliveryTestCase : TestCaze<DeliveryPlanner, TestCheck, (bool routeExists, int[] route)>
    {
        private readonly Graph<int> railway;
        private readonly int[] eggDemand;
        private readonly bool[] isFuelStation;
        private readonly int truckCapacity;
        private readonly int tankEngineRange;
        private readonly bool solutionExists;
        private readonly int? solutionLength;

        public EggDeliveryTestCase(Graph<int> railway, int[] eggDemand, bool[] isFuelStation, int truckCapacity, int tankEngineRange, bool solutionExists, int? solutionLength, bool checkLowEfficiency, double timeLimit, string description) : base(checkLowEfficiency, timeLimit, null, description)
        {
            this.railway = railway;
            this.eggDemand = eggDemand;
            this.isFuelStation = isFuelStation;
            this.truckCapacity = truckCapacity;
            this.tankEngineRange = tankEngineRange;
            this.solutionExists = solutionExists;
            this.solutionLength = solutionLength;
        }

        protected override (bool routeExists, int[] route) calculateResult(DeliveryPlanner prototype) => prototype.PlanDelivery(railway, eggDemand, truckCapacity, tankEngineRange, isFuelStation, !solutionLength.HasValue);


        protected override (Result resultCode, string message) VerifyResult((bool routeExists, int[] route) result, TestCheck settings)
        {
            if (result.routeExists != this.solutionExists)
            {
                return (Result.WrongResult, this.solutionExists ? "Zwrócono, że nie ma rozwiązania, choć istnieje" : "Zwrócono, że jest rozwiązanie, choć nie istnieje");
            }
            if (this.solutionExists && settings == TestCheck.Full)
            {
                if (result.route is null)
                {
                    return (Result.WrongResult, "Zwrócona tablica z rozwiązaniem jest nullem");
                }
                else
                {
                    return CheckSolution(result.route);
                }
            }
            else
            {
                return (Result.Success, "OK: Nie ma rozwiązania");
            }
        }

        private (Result resultCode, string message) CheckSolution(int[] route)
        {
            if (route.Length < 1) { return (Result.WrongResult, "Zwrócona tablica ma długość 0"); }
            else if (route[0] != 0) { return (Result.WrongResult, "Zwrócona tablica nie zaczyna się od wierzchołka 0"); }
            else if (route[route.Length - 1] != 0) { return (Result.WrongResult, "Zwrócona tablica nie kończy się w wierzchołku 0"); }
            else
            {
                int length = 0;
                int currentRange = tankEngineRange;
                int currentEggs = truckCapacity;
                bool[] visited = new bool[railway.VertexCount];
                for (int i = 0; i < route.Length - 1; i++)
                {
                    if (route[i] != 0)
                    {
                        visited[route[i]] = true;
                    }
                    if (!railway.HasEdge(route[i], route[i + 1]))
                    {
                        return (Result.WrongResult, $"Nie ma krawędzi pomiędzy wierzhołkami {route[i]} i {route[i + 1]}, które są obok siebie w zwróconej trasie");
                    }
                    else
                    {
                        int len = railway.GetEdgeWeight(route[i], route[i + 1]);
                        if (currentRange < len)
                        {
                            return (Result.WrongResult, $"Zabrakło zasięgu na drodze do {i + 1} elementu trasy");
                        }
                        else if (currentEggs < eggDemand[route[i + 1]])
                        {
                            return (Result.WrongResult, $"Zabrakło jajek na drodze do {i + 1} elementu trasy");
                        }
                        else if (visited[route[i + 1]])
                        {
                            return (Result.WrongResult, $"Wierzchołek {route[i + 1]} odwiedzony dwukrotnie");
                        }
                        else
                        {
                            length += len;
                            currentRange -= len;
                            currentEggs -= eggDemand[route[i + 1]];
                            if (route[i + 1] == 0) { currentEggs = truckCapacity; }
                            if (isFuelStation[route[i + 1]]) { currentRange = tankEngineRange; }
                        }
                    }
                }

                if (solutionLength.HasValue)
                {
                    if (solutionLength.Value == length)
                    {
                        return (Result.Success, "OK");
                    }
                    else
                    {
                        return (Result.WrongResult, $"Zwrócone rozwiązanie ma złą długość: {length}, a oczekiwane {solutionLength.Value}");
                    }
                }
                else
                {
                    return (Result.Success, "OK");
                }
            }
        }
    }


    public class EggDeliveryTests : TestModule
    {
        private readonly CheckedTestSet<DeliveryPlanner, TestCheck, EggDeliveryTestCase> anyRouteCheck = new CheckedTestSet<DeliveryPlanner, TestCheck, EggDeliveryTestCase>(new DeliveryPlanner(), "Istnienie jakiegokolwiek rozwiązania -- bez ścieżek", TestCheck.ExistenceOnly);
        private readonly CheckedTestSet<DeliveryPlanner, TestCheck, EggDeliveryTestCase> anyRouteCheckWithRoute = new CheckedTestSet<DeliveryPlanner, TestCheck, EggDeliveryTestCase>(new DeliveryPlanner(), "Istnienie jakiegokolwiek rozwiązania -- istnienie+ścieżka", TestCheck.Full);
        private readonly CheckedTestSet<DeliveryPlanner, TestCheck, EggDeliveryTestCase> bestRouteCheck = new CheckedTestSet<DeliveryPlanner, TestCheck, EggDeliveryTestCase>(new DeliveryPlanner(), "Znalezienie najlepszego możliwego rozwiązania", TestCheck.Full);

        public EggDeliveryTests()
        {
            TestSets.Add("Pierwszy etap, część 1 (tylko istnienie)", anyRouteCheck);
            TestSets.Add("Pierwszy etap, część 2", anyRouteCheckWithRoute);
            TestSets.Add("Drugi etap", bestRouteCheck);
        }

        public override void PrepareTestSets()
        {
            {
                var railway = new Graph<int>(9, new MatrixGraphRepresentation());
                for (int i = 1; i < 9; i++)
                {
                    railway.AddEdge(0, i, 1);
                    for (int j = i + 1; j < 9; j++)
                    {
                        railway.AddEdge(i, j, 5);
                    }
                }
                var demand = new int[9] { 0, 1, 1, 1, 1, 1, 1, 1, 1 };
                var isFuelStation = new bool[9] { true, false, false, false, false, false, false, false, false };
                var tankEngineRange = 6;
                var truckCapacity = 50;
                var bestSolutionLength = 16;
                var description = "Graf pełny, duże wagi krawędzie z końcami innymi niż 0";

                var tc1 = new EggDeliveryTestCase(railway, demand, isFuelStation, truckCapacity, tankEngineRange, true, null, true, 1, description);

                anyRouteCheck.AddTestCase(tc1);
                anyRouteCheckWithRoute.AddTestCase(tc1);
                bestRouteCheck.AddTestCase(new EggDeliveryTestCase(railway, demand, isFuelStation, truckCapacity, tankEngineRange, true, bestSolutionLength, true, 3, description));
            }

            {
                var railway = new Graph<int>(9, new MatrixGraphRepresentation());
                for (int i = 1; i < 9; i++)
                {
                    railway.AddEdge(0, i, 1);
                    for (int j = i + 1; j < 9; j++)
                    {
                        railway.AddEdge(i, j, 1);
                    }
                }
                var demand = new int[9] { 0, 3, 3, 3, 3, 3, 3, 3, 3 };
                var isFuelStation = new bool[9] { false, false, false, false, false, false, false, false, false };
                var tankEngineRange = 100;
                var truckCapacity = 5;
                var bestSolutionLength = 16;
                var description = "Graf pełny, trzeba wracać po każdej dostawie do 0 po jajka";

                var tc1 = new EggDeliveryTestCase(railway, demand, isFuelStation, truckCapacity, tankEngineRange, true, null, true, 1, description);

                anyRouteCheck.AddTestCase(tc1);
                anyRouteCheckWithRoute.AddTestCase(tc1);
                bestRouteCheck.AddTestCase(new EggDeliveryTestCase(railway, demand, isFuelStation, truckCapacity, tankEngineRange, true, bestSolutionLength, true, 3, description));
            }

            {
                var railway = new Graph<int>(30); // stiatka 6×5
                int replaceVNum(int v) // 0 jest mniej więcej na środku
                {
                    if (v == 0) return 14;
                    else if (v == 14) return 0;
                    else return v;
                }
                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        if (j < 4)
                        {
                            railway.AddEdge(replaceVNum(i * 5 + j), replaceVNum(i * 5 + j + 1), (i * 5 + j) % 2 == 0 ? 1 : 1);
                        }
                        if (i < 5)
                        {
                            railway.AddEdge(replaceVNum(i * 5 + j), replaceVNum((i + 1) * 5 + j), (i * 5 + j) % 2 == 0 ? 1 : 1);
                        }
                    }
                }

                var demand = new int[30];
                for (int i = 1; i < 30; i++) { demand[i] = 1; }
                var isFuelStation = new bool[30]; // narożniki
                isFuelStation[14] = true;
                isFuelStation[5] = true;
                isFuelStation[24] = true;
                isFuelStation[29] = true;

                var tankEngineRange = 10;
                var truckCapacity = 20;
                var bestSolutionLength = 30;
                var description = "Siatka możliwość tankowania tylko w narożnikach, ograniczenie na pojemność, nie ma rozwiązania";

                var tc1 = new EggDeliveryTestCase(railway, demand, isFuelStation, truckCapacity, tankEngineRange, false, null, true, 3, description);

                anyRouteCheck.AddTestCase(tc1);
                anyRouteCheckWithRoute.AddTestCase(tc1);
                bestRouteCheck.AddTestCase(new EggDeliveryTestCase(railway, demand, isFuelStation, truckCapacity, tankEngineRange, false, bestSolutionLength, true, 3, description));
            }

            {
                var railway = new Graph<int>(30); // stiatka 6×5
                int replaceVNum(int v) // 0 jest mniej więcej na środku
                {
                    if (v == 0) return 14;
                    else if (v == 14) return 0;
                    else return v;
                }
                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        if (j < 4)
                        {
                            railway.AddEdge(replaceVNum(i * 5 + j), replaceVNum(i * 5 + j + 1), (i * 5 + j) % 2 == 0 ? 1 : 1);
                        }
                        if (i < 5)
                        {
                            railway.AddEdge(replaceVNum(i * 5 + j), replaceVNum((i + 1) * 5 + j), (i * 5 + j) % 2 == 0 ? 1 : 1);
                        }
                    }
                }

                railway.AddEdge(0, 18, 1);

                var demand = new int[30];
                for (int i = 1; i < 30; i++) { demand[i] = 1; }
                var isFuelStation = new bool[30]; // narożniki
                isFuelStation[14] = true;
                isFuelStation[5] = true;
                isFuelStation[24] = true;
                isFuelStation[29] = true;

                var tankEngineRange = 10;
                var truckCapacity = 20;
                var bestSolutionLength = 31;
                var description = "Siatka możliwość tankowania tylko w narożnikach, ograniczenie na pojemność, dodatkowa krawędź";

                var tc1 = new EggDeliveryTestCase(railway, demand, isFuelStation, truckCapacity, tankEngineRange, true, null, true, 6, description);

                anyRouteCheck.AddTestCase(tc1);
                anyRouteCheckWithRoute.AddTestCase(tc1);
                bestRouteCheck.AddTestCase(new EggDeliveryTestCase(railway, demand, isFuelStation, truckCapacity, tankEngineRange, true, bestSolutionLength, true, 7, description));
            }


            {
                var railway = new Graph<int>(30); // stiatka 6×5
                int replaceVNum(int v) // 0 jest mniej więcej na środku
                {
                    if (v == 0) return 14;
                    else if (v == 14) return 0;
                    else return v;
                }
                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        if (j < 4)
                        {
                            railway.AddEdge(replaceVNum(i * 5 + j), replaceVNum(i * 5 + j + 1), (i * 5 + j) % 2 == 0 ? 1 : 1);
                        }
                        if (i < 5)
                        {
                            railway.AddEdge(replaceVNum(i * 5 + j), replaceVNum((i + 1) * 5 + j), (i * 5 + j) % 2 == 0 ? 1 : 1);
                        }
                    }
                }

                var demand = new int[30];
                for (int i = 1; i < 30; i++) { demand[i] = 1; }
                var isFuelStation = new bool[30]; // narożniki
                isFuelStation[14] = true;
                isFuelStation[5] = true;
                isFuelStation[24] = true;
                isFuelStation[29] = true;

                var tankEngineRange = 10;
                var truckCapacity = 50;
                var bestSolutionLength = 30;
                var description = "Siatka możliwość tankowania tylko w narożnikach";

                var tc1 = new EggDeliveryTestCase(railway, demand, isFuelStation, truckCapacity, tankEngineRange, true, null, true, 1, description);

                anyRouteCheck.AddTestCase(tc1);
                anyRouteCheckWithRoute.AddTestCase(tc1);
                bestRouteCheck.AddTestCase(new EggDeliveryTestCase(railway, demand, isFuelStation, truckCapacity, tankEngineRange, true, bestSolutionLength, true, 4, description));
            }

            {
                var railway = new Graph<int>(30); // stiatka 6×5
                int replaceVNum(int v) // 0 jest mniej więcej na środku
                {
                    if (v == 0) return 14;
                    else if (v == 14) return 0;
                    else return v;
                }
                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        if (j < 4)
                        {
                            railway.AddEdge(replaceVNum(i * 5 + j), replaceVNum(i * 5 + j + 1), (i * 5 + j) % 2 == 0 ? 2 : 1);
                        }
                        if (i < 5)
                        {
                            railway.AddEdge(replaceVNum(i * 5 + j), replaceVNum((i + 1) * 5 + j), (i * 5 + j) % 2 == 0 ? 1 : 3);
                        }
                    }
                }

                var demand = new int[30];
                for (int i = 1; i < 30; i++) { demand[i] = 1; }
                var isFuelStation = new bool[30]; // narożniki
                isFuelStation[14] = true;
                isFuelStation[5] = true;
                isFuelStation[24] = true;
                isFuelStation[29] = true;

                var tankEngineRange = 16;
                var truckCapacity = 50;
                var bestSolutionLength = 44;
                var description = "Siatka możliwość tankowania tylko w narożnikach, zróżnicowane wagi krawędzi";

                var tc1 = new EggDeliveryTestCase(railway, demand, isFuelStation, truckCapacity, tankEngineRange, true, null, true, 2, description);

                anyRouteCheck.AddTestCase(tc1);
                anyRouteCheckWithRoute.AddTestCase(tc1);
                bestRouteCheck.AddTestCase(new EggDeliveryTestCase(railway, demand, isFuelStation, truckCapacity, tankEngineRange, true, bestSolutionLength, true, 2, description));
            }


            {
                var railway = new Graph<int>(30); // stiatka 6×5
                int replaceVNum(int v) // 0 jest mniej więcej na środku
                {
                    if (v == 0) return 14;
                    else if (v == 14) return 0;
                    else return v;
                }
                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        if (j < 4)
                        {
                            railway.AddEdge(replaceVNum(i * 5 + j), replaceVNum(i * 5 + j + 1), (i * 5 + j) % 2 == 0 ? 2 : 1);
                        }
                        if (i < 5)
                        {
                            railway.AddEdge(replaceVNum(i * 5 + j), replaceVNum((i + 1) * 5 + j), (i * 5 + j) % 2 == 0 ? 1 : 3);
                        }
                    }
                }

                var demand = new int[30];
                for (int i = 1; i < 30; i++) { demand[i] = 1; }
                var isFuelStation = new bool[30]; // narożniki
                isFuelStation[14] = true;
                isFuelStation[5] = true;
                isFuelStation[24] = true;
                isFuelStation[29] = true;

                var tankEngineRange = 15;
                var truckCapacity = 50;
                var description = "Siatka możliwość tankowania tylko w narożnikach, zróżnicowane wagi krawędzi, z mały zasięg";

                var tc1 = new EggDeliveryTestCase(railway, demand, isFuelStation, truckCapacity, tankEngineRange, false, null, true, 2, description);

                anyRouteCheck.AddTestCase(tc1);
                anyRouteCheckWithRoute.AddTestCase(tc1);
                bestRouteCheck.AddTestCase(new EggDeliveryTestCase(railway, demand, isFuelStation, truckCapacity, tankEngineRange, false, null, true, 2, description));
            }
            {
                RandomGraphGenerator rgg = new RandomGraphGenerator(1503);
                Random rand = new Random(1503);
                Graph<int> railway = rgg.AssignWeights(rgg.Graph(20, 0.15), 1, 10);
                var demand = new int[20];
                var isFuelStation = new bool[20];
                for (int i = 0; i < 20; i++)
                {
                    if (i != 0)
                    {
                        demand[i] = rand.Next(1, 10);
                    }
                    isFuelStation[i] = rand.NextDouble() < 0.15;
                }
                var tankEngineRange = 25;
                var truckCapacity = 25;
                var description = "Losowy graf";
                var tc1 = new EggDeliveryTestCase(railway, demand, isFuelStation, truckCapacity, tankEngineRange, false, null, true, 2, description);

                anyRouteCheck.AddTestCase(tc1);
                anyRouteCheckWithRoute.AddTestCase(tc1);
                bestRouteCheck.AddTestCase(new EggDeliveryTestCase(railway, demand, isFuelStation, truckCapacity, tankEngineRange, false, null, true, 2, description));
            }
            {
                RandomGraphGenerator rgg = new RandomGraphGenerator(-44);
                Random rand = new Random(-44);
                int graphsize = 15;
                Graph<int> railway = rgg.AssignWeights(rgg.Graph(graphsize, 0.3), 1, 10);
                var demand = new int[graphsize];
                var isFuelStation = new bool[graphsize];
                for (int i = 0; i < graphsize; i++)
                {
                    if (i != 0)
                    {
                        demand[i] = rand.Next(1, 10);
                    }
                    isFuelStation[i] = rand.NextDouble() < 0.15;
                }
                var tankEngineRange = 100;
                var truckCapacity = 150;
                var description = "Losowy graf 2";
                var solutionLength = 74;
                var tc1 = new EggDeliveryTestCase(railway, demand, isFuelStation, truckCapacity, tankEngineRange, true, null, true, 2, description);

                anyRouteCheck.AddTestCase(tc1);
                anyRouteCheckWithRoute.AddTestCase(tc1);
                bestRouteCheck.AddTestCase(new EggDeliveryTestCase(railway, demand, isFuelStation, truckCapacity, tankEngineRange, true, solutionLength, true, 2, description));
            }
            {
                RandomGraphGenerator rgg = new RandomGraphGenerator(3141);
                Random rand = new Random(3141);
                int graphsize = 15;
                Graph<int> railway = new Graph<int>(graphsize);
                for (int perm = 0; perm < 3; perm++)
                {
                    var seq = Enumerable.Range(0, graphsize).OrderBy(c => rand.Next()).ToArray();
                    for (int i = 0; i < graphsize; i++)
                    {
                        railway.AddEdge(seq[i], seq[(i + 1) % graphsize], rand.Next(1, 5));
                    }
                }


                var demand = new int[graphsize];
                var isFuelStation = new bool[graphsize];
                for (int i = 0; i < graphsize; i++)
                {
                    if (i != 0)
                    {
                        demand[i] = rand.Next(1, 3);
                    }
                    isFuelStation[i] = rand.NextDouble() < 0.15;
                }
                var tankEngineRange = 30;
                var truckCapacity = 30;
                var description = "Losowy graf 3";
                var solutionLength = 29;
                var tc1 = new EggDeliveryTestCase(railway, demand, isFuelStation, truckCapacity, tankEngineRange, true, null, true, 2, description);

                anyRouteCheck.AddTestCase(tc1);
                anyRouteCheckWithRoute.AddTestCase(tc1);
                bestRouteCheck.AddTestCase(new EggDeliveryTestCase(railway, demand, isFuelStation, truckCapacity, tankEngineRange, true, solutionLength, true, 2, description));
            }
        }

        public override double ScoreResult()
        {
            throw new NotImplementedException();
        }
    }

}

namespace ASD
{
    public abstract class CheckedTestCaze<Prototype, Settings> : TestCase
    {
        protected CheckedTestCaze(double timeLimit, Exception expectedException, string description) : base(timeLimit, expectedException, description)
        {
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings) =>
        VerifyTestCaseChecked((Settings)settings);

        protected override void PerformTestCase(object prototypeObject) =>
            PerformTestCaseChecked((Prototype)prototypeObject);

        protected abstract (Result resultCode, string message) VerifyTestCaseChecked(Settings settings);
        protected abstract void PerformTestCaseChecked(Prototype prototypeObject);
    }

    public abstract class OptionalResultTextCaze<Prototype, ResultType, ExpectedResultInfo, Settings> : TestCaze<Prototype, Settings, ResultType?> where ResultType : struct where ExpectedResultInfo : struct
    {
        private ExpectedResultInfo? expectedResultInfo;

        protected OptionalResultTextCaze(ExpectedResultInfo? expectedResultInfo, bool checkLowEfficiency, double timeLimit, Exception expectedException, string description) : base(checkLowEfficiency, timeLimit, expectedException, description)
        {
            this.expectedResultInfo = expectedResultInfo;
        }

        protected abstract (Result resultCode, string message) VerityResult(ResultType result, ExpectedResultInfo expectedResultInfo, Settings settings);

        protected override (Result resultCode, string message) VerifyResult(ResultType? result, Settings settings)
        {
            if (expectedResultInfo.HasValue && result.HasValue)
            {
                return VerityResult(result.Value, expectedResultInfo.Value, settings);
            }
            else if (!expectedResultInfo.HasValue && !result.HasValue)
            {
                return (Result.Success, "OK. Nie ma rozwiązania i nie zwrócono");
            }
            else if (!expectedResultInfo.HasValue && result.HasValue)
            {
                return (Result.WrongResult, "Zwrócono rozwiązanie, choć nie istnieje");
            }
            else
            {
                return (Result.WrongResult, "Nie zwrócono rozwiązania, choś istnieje");
            }
        }
    }

    public abstract class TestCaze<Prototype, SettingsType, ResultType> : CheckedTestCaze<Prototype, SettingsType>
    {
        private ResultType res;
        private readonly bool checkLowEfficiency;

        protected TestCaze(bool checkLowEfficiency, double timeLimit, Exception expectedException, string description) : base(timeLimit, expectedException, description)
        {
            this.checkLowEfficiency = checkLowEfficiency;
        }

        protected abstract (Result resultCode, string message) VerifyResult(ResultType result, SettingsType settings);

        private String extendMessage(String message) =>
            $"{message} {PerformanceTime} [{Description}]";

        private Result lowEffCheck(Result code)
        {
            if (code == Result.Success && PerformanceTime > TimeLimit)
                return Result.LowEfficiency;
            else
                return code;
        }

        protected abstract ResultType calculateResult(Prototype prototype);

        protected override void PerformTestCaseChecked(Prototype prototypeObject)
        {
            res = calculateResult((Prototype)prototypeObject);
        }


        protected override (Result resultCode, string message) VerifyTestCaseChecked(SettingsType settings)
        {
            var (code, msg) = VerifyResult(res, (SettingsType)settings);
            return (lowEffCheck(code), extendMessage(msg));
        }
    }

    public class CheckedTestSet<Prototype, Settings, Case> : TestSet where Case : CheckedTestCaze<Prototype, Settings>
    {
        public CheckedTestSet(Prototype prototypeObject,
                              string description,
                              Settings settings,
                  Action speedFactorMeasureFunction = null,
                              int stackSize = 1) : base(prototypeObject, description, speedFactorMeasureFunction,
                                                        settings, stackSize)
        {
        }

        /// <summary>Dodawanie nowego przypadku testowego. Z racji tego, że TestCases jest publicznym polem
        /// nie ma sposobu, żeby zasłonić TestCases.Add. Ale sugestia jest taka, żeby go nie używać.</summary>
        /// <param name="tcase">Przypadek do dodania</param>
        public void AddTestCase(Case tcase)
        {
            TestCases.Add(tcase);
        }

        public bool CheckTestSet(int allowedNotPassed, int allowedTimeout, int allowedLowEfficiency) =>
                PassedCount + allowedNotPassed >= TestCases.Count && TimeoutsCount <= allowedTimeout && LowEfficiencyCount <= allowedLowEfficiency;

        public bool CheckTestSet(int allowedNotPassed, int allowedTimeoutAndLowEfficiency) =>
                PassedCount + allowedNotPassed >= TestCases.Count && TimeoutsCount + LowEfficiencyCount <= allowedTimeoutAndLowEfficiency;

    }
}
