using System;
using System.Collections.Generic;
using System.Text;

namespace ASD
{
    public class LZ77 : MarshalByRefObject
    {
        /// <summary>
        /// Odkodowywanie napisu zakodowanego algorytmem LZ77. Dane kodowanie jest poprawne (nie trzeba tego sprawdzać).
        /// </summary>
        public string Decode(List<EncodingTriple> encoding)
        {
            int n = 0;
            foreach (var triple in encoding)
                n += triple.c + 1;

            char[] str = new char[n];
            int last = 0;
            foreach ((int p, int c, char s) in encoding)
            {
                int k = last - p - 1;
                for (int i = 0; i < c; i++)
                    str[last++] = str[k + i];

                str[last++] = s;
            }
            
            return new string(str);
        }

        /// <summary>
        /// Kodowanie napisu s algorytmem LZ77
        /// </summary>
        /// <returns></returns>
        public List<EncodingTriple> Encode(string s, int maxP)
        {
            return null;
        }
    }

    [Serializable]
    public struct EncodingTriple
    {
        public int p, c;
        public char s;

        public EncodingTriple(int p, int c, char s)
        {
            this.p = p;
            this.c = c;
            this.s = s;
        }

        public void Deconstruct(out int i, out int i1, out char c1)
        {
            i = p;
            i1 = c;
            c1 = s;
        }
    }
}