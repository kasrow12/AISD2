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
            
            // wykorzystamy wartowników
            string myText = "#" + text + "$";
            
            // R[0,...] promienie parzystych, R[1,...] nieparzystych
            int[,] R = new int[2, myText.Length];
            
            // k = 0 -> palindromy parzyste,
            // k = 1 -> palindromy nieparzyste
            for (int k = 0; k < 2; k++)
            {
                int left = 1; 
                int right = 1; // prawy koniec palindromu
                for (int i = 2; i < myText.Length - 1; i++) // zaczniemy od razu od 2, żeby promień co najmniej 1
                {
                    // Czy wewnątrz jakiegoś palindromu,
                    // clampujemy radius między poprzednim lustrzanym wynikiem, a pozostałą długością w palindromie 
                    if (i < right)
                        R[k, i] = Math.Min(right - i, R[k, left + (right - i)]);
                    
                    // Standardowe rozszerzanie
                    // W najgorszym przypadku porównamy jeszcze raz, ale też dajemy możliwość rozszerzenia się
                    // dla przypadku z Rysunku 4
                    while (myText[i - R[k, i] - 1] == myText[i + R[k, i] + k])
                        R[k, i]++;

                    // Rozszerzony z prawej (Rysunek 4) albo po prostu nowy większy palindrom
                    if (i + R[k, i] > right)
                    {
                        left = i - R[k, i];
                        right = i + R[k, i];
                    }

                    if (R[k, i] > 0) // (2 * R[k, i] + 1) / 2 = R[k, i]
                    {
                        int pos = i - R[k, i] - 1; // -1 przez wartownika
                        int len = 2 * R[k, i] + k;
                        result.Add((pos, len));
                    }
                }
            }
            
            return result.ToArray();
        }
    }

}