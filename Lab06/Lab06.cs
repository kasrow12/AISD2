using ASD.Graphs;
using System;
using System.Collections.Generic;

namespace ASD
{
    public class Lab06 : MarshalByRefObject
    {
        public int GetI(int v, int c, int colors)
        {
            return v * colors + c;
        }

        public (int, int) GetVC(int i, int colors)
        {
            return (i / colors, i % colors);
        }

        /// <summary>Etap 1</summary>
        /// <param name="n">Liczba kolorów (równa liczbie wierzchołków w c)</param>
        /// <param name="c">Graf opisujący możliwe przejścia między kolorami. Waga to wysiłek.</param>
        /// <param name="g">Graf opisujący drogi w mieście. Waga to kolor drogi.</param>
        /// <param name="target">Wierzchołek docelowy (dom Grzesia).</param>
        /// <param name="start">Wierzchołek startowy (wejście z lasu).</param>
        /// <returns>
        ///     Pierwszy element pary to informacja, czy rozwiązanie istnieje. Drugi element pary, to droga będąca
        ///     rozwiązaniem: sekwencja odwiedzanych wierzchołków (pierwszy musi być start, ostatni target). W przypadku, gdy nie
        ///     ma rozwiązania, ma być tablica o długości 0.
        /// </returns>
        public (bool possible, int[] path) Stage1(int n, DiGraph<int> c, Graph<int> g, int target, int start)
        {
            int[] visited = new int[g.VertexCount * n];
            for (int i = 0; i < g.VertexCount * n; i++)
                visited[i] = -1;

            var q = new Queue<int>();
            // Wrzucenie do kolejki startów we wszystkich kolorach
            for (int i = 0; i < n; i++)
            {
                q.Enqueue(GetI(start, i, n));
                visited[i] = -2; 
            }

            // BFS
            while (q.Count > 0)
            {
                for (int t = q.Count; t > 0; t--)
                {
                    int i = q.Dequeue();
                    (int v, int color) = GetVC(i, n);

                    // Znaleziono trasę
                    if (v == target)
                    {
                        var list = new List<int>();
                        while (i != -2)
                        {
                            list.Insert(0, i / n);
                            i = visited[i];
                        }

                        return (true, list.ToArray());
                    }

                    // Przeszukiwanie możliwych jeszcze nieodwiedzonych miast 
                    foreach (var e in g.OutEdges(v))
                    {
                        int dest = GetI(e.To, e.Weight, n);
                        if (visited[dest] != -1)
                            continue;

                        // Czy zgodny kolor albo możliwa zmiana
                        if (color == e.Weight || c.HasEdge(color, e.Weight))
                        {
                            q.Enqueue(dest);
                            visited[dest] = i;
                        }
                    }
                }
            }

            return (false, new int[0]);
        }


        /// <summary>Drugi etap</summary>
        /// <param name="n">Liczba kolorów (równa liczbie wierzchołków w c)</param>
        /// <param name="c">Graf opisujący możliwe przejścia między kolorami. Waga to wysiłek.</param>
        /// <param name="g">Graf opisujący drogi w mieście. Waga to kolor drogi.</param>
        /// <param name="target">Wierzchołek docelowy (dom Grzesia).</param>
        /// <param name="starts">Wierzchołki startowe (wejścia z lasu).</param>
        /// <returns>
        ///     Pierwszy element pary to koszt najlepszego rozwiązania lub null, gdy rozwiązanie nie istnieje. Drugi element
        ///     pary, tak jak w etapie 1, to droga będąca rozwiązaniem: sekwencja odwiedzanych wierzchołków (pierwszy musi być
        ///     start, ostatni target). W przypadku, gdy nie ma rozwiązania, ma być tablica o długości 0.
        /// </returns>
        public (int? cost, int[] path) Stage2(int n, DiGraph<int> c, Graph<int> g, int target, int[] starts)
        {
            var graph = new DiGraph<int>(g.VertexCount * n + 2, g.Representation);
            int start = g.VertexCount * n;
            int end = g.VertexCount * n + 1;

            // Tworzymy graf pomocniczy, w którym wierzchołki są postaci (miasto, bieżący_kolor)
            foreach (var e in g.BFS().SearchAll())
            {
                int from = GetI(e.From, e.Weight, n);
                int color = e.Weight;
                graph.AddEdge(from, GetI(e.To, color, n), 1);

                // Zmiany kolorów obsługujemy poprzez dodanie dodatkowych krawędzi prowadzących do miasta,
                // do którego możemy dostać się poprzez jedną zmianę koloru (sąsiedzi w grafie c)
                foreach (var colorChange in c.OutEdges(color))
                    graph.AddEdge(from, GetI(e.To, colorChange.To, n), colorChange.Weight + 1);
            }

            // Tworzymy sztuczny wierzchołek wejściowy, który będzie połączony ze wszystkimi startami we wszystkich
            // możliwych kolorach oraz wierzchołek końcowy, połączony z końcem w każdym kolorze. 
            for (int color = 0; color < n; color++)
            {
                foreach (int v in starts)
                    graph.AddEdge(start, GetI(v, color, n), 0);

                graph.AddEdge(GetI(target, color, n), end, 0);
            }

            // Wyszukiwanie najkrótszej ścieżki w grafie pomocnicznym
            var pathsInfo = Paths.Dijkstra(graph, start);
            var path = pathsInfo.GetPath(start, end);
            if (path == null)
                return (null, new int[0]);

            var list = new List<int>();
            for (int i = 1; i < path.Length - 1; i++)
                list.Add(path[i] / n);

            return (pathsInfo.GetDistance(start, end), list.ToArray());
        }
    }
}