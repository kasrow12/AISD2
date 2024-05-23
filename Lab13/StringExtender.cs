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
            return s.Length - ComputePrefix(s)[s.Length - 1];
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
        static int[] makeP(string s)
        {
            int[] P = new int[s.Length + 1];
            int t = 0;

            for(int i=2;i<=s.Length;i++)
            {
                while (s[t] != s[i-1] && t > 0)
                    t = P[t];
                P[i] = t;
                if (s[i - 1] == s[t])
                    P[i] = ++t;
            }
            return P;
        }
        static public int MaxPower(this string s, out int startIndex, out int endIndex)
        {
            if (s == null || s.Length == 0)
            {
                startIndex = endIndex = 0;
                return 0;
            }
            
            startIndex = 0;
            endIndex = 1;
            int maxPow = 1;
            int n = s.Length;
            
            for (int i = 0; i < n; i++)
            {
                int[] prefix = ComputePrefix(s[i..]);
                for (int j = 2; j <= n - i; j++)
                {
                    if (j % (j - prefix[j - 1]) == 0 && j / (j - prefix[j - 1]) > maxPow)
                    {
                        maxPow = j / (j - prefix[j - 1]);
                        startIndex = i;
                        endIndex = i + j;
                    }
                }
            }

            return maxPow;
        }


        public static int[] ComputePrefix(string pattern)
        {
            int length = 0;
            int i = 1;
            int[] lps = new int[pattern.Length];
            lps[0] = 0;

            while (i < pattern.Length)
            {
                if (pattern[i] == pattern[length])
                {
                    length++;
                    lps[i] = length;
                    i++;
                }
                else
                {
                    if (length != 0)
                    {
                        length = lps[length - 1];
                    }
                    else
                    {
                        lps[i] = 0;
                        i++;
                    }
                }
            }
            return lps;
        }
        
        // static public int[] ComputePrefix(string pattern)
        // {
        //     int[] preifx = new int[pattern.Length + 1];
        //     int k = 0;
        //     for (int q = 2; q < pattern.Length; q++)
        //     {
        //         while (k > 0 && pattern[k + 1] != pattern[q])
        //             k = preifx[k];
        //
        //         if (pattern[k + 1] == pattern[q])
        //             k++;
        //
        //         preifx[q] = k;
        //     }
        //
        //     return preifx[1..];
        // }
        
        static public bool KMP(string pattern, string text)
        {
            int[] prefix = ComputePrefix(pattern);
            int q = 0;
            for (int i = 1; i < pattern.Length; i++)
            {
                while (q > 0 && pattern[q + 1] != text[i])
                    q = prefix[q];
                
                if (pattern[q + 1] == text[i])
                    q++;
                
                if (q == pattern.Length)
                    return true;
            }

            return false;
        }
    }
}