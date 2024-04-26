using ASD.Graphs;
using System;
using System.Collections.Generic;

namespace ASD
{
    public class Lab10 : MarshalByRefObject
    {
        /// <param name="labyrinth">Graf reprezentujący labirynt</param>
        /// <param name="startingTorches">Ilość pochodni z jaką startują bohaterowie</param>
        /// <param name="roomTorches">Ilość pochodni w poszczególnych pokojach</param>
        /// <param name="debt>">Ilość złota jaką bohaterowie muszą zebrać</param>
        /// <param name="roomGold">Ilość złota w poszczególnych pokojach</param>
        /// <returns>
        ///     Informację czy istnieje droga przez labirytn oraz tablicę reprezentującą kolejne wierzchołki na drodze. W
        ///     przypadku, gdy zwracany jest false, wartość tego pola powinna być null.
        /// </returns>
        public (bool routeExists, int[] route) FindEscape(Graph labyrinth, int startingTorches, int[] roomTorches, int debt,
            int[] roomGold)
        {
            int n = labyrinth.VertexCount;
            var S = new List<int>() { 0 };
            int curTorches = startingTorches + roomTorches[0];
            int curDebt = debt - roomGold[0];

            bool[] visited = new bool[n];
            visited[0] = true;

            bool FindPath(int last)
            {
                if (last == n - 1)
                {
                    if (curDebt > 0)
                        return false;
                
                    return true;
                }

                if (curTorches == 0)
                    return false;

                foreach (int u in labyrinth.OutNeighbors(last))
                {
                    if (visited[u]) 
                        continue;

                    S.Add(u);
                    visited[u] = true;
                    curTorches--;
                    curTorches += roomTorches[u];
                    curDebt -= roomGold[u];

                    if (FindPath(u))
                        return true;

                    visited[u] = false;
                    curTorches -= roomTorches[u];
                    curTorches++;
                    curDebt += roomGold[u];
                    S.RemoveAt(S.Count - 1);
                }

                return false;
            }

            bool ret = FindPath(0);
            if (!ret)
                return (false, null);

            return (true, S.ToArray());
        }

        /// <param name="labyrinth">Graf reprezentujący labirynt</param>
        /// <param name="startingTorches">Ilość pochodni z jaką startują bohaterowie</param>
        /// <param name="roomTorches">Ilość pochodni w poszczególnych pokojach</param>
        /// <param name="debt">Ilość złota jaką bohaterowie muszą zebrać</param>
        /// <param name="roomGold">Ilość złota w poszczególnych pokojach</param>
        /// <param name="dragonDelay">Opóźnienie z jakim wystartuje smok</param>
        /// <returns>
        ///     Informację czy istnieje droga przez labirynt oraz tablicę reprezentującą kolejne wierzchołki na drodze. W
        ///     przypadku, gdy zwracany jest false, wartość tego pola powinna być null.
        /// </returns>
        public (bool routeExists, int[] route) FindEscapeWithHeadstart(Graph labyrinth, int startingTorches,
            int[] roomTorches, int debt, int[] roomGold, int dragonDelay)
        {
        
            int n = labyrinth.VertexCount;
            var S = new List<int>() { 0 };
            int curTorches = startingTorches + roomTorches[0];
            int curDebt = debt - roomGold[0];

            int[] visited = new int[n];
            bool[] usedTorches = new bool[n];
            visited[0] = 0;
            int curTime = 0;
            usedTorches[0] = true;

            bool FindPath(int last)
            {
                if (last == n - 1)
                {
                    if (curDebt > 0)
                        return false;
                
                    return true;
                }

                if (curTorches == 0)
                    return false;

                foreach (int u in labyrinth.OutNeighbors(last))
                {
                    bool changed = false;
                    if (visited[u] > 0)
                    {
                        if (curTime - visited[u] >= dragonDelay)
                            continue;
                        changed = true;
                        dragonDelay -= curTime - visited[u];
                    }

                    S.Add(u);
                    int lastVisited = visited[u];
                    visited[u] = curTime;
                    curTorches--;
                    bool used = false;
                    if (!usedTorches[u])
                    {
                        used = true;
                        curTorches += roomTorches[u];
                        usedTorches[u] = true;
                        curDebt -= roomGold[u];
                    }
                    curTime++;

                    if (FindPath(u))
                        return true;

                    curTime--;
                    visited[u] = lastVisited;
                    if (used)
                    {
                        curTorches -= roomTorches[u];
                        usedTorches[u] = false;
                        curDebt += roomGold[u]; 
                    }
                    curTorches++;
                    if (changed)
                        dragonDelay += curTime - visited[u];
                
                    S.RemoveAt(S.Count - 1);
                }

                return false;
            }

            bool ret = FindPath(0);
            if (!ret)
                return (false, null);

            // Console.WriteLine(String.Join(',', S));

            return (true, S.ToArray());
        }
    }
}