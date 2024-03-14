using ASD.Graphs;

namespace ASD;

public class Lab04 : MarshalByRefObject
{
    private int end;
    private DiGraph g;
    private int[] visited;

    /// <summary>
    ///     Etap 1 - Szukanie mozliwych do odwiedzenia miast z grafu skierowanego
    ///     przy zalozeniu, ze pociagi odjezdzaja co godzine.
    /// </summary>
    /// <param name="graph">Graf skierowany przedstawiający siatke pociagow</param>
    /// <param name="miastoStartowe">Numer miasta z ktorego zaczyna sie podroz pociagiem</param>
    /// <param name="K">Godzina o ktorej musi zakonczyc sie nasza podroz</param>
    /// <returns>Tablica numerow miast ktore mozna odwiedzic. Posortowana rosnaco.</returns>
    public int[] Lab04Stage1(DiGraph graph, int miastoStartowe, int K)
    {
        end = K;
        visited = new int[graph.VertexCount];
        for (int i = 0; i < graph.VertexCount; i++)
            visited[i] = int.MaxValue;
        g = graph;

        Dfs(miastoStartowe, 8);

        var list = new List<int>();
        for (int i = 0; i < graph.VertexCount; i++)
        {
            if (visited[i] != int.MaxValue)
                list.Add(i);
        }

        return list.ToArray();
    }

    private void Dfs(int v, int h)
    {
        visited[v] = h;
        foreach (int u in g.OutNeighbors(v))
        {
            if (h + 1 <= end && visited[u] > h + 1)
                Dfs(u, h + 1);
        }
    }

    /// <summary>
    ///     Etap 2 - Szukanie mozliwych do odwiedzenia miast z grafu skierowanego.
    ///     Waga krawedzi oznacza, ze pociag rusza o tej godzinie
    /// </summary>
    /// <param name="graph">Wazony graf skierowany przedstawiający siatke pociagow</param>
    /// <param name="miastoStartowe">Numer miasta z ktorego zaczyna sie podroz pociagiem</param>
    /// <param name="K">Godzina o ktorej musi zakonczyc sie nasza podroz</param>
    /// <returns>Tablica numerow miast ktore mozna odwiedzic. Posortowana rosnaco.</returns>
    public int[] Lab04Stage2(DiGraph<int> graph, int miastoStartowe, int K)
    {
        int[] visitedAt = new int[graph.VertexCount];
        for (int i = 0; i < graph.VertexCount; i++)
            visitedAt[i] = int.MaxValue;

        var q = new PriorityQueue<int, (int, int)>();
        q.Insert((miastoStartowe, 8), 8);

        // Dijkstra
        while (q.Count > 0)
        {
            (int v, int hour) = q.Extract();

            if (visitedAt[v] <= hour)
                continue;

            visitedAt[v] = hour;
            if (hour >= K)
                continue;

            foreach (var e in graph.OutEdges(v))
            {
                // odjazd później && + godzina na dojazd && będziemy tam szybciej niż wcześniej
                int arrival = e.Weight + 1;
                if (hour <= e.Weight && arrival <= K && arrival < visitedAt[e.To])
                    q.Insert((e.To, arrival), arrival);
            }
        }


        var cities = new List<int>();
        for (int i = 0; i < graph.VertexCount; i++)
        {
            if (visitedAt[i] != int.MaxValue)
                cities.Add(i);
        }

        return cities.ToArray();
    }
}