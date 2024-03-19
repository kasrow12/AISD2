using ASD.Graphs;
using System;
using System.Collections.Generic;

namespace ASD
{
    public class Lab04 : MarshalByRefObject
    {
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
            bool[] visited = new bool[graph.VertexCount];

            var q = new Queue<int>();
            q.Enqueue(miastoStartowe);
            visited[miastoStartowe] = true;

            // Wykonujemy BFS'a tak głęboko, ile mamy godzin na jazdę
            for (int i = 8; i < K; i++)
            {
                // Należy zapamiętać ile było elementów w kolejce na tej głębokości, będą dochodzić w trakcie kolejne
                for (int t = q.Count; t > 0; t--)
                {
                    int v = q.Dequeue();
                    foreach (int u in graph.OutNeighbors(v))
                    {
                        if (!visited[u])
                        {
                            q.Enqueue(u);
                            visited[u] = true;
                        }
                    }
                }
            }

            // Możliwe do odwiedzenia miasta. Przy okazji spełniamy warunek sortowania rosnącego
            var cities = new List<int>();
            for (int i = 0; i < graph.VertexCount; i++)
            {
                if (visited[i])
                    cities.Add(i);
            }

            return cities.ToArray();
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
            visitedAt[miastoStartowe] = 8;

            // Dijkstra
            while (q.Count > 0)
            {
                (int v, int hour) = q.Extract();

                // Znaleziono już lepszą trasę, niż ta znajdująca się jeszcze w kolejce
                if (hour > visitedAt[v])
                    continue;

                foreach (var e in graph.OutEdges(v))
                {
                    int arrival = e.Weight + 1;
                    // Odjazd później && Dojazd w czasie && Będziemy tam szybciej niż dotychczas
                    if (e.Weight >= hour && arrival <= K && arrival < visitedAt[e.To])
                    {
                        // Nie ma sensu dodawać, jak nigdzie stamtąd nie pojedziemy
                        if (arrival != K)
                            q.Insert((e.To, arrival), arrival);

                        visitedAt[e.To] = arrival;
                    }
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
}