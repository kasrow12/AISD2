using System;
using System.Collections.Generic;
using System.Drawing;
using ASD.Graphs;

namespace ASD2
{
    public class GraphColorer : MarshalByRefObject
    {
        /// <summary>
        /// Metoda znajduje kolorowanie zadanego grafu g używające najmniejsze możliwej liczby kolorów.
        /// </summary>
        /// <param name="g">Graf (nieskierowany)</param>
        /// <returns>Liczba użytych kolorów i kolorowanie (coloring[i] to kolor wierzchołka i). Kolory mogą być dowolnymi liczbami całkowitymi.</returns>
        public (int numberOfColors, int[] coloring) FindBestColoring(Graph g)
        {
            int n = g.VertexCount;
            if (n == 0)
                return (0, null);
            
            int[] coloring = new int[n];
            int colors = 1;

            bool IsSafe(int v, int c)
            {
                foreach (int i in g.OutNeighbors(v))
                    if (coloring[i] == c)
                        return false;
                return true;
            }
            
            // int[] vertices = Enumerable.Range(0, n).ToArray();
            // Array.Sort(vertices, (v1, v2) => g.OutNeighbors(vertices[v1]).Count().CompareTo(g.OutNeighbors(vertices[v2]).Count()));

            int[] avail = new int[n];
            for (int i = 0; i < n; i++)
                avail[i] = colors;

            int FindMin()
            {
                int umin = -1;
                int availmin = int.MaxValue;
                for (int i = 0; i < n; i++)
                {
                    if (coloring[i] == 0 && avail[i] < availmin)
                    {
                        umin = i;
                        availmin = avail[umin];
                    }
                }

                return umin;
            }

            bool[,] used = new bool[n, colors + 1];
            bool ColorGraph(int i)
            {
                if (i == -1)
                    return true;
                
                // Consider this vertex v and try different colors
                for (int c = 1; c <= colors; c++)
                {
                    // Check if assignment of color c to v is fine
                    // if (IsSafe(vertices[i], c))
                    if (!used[i, c])
                    {
                        List<int> changed = new List<int>();
                        
                        coloring[i] = c;
                        used[i, c] = true;
                        foreach (int u in g.OutNeighbors(i))
                        {
                            if (!used[u, c])
                            {
                                changed.Add(u);
                                used[u, c] = true;
                                avail[u]--;
                            }
                        }

                        // Recur to assign colors to the rest of the vertices
                        if (ColorGraph(FindMin()))
                            return true;

                        foreach (int u in changed)
                        {
                            used[u, c] = false;
                            avail[u]++;
                        }
                        used[i, c] = false;
                        coloring[i] = 0;
                    }
                }

                return false;
            }

            bool ColorAll()
            {
                for (int v = 0; v < n; v++)
                {
                    if (coloring[v] == 0)
                        if (!ColorGraph(v))
                            return false;
                }

                return true;
            }

            while (!ColorAll())
            {
                colors++;
                for (int i = 0; i < n; i++)
                    avail[i] = colors;

                // Console.WriteLine(colors);
                used = new bool[n, colors + 1];
            }
            
            return (colors, coloring);
        }

    }
}