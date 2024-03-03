using System;
using System.Collections.Generic;

namespace ASD
{
    public class Lab02 : MarshalByRefObject
    {
        /// <summary>
        ///     Etap 1 - wyznaczenie najtańszej trasy, zgodnie z którą pionek przemieści się z pozycji poczatkowej (0,0) na pozycję
        ///     docelową
        /// </summary>
        /// <param name="n">wysokość prostokąta</param>
        /// <param name="m">szerokość prostokąta</param>
        /// <param name="moves">
        ///     tablica z dostępnymi ruchami i ich kosztami (di - o ile zwiększamy numer wiersza, dj - o ile
        ///     zwiększamy numer kolumnj, cost - koszt ruchu)
        /// </param>
        /// <returns>
        ///     (bool result, int cost, (int, int)[] path) - result ma wartość true jeżeli trasa istnieje, false wpp., cost to
        ///     minimalny koszt, path to wynikowa trasa
        /// </returns>
        public (bool result, int cost, (int i, int j)[] path) Lab02Stage1(int n, int m,
            ((int di, int dj) step, int cost)[] moves)
        {
            int[,] T = new int[n, m];
            int[,] used = new int[n, m];

            // Warunki początkowe
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                    T[i, j] = int.MaxValue;

            T[0, 0] = 0;

            // Szukanie do przodu od góry
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (T[i, j] == int.MaxValue)
                        continue;

                    for (int k = 0; k < moves.Length; k++)
                    {
                        int x = i + moves[k].step.di;
                        int y = j + moves[k].step.dj;
                        if (x >= n || y >= m)
                            continue;

                        if (T[i, j] + moves[k].cost < T[x, y])
                        {
                            T[x, y] = T[i, j] + moves[k].cost;
                            used[x, y] = k;
                        }
                    }
                }
            }

            // Wyszukanie kolumny z najmniejszym kosztem w ostatnim wierszu
            int minCol = 0;
            for (int j = 1; j < m; j++)
            {
                if (T[n - 1, j] < T[n - 1, minCol])
                    minCol = j;
            }

            // Nie ma trasy do ostatniego wiersza
            if (T[n - 1, minCol] == int.MaxValue)
                return (false, int.MaxValue, null);

            // Wyszukiwanie ścieżki
            int x1 = n - 1;
            int y1 = minCol;
            var path = new (int, int)[n + m];

            int t = 0;
            while (true)
            {
                path[t++] = (x1, y1);
                if (x1 == 0 && y1 == 0)
                    break;

                (int di, int dj) = moves[used[x1, y1]].step;
                x1 -= di;
                y1 -= dj;
            }

            // Odwrócenie ścieżki
            var newPath = new (int, int)[t];
            for (int a = 0; a < t; a++) newPath[a] = path[t - a - 1];

            return (true, T[n - 1, minCol], newPath);
        }


        /// <summary>
        ///     Etap 2 - wyznaczenie najtańszej trasy, zgodnie z którą pionek przemieści się z pozycji poczatkowej (0,0) na pozycję
        ///     docelową - dodatkowe założenie, każdy ruch może być wykonany co najwyżej raz
        /// </summary>
        /// <param name="n">wysokość prostokąta</param>
        /// <param name="m">szerokość prostokąta</param>
        /// <param name="moves">
        ///     tablica z dostępnymi ruchami i ich kosztami (di - o ile zwiększamy numer wiersza, dj - o ile
        ///     zwiększamy numer kolumnj, cost - koszt ruchu)
        /// </param>
        /// <returns>
        ///     (bool result, int cost, (int, int)[] path) - result ma wartość true jeżeli trasa istnieje, false wpp., cost to
        ///     minimalny koszt, path to wynikowa trasa
        /// </returns>
        public (bool result, int cost, (int i, int j)[] pat) Lab02Stage2(int n, int m,
            ((int di, int dj) step, int cost)[] moves)
        {
            int[,,] T = new int[n, m, moves.Length + 1];

            // Warunki początkowe
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                    for (int k = 0; k <= moves.Length; k++)
                        T[i, j, k] = int.MaxValue;

            for (int j = 0; j < m; j++)
                T[n - 1, j, 0] = 0;

            // Wyszukiwanie w tył
            // Warto zauważyć, że kolejność ruchów nie ma znaczenia
            for (int k = 1; k <= moves.Length; k++)
            {
                ((int di, int dj), int cost) = moves[k - 1];

                for (int i = n - 1; i >= 0; i--)
                {
                    for (int j = m - 1; j >= 0; j--)
                    {
                        if (T[i, j, k - 1] == int.MaxValue)
                            continue;

                        // Przepisanie kosztu, taniej dostać się tutaj bez tego ruchu
                        if (T[i, j, k] > T[i, j, k - 1])
                            T[i, j, k] = T[i, j, k - 1];

                        int x = i - di;
                        int y = j - dj;
                        if (x < 0 || y < 0)
                            continue;

                        // Czy użycie tego ruchu będzie tańsze niż trasa bez jego użycia
                        if (T[x, y, k - 1] > T[i, j, k - 1] + cost)
                            T[x, y, k] = T[i, j, k - 1] + cost;
                    }
                }
            }

            if (T[0, 0, moves.Length] == int.MaxValue)
                return (false, int.MaxValue, null);

            // Znajdowanie ścieżki
            var path = new List<(int, int)> { (0, 0) };
            int x1 = 0;
            int y1 = 0;
            int k1 = moves.Length;
            while (true)
            {
                if (x1 == n - 1)
                    break;

                // Nie wykorzystano tego ruchu (koszt pozostał ten sam, a koszt ruchu nie może być zerem)
                if (T[x1, y1, k1] == T[x1, y1, k1 - 1])
                {
                    k1--;
                    continue;
                }

                (int di, int dj) = moves[k1 - 1].step;
                x1 += di;
                y1 += dj;
                k1--;
                path.Add((x1, y1));
            }

            return (true, T[0, 0, moves.Length], path.ToArray());
        }
    }
}