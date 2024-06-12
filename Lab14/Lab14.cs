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
            string myText = "#" + text + "$";
            int[,] R = new int[2, myText.Length];

            int left = 1;
            int right = 1;
            // parzyste
            for (int i = 1; i < myText.Length - 2; i++)
            {
                // czy wewnątrz jakiegoś palindroma
                if (i < right)
                    R[0, i] = Math.Min(right - i, R[0, left + (right - i)]);
                
                // rozszerzanie
                while (myText[i - R[0, i]] == myText[i + R[0, i] + 1])
                    R[0, i]++;

                // powiększony z prawej, albo nowy palindrom
                if (i + R[0, i] > right)
                {
                    left = i - R[0, i];
                    right = i + R[0, i];
                }

                if (R[0, i] > 0)
                    result.Add(((i - R[0, i]), 2 * R[0, i]));
            }
            
            left = 2;
            right = 2;
            // nieparzyste
            for (int i = 2; i < myText.Length - 2; i++)
            {
                // czy wewnątrz jakiegoś palindroma
                if (i < right)
                    R[1, i] = Math.Min(right - i, R[1, left + (right - i)]);
                
                // rozszerzanie
                while (myText[i - R[1, i] - 1] == myText[i + R[1, i] + 1])
                    R[1, i]++;
            
                // powiększony z prawej, albo nowy palindrom
                if (i + R[1, i] > right)
                {
                    left = i - R[1, i];
                    right = i + R[1, i];
                }
            
                if (R[1, i] > 0)
                    result.Add(((i - R[1, i] - 1), 2 * R[1, i] + 1));
            }
            
            return result.ToArray();
        }
    }

}