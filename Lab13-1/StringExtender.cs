using System;
using System.Text;

namespace Lab15
{
    public static class stringExtender
    {
        /// <summary>
        /// Metoda zwraca okres słowa s, tzn. najmniejszą dodatnią liczbę p taką, że s[i]=s[i+p] dla każdego i od 0 do |s|-p-1.
        /// 
        /// Metoda musi działać w czasie O(|s|)
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        static public int Period(this string s)
        {
            return s.Length - ComputePrefix(s)[s.Length];
        }

        /// <summary>
        /// Metoda wyznacza największą potęgę zawartą w słowie s.
        /// 
        /// Jeżeli x jest słowem, wówczas przez k-tą potęgę słowa x rozumiemy k-krotne powtórzenie słowa x
        /// (na przykład xyzxyzxyz to trzecia potęga słowa xyz).
        /// 
        /// Należy zwrócić największe k takie, że k-ta potęga jakiegoś słowa jest zawarta w s jako spójny podciąg.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="startIndex">Pierwszy indeks fragmentu zawierającego znalezioną potęgę</param>
        /// <param name="endIndex">Pierwszy indeks po fragmencie zawierającym znalezioną potęgę</param>
        /// <returns></returns>
        static public int MaxPower(this string s, out int startIndex, out int endIndex)
        {
            startIndex = 0;
            endIndex = 0;
            int maxPow = 1;
            int n = s.Length;
            
            for (int i = 0; i < n; i++)
            {
                int[] prefix = ComputePrefix(s[i..]);
                for (int j = 2; j <= n - i; j++)
                {
                    // czy długość jest podzielna przez okres
                    int period = (j - prefix[j]);
                    if (j % period == 0 && j / period > maxPow)
                    {
                        maxPow = j / period;
                        startIndex = i;
                        endIndex = i + j;
                    }
                }
            }

            return maxPow;
        }
        
        static public int[] ComputePrefix(string pattern)
        {
            // zwraca dł. n + 1 !
            int[] preifx = new int[pattern.Length + 1];
            int k = 0;
            for (int q = 2; q <= pattern.Length; q++)
            {
                while (k > 0 && pattern[k] != pattern[q - 1])
                    k = preifx[k];
        
                if (pattern[k] == pattern[q - 1])
                    k++;
        
                preifx[q] = k;
            }
        
            return preifx;
        }
        
        // unused
        static public List<int> KMP(string pattern, string text)
        {
            var list = new List<int>();
            int[] prefix = ComputePrefix(pattern);
            for (int i = 0, j = 0; i <= text.Length - pattern.Length; i += Math.Max(j - prefix[j], 1))
            {
                j = prefix[j];
                while (j < pattern.Length && pattern[j] == text[i + j])
                    j++;
                
                if (j == pattern.Length)
                    list.Add(i);
            }

            return list;
        }
    }
}