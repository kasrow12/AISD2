using ASD.Graphs;

/// <summary>
///     Klasa rozszerzająca klasę Graph o rozwiązania problemów największej kliki i izomorfizmu grafów metodą pełnego
///     przeglądu (backtracking)
/// </summary>
public static class Lab10GraphExtender
{
    /// <summary>
    ///     Wyznacza największą klikę w grafie i jej rozmiar metodą pełnego przeglądu (backtracking)
    /// </summary>
    /// <param name="g">Badany graf</param>
    /// <param name="clique">Wierzchołki znalezionej największej kliki - parametr wyjściowy</param>
    /// <returns>Rozmiar największej kliki</returns>
    /// <remarks>
    ///     Nie wolno modyfikować badanego grafu.
    /// </remarks>
    public static int MaxClique(this Graph g, out int[] clique)
    {
        List<int> S = new();
        List<int> bestS = new();
        int n = g.VertexCount;

        MaxCliqueRec(0);

        void MaxCliqueRec(int k)
        {
            if (S.Count > bestS.Count)
                bestS = S[..];

            for (int v = k; v < n; v++)
            {
                if (n - k + 1 + S.Count < bestS.Count)
                    return;

                bool isValid = true;
                foreach (int u in S)
                {
                    if (!g.HasEdge(v, u))
                    {
                        isValid = false;
                        break;
                    }
                }

                if (!isValid)
                    continue;

                S.Add(v);
                MaxCliqueRec(v + 1);
                S.RemoveAt(S.Count - 1); // remove last
            }
        }

        clique = bestS.ToArray();
        return bestS.Count;
    }

    /// <summary>
    ///     Bada izomorfizm grafów metodą pełnego przeglądu (backtracking)
    /// </summary>
    /// <param name="g">Pierwszy badany graf</param>
    /// <param name="h">Drugi badany graf</param>
    /// <param name="map">
    ///     Mapowanie wierzchołków grafu h na wierzchołki grafu g (jeśli grafy nie są izomorficzne to null) -
    ///     parametr wyjściowy
    /// </param>
    /// <returns>Informacja, czy grafy g i h są izomorficzne</returns>
    /// <remarks>
    ///     1) Uwzględniamy wagi krawędzi
    ///     3) Nie wolno modyfikować badanych grafów.
    /// </remarks>
    public static bool IsomorphismTest(this Graph<int> g, Graph<int> h, out int[] map)
    {
        map = null;
        int n = g.VertexCount;

        if (n != h.VertexCount || g.EdgeCount != h.EdgeCount)
            return false;

        int[] mapping = new int[n];
        bool[] used = new bool[n];

        bool Iso(int k)
        {
            if (k == n)
                return true;

            for (int m = 0; m < n; m++)
            {
                if (used[m] || h.OutNeighbors(k).Count() != g.OutNeighbors(m).Count())
                    continue;

                bool valid = true;
                for (int v = 0; v < k; v++)
                {
                    if (g.HasEdge(mapping[v], m) != h.HasEdge(v, k)
                        || (g.HasEdge(mapping[v], m) && g.GetEdgeWeight(mapping[v], m) != h.GetEdgeWeight(v, k)))
                    {
                        valid = false;
                        break;
                    }
                }

                if (!valid)
                    continue;

                used[m] = true;
                mapping[k] = m;

                if (Iso(k + 1))
                    return true;

                used[m] = false;
            }

            return false;
        }

        if (!Iso(0))
            return false;

        map = mapping;
        return true;
    }
}