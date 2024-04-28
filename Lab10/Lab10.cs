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
        /// <param name="debt">Ilość złota jaką bohaterowie muszą zebrać</param>
        /// <param name="roomGold">Ilość złota w poszczególnych pokojach</param>
        /// <returns>
        ///     Informację czy istnieje droga przez labirynt oraz tablicę reprezentującą kolejne wierzchołki na drodze.
        ///     W przypadku, gdy zwracany jest false, wartość tego pola powinna być null.
        /// </returns>
        public (bool routeExists, int[] route) FindEscape(Graph labyrinth, int startingTorches, int[] roomTorches,
            int debt, int[] roomGold)
        {
            int n = labyrinth.VertexCount;

            // Wartości początkowe
            var S = new List<int> { 0 };
            int curTorches = startingTorches + roomTorches[0];
            int curDebt = debt - roomGold[0];

            bool[] visited = new bool[n];
            visited[0] = true;

            bool FindPath(int last)
            {
                if (last == n - 1)
                    return curDebt <= 0;

                if (curTorches == 0)
                    return false;

                foreach (int u in labyrinth.OutNeighbors(last))
                {
                    // W tym etapie możemy wejść do każdego wierzchołka tylko raz
                    if (visited[u])
                        continue;

                    S.Add(u);
                    visited[u] = true;
                    curTorches += roomTorches[u] - 1;
                    curDebt -= roomGold[u];

                    if (FindPath(u))
                        return true;

                    visited[u] = false;
                    curTorches -= roomTorches[u] - 1; // -(-1) = +1
                    curDebt += roomGold[u];
                    S.RemoveAt(S.Count - 1);
                }

                return false;
            }

            bool routeExists = FindPath(0);
            return (routeExists, routeExists ? S.ToArray() : null);
        }

        /// <param name="labyrinth">Graf reprezentujący labirynt</param>
        /// <param name="startingTorches">Ilość pochodni z jaką startują bohaterowie</param>
        /// <param name="roomTorches">Ilość pochodni w poszczególnych pokojach</param>
        /// <param name="debt">Ilość złota jaką bohaterowie muszą zebrać</param>
        /// <param name="roomGold">Ilość złota w poszczególnych pokojach</param>
        /// <param name="dragonDelay">Opóźnienie z jakim wystartuje smok</param>
        /// <returns>
        ///     Informację czy istnieje droga przez labirynt oraz tablicę reprezentującą kolejne wierzchołki na drodze.
        ///     W przypadku, gdy zwracany jest false, wartość tego pola powinna być null.
        /// </returns>
        public (bool routeExists, int[] route) FindEscapeWithHeadstart(Graph labyrinth, int startingTorches,
            int[] roomTorches, int debt, int[] roomGold, int dragonDelay)
        {
            int n = labyrinth.VertexCount;

            // Wartości początkowe
            var S = new List<int> { 0 };
            int destTime = 1; // Zaczniemy od 1, żeby domyślne 0 było jako nieodwiedzone
            int curDebt = debt - roomGold[0];
            int curTorches = startingTorches + roomTorches[0];

            // (czas w którym odwiedzono wierzchołek, dług wtedy, liczba pochodni wtedy)
            var visited = new (int, int, int)[n]; // domyślnie (0, 0, 0)
            visited[0] = (destTime, curDebt, curTorches);

            bool FindPath(int last)
            {
                if (last == n - 1)
                    return curDebt <= 0;

                if (curTorches == 0)
                    return false;

                destTime++;
                curTorches--;

                foreach (int u in labyrinth.OutNeighbors(last))
                {
                    (int visitedAt, int debtThen, int torchesThen) = visited[u];
                    int lastDelay = dragonDelay;
                    int lastDestTime = destTime;

                    // Czy już tutaj byliśmy
                    if (visitedAt > 0)
                    {
                        // Smok może iść tylko po naszych śladach, nie znajdzie ścieżki złożonej z innych krawędzi.
                        // Zatem może nas dogonić tylko pomijając cykle.
                        // Sprawdzamy, czy ta różnica jest większa od delay'a, w takim przypadku znaczy to, że smok
                        // zdążył dostać się do tamtego wierzchołka i go zniszczyć (delay będzie się zmniejszał
                        // o te zamykane cykle). (nierówność ostra, bo smok jeszcze traci jeden ruch na początku)
                        if (destTime - visitedAt > dragonDelay)
                            continue;

                        // Nie ma sensu wchodzić z powrotem, jeżeli nie zebraliśmy więcej złota
                        // i nie zdobyliśmy dodatkowych pochodni. (Eliminujemy zbędne kręcenie się w kółko)
                        if (curDebt >= debtThen && curTorches <= torchesThen)
                            continue;

                        // Pomniejszamy delay o różnicę czasu i czasu wejścia, bo w tym momencie zrobiliśmy jakiś cykl,
                        // którego długość jest tą różnicą.
                        dragonDelay -= destTime - visitedAt;
                        // W tym przypadku też trzeba zmniejszyć destTime o długość tego cyklu (ustawić go na visitedAt),
                        // bo zamykając ten cykl, zmniejszyliśmy wyżej dragonDelay i jakby zapomniamy o nim. 
                        destTime = visitedAt;
                    }

                    S.Add(u);

                    // Zbieramy przedmioty, jeżeli jesteśmy tutaj za pierwszym razem
                    if (visitedAt == 0)
                    {
                        curDebt -= roomGold[u];
                        curTorches += roomTorches[u];
                    }

                    visited[u] = (destTime, curDebt, curTorches);

                    if (FindPath(u))
                        return true;
                    
                    if (visitedAt == 0)
                    {
                        curDebt += roomGold[u];
                        curTorches -= roomTorches[u];
                    }

                    visited[u] = (visitedAt, debtThen, torchesThen);

                    dragonDelay = lastDelay;
                    destTime = lastDestTime;
                    S.RemoveAt(S.Count - 1);
                }

                destTime--;
                curTorches++;
                return false;
            }

            bool routeExists = FindPath(0);
            return (routeExists, routeExists ? S.ToArray() : null);
        }
    }
}