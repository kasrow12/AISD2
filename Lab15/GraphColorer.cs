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
            
            // Domyślnie zera, czyli brak koloru
            int[] coloring = new int[n];
            int maxColors = 1;

            int[] numOfAvailableColors = new int[n];
            for (int i = 0; i < n; i++)
                numOfAvailableColors[i] = maxColors;

            // Znajduje wierzchołek z najmniejszą liczbą dostępnych kolorów
            // (nie bierze pod uwagę jednej późniejszej optymalizacji)
            int FindMin()
            {
                int availMinIndex = -1;
                int availMin = int.MaxValue;
                for (int i = 0; i < n; i++)
                {
                    if (coloring[i] == 0 && numOfAvailableColors[i] < availMin)
                    {
                        availMinIndex = i;
                        availMin = numOfAvailableColors[availMinIndex];
                    }
                }

                return availMinIndex;
            }

            // +1, bo kolory indeksujemy od 1 (0 tak jakby nullem)
            bool[,] used = new bool[n, maxColors + 1];
            int numOfColoredVertices = 0;
            
            bool ColorGraph(int i)
            {
                numOfColoredVertices++;
                if (i == -1) // FindMin zwraca -1, kiedy nie ma dostępnego wierzchołka
                {
                    numOfColoredVertices--;
                    return true;
                }
                
                // Gdy kolorujemy n-ty wierzchołek, możemy go pokolorować na max. n kolory,
                // albo mniej, jeśli globalne max jest mniejsze - stąd te Math.Min
                for (int c = 1; c <= Math.Min(maxColors, numOfColoredVertices); c++)
                {
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
                                numOfAvailableColors[u]--;
                            }
                        }

                        if (ColorGraph(FindMin()))
                        {
                            numOfColoredVertices--;
                            return true;
                        }

                        foreach (int u in changed)
                        {
                            used[u, c] = false;
                            numOfAvailableColors[u]++;
                        }
                        used[i, c] = false;
                        coloring[i] = 0;
                    }
                }

                numOfColoredVertices--;
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

            // Sprawdzamy możliwe k-kolorowania od 1 wzwyż, dopóki nie pokolorujemy
            while (!ColorAll())
            {
                maxColors++;
                for (int i = 0; i < n; i++)
                    numOfAvailableColors[i] = maxColors;

                used = new bool[n, maxColors + 1];
            }
            
            return (maxColors, coloring);
        }

    }
}