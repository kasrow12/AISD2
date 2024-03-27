using ASD.Graphs;

namespace ASD;

public class Lab06 : MarshalByRefObject
{
    public int GetV(int i, int j, int l)
    {
        return i * l + j;
    }

    public (int, int) GetIJ(int v, int l)
    {
        return (v / l, v % l);
    }

    /// <summary>
    ///     Etap 1 i 2 - szukanie trasy w nieplynacej rzece
    /// </summary>
    /// <param name="w"> Odległość między brzegami rzeki</param>
    /// <param name="l"> Długość fragmentu rzeki </param>
    /// <param name="lilie"> Opis lilii na rzece </param>
    /// <param name="sila"> Siła żabki - maksymalny kwadrat długości jednego skoku </param>
    /// <param name="start"> Początkowa pozycja w metrach od lewej strony </param>
    /// <returns>
    ///     (int total, (int x, int y)[] route) - total - suma sił koniecznych do wszystkich skoków, route -
    ///     lista par opisujących skoki. Opis jednego skoku (x,y) to dystans w osi x i dystans w osi y, jaki skok pokonuje
    /// </returns>
    public (int total, (int, int)[] route) Lab06_FindRoute(int w, int l, int[,] lilie, int sila, int start)
    {
        var g = new DiGraph<int>(w * l + 2, new DictionaryGraphRepresentation()); // List
        int s = w * l;
        int e = w * l + 1;

        // start
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < l; j++)
            {
                if (lilie[i, j] == 0)
                    continue;

                int dx = i + 1;
                int dy = start - j;
                int cost = dx * dx + dy * dy;
                if (cost <= sila)
                    g.AddEdge(s, GetV(i, j, l), cost);
            }
        }

        // wszystko inne
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < l; j++)
            {
                if (lilie[i, j] == 0)
                    continue;

                int v = GetV(i, j, l);

                for (int i2 = 0; i2 <= w; i2++)
                {
                    for (int j2 = 0; j2 < l; j2++)
                    {
                        // brzeg
                        if (i2 == w)
                        {
                            int dx2 = i2 - i;
                            int cost2 = dx2 * dx2;
                            if (cost2 <= sila)
                                g.AddEdge(v, e, cost2);
                            continue;
                        }

                        if (lilie[i2, j2] == 0 || (i == i2 && j == j2))
                            continue;

                        int dx = i2 - i;
                        int dy = j2 - j;
                        int cost = dx * dx + dy * dy;
                        if (cost <= sila)
                            g.AddEdge(v, GetV(i2, j2, l), cost);
                    }
                }
            }
        }

        var pathsInfo = Paths.Dijkstra(g, s);
        int[]? path = pathsInfo.GetPath(s, e);
        if (path == null)
            return (0, null);

        var route = new List<(int, int)>();
        for (int t = 1; t < path.Length; t++)
        {
            (int ii, int jj) = GetIJ(path[t - 1], l);
            (int i, int j) = GetIJ(path[t], l);
            if (path[t - 1] == s) // start
            {
                ii = -1;
                jj = start;
            }

            int dx = i - ii;
            int dy = j - jj;
            route.Add((dx, dy));
        }

        return (pathsInfo.GetDistance(s, e), route.ToArray());
    }

    /// <summary>
    ///     Etap 3 i 4 - szukanie trasy w nieplynacej rzece
    /// </summary>
    /// <param name="w"> Odległość między brzegami rzeki</param>
    /// <param name="l"> Długość fragmentu rzeki </param>
    /// <param name="lilie"> Opis lilii na rzece </param>
    /// <param name="sila"> Siła żabki - maksymalny kwadrat długości jednego skoku </param>
    /// <param name="start"> Początkowa pozycja w metrach od lewej strony </param>
    /// <param name="max_skok"> Maksymalna ilość skoków </param>
    /// <param name="v"> Prędkość rzeki </param>
    /// <returns>
    ///     (int total, (int x, int y)[] route) - total - suma sił koniecznych do wszystkich skoków, route -
    ///     lista par opisujących skoki. Opis jednego skoku (x,y) to dystans w osi x i dystans w osi y, jaki skok pokonuje
    /// </returns>
    public (int total, (int, int)[] route) Lab06_FindRouteFlowing(int w, int l, int[,] lilie, int sila, int start,
        int max_skok, int v)
    {
        var g = new DiGraph<int>(w * l + 2, new DictionaryGraphRepresentation()); // List
        int s = w * l;
        int e = w * l + 1;

        // start
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < l; j++)
            {
                if (lilie[i, j] == 0)
                    continue;

                int dx = i + 1;
                int dy = start - j;
                int cost = dx * dx + dy * dy;
                if (cost <= sila)
                    g.AddEdge(s, GetV(i, j, l), cost);
            }
        }

        // wszystko inne
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < l; j++)
            {
                if (lilie[i, j] == 0)
                    continue;

                int vv = GetV(i, j, l);

                for (int i2 = 0; i2 <= w; i2++)
                {
                    for (int j2 = 0; j2 < l; j2++)
                    {
                        // brzeg
                        if (i2 == w)
                        {
                            int dx2 = i2 - i;
                            int cost2 = dx2 * dx2;
                            if (cost2 <= sila)
                                g.AddEdge(vv, e, cost2);
                            continue;
                        }

                        if (lilie[i2, j2] == 0 || (i == i2 && j == j2))
                            continue;

                        int dx = i2 - i;
                        int dy = j2 - j;
                        int cost = dx * dx + dy * dy;
                        if (cost <= sila)
                            g.AddEdge(vv, GetV(i2, j2, l), cost);
                    }
                }
            }
        }

        // chyba warstwy jednak

        int[] costs = new int[g.VertexCount];
        for (int i = 0; i < g.VertexCount; i++)
            costs[i] = int.MaxValue;

        var q = new Queue<int>();
        q.Enqueue(s);
        costs[s] = 0;

        int[] from = new int[g.VertexCount];

        for (int i = 0; i < max_skok; i++)
        {
            for (int t = q.Count; t > 0; t--)
            {
                int vv = q.Dequeue();
                foreach (int u in g.OutNeighbors(vv))
                {
                    int cst = costs[vv] + g.GetEdgeWeight(vv, u);
                    if (cst < costs[u])
                    {
                        q.Enqueue(u);
                        costs[u] = cst;
                        from[u] = vv;
                    }
                }
            }
        }

        if (costs[e] == int.MaxValue)
            return (0, null);


        var route = new List<(int, int)>();
        int r = e;
        int pr;
        while (r != s)
        {
            pr = r;
            r = from[r];
            (int ii, int jj) = GetIJ(r, l);
            (int i, int j) = GetIJ(pr, l);
            if (r == s) // start
            {
                ii = -1;
                jj = start;
            }

            int dx = i - ii;
            int dy = j - jj;
            route.Insert(0, (dx, dy));
            Console.WriteLine($"{i} {j}: {(dx, dy)}");
        }

        // foreach (var x in route.ToArray())
        // {
        //     Console.WriteLine(x);
        // }
        // var yy = route.ToArray();

        return (costs[e], route.ToArray());
    }
}