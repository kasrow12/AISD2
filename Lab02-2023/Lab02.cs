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
        var T = new char[n, m];
        T[n - 1, m - 1] = 'O';

        var i = n - 1;
        var j = m - 1;
        var starting_bot = j;
        while (true)
        {
            // Console.WriteLine($"{i}, {j}");
            if (obstacles.Contains((i, j)))
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
                j = --starting_bot;
                i = n - 1;
                if (j < 0)
                {
                    i -= 0 - j;
                    j = 0;
                }
            }
            else
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
        return (false, "");
    }
}