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
            
            int[] vertices = Enumerable.Range(0, n).ToArray();
            Array.Sort(vertices, (v1, v2) => g.OutNeighbors(v1).Count().CompareTo(g.OutNeighbors(v2).Count()));
            
            bool ColorGraph(int i)
            {
                // Base case: If all vertices are assigned a color then return true
                if (i == n)
                    return true;

                // Consider this vertex v and try different colors
                for (int c = 1; c <= colors; c++)
                {
                    // Check if assignment of color c to v is fine
                    if (IsSafe(vertices[i], c))
                    {
                        coloring[vertices[i]] = c;

                        // Recur to assign colors to the rest of the vertices
                        if (ColorGraph(i + 1))
                            return true;

                        // If assigning color c doesn't lead to a solution then remove it (backtrack)
                        coloring[vertices[i]] = 0;
                    }
                }

                // If no color can be assigned to this vertex then return false
                return false;
            }

            while (!ColorGraph(0) && colors <= n)
            {
                Console.WriteLine(colors);
                colors++;
            }
            
            return (colors, coloring);
        }

    }
}