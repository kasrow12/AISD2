using System;
using ASD.Graphs;
using System.Collections.Generic;

namespace ASD
{
    public class Lab04 : MarshalByRefObject
    {
        /// <summary>
        /// Etap 1 - Szukanie mozliwych do odwiedzenia miast z grafu skierowanego
        /// przy zalozeniu, ze pociagi odjezdzaja co godzine.
        /// </summary>
        /// <param name="graph">Graf skierowany przedstawiający siatke pociagow</param>
        /// <param name="miastoStartowe">Numer miasta z ktorego zaczyna sie podroz pociagiem</param>
        /// <param name="K">Godzina o ktorej musi zakonczyc sie nasza podroz</param>
        /// <returns>Tablica numerow miast ktore mozna odwiedzic. Posortowana rosnaco.</returns>
        public int[] Lab04Stage1(DiGraph graph, int miastoStartowe, int K)
        {
            end = K;
            visited = new bool[graph.VertexCount];
            g = graph;
            
            Dfs(miastoStartowe, 8);

            List<int> list = new List<int>();
            for (int i = 0; i < graph.VertexCount; i++)
            {
                if (visited[i])
                    list.Add(i);
            }

            int[] miasta = list.ToArray();
            Array.Sort(miasta);
            return miasta;
        }

        private int end;
        private bool[] visited;
        private DiGraph g;
        void Dfs(int v, int h)
        {
            if (h > end)
                return;

            visited[v] = true;
            foreach (var u in g.OutNeighbors(v))
            {
                Dfs(u, h + 1);
            }
        }

        /// <summary>
        /// Etap 2 - Szukanie mozliwych do odwiedzenia miast z grafu skierowanego.
        /// Waga krawedzi oznacza, ze pociag rusza o tej godzinie
        /// </summary>
        /// <param name="graph">Wazony graf skierowany przedstawiający siatke pociagow</param>
        /// <param name="miastoStartowe">Numer miasta z ktorego zaczyna sie podroz pociagiem</param>
        /// <param name="K">Godzina o ktorej musi zakonczyc sie nasza podroz</param>
        /// <returns>Tablica numerow miast ktore mozna odwiedzic. Posortowana rosnaco.</returns>
        public int[] Lab04Stage2(DiGraph<int> graph, int miastoStartowe, int K)
        {
            // TODO
            int[] miastaMozliweDoOdwiedzenia = new int[] { miastoStartowe };
            return miastaMozliweDoOdwiedzenia;
        }
    }
}