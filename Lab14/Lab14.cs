using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labratoria_ASD2_2024
{
    public class Lab14 : MarshalByRefObject
    {
        /// <summary>
        /// Znajduje wszystkie maksymalne palindromy długości przynajmniej 2 w zadanym słowie. Wykorzystuje Algorytm Manachera.
        /// 
        /// Palindromy powinny być zwracane jako lista par (indeks pierwszego znaku, długość palindromu), 
        /// tzn. para (i, d) oznacza, że pod indeksem i znajduje się pierwszy znak d-znakowego palindromu.
        /// 
        /// Kolejność wyników nie ma znaczenia.
        /// 
        /// Można założyć, że w tekście wejściowym nie występują znaki '#' i '$' - można je wykorzystać w roli wartowników
        /// </summary>
        /// <param name="text">Tekst wejściowy</param>
        /// <returns>Tablica znalezionych palindromów</returns>
        public (int startIndex, int length)[] FindPalindromes(string text)
        {
            var result = new List<(int, int)>();
            string myText = '#' + text + '$';
            // string myText = "#";
            //
            // foreach (char c in text)
            // {
            //     myText += c + "#";
            // }
            // Console.WriteLine(myText);
            //
            int[] R = new int[myText.Length];
            int left = 2;
            int right = 2;
            for (int i = 2; i < myText.Length - 1; i++)
            {
                R[i] = Math.Min(right - i, R[left + (right - i)]);
                if (R[i] < 0)
                    R[i] = 0;
                
                // rozszerzanie
                while (myText[i - R[i] - 1] == myText[i + R[i] + 1])
                    R[i]++;

                if (R[i] >= right - i)
                {
                    left = i - R[i];
                    right = i + R[i];
                }

                int d = 2 * R[i] + 1;
                if (d > 1)
                    result.Add((i - R[i] - 1, d));
            }
            
            return result.ToArray();
        }
    }

}