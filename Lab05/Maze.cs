using ASD.Graphs;

namespace ASD;

public class Maze : MarshalByRefObject
{
    public int getV(int i, int j, int m)
    {
        return i * m + j;
    }

    public int getV(int i, int j, int k, int n, int m)
    {
        return k * n * m + i * m + j;
    }

    public (int, int) getIJ(int v, int m)
    {
        return (v / m, v % m);
    }

    public (int, int, int) getIJK(int v, int n, int m)
    {
        int r = v % (n * m);
        return (r / m, r % m, v / (n * m));
    }

    /// <summary>
    ///     Wersje zadania I oraz II
    ///     Zwraca najkrótszy możliwy czas przejścia przez labirynt bez dynamitów lub z dowolną ich liczbą
    /// </summary>
    /// <param name="maze">labirynt</param>
    /// <param name="withDynamite">
    ///     informacja, czy dostępne są dynamity
    ///     Wersja I zadania -> withDynamites = false, Wersja II zadania -> withDynamites = true
    /// </param>
    /// <param name="path">zwracana ścieżka</param>
    /// <param name="t">czas zburzenia ściany (dotyczy tylko wersji II)</param>
    public int FindShortestPath(char[,] maze, bool withDynamite, out string path, int t = 0)
    {
        int n = maze.GetLength(0);
        int m = maze.GetLength(1);
        var graph = new DiGraph<int>(n * m, new ListGraphRepresentation());

        int start = -1;
        int end = -1;
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                int v = getV(i, j, m);

                if (!withDynamite && maze[i, j] == 'X')
                    continue;

                if (i + 1 < n && maze[i + 1, j] != 'X')
                    graph.AddEdge(v, getV(i + 1, j, m), 1);

                if (j + 1 < m && maze[i, j + 1] != 'X')
                    graph.AddEdge(v, getV(i, j + 1, m), 1);

                if (i - 1 >= 0 && maze[i - 1, j] != 'X')
                    graph.AddEdge(v, getV(i - 1, j, m), 1);

                if (j - 1 >= 0 && maze[i, j - 1] != 'X')
                    graph.AddEdge(v, getV(i, j - 1, m), 1);

                if (withDynamite)
                {
                    if (i + 1 < n && maze[i + 1, j] == 'X')
                        graph.AddEdge(v, getV(i + 1, j, m), t);

                    if (j + 1 < m && maze[i, j + 1] == 'X')
                        graph.AddEdge(v, getV(i, j + 1, m), t);

                    if (i - 1 >= 0 && maze[i - 1, j] == 'X')
                        graph.AddEdge(v, getV(i - 1, j, m), t);

                    if (j - 1 >= 0 && maze[i, j - 1] == 'X')
                        graph.AddEdge(v, getV(i, j - 1, m), t);
                }

                if (maze[i, j] == 'S')
                    start = v;

                if (maze[i, j] == 'E')
                    end = v;
            }
        }

        var pathsInfo = Paths.Dijkstra(graph, start);
        int[]? p = pathsInfo.GetPath(start, end);
        path = "";

        if (p == null)
            return -1;

        int cost = 0;
        for (int l = 1; l < p.Length; l++)
        {
            (int ii, int jj) = getIJ(p[l - 1], m);
            (int i, int j) = getIJ(p[l], m);
            if (i < ii)
                path += 'N';
            else if (j > jj)
                path += 'E';
            else if (j < jj)
                path += 'W';
            else if (i > ii)
                path += 'S';

            if (maze[i, j] == 'X')
                cost += t;
            else
                cost += 1;
        }

        return cost;
    }

    /// <summary>
    ///     Wersja III i IV zadania
    ///     Zwraca najkrótszy możliwy czas przejścia przez labirynt z użyciem co najwyżej k lasek dynamitu
    /// </summary>
    /// <param name="maze">labirynt</param>
    /// <param name="k">liczba dostępnych lasek dynamitu, dla wersji III k=1</param>
    /// <param name="path">zwracana ścieżka</param>
    /// <param name="t">czas zburzenia ściany</param>
    public int FindShortestPathWithKDynamites(char[,] maze, int k, out string path, int t)
    {
        int n = maze.GetLength(0);
        int m = maze.GetLength(1);
        var graph = new DiGraph<int>((k + 1) * n * m, new ListGraphRepresentation());
        int[,] visited = new int[n, m];

        int start = -1;
        int endi = -1;
        int endj = -1;
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                for (int l = 0; l <= k; l++)
                {
                    int v = getV(i, j, l, n, m);
                    int up = l + 1;

                    if (i + 1 < n && maze[i + 1, j] != 'X')
                        graph.AddEdge(v, getV(i + 1, j, l, n, m), 1);

                    if (j + 1 < m && maze[i, j + 1] != 'X')
                        graph.AddEdge(v, getV(i, j + 1, l, n, m), 1);

                    if (i - 1 >= 0 && maze[i - 1, j] != 'X')
                        graph.AddEdge(v, getV(i - 1, j, l, n, m), 1);

                    if (j - 1 >= 0 && maze[i, j - 1] != 'X')
                        graph.AddEdge(v, getV(i, j - 1, l, n, m), 1);

                    if (up <= k)
                    {
                        bool used = false;
                        if (i + 1 < n && maze[i + 1, j] == 'X')
                        {
                            graph.AddEdge(v, getV(i + 1, j, up, n, m), t);
                            used = true;
                        }

                        if (j + 1 < m && maze[i, j + 1] == 'X')
                        {
                            graph.AddEdge(v, getV(i, j + 1, up, n, m), t);
                            used = true;
                        }

                        if (i - 1 >= 0 && maze[i - 1, j] == 'X')
                        {
                            graph.AddEdge(v, getV(i - 1, j, up, n, m), t);
                            used = true;
                        }

                        if (j - 1 >= 0 && maze[i, j - 1] == 'X')
                        {
                            graph.AddEdge(v, getV(i, j - 1, up, n, m), t);
                            used = true;
                        }

                        if (used && visited[i, j] < up)
                            visited[i, j] = up;
                    }
                }

                if (maze[i, j] == 'S')
                    start = getV(i, j, 0, n, m);

                if (maze[i, j] == 'E')
                {
                    endi = i;
                    endj = j;
                }
            }
        }

        for (int e = 1; e <= k; e++)
            graph.AddEdge(getV(endi, endj, e - 1, n, m), getV(endi, endj, e, n, m), 0);


        var pathsInfo = Paths.Dijkstra(graph, start);
        int[]? p = pathsInfo.GetPath(start, getV(endi, endj, k, n, m));
        path = "";

        if (p == null)
            return -1;

        int cost = 0;
        for (int d = 1; d < p.Length; d++)
        {
            (int ii, int jj, int ll) = getIJK(p[d - 1], n, m);
            (int i, int j, int l) = getIJK(p[d], n, m);
            if (i < ii)
                path += 'N';
            else if (j > jj)
                path += 'E';
            else if (j < jj)
                path += 'W';
            else if (i > ii)
                path += 'S';

            if (maze[i, j] == 'X')
                cost += t;
            else
                cost += 1;

            if (i == endi && j == endj)
                break;
        }

        return cost;
    }
}