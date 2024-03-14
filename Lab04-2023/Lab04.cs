using ASD.Graphs;

namespace ASD;

public class Lab04 : MarshalByRefObject
{
    private Dictionary<int, (int, int)> dict2;
    private DiGraph g;

    private bool[] goalsss;

    /// <summary>
    ///     Etap 1 - wyznaczanie numerów grup, które jest w stanie odwiedzić Karol, zapisując się na początku do podanej grupy
    /// </summary>
    /// <param name="graph">Ważony graf skierowany przedstawiający zasady dołączania do grup</param>
    /// <param name="start">Numer grupy, do której początkowo zapisuje się Karol</param>
    /// <returns>Tablica numerów grup, które może odwiedzić Karol, uporządkowana rosnąco</returns>
    public int[] Lab04Stage1(DiGraph<int> graph, int start)
    {
        var dict = new Dictionary<(int, int), int>();
        var dict2 = new Dictionary<int, (int, int)>();
        var g = new DiGraph(graph.VertexCount * graph.VertexCount);

        var startingPoints = new List<int>();

        // Tworzymy graf, w którym wierzchołki są postaci (grupa, poprzednia_grupa)
        int index = 0;
        foreach (var e in graph.DFS().SearchAll())
        {
            if (e.Weight < 0 && e.From != start)
                continue;

            if (!dict.TryGetValue((e.To, e.From), out int dest))
            {
                dest = index;
                dict[(e.To, e.From)] = index;
                dict2[index++] = (e.To, e.From);
            }

            // Wszystkie drogi zaczynać się będą od 1. zasady, czyli gdy osoba jest nowym członkiem (w = -1)
            if (e.Weight < 0 && e.From == start)
            {
                startingPoints.Add(dest);
                continue;
            }

            if (!dict.TryGetValue((e.From, e.Weight), out int from))
            {
                from = index;
                dict.Add((e.From, e.Weight), index);
                dict2.Add(index++, (e.From, e.Weight));
            }

            g.AddEdge(from, dest);
        }

        // Zawsze będziemy w startowej grupie
        var groups = new HashSet<int> { start };
        foreach (int idx in startingPoints)
        {
            foreach (var e in g.DFS().SearchFrom(idx))
                groups.Add(dict2[e.To].Item1);

            // startingIndex może być izolowany, dodajemy jego grupę
            groups.Add(dict2[idx].Item1);
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
        // Pozmieniać słowniki na bijekcję
        var dict = new Dictionary<(int, int), int>();
        dict2 = new Dictionary<int, (int, int)>();
        g = new DiGraph(graph.VertexCount * graph.VertexCount);

        var startingPoints = new List<(int, int)>();

        // Tworzymy graf, w którym wierzchołki są postaci (grupa, poprzednia_grupa)
        int index = 0;
        bool[] startsss = new bool[graph.VertexCount];
        foreach (int i in starts)
            startsss[i] = true;

        goalsss = new bool[graph.VertexCount];
        foreach (int a in goals)
        {
            goalsss[a] = true;
            if (startsss[a])
                return (true, [a]);
        }


        foreach (var e in graph.DFS().SearchAll())
        {
            if (e.Weight < 0 && !startsss[e.From])
                continue;

            if (!dict.TryGetValue((e.To, e.From), out int dest))
            {
                dest = index;
                dict[(e.To, e.From)] = index;
                dict2[index++] = (e.To, e.From);
            }

            // Wszystkie drogi zaczynać się będą od 1. zasady, czyli gdy osoba jest nowym członkiem (w = -1)
            if (e.Weight < 0 && startsss[e.From])
            {
                startingPoints.Add((dest, e.From));
                continue;
            }

            if (!dict.TryGetValue((e.From, e.Weight), out int from))
            {
                from = index;
                dict.Add((e.From, e.Weight), index);
                dict2.Add(index++, (e.From, e.Weight));
            }

            g.AddEdge(from, dest);
        }

        foreach ((int idx, int st) in startingPoints)
        {
            var s = new Stack<int>();
            s.Push(st);
            if (goalsss[dict2[idx].Item1])
                return (true, [st, dict2[idx].Item1]);

            s.Push(dict2[idx].Item1);
            if (Dfs(idx, s))
            {
                int[] xd = s.ToArray();
                Array.Reverse(xd);
                return (true, xd);
            }
        }

        return (false, null);
    }

    private bool Dfs(int v, Stack<int> s)
    {
        foreach (int u in g.OutNeighbors(v))
        {
            s.Push(dict2[u].Item1);

            if (goalsss[dict2[u].Item1])
                return true;
            if (Dfs(u, s))
                return true;

            s.Pop();
        }

        return false;
    }
}