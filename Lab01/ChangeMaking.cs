
using System;

namespace ASD
{

    class ChangeMaking
    {

        /// <summary>
        /// Metoda wyznacza rozwiązanie problemu wydawania reszty przy pomocy minimalnej liczby monet
        /// bez ograniczeń na liczbę monet danego rodzaju
        /// </summary>
        /// <param name="amount">Kwota reszty do wydania</param>
        /// <param name="coins">Dostępne nominały monet</param>
        /// <param name="change">Liczby monet danego nominału użytych przy wydawaniu reszty</param>
        /// <returns>Minimalna liczba monet potrzebnych do wydania reszty</returns>
        /// <remarks>
        /// coins[i]  - nominał monety i-tego rodzaju
        /// change[i] - liczba monet i-tego rodzaju (nominału) użyta w rozwiązaniu
        /// Jeśli dostepnymi monetami nie da się wydać danej kwoty to change = null,
        /// a metoda również zwraca null
        ///
        /// Wskazówka/wymaganie:
        /// Dodatkowa uzyta pamięć powinna (musi) być proporcjonalna do wartości amount ( czyli rzędu o(amount) )
        /// </remarks>
        public int? NoLimitsDynamic(int amount, int[] coins, out int[] change)
        {
            int[] T = new int[amount + 1];
            // T[0] = 0; // można wydać zero
            int[] used = new int[amount + 1];  
            
            for (int i = 1; i <= amount; i++)
            {
                int min = int.MaxValue;
                for (int j = 0; j < coins.Length; j++)
                {
                    if (coins[j] > i)
                        continue;
                    
                    int t = T[i - coins[j]];
                    if (t == int.MaxValue)
                        continue;

                    if (t + 1 < min)
                    {
                        min = t + 1;
                        used[i] = j;
                    }
                }

                T[i] = min;
            }

            if (T[amount] == int.MaxValue)
            {
                change = null;  
                return null;
            }

            int it = amount;
            change = new int[coins.Length];
            while (it > 0)
            {
                change[used[it]]++;
                it -= coins[used[it]];
            }
            
            return T[amount];      
        }

        /// <summary>
        /// Metoda wyznacza rozwiązanie problemu wydawania reszty przy pomocy minimalnej liczby monet
        /// z uwzględnieniem ograniczeń na liczbę monet danego rodzaju
        /// </summary>
        /// <param name="amount">Kwota reszty do wydania</param>
        /// <param name="coins">Dostępne nominały monet</param>
        /// <param name="limits">Liczba dostępnych monet danego nomimału</param>
        /// <param name="change">Liczby monet danego nominału użytych przy wydawaniu reszty</param>
        /// <returns>Minimalna liczba monet potrzebnych do wydania reszty</returns>
        /// <remarks>
        /// coins[i]  - nominał monety i-tego rodzaju
        /// limits[i] - dostepna liczba monet i-tego rodzaju (nominału)
        /// change[i] - liczba monet i-tego rodzaju (nominału) użyta w rozwiązaniu
        /// Jeśli dostepnymi monetami nie da się wydać danej kwoty to change = null,
        /// a metoda również zwraca null
        ///
        /// Wskazówka/wymaganie:
        /// Dodatkowa uzyta pamięć powinna (musi) być proporcjonalna do wartości iloczynu amount*(liczba rodzajów monet)
        /// ( czyli rzędu o(amount*(liczba rodzajów monet)) )
        /// </remarks>
        public int? Dynamic(int amount, int[] coins, int[] limits, out int[] change)
        {
            int[,] T = new int[amount + 1, coins.Length];
            
            // used będzie miało "ile razy wziąć monetę z tej kolumny"
            int[,] used = new int[amount + 1, coins.Length];
            for (int i = 1; i <= amount; i++)
                for (int j = 0; j < coins.Length; j++) 
                    T[i, j] = int.MaxValue;           
            
            for (int i = 1; i <= amount; i++)
                for (int j = 0; j < coins.Length; j++) 
                    used[i, j] = -1;           
            
            // wypełnienie pierwszej kolumny
            for (int n = 1; n <= limits[0] && n * coins[0] <= amount; n++)
            {
                T[n * coins[0], 0] = n;
                used[n * coins[0], 0] = n;
            }

            for (int j = 1; j < coins.Length; j++)
            {
                for (int i = 0; i <= amount; i++)
                {
                    if (T[i, j - 1] == int.MaxValue)
                        continue;

                    for (int k = 0; k <= limits[j]; k++)
                    {
                        int c = i + k * coins[j];
                        if (c > amount)
                            break;
                        
                        if (T[i, j - 1] + k < T[c, j])
                        {
                            T[c, j] = T[i, j - 1] + k;
                            if (k > 0)
                                used[c, j] = k;
                            else 
                                used[c, j] = -1;
                        }
                    }
                }
            }

            int res = T[amount, coins.Length - 1];
            if (res == int.MaxValue)
            {
                change = null;
                return null;
            }

            change = new int[coins.Length];

            int x = amount;
            int y = coins.Length - 1;
            while (x > 0 && y >= 0)
            {
                if (used[x, y] > 0)
                {
                    change[y] += used[x, y]; // ile monet tego typu
                    x -= used[x, y] * coins[y]; // cofamy się
                    y--;                        // i do poprzedniej monety
                }
                else
                    y--; // bez monety
            }
            
            return res;
        }

    }

}
