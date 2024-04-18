using ASD.Graphs;
using System;
using System.Collections.Generic;

namespace ASD
{
    public class Lab08 : MarshalByRefObject
    {
        private (int val, DiGraph<int>, DiGraph<int>) GetFlow(int h, int l, int[,] pleasure)
        {
            // Maksymalna wysokość działki (możemy budować tylko w dolnotrójkącie)
            int d = Math.Min(h, l);

            var g = new DiGraph<int>(l * d + 2);
            int source = l * d;
            int target = l * d + 1;

            // Sumaryczna dostępna wartość zadowolenia
            int input = 0;
            for (int x = 0; x < l; x++)
            {
                int t = Math.Min(d, l - x);
                for (int y = 1; y < t; y++)
                {
                    int v = y * l + x;

                    // Krawędzie wychodzące z każdego bloczka o wadze 1 - dziurki durszlaka
                    g.AddEdge(v, target, 1);

                    // Przejścia niżej i na prawy ukos niżej 
                    g.AddEdge(v, v - l, int.MaxValue);
                    g.AddEdge(v, v - l + 1, int.MaxValue);

                    int p = pleasure[x, y];
                    if (p > 0)
                    {
                        input += p;
                        // Wlewamy wodę do durszlaka
                        g.AddEdge(source, v, p);
                    }
                }

                // + Krawędzie dla dolnego wiersza
                g.AddEdge(x, target, 1);

                int pp = pleasure[x, 0];
                if (pp > 0)
                {
                    input += pp;
                    g.AddEdge(source, x, pp);
                }
            }

            (int val, var flow) = Flows.FordFulkerson(g, source, target);
            return (input - val, flow, g);
        }

        /// <summary>Etap I: prace przedprojektowe</summary>
        /// <param name="l">Długość działki, którą dysponuje Kameleon Kazik.</param>
        /// <param name="h">Maksymalna wysokość budowli.</param>
        /// <param name="pleasure">Tablica rozmiaru [l,h] zawierająca wartości zadowolenia p(x,y) dla każdych x i y.</param>
        /// <returns>Odpowiedź na pytanie, czy istnieje budowla zadowalająca Kazika.</returns>
        public bool Stage1ExistsBuilding(int l, int h, int[,] pleasure)
        {
            (int r, _, _) = GetFlow(h, l, pleasure);
            return r > 0;
        }

        /// <summary>Etap II: kompletny projekt</summary>
        /// <param name="l">Długość działki, którą dysponuje Kameleon Kazik.</param>
        /// <param name="h">Maksymalna wysokość budowli.</param>
        /// <param name="pleasure">Tablica rozmiaru [l,h] zawierająca wartości zadowolenia p(x,y) dla każdych x i y.</param>
        /// <param name="blockOrder">
        ///     Argument wyjściowy, w którym należy zwrócić poprawną kolejność ustawienia bloków w znalezionym rozwiązaniu;
        ///     kolejność jest poprawna, gdy przed blokiem (x,y) w tablicy znajdują się bloki (x,y-1) i (x+1,y-1) lub gdy y=0.
        ///     Ustawiane bloki powinny mieć współrzędne niewychodzące poza granice obszaru budowy (0<=x<l, 0<=y<h).
        ///         W przypadku braku rozwiązania należy zwrócić null.
        /// </param>
        /// <returns>Maksymalna wartość zadowolenia z budowli; jeśli nie istnieje budowla zadowalająca Kazika, zależy zwrócić null.</returns>
        public int? Stage2GetOptimalBuilding(int l, int h, int[,] pleasure, out (int x, int y)[] blockOrder)
        {
            (int val, var flow, var g) = GetFlow(h, l, pleasure);

            if (val <= 0)
            {
                blockOrder = null;
                return null;
            }

            int d = Math.Min(h, l);
            int source = l * d;

            // Tworzenie ala sieci rezydualnej (bez znaczenia wagi)
            foreach (var e in flow.DFS().SearchFrom(source))
            {
                g.AddEdge(e.To, e.From, 1);
        
                if (e.Weight == g.GetEdgeWeight(e.From, e.To))
                    g.RemoveEdge(e.From, e.To);
            }
        
            bool[] visited = new bool[g.VertexCount];
        
            // Z własności maksymalnego przepływu/minimalnego przekroju,
            // bierzemy przekrój połączony ze źródłem.
            foreach (var e in g.DFS().SearchFrom(source))
                visited[e.To] = true;

            var list = new List<(int, int)>();
            for (int y = 0; y < d; y++)
            {
                for (int x = 0; x < l - y; x++)
                {
                    if (visited[y * l + x])
                        list.Add((x, y));
                }
            }

            blockOrder = list.ToArray();
            return val;
        }
    }
}