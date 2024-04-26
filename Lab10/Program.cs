using ASD;
using ASD.Graphs;
using ASD.Graphs.Testing;
using System;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using static ASD.TestCase;

namespace ASD
{
    class Lab10Main
    {
        public static void Main(string[] args)
        {
            var tests = new Lab10Tests();
            tests.PrepareTestSets();
            foreach (var testSet in tests.TestSets)
            {
                testSet.Value.PerformTests(verbose: true, checkTimeLimit: false);
            }
        }
    }

    internal abstract class Lab10StageBase : TestCase
    {
        protected readonly Graph labyrinth;
        protected readonly int startingTorches, debt;
        protected readonly int[] roomTorches, roomGold;
        protected bool expectedResult;
        protected bool? routeFound;
        protected int[] route;


        public Lab10StageBase(Graph labyrinth, int startingTorches, int debt, int[] roomTorches, int[] roomGold, bool expectedResult, double timeLimit, string description) : base(timeLimit, null, description)
        {
            this.labyrinth = labyrinth;
            this.startingTorches = startingTorches;
            this.debt = debt;
            this.roomTorches = roomTorches;
            this.roomGold = roomGold;
            this.expectedResult = expectedResult;
        }
        
        protected (Result resultCode, string message) OkResult(string message) => (TimeLimit < PerformanceTime ? Result.LowEfficiency : Result.Success, $"{message} {PerformanceTime.ToString("#0.00")}s");
    }

    internal class Lab10Stage1Case : Lab10StageBase
    {
        public Lab10Stage1Case(Graph labyrinth, int startingTorches, int debt, int[] roomTorches, int[] roomGold, bool expectedResult, double timeLimit, string description) : base(labyrinth, startingTorches, debt, roomTorches, roomGold, expectedResult, timeLimit, description)
        {
        }
        protected override void PerformTestCase(object prototypeObject)
        {
            (routeFound, route) = ((Lab10)prototypeObject).FindEscape(labyrinth, startingTorches, roomTorches, debt, roomGold);
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            if (routeFound is null)
            {
                return (Result.NotPerformed, "Wewnętrzny błąd");
            }

            if (routeFound != expectedResult)
            {
                return (Result.WrongResult, routeFound.Value ? "Nie istnieje droga ucieczki z labirytnu, zwrócono przeciwnie. " + Description : "Istnieje droga ucieczki z labiryntu, zwrócono przeciwnie. " + Description );
            }

            int lastRoom = -1;
            bool[] visited = new bool[labyrinth.VertexCount];
            int currentGold = 0;
            int currentTorches = startingTorches + 1;
            int exit = labyrinth.VertexCount - 1;

            if (routeFound == false)
            {
                if (route is null)
                {
                    return (OkResult("OK"));
                }
                else
                {
                    return (Result.WrongResult, "Niepoprawna forma wyniku");
                }
            }
            if (route is null)
            {
                return (Result.WrongResult, "Dobry wynik, pusta droga ucieczki");
            }
            foreach (int room in route)
            {
                if (room  < 0 || room >= labyrinth.VertexCount)
                {
                    return (Result.WrongResult, "Niepoprawny numer pokoju w drodze ucieczki");
                }

                if (lastRoom != -1 && !labyrinth.HasEdge(lastRoom, room))
                {
                    return (Result.WrongResult, "Niepoprawne przejście między pokojami");
                }

                lastRoom = room;

                if (visited[room])
                {
                    return (Result.WrongResult, "Pokój w drodze ucieczki odwiedzony więcej niż raz");
                }

                visited[room] = true;
                currentGold += roomGold[room];
                currentTorches += roomTorches[room] - 1;

                if (room != exit && currentTorches == 0)
                {
                    return (Result.WrongResult, "Na wybranej drodze skończyły się pochodnie");
                }
            }

            if (lastRoom != exit)
            {
                return (Result.WrongResult, "Ostatni pokój w drodze ucieczki nie jest wyjściem z labiryntu");
            }

            if (currentGold < debt)
            {
                return (Result.WrongResult, "Na wybranej drodze nie zebrano wystarczająco złota");
            }

            return OkResult("OK");
        }
    }

    internal class Lab10Stage2Case : Lab10StageBase
    {
        protected readonly int dragonDelay;
        protected int[] originalTorches, originalGold;

