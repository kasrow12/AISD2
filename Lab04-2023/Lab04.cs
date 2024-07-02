using ASD.Graphs;

namespace ASD;

public class Lab04 : MarshalByRefObject
{
    /// <summary>
    ///     Etap 1 - wyznaczanie numerów grup, które jest w stanie odwiedzić Karol, zapisując się na początku do podanej grupy
    /// </summary>
    /// <param name="graph">Ważony graf skierowany przedstawiający zasady dołączania do grup</param>
    /// <param name="start">Numer grupy, do której początkowo zapisuje się Karol</param>
    /// <returns>Tablica numerów grup, które może odwiedzić Karol, uporządkowana rosnąco</returns>
    public int[] Lab04Stage1(DiGraph<int> graph, int start)
    {
        int n = graph.VertexCount;
        var g = new DiGraph(n * n);

        var startingPoints = new List<int>();

        // Tworzymy graf, w którym wierzchołki są postaci (grupa, poprzednia_grupa)
        foreach (var e in graph.DFS().SearchAll())
        {
            if (e.Weight < 0 && e.From != start)
                continue;

            int from = e.From * n + e.Weight;
            int dest = e.To * n + e.From;

            // Wszystkie drogi zaczynać się będą od 1. zasady, czyli gdy osoba jest nowym członkiem (w = -1)
            if (e.Weight < 0 && e.From == start)
            {
                startingPoints.Add(dest);
                continue;
            }

            g.AddEdge(from, dest);
        }

        // Zawsze będziemy w startowej grupie
        var groups = new HashSet<int> { start };
        foreach (int idx in startingPoints)
        {
            foreach (var e in g.DFS().SearchFrom(idx))
                groups.Add(e.To / n);

            // startingIndex może być izolowany, dodajemy jego grupę
            groups.Add(idx / n);
        }

        int[] ret = groups.ToArray();
        Array.Sort(ret);
        return ret;
    }

    /// <summary>
    ///     Etap 2 - szukanie możliwości przejścia z jednej z grup z `starts` do jednej z grup z `goals`
    /// </summary>
    /// <param name="graph">Ważony graf skierowany przedstawiający zasady dołączania do grup</param>
    /// <param name="starts">Tablica z numerami grup startowych (trasę należy zacząć w jednej z nich)</param>
    /// <param name="goals">Tablica z numerami grup docelowych (trasę należy zakończyć w jednej z nich)</param>
    /// <returns>
    ///     (possible, route) - `possible` ma wartość true gdy istnieje możliwość przejścia, wpp. false,
    ///     route to tablica z numerami kolejno odwiedzanych grup (pierwszy numer to numer grupy startowej, ostatni to numer
    ///     grupy docelowej),
    ///     jeżeli possible == false to route ustawiamy na null
    /// </returns>
    public (bool possible, int[] route) Lab04Stage2(DiGraph<int> graph, int[] starts, int[] goals)
    {
        int n = graph.VertexCount;
        var g = new DiGraph(n * n);
        var startingPoints = new List<(int, int)>();

        // Tworzymy graf, w którym wierzchołki są postaci (grupa, poprzednia_grupa)
        bool[] isStart = new bool[n];
        foreach (int i in starts)
            isStart[i] = true;

        bool[] isGoal = new bool[n];
        foreach (int i in goals)
        {
            isGoal[i] = true;
            if (isStart[i]) // dla grafu bez krawędzi
                return (true, [i]);
        }

        // Różnica względem etapu 1. - kilka możliwych grup początkowych
        foreach (var e in graph.DFS().SearchAll())
        {
            int from = e.From * n + e.Weight;
            int dest = e.To * n + e.From;
            if (e.Weight < 0)
            {
                // Wszystkie drogi zaczynać się będą od 1. zasady, czyli gdy osoba jest nowym członkiem (w = -1)
                if (isStart[e.From])
                    startingPoints.Add((dest, e.From));

                continue;
            }

            g.AddEdge(from, dest);
        }

        foreach ((int idx, int start) in startingPoints)
        {
            var path = new Stack<int>();
            path.Push(start);
            path.Push(idx / n);
            
            if (Dfs(idx))
            {
                int[] route = path.ToArray();
                Array.Reverse(route);
                return (true, route);
            }

            bool Dfs(int v)
            {
                foreach (int u in g.OutNeighbors(v))
                {
                    path.Push(u / n);

                    if (isGoal[u / n])
                        return true;
                    if (Dfs(u))
                        return true;

                    path.Pop();
                }

                return false;
            }
        }

        return (false, null);
    }
}