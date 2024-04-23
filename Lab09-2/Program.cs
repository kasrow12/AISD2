using System;
using System.Collections.Generic;
using System.Linq;
using ASD.Graphs;
using ASD.Graphs.Testing;

namespace ASD
{
    class ElectionCampaignTestCase : TestCase
    {
        private readonly Graph<int> _cities;
        private readonly int[] _citiesPopulation;
        private readonly double[] _meetingCosts;
        private readonly double _budget;
        private readonly int _capitalCity;
        private readonly int _bestCampaignPeopleMet;
        private readonly int _bestCampaignCost;

        // Cloned properties to verify that input data has not been modified
        private readonly Graph _citiesClone;
        private readonly int[] _citiesPopulationClone;
        private readonly double[] _meetingCostsClone;

        private int _peopleMet;
        private (int city, bool shouldOrganizeMeeting)[] _path;

        public ElectionCampaignTestCase(double timeLimit, Exception expectedException, string description,
            Graph<int> cities, int[] citiesPopulation, double[] meetingCosts, double budget,
            int capitalCity, int bestCampaignPeopleMet, int bestCampaignCost)
            : base(timeLimit, expectedException, description)
        {
            _cities = cities;
            _citiesPopulation = citiesPopulation;
            _meetingCosts = meetingCosts;
            _budget = budget;
            _capitalCity = capitalCity;
            _bestCampaignPeopleMet = bestCampaignPeopleMet;
            _bestCampaignCost = bestCampaignCost;

            _citiesClone = (Graph<int>)_cities.Clone();
            _citiesPopulationClone = _citiesPopulation.ToArray();
            _meetingCostsClone = _meetingCosts.ToArray();
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            _peopleMet = ((Lab08)prototypeObject).ComputeElectionCampaignPath(_cities, _citiesPopulation,
                _meetingCosts, _budget, _capitalCity, out _path);
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            var foundCampaignPeopleMet = 0;
            var budgetLeft = _budget;
            var visitedCities = new HashSet<int>();

            if (!_cities.Equals(_citiesClone))
                return (Result.WrongResult, "input graph has been modified");
            if (!_citiesPopulation.SequenceEqual(_citiesPopulationClone))
                return (Result.WrongResult, "cities populations have been modified");
            if (!_meetingCosts.SequenceEqual(_meetingCostsClone))
                return (Result.WrongResult, "meeting costs have been modified");

            if (_peopleMet != _bestCampaignPeopleMet)
                return (Result.WrongResult, $"Wrong result: {_peopleMet} (expected {_bestCampaignPeopleMet})");

            if (_path == null)
                return (Result.WrongResult, $"path was null");

            if (_path.Length == 0)
                return (Result.WrongResult, $"the path is empty (it's length is 0)"); // the path should always contain at least the capital

            for (var i = 0; i < _path.Length; i++)
            {
                // Make sure the vertices are valid graph vertices
                var cityValidationError = VerifyValidVertex(_path[i].city, i);

                if (cityValidationError.HasValue)
                    return cityValidationError.Value;
            }

            {
                // Start in the capital
                var initialCity = _path[0];
                visitedCities.Add(initialCity.city);

                if (initialCity.city != _capitalCity)
                    return (Result.WrongResult, $"Path does not start in the capital city ({_capitalCity})");

                if (initialCity.shouldOrganizeMeeting)
                {
                    foundCampaignPeopleMet = _citiesPopulation[initialCity.city];
                    budgetLeft -= _meetingCosts[initialCity.city];

                    if (budgetLeft < 0)
                        return (Result.WrongResult, $"Not enough money to organize a meeting in city {initialCity.city}");
                }
            }

            for (var i = 1; i < _path.Length; i++)
            {
                // Travel between cities

                var fromCity = _path[i - 1];
                var toCity = _path[i];

                if (visitedCities.Contains(toCity.city))
                    return (Result.WrongResult, $"City {toCity.city} is visited twice");
                visitedCities.Add(toCity.city);

                if(!_cities.HasEdge(fromCity.city, toCity.city))
                    return (Result.WrongResult, $"The solution tries to use an edge that does not exist (between cities {fromCity.city} and {toCity.city})");

                var travelCost = _cities.GetEdgeWeight(fromCity.city, toCity.city);

                budgetLeft -= travelCost;
                if (budgetLeft < 0)
                    return (Result.WrongResult, $"Not enough budget to complete the path (cannot travel from city {fromCity.city} to {toCity.city})");

                if (toCity.shouldOrganizeMeeting)
                {
                    foundCampaignPeopleMet += _citiesPopulation[toCity.city];
                    budgetLeft -= _meetingCosts[toCity.city];

                    if (budgetLeft < 0)
                        return (Result.WrongResult, $"Not enough money to organize a meeting in city {toCity.city}");
                }
            }

            if (_path.Length > 1)
            {
                // Travel from the last city to the capital city
                var lastCity = _path.Last();

                if (!_cities.HasEdge(lastCity.city, _capitalCity))
                    return (Result.WrongResult, $"Edge from the last city on the path ({lastCity.city}) to the capital does not exist");
                var travelCost = _cities.GetEdgeWeight(lastCity.city, _capitalCity);

                budgetLeft -= travelCost;
                if (budgetLeft < 0)
                    return (Result.WrongResult, $"Not enough budget to complete the path, cannot travel back to the capital (from {lastCity.city} to {_capitalCity})");
            }

            if (foundCampaignPeopleMet != _peopleMet)
                return (Result.WrongResult, $"Wrong path: {foundCampaignPeopleMet} people met on returned path (expected {_bestCampaignPeopleMet})");

            {
                // Verify that the least expensive path is returned
                var pathCost = _budget - budgetLeft;
                if (pathCost != _bestCampaignCost)
                    return (Result.WrongResult, $"Wrong path cost: {pathCost} (expected {_bestCampaignCost})");
            }

            return (Result.Success, $"OK, {PerformanceTime:#0.00}");
        }