        public Lab10Stage2Case(Graph labyrinth, int startingTorches, int debt, int[] roomTorches, int[] roomGold, int dragonDelay, bool expectedResult, double timeLimit, string description) : base(labyrinth, startingTorches, debt, roomTorches, roomGold, expectedResult, timeLimit, description)
        {
            this.dragonDelay = dragonDelay;
            originalTorches = new int[roomTorches.Length];
            roomTorches.CopyTo(originalTorches, 0);
            originalGold = new int[roomGold.Length];
            roomGold.CopyTo(originalGold, 0);
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            (routeFound, route) = ((Lab10)prototypeObject).FindEscapeWithHeadstart(labyrinth, startingTorches, roomTorches, debt, roomGold, dragonDelay);
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            if (routeFound is null)
            {
                return (Result.NotPerformed, "Wewnętrzny błąd");
            }

            if (routeFound != expectedResult)
            {
                return (Result.WrongResult, routeFound.Value ? "Nie istnieje droga ucieczki z labirytnu, zwrócono przeciwnie. " + Description : "Istnieje droga ucieczki z labiryntu, zwrócono przeciwnie. " + Description);
            }

            int lastRoom = -1;

            int[] distance = new int[labyrinth.VertexCount];
            for (int i = 0; i < labyrinth.VertexCount; i++) distance[i] = int.MaxValue;

            int currentDistance = 0;
            int currentGold = 0;
            int currentTorches = startingTorches + 1;
            int exit = labyrinth.VertexCount - 1;


            if (routeFound == false)
            {
                if (route is null)
                {
                    return (OkResult("OK"));
                }
                else
                {
                    return (Result.WrongResult, "Niepoprawna forma wyniku");
                }
            }
            if (route is null)
            {
                return (Result.WrongResult, "Dobry wynik, pusta droga ucieczki");
            }
            foreach (int room in route)
            {
                if (room < 0 || room >= labyrinth.VertexCount)
                {
                    return (Result.WrongResult, "Niepoprawny numer pokoju w drodze ucieczki");
                }

                if (lastRoom != -1 && !labyrinth.HasEdge(lastRoom, room))
                {
                    return (Result.WrongResult, "Niepoprawne przejście między pokojami");
                }
                if (lastRoom == labyrinth.VertexCount - 1)
                {
                    return (Result.WrongResult, "Droga przechodzi przez wyjście");
                }
                lastRoom = room;

                if (distance[room] <= ++currentDistance - dragonDelay - 1)
                {
                    return (Result.WrongResult, "Odwiedzono pokój po którym przeszedł smok");
                }

                currentDistance = currentDistance < distance[room] ? currentDistance : distance[room];
                distance[room] = currentDistance;


                currentGold += originalGold[room];
                originalGold[room] = 0;
                currentTorches += originalTorches[room] - 1;
                originalTorches[room] = 0;

                if (room != exit && currentTorches == 0)
                {
                    return (Result.WrongResult, "Na wybranej drodze skończyły się pochodnie");
                }
            }

            if (lastRoom != exit)
            {
                return (Result.WrongResult, "Ostatni pokój w drodze ucieczki nie jest wyjściem z labiryntu");
            }

            if (currentGold < debt)
            {
                return (Result.WrongResult, "Na wybranej drodze nie zebrano wystarczająco złota");
            }

            return OkResult("OK");
        }
    }

    class Lab10Tests : TestModule
    {
        public TestSet stage1Tests;
        public TestSet stage2Tests;

        public override void PrepareTestSets()
        {
            stage1Tests = new TestSet(new Lab10(), "Etap I: smok depczący po piętach");
            stage2Tests = new TestSet(new Lab10(), "Etap II: smok z opóźnieniem");
            TestSets["Etap 1"] = stage1Tests;
            TestSets["Etap 2"] = stage2Tests;
            PrepareTests();
        }

