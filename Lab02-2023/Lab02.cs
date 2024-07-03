using System.Text;

namespace Lab02;

public class PatternMatching : MarshalByRefObject
{
    /// <summary>
    ///     Etap 1 - wyznaczenie trasy, zgodnie z którą robot przemieści się z pozycji poczatkowej (0,0) na pozycję docelową
    ///     (-n-1, m-1)
    /// </summary>
    /// <param name="n">wysokość prostokąta</param>
    /// <param name="m">szerokość prostokąta</param>
    /// <param name="obstacles">tablica ze współrzędnymi przeszkód</param>
    /// <returns>
    ///     krotka (bool result, string path) - result ma wartość true jeżeli trasa istnieje, false wpp., path to wynikowa
    ///     trasa
    /// </returns>
    public (bool result, string path) Lab02Stage1(int n, int m, (int, int)[] obstacles)
    {
        int i = n - 1;
        int j = m - 1;

        char[,] T = new char[n, m];
        T[i, j] = 'O';

        int startingColumn = j;
        while (true)
        {
            // Console.WriteLine($"{i}, {j}");
            if (obstacles.Contains((i, j))) // można by było zapisać w tablicy i odczyt O(1)
                T[i, j] = 'X';
            else if (i + 1 < n && T[i + 1, j] != 'X')
                T[i, j] = 'D';
            else if (j + 1 < m && T[i, j + 1] != 'X')
                T[i, j] = 'R';
            else if (T[i, j] != 'O')
                T[i, j] = 'X';

            if (i == 0 && j == 0)
                break;

            if (j == m - 1 || i == 0)
            {
                // j = m - (n - i) - 1;
                j = --startingColumn;
                i = n - 1; // last row
                if (j < 0)
                {
                    i += j; // i -= -j;
                    j = 0;
                }
            }
            else // idziemy w prawy górny
            {
                i--;
                j++;
            }
        }

        if (T[0, 0] == 'X')
            return (false, "");

        var sb = new StringBuilder("");
        i = 0;
        j = 0;
        while (i != n - 1 || j != m - 1)
        {
            sb.Append(T[i, j]);
            if (T[i, j] == 'D')
                i++;
            else
                j++;
        }

        return (true, sb.ToString());
    }

    /// <summary>
    ///     Etap 2 - wyznaczenie trasy realizującej zadany wzorzec, zgodnie z którą robot przemieści się z pozycji poczatkowej
    ///     (0,0) na pozycję docelową (-n-1, m-1)
    /// </summary>
    /// <param name="n">wysokość prostokąta</param>
    /// <param name="m">szerokość prostokąta</param>
    /// <param name="pattern">zadany wzorzec</param>
    /// <param name="obstacles">tablica ze współrzędnymi przeszkód</param>
    /// <returns>
    ///     krotka (bool result, string path) - result ma wartość true jeżeli trasa istnieje, false wpp., path to wynikowa
    ///     trasa
    /// </returns>
    public (bool result, string path) Lab02Stage2(int n, int m, string pattern, (int, int)[] obstacles)
    {
        // robot musi realizować cały wzorzec, jedynie gwiazdka może być 0 ruchów
        if (n == 1 && m == 1)
            return (pattern[0] == '*', "");

        int k = pattern.Length;
        // inicjalizowana zerami => brak trasy
        char[,,] T = new char[n, m, k + 1];
        T[0, 0, 0] = 'O';

        int i, j;
        // idziemy od lewego górnego, wierszami od lewej do prawej, bo można się poruszać tylko prawo/dół
        for (i = 0; i < n; i++)
        {
            for (j = 0; j < m; j++)
            {
                if (obstacles.Contains((i, j))) // można zrobić w macierzy, dla złożoności
                    continue;

                // spróbujemy użyć l-tego znaku we wzorcu
                for (int l = 1; l <= k; l++)
                {
                    // nie dostaliśmy się (l-1)-szym
                    if (T[i, j, l - 1] == 0)
                        continue;

                    if (i + 1 < n && (pattern[l - 1] == 'D' || pattern[l - 1] == '?'))
                        T[i + 1, j, l] = 'D';

                    if (i + 1 < n && pattern[l - 1] == '*')
                    {
                        T[i + 1, j, l] = 'D'; // ala ostatnie wykorzystanie gwiazdki
                        T[i + 1, j, l - 1] =
                            'd'; // udajemy, że dostaliśmy się (l-1)-szym, a tak naprawdę poprzednim użyciem gwiazdki
                    }

                    if (j + 1 < m && (pattern[l - 1] == 'R' || pattern[l - 1] == '?'))
                        T[i, j + 1, l] = 'R';

                    if (j + 1 < m && pattern[l - 1] == '*')
                    {
                        T[i, j + 1, l] = 'R';
                        T[i, j + 1, l - 1] = 'r';
                    }
                }
            }
        }

        if (T[n - 1, m - 1, k] == 0)
            return (false, "");

        var sb = new StringBuilder("");
        i = n - 1;
        j = m - 1;
        while (i != 0 || j != 0)
        {
            sb.Insert(0, char.ToUpper(T[i, j, k]));

            if (T[i, j, k] == 'D')
            {
                i--;
                k--;
            }
            else if (T[i, j, k] == 'd')
            {
                i--;
            }
            else if (T[i, j, k] == 'R')
            {
                j--;
                k--;
            }
            else if (T[i, j, k] == 'r')
            {
                j--;
            }
        }

        return (true, sb.ToString());
    }
}