        private (Result resultCode, string message)? VerifyValidVertex(int vertex, int index)
        {
            if (vertex < 0)
                return (Result.WrongResult, $"Negative vertex used ({vertex}) at index {index}");

            if (vertex >= _cities.VertexCount)
                return (Result.WrongResult, $"Vertex exceeds graph size ({vertex}) at index {index}");

            return null;
        }
    }

    class ElectionCampaignTester : TestModule
    {
        private const int TIME_MULTIPLIER = 1;

        public override void PrepareTestSets()
        {
            var freeMeetingsTestSet = new TestSet(new Lab08(), "Część 1 - bez kosztów organizacji spotkań (1.5 pkt)", null, null, 2);
            var meetingsWithCostsTestSet = new TestSet(new Lab08(), "Część 2 - z kosztami organizacji spotkań (1.5 pkt)", null, null, 2);

            var performanceFreeMeetingsTestSet = new TestSet(new Lab08(), "Część 1 (wydajnościowe) - bez kosztów organizacji spotkań (0.5 pkt)", null, null, 2);
            var performanceMeetingsWithCostsTestSet = new TestSet(new Lab08(), "Część 2 (wydajnościowe) - z kosztami organizacji spotkań (0.5 pkt)", null, null, 2);

            TestSets["ElectionCampaignFreeMeetings"] = freeMeetingsTestSet;
            TestSets["ElectionCampaignMeetingsWithCosts"] = meetingsWithCostsTestSet;
            TestSets["ElectionCampaignEfficiencyFreeMeetings"] = performanceFreeMeetingsTestSet;
            TestSets["ElectionCampaignEfficiencyMeetingsWithCosts"] = performanceMeetingsWithCostsTestSet;

            var freeMeetingsTestCases = freeMeetingsTestSet.TestCases;
            var meetingsWithCostsTestCases = meetingsWithCostsTestSet.TestCases;
            var performanceFreeMeetingsTestCases = performanceFreeMeetingsTestSet.TestCases;
            var performanceMeetingsWithCostsTestCases = performanceMeetingsWithCostsTestSet.TestCases;

            #region Correctness tests
            {
                var cities = new Graph<int>(4);
                var populations = new int[4] { 1200, 1300, 2000, 4000 };
                var meetingCosts = new double[4] { 1, 1, 1, 4 };
                cities.AddEdge(0, 1, 2);
                cities.AddEdge(0, 3, 6);
                cities.AddEdge(1, 2, 2);
                cities.AddEdge(1, 3, 6);
                cities.AddEdge(2, 3, 6);

                var description = "Prosty mały graf";
                freeMeetingsTestCases.Add(new ElectionCampaignTestCase(TIME_MULTIPLIER, null, description, cities,
                    populations, meetingCosts: new double[4], budget: 5, capitalCity: 2, bestCampaignPeopleMet: 3300, bestCampaignCost: 4));
                meetingsWithCostsTestCases.Add(new ElectionCampaignTestCase(TIME_MULTIPLIER, null, description, cities,
                    populations, meetingCosts, budget: 7, capitalCity: 2, bestCampaignPeopleMet: 3300, bestCampaignCost: 6));
            }

            {
                var cities = new Graph<int>(1);
                var populations = new int[1] { 2000 };
                var meetingCosts = new double[1] { 20 };

                freeMeetingsTestCases.Add(new ElectionCampaignTestCase(TIME_MULTIPLIER, null, "Wierzchołek izolowany", cities,
                    populations, meetingCosts: new double[1], budget: 10, capitalCity: 0, bestCampaignPeopleMet: 2000, bestCampaignCost: 0));
                meetingsWithCostsTestCases.Add(new ElectionCampaignTestCase(TIME_MULTIPLIER, null, "Wierzchołek izolowany, brak budżetu na spotkanie", cities,
                    populations, meetingCosts, budget: 10, capitalCity: 0, bestCampaignPeopleMet: 0, bestCampaignCost: 0));
            }

            {
                var cities = new Graph<int>(1);
                var populations = new int[1] { 2000 };
                var meetingCosts = new double[1] { 20 };

                var description = "Wierzchołek izolowany, jest budżet na spotkanie";
                meetingsWithCostsTestCases.Add(new ElectionCampaignTestCase(TIME_MULTIPLIER, null, description, cities,
                    populations, meetingCosts, budget: 20, capitalCity: 0, bestCampaignPeopleMet: 2000, bestCampaignCost: 20));
            }

            {
                var cities = new Graph<int>(3);
                cities.AddEdge(0, 1, 5);
                cities.AddEdge(0, 2, 5);
                cities.AddEdge(1, 2, 5);
                var meetingCosts = new double[3] { 1, 1, 1 };
                var populations = new int[3] { 1, 1, 1 };

                var description = "Klika 3, brak budżetu";
                freeMeetingsTestCases.Add(new ElectionCampaignTestCase(TIME_MULTIPLIER, null, description, cities,
                    populations, meetingCosts: new double[3], budget: 0, capitalCity: 0, bestCampaignPeopleMet: 1, bestCampaignCost: 0));
                meetingsWithCostsTestCases.Add(new ElectionCampaignTestCase(TIME_MULTIPLIER, null, description, cities,
                    populations, meetingCosts, budget: 0, capitalCity: 0, bestCampaignPeopleMet: 0, bestCampaignCost: 0));
            }

            {
                // Drzewo
                var rgg = new RandomGraphGenerator(7531);
                var rng = new Random(7531);
                var N = 25;
                var cities = rgg.AssignWeights(rgg.Tree(N), 1, 3);
                var meetingCosts = new double[N];
                var populations = new int[N];
                for (int i = 0; i < populations.Length; i++)
                {
                    populations[i] = rng.Next(1000, 5000);
                    meetingCosts[i] = rng.Next(1, 10);
                }

                var description = $"Drzewo o {N} wierzchołkach";
                freeMeetingsTestCases.Add(new ElectionCampaignTestCase(TIME_MULTIPLIER, null, description, cities,
                    populations, meetingCosts: new double[N], budget: 40, capitalCity: 0, bestCampaignPeopleMet: 4303, bestCampaignCost: 6));
                meetingsWithCostsTestCases.Add(new ElectionCampaignTestCase(TIME_MULTIPLIER, null, description, cities,
                    populations, meetingCosts, budget: 10, capitalCity: 0, bestCampaignPeopleMet: 2858, bestCampaignCost: 8));
            }

            {
                // Graf niespójny
                const int N = 10;
                var cities = new Graph<int>(N);

                cities.AddEdge(0, 1, 5);
                cities.AddEdge(0, 2, 1);
                cities.AddEdge(1, 2, 4);
                cities.AddEdge(2, 3, 3);
                cities.AddEdge(2, 4, 8);
                cities.AddEdge(3, 4, 10);

                cities.AddEdge(5, 6, 1);
                cities.AddEdge(5, 7, 1);
                cities.AddEdge(5, 8, 1);
                cities.AddEdge(5, 9, 1);
                cities.AddEdge(6, 8, 5);
                cities.AddEdge(6, 9, 4);
                cities.AddEdge(8, 9, 3);

                var meetingCosts = new double[N] { 1, 1, 1, 1, 1, 7, 4, 2, 6, 1 };
                var populations = new int[N] { 1, 1, 1, 1, 1, 10, 81, 20, 56, 35 };

                var description = "Mały graf niespójny";
                freeMeetingsTestCases.Add(new ElectionCampaignTestCase(TIME_MULTIPLIER, null, description, cities,
                    populations, meetingCosts: new double[N], budget: 20, capitalCity: 5, bestCampaignPeopleMet: 182, bestCampaignCost: 9));
                meetingsWithCostsTestCases.Add(new ElectionCampaignTestCase(TIME_MULTIPLIER, null, description, cities,
                    populations, meetingCosts, budget: 20, capitalCity: 5, bestCampaignPeopleMet: 172, bestCampaignCost: 20));
            }

            {
                // Losowy graf, budżet na spotkanie tylko w stolicy
                var rgg = new RandomGraphGenerator(12345);
                var N = 50;
                var cities = rgg.WeightedGraph(N, 0.8, 2, 5);
                var meetingCosts = new double[N];
                var populations = new int[N];
                populations[0] = 5000;
                meetingCosts[0] = 1;
                for (int i = 1; i < N; i++)
                {
                    populations[i] = 4000;
                    meetingCosts[i] = 2;
                }

                var description = "Budżet na spotkanie tylko w stolicy";
                freeMeetingsTestCases.Add(new ElectionCampaignTestCase(TIME_MULTIPLIER, null, description, cities,
                    populations, meetingCosts: new double[N], budget: 1, capitalCity: 0, bestCampaignPeopleMet: 5000, bestCampaignCost: 0));
                meetingsWithCostsTestCases.Add(new ElectionCampaignTestCase(TIME_MULTIPLIER, null, description, cities,
                    populations, meetingCosts, budget: 1, capitalCity: 0, bestCampaignPeopleMet: 5000, bestCampaignCost: 1));
            }

            {
                // Graf z wartym do odwiedzenia cyklem
                var cities = new Graph<int>(5);
                cities.AddEdge(0, 1, 1);
                cities.AddEdge(0, 2, 1);
                cities.AddEdge(1, 2, 1);
                cities.AddEdge(1, 3, 4);
                cities.AddEdge(3, 4, 4);
                cities.AddEdge(2, 4, 4);

                var meetingCosts = new double[5] { 1, 1, 1, 1, 1 };
                var populations = new int[5] { 1000, 1000, 1000, 500, 500 };

                var description = "Graf z wartym do odwiedzenia cyklem";
                freeMeetingsTestCases.Add(new ElectionCampaignTestCase(TIME_MULTIPLIER, null, description, cities,
                    populations, meetingCosts: new double[5], budget: 50, capitalCity: 0, bestCampaignPeopleMet: 4000, bestCampaignCost: 14));
                meetingsWithCostsTestCases.Add(new ElectionCampaignTestCase(TIME_MULTIPLIER, null, description, cities,
                    populations, meetingCosts, budget: 6, capitalCity: 0, bestCampaignPeopleMet: 3000, bestCampaignCost: 6));
            }

            {
                // Graf z dwoma cyklami o równej populacji ale różnym koszcie
                var cities = new Graph<int>(5);
                cities.AddEdge(0, 1, 5);
                cities.AddEdge(0, 2, 5);
                cities.AddEdge(1, 2, 5);

                cities.AddEdge(0, 3, 1);
                cities.AddEdge(0, 4, 1);
                cities.AddEdge(3, 4, 1);

                var meetingCosts = new double[5] { 1, 1, 1, 1, 1 };
                var populations = new int[5] { 100, 100, 100, 100, 100 };

                var description = "Graf z dwoma cyklami o równej populacji ale różnym koszcie";
                freeMeetingsTestCases.Add(new ElectionCampaignTestCase(TIME_MULTIPLIER, null, description, cities,
                    populations, meetingCosts: new double[5], budget: 100, capitalCity: 0, bestCampaignPeopleMet: 300, bestCampaignCost: 3));
                meetingsWithCostsTestCases.Add(new ElectionCampaignTestCase(TIME_MULTIPLIER, null, description, cities,
                    populations, meetingCosts, budget: 100, capitalCity: 0, bestCampaignPeopleMet: 300, bestCampaignCost: 6));
            }

            {
                // Losowy graf
                var rgg = new RandomGraphGenerator(2649);
                var rng = new Random(2649);
                var N = 20;
                var cities = rgg.WeightedGraph(N, 0.7, 1, 20);
                var meetingCosts = new double[N];
                var populations = new int[N];
                for (int i = 0; i < populations.Length; i++)
                {
                    populations[i] = rng.Next(1000, 5000);
                    meetingCosts[i] = rng.Next(1, 10);
                }

                var description = "Losowy graf";
                freeMeetingsTestCases.Add(new ElectionCampaignTestCase(2 * TIME_MULTIPLIER, null, description, cities,
                    populations, meetingCosts: new double[N], budget: 25, capitalCity: 0, bestCampaignPeopleMet: 37031, bestCampaignCost: 24));
                meetingsWithCostsTestCases.Add(new ElectionCampaignTestCase(3 * TIME_MULTIPLIER, null, description, cities,
                    populations, meetingCosts, budget: 25, capitalCity: 0, bestCampaignPeopleMet: 19402, bestCampaignCost: 24));
            }
            #endregion

            #region Performance tests
            {
                // Losowy gęsty graf
                var rgg = new RandomGraphGenerator(2650);
                var rng = new Random(2650);
                var N = 25;
                var cities = rgg.WeightedGraph(N, 0.9, 1, 20);
                var meetingCosts = new double[N];
                var populations = new int[N];
                for (int i = 0; i < populations.Length; i++)
                {
                    populations[i] = rng.Next(1000, 5000);
                    meetingCosts[i] = rng.Next(1, 10);
                }

                var description = "Losowy gęsty graf";
                performanceFreeMeetingsTestCases.Add(new ElectionCampaignTestCase(15 * TIME_MULTIPLIER, null, description, cities,
                    populations, meetingCosts: new double[N], budget: 25, capitalCity: 0, bestCampaignPeopleMet: 45097, bestCampaignCost: 25));
                performanceMeetingsWithCostsTestCases.Add(new ElectionCampaignTestCase(50 * TIME_MULTIPLIER, null, description, cities,
                    populations, meetingCosts, budget: 25, capitalCity: 0, bestCampaignPeopleMet: 12843, bestCampaignCost: 25));
            }

            {
                // Losowy rzadki graf
                var rgg = new RandomGraphGenerator(2650);
                var rng = new Random(2650);
                var N = 500;
                var cities = rgg.WeightedGraph(N, 0.3, 5, 20);
                var meetingCosts = new double[N];
                var populations = new int[N];
                for (int i = 0; i < populations.Length; i++)
                {
                    populations[i] = rng.Next(1000, 5000);
                    meetingCosts[i] = rng.Next(5, 15);
                }

                var description = "Losowy rzadki graf";
                performanceFreeMeetingsTestCases.Add(new ElectionCampaignTestCase(60 * TIME_MULTIPLIER, null, description, cities,
                    populations, meetingCosts: new double[N], budget: 22, capitalCity: 0, bestCampaignPeopleMet: 16043, bestCampaignCost: 22));
                performanceMeetingsWithCostsTestCases.Add(new ElectionCampaignTestCase(60 * TIME_MULTIPLIER, null, description, cities,
                    populations, meetingCosts, budget: 22, capitalCity: 0, bestCampaignPeopleMet: 4998, bestCampaignCost: 20));
            }

            {
                // Losowy średnio-gęsty graf
                var rgg = new RandomGraphGenerator(2651);
                var rng = new Random(2651);
                var N = 60;
                var cities = rgg.WeightedGraph(N, 0.5, 5, 20);
                var meetingCosts = new double[N];
                var populations = new int[N];
                for (int i = 0; i < populations.Length; i++)
                {
                    populations[i] = rng.Next(1000, 5000);
                    meetingCosts[i] = rng.Next(5, 15);
                }

                var description = "Losowy średnio-gęsty graf (50% szans na krawędź)";
                performanceFreeMeetingsTestCases.Add(new ElectionCampaignTestCase(50 * TIME_MULTIPLIER, null, description, cities,
                    populations, meetingCosts: new double[N], budget: 42, capitalCity: 0, bestCampaignPeopleMet: 26495, bestCampaignCost: 41));
                performanceMeetingsWithCostsTestCases.Add(new ElectionCampaignTestCase(60 * TIME_MULTIPLIER, null, description, cities,
                    populations, meetingCosts, budget: 42, capitalCity: 0, bestCampaignPeopleMet: 9086, bestCampaignCost: 42));
            }

            {
                // Długi cykl (tylko wersja bez kosztów spotkań)
                var rgg = new RandomGraphGenerator(2650);
                var rng = new Random(2650);
                var N = 4000;
                var cities = rgg.AssignWeights(GraphExamples.Cycle(N), 1, 1);
                var populations = new int[N];
                for (int i = 0; i < populations.Length; i++)
                {
                    populations[i] = rng.Next(10, 20);
                }

                performanceFreeMeetingsTestCases.Add(new ElectionCampaignTestCase(3 * TIME_MULTIPLIER, null, "Długi cykl", cities,
                    populations, meetingCosts: new double[N], budget: 500000, capitalCity: 0, bestCampaignPeopleMet: 57805, bestCampaignCost: 4000));
            }
            #endregion
        }
    }

    class Lab08Main
    {
        static void Main(string[] args)
        {
            var tests = new ElectionCampaignTester();
            tests.PrepareTestSets();

            tests.TestSets["ElectionCampaignFreeMeetings"].PerformTests(false);
            tests.TestSets["ElectionCampaignMeetingsWithCosts"].PerformTests(false);

            tests.TestSets["ElectionCampaignEfficiencyFreeMeetings"].PerformTests(true);
            tests.TestSets["ElectionCampaignEfficiencyMeetingsWithCosts"].PerformTests(true);
        }
    }
}