        void PrepareTests()
        {
            {
                // Prosty test przykładowy
                // 1.1
                // 2.1
                Graph lab = new Graph(7);
                lab.AddEdge(0, 1);
                lab.AddEdge(1, 2);
                lab.AddEdge(1, 5);
                lab.AddEdge(2, 5);
                lab.AddEdge(2, 4);
                lab.AddEdge(2, 3);
                lab.AddEdge(3, 4);
                lab.AddEdge(4, 5);
                lab.AddEdge(4, 6);
                lab.AddEdge(5, 6);
                int[] torches = new int[7];
                torches[3] = 3;
                int[] gold = new int[7];
                gold[5] = 1;
                gold[4] = 1;
                string description = "Przykład 1 z zadania";
                stage1Tests.TestCases.Add(new Lab10Stage1Case(lab, 3, 2, torches, gold, true, 1, description));
                stage2Tests.TestCases.Add(new Lab10Stage2Case(lab, 3, 2, torches, gold, 2, true, 1, description));
            }

            {
                // Prosty test z odstającą gałęzią w którą trzeba wejść
                // 1.2
                // 2.2
                Graph lab = new Graph(4);
                lab.AddEdge(0, 1);
                lab.AddEdge(1, 2);
                lab.AddEdge(1, 3);
                int[] torches = new int[4];
                torches[2] = 2;
                int [] gold = new int[4];
                gold[2] = 2;
                string description1 = "Przykład 2, przykład z zadania, bez opóźnienia niemożliwy";
                string description2 = "Przykład 2, przykład z zadania, z opóźnieniem do zrobienia";
                stage1Tests.TestCases.Add(new Lab10Stage1Case(lab, 2, 2, torches, gold, false, 1, description1));
                stage2Tests.TestCases.Add(new Lab10Stage2Case(lab, 2, 2, torches, gold, 2, true, 1, description2));
            }

            {
                // 1.3
                Graph lab = new Graph(4);
                lab.AddEdge(0, 1);
                lab.AddEdge(0, 2);
                lab.AddEdge(1, 2);
                lab.AddEdge(1, 3);
                lab.AddEdge(2, 3);
                int [] torches = new int[4];
                int [] gold = new int[4];
                gold[1] = gold[2] = 1;
                string description = "Prosty graf gdzie brakuje złota";
                stage1Tests.TestCases.Add(new Lab10Stage1Case(lab, 5, 3, torches, gold, false, 1, description));
            }

            {
                // 1.4
                Graph lab = new Graph(6);
                lab.AddEdge(0, 1);
                lab.AddEdge(1, 2);
                lab.AddEdge(2, 3);
                lab.AddEdge(3, 4);
                lab.AddEdge(4, 5);
                int[] torches = new int[6];
                for (int i = 1; i < 4; i++) torches[i] = 1;
                int[] gold = new int[6];
                string description = "Prosty graf gdzie brakuje pochodni";
                stage1Tests.TestCases.Add(new Lab10Stage1Case(lab, 1, 0, torches, gold, false, 1, description));
            }

            {
                // 1.5
                int layers = 20;
                int n = (int)Math.Pow(2, layers);
                Graph lab = new Graph(n);
                for (int i = 0; i < n/2 -1; i++)
                {
                    lab.AddEdge(i, 2 * i + 1);
                    lab.AddEdge(i, 2 * i + 2);
                }
                lab.AddEdge(n - 5, n - 1);
                int[] torches = new int[n];
                int[] gold = new int[n];
                for (int i= 0; i < n; i++)
                {
                    torches[i] = 1;
                    gold[i] = 1;
                }
                string description = "Duży graf drzewiasty";
                stage1Tests.TestCases.Add(new Lab10Stage1Case(lab, 1, layers + 1, torches, gold, true, 3, description));
               
            }


            {
                // 1.6
                // 2.3
                int seed = 299;
                RandomGraphGenerator rgg = new RandomGraphGenerator(seed);
                Random rand = new Random(seed);
                Graph g = rgg.Graph(10, 0.3);
                int[] torches = new int[10];
                int[] gold = new int[10];
                for (int i = 0; i < 5; i++)
                {
                    int room = rand.Next(10);
                    while (torches[room] != 0) room = rand.Next(10);
                    torches[room] = rand.Next(1, 3);
                }
                for (int i = 0; i < 5; i++)
                {
                    int room = rand.Next(10);
                    while (gold[room] != 0) room = rand.Next(10);
                    gold[room] = 1;
                }
                string description1 = "Losowy graf, bez opóźnienia niemożliwy";
                string description2 = "Losowy graf, z opóźnieniem do zrobienia";
                stage1Tests.TestCases.Add(new Lab10Stage1Case(g, 3, 5, torches, gold, false, 1, description1));
                stage2Tests.TestCases.Add(new Lab10Stage2Case(g, 3, 5, torches, gold, 10, true, 1, description2));
            }

            {
                // 1.7
                int seed = -28;
                int n = 15;
                RandomGraphGenerator rgg = new RandomGraphGenerator(seed);
                Random rand = new Random(seed);
                Graph g = rgg.Graph(n, 0.4);
                int[] torches = new int[n];
                int[] gold = new int[n];
                for (int i = 0; i < n / 2; i++)
                {
                    int room = rand.Next(n);
                    while (torches[room] != 0) room = rand.Next(n);
                    torches[room] = rand.Next(1, 4);
                }
                for (int i = 0; i < 5; i++)
                {
                    int room = rand.Next(n);
                    while (gold[room] != 0) room = rand.Next(n);
                    gold[room] = rand.Next(1, 4);
                }
                string description = "Średni graf losowy";
                stage1Tests.TestCases.Add(new Lab10Stage1Case(g, 3, n * 2 / 3, torches, gold, true, 1, description));
            }

            {
                // 2.4
                int seed = 37;
                int n = 10;
                RandomGraphGenerator rgg = new RandomGraphGenerator(seed);
                Random rand = new Random(seed);
                Graph g = rgg.Graph(n, 0.4);
                int[] torches = new int[n];
                int[] gold = new int[n];
                for (int i = 0; i < n / 2; i++)
                {
                    int room = rand.Next(n);
                    while (torches[room] != 0) room = rand.Next(n);
                    torches[room] = rand.Next(1, 4);
                }
                for (int i = 0; i < 5; i++)
                {
                    int room = rand.Next(n);
                    while (gold[room] != 0) room = rand.Next(n);
                    gold[room] = rand.Next(1, 4);
                }
                string description = "Średni graf losowy";
                stage2Tests.TestCases.Add(new Lab10Stage2Case(g, 3, n * 2 / 3, torches, gold, 5, true, 1, description));
            }

            {
                // 2.5
                int layers = 7;
                int n = (int)Math.Pow(2, layers);
                Graph lab = new Graph(n);
                for (int i = 0; i < n / 2 - 1; i++)
                {
                    lab.AddEdge(i, 2 * i + 1);
                    lab.AddEdge(i, 2 * i + 2);
                }
                lab.AddEdge(n - 5, n - 1);
                int[] torches = new int[n];
                int[] gold = new int[n];
                for (int i = 0; i < n; i++)
                {
                    torches[i] = 1;
                    gold[i] = 1;
                }
                torches[2] = 2;
                torches[14] = 2;
                torches[30] = 2;
                torches[61] = 2;
                string description = "Graf drzewiasty, wymaga zboczenia ze ścieżki kilka razy";
                stage2Tests.TestCases.Add(new Lab10Stage2Case(lab, 1, layers + 1 + 3, torches, gold, 10, true, 1, description));
            }

            {
                // 2.6
                Graph graph = new Graph(13);
                for (int i = 0; i < 3; i++) graph.AddEdge(i, i + 1);
                for (int i = 4; i < 6; i++) graph.AddEdge(i, i + 1);
                graph.AddEdge(1, 4);
                graph.AddEdge(6, 1);
                for (int i = 7; i < 11; i++) graph.AddEdge(i, i + 1);
                graph.AddEdge(2, 7);
                graph.AddEdge(11, 2);

                graph.AddEdge(3, 12);

                int[] torches = new int[13];
                int[] gold = new int[13];

                torches[1] = 2;
                torches[4] = torches[5] = torches[6] = 1;
                torches[2] = 5;
                torches[3] = 1;

                gold[5] = 1;
                gold[7] = 1;
                gold[11] = 1;

                string description = "Jedna pętla po której trzeba przejść i jeden wierzchołej z którego trzeba trzy razy wyjść";
                stage2Tests.TestCases.Add(new Lab10Stage2Case(graph, 1, 3, torches,  gold, 8, true, 1, description));

            }

            // WYMAGAJĄCY TEST DO PRZYGOTOWANIA CZĘŚCI DOMOWEJ

            {
                // Test z pętlami, które niektóre można przejść całe, a inne trzeba wejść po jednym z obu stron, jeżeli nie ma optymalizacji na odwiedzanie starych pokoi to będzie bardzo długo trwał
                int n = 6;
                Graph lab = new Graph(n + (n - 2) / 2 * 3 + (n - 2) / 2 * 5 + 1);
                int[] torches = new int[lab.VertexCount];
                int[] gold = new int[lab.VertexCount];
                for (int i = 0; i < n - 1; i++)
                {
                    lab.AddEdge(i, i + 1);
                }
                int currentLoopVertex = n;
                for (int i = 1; i < n - 1; i += 2)
                {
                    lab.AddEdge(i, currentLoopVertex);
                    lab.AddEdge(currentLoopVertex, currentLoopVertex + 1);
                    lab.AddEdge(currentLoopVertex + 1, currentLoopVertex + 2);
                    lab.AddEdge(currentLoopVertex + 2, i);
                    gold[currentLoopVertex + 1] = 1;
                    currentLoopVertex += 3;
                }
                for (int i = 2; i < n - 1; i += 2)
                {
                    lab.AddEdge(i, currentLoopVertex);
                    lab.AddEdge(currentLoopVertex, currentLoopVertex + 1);
                    lab.AddEdge(currentLoopVertex + 1, currentLoopVertex + 2);
                    lab.AddEdge(currentLoopVertex + 2, currentLoopVertex + 3);
                    lab.AddEdge(currentLoopVertex + 3, currentLoopVertex + 4);
                    lab.AddEdge(currentLoopVertex + 4, i);
                    gold[currentLoopVertex] = 1;
                    gold[currentLoopVertex + 4] = 1;
                    currentLoopVertex += 5;
                }
                for (int i = 1; i < n - 1; i++)
                {
                    torches[i] = 5;
                }
                torches[n - 1] = 1;
                lab.AddEdge(n -1, lab.VertexCount - 1);
                string description = "Ścieżka z dużą ilością przymocowanych pętli";
                stage2Tests.TestCases.Add(new Lab10Stage2Case(lab, 1, n - 2 + (n - 2) / 2, torches, gold, int.MaxValue, true, 1, description));
            }
        }
    }

}