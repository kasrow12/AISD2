using System;

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
            var T = new int[n, m];
            var used = new int[n, m];

            // Warunki początkowe
            for (var i = 0; i < n; i++)
            for (var j = 0; j < m; j++)
                T[i, j] = int.MaxValue;

            T[0, 0] = 0;

            // Szukanie do przodu od góry
            for (var i = 0; i < n; i++)
            for (var j = 0; j < m; j++)
            {
                if (T[i, j] == int.MaxValue)
                    continue;

                for (var k = 0; k < moves.Length; k++)
                {
                    var x = i + moves[k].step.di;
                    var y = j + moves[k].step.dj;
                    if (x >= n || y >= m)
                        continue;

                    if (T[i, j] + moves[k].cost < T[x, y])
                    {
                        T[x, y] = T[i, j] + moves[k].cost;
                        used[x, y] = k;
                    }
                }
            }

            // Wyszukanie kolumny z najmniejszym kosztem w ostatnim wierszu
            var minCol = 0;
            for (var j = 1; j < m; j++)
                if (T[n - 1, j] < T[n - 1, minCol])
                    minCol = j;

            // Nie ma trasy do ostatniego wiersza
            if (T[n - 1, minCol] == int.MaxValue)
                return (false, int.MaxValue, null);

            // Wyszukiwanie ścieżki
            var x1 = n - 1;
            var y1 = minCol;
            var path = new (int, int)[n + m];

            var t = 0;
            while (true)
            {
                path[t++] = (x1, y1);
                if (x1 == 0 && y1 == 0)
                    break;

                var (di, dj) = moves[used[x1, y1]].step;
                x1 -= di;
                y1 -= dj;
            }

            // Odwrócenie ścieżki
            var newPath = new (int, int)[t];
            for (var a = 0; a < t; a++)
                newPath[a] = path[t - a - 1];

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
            return (false, int.MaxValue, null);
        }
    }
}