using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace ASD
{
    class Lab12Main
    {

        static void Main(string[] args)
        {
            Lab14TestModule lab14test = new Lab14TestModule();
            lab14test.PrepareTestSets();

            foreach (var ts in lab14test.TestSets)
            {
                ts.Value.PerformTests(verbose: true, checkTimeLimit: ts.Key.ToLower().Contains("performance"));
            }
        }
    }

    class Lab14TestModule : TestModule
    {
        public override void PrepareTestSets()
        {
            LZ77 solver = new LZ77();

            string DecodingTests = "DecodingTestsLab";
            TestSets[DecodingTests] = new TestSet(solver, "Decoding correctness tests (Lab)");
            TestSets[DecodingTests].TestCases.Add(new DecodingTestCase(1, "Empty list", "", new List<EncodingTriple>()));
            List<EncodingTriple> aababac = new List<EncodingTriple>(new EncodingTriple[] { new EncodingTriple(0, 0, 'a'), new EncodingTriple(0, 1, 'b'), new EncodingTriple(1, 3, 'c') });
            TestSets[DecodingTests].TestCases.Add(new DecodingTestCase(1, "aababac", "aababac", aababac));
            TestSets[DecodingTests].TestCases.Add(new DecodingTestCase(1, "abacaba", makeZimin(3), makeZiminEncoded(3)));
            TestSets[DecodingTests].TestCases.Add(new DecodingTestCase(1, "abcdef", "abcdef", makeSillyEncoding("abcdef")));
            List<EncodingTriple> aaaabaaabaacacacacb = new List<EncodingTriple>();
            aaaabaaabaacacacacb.Add(new EncodingTriple(0, 0, 'a'));
            aaaabaaabaacacacacb.Add(new EncodingTriple(0, 3, 'b'));
            aaaabaaabaacacacacb.Add(new EncodingTriple(3, 6, 'c'));
            aaaabaaabaacacacacb.Add(new EncodingTriple(1, 6, 'b'));
            TestSets[DecodingTests].TestCases.Add(new DecodingTestCase(1, "aaaabaaabaacacacacb", "aaaabaaabaacacacacb", aaaabaaabaacacacacb));
            TestSets[DecodingTests].TestCases.Add(new DecodingTestCase(1, "Random 1", makeRandomLowEntropy(400, 2, 3, 5, 13), makeRandomLowEntropyEncoded(400, 2, 3, 5, 13)));
            TestSets[DecodingTests].TestCases.Add(new DecodingTestCase(1, "Random 2", makeRandomLowEntropy(1400, 7, 5, 20, 14), makeRandomLowEntropyEncoded(1400, 7, 5, 20, 14)));


            string DecodingTestsPerformance = "DecodingTestsPerformanceLab";
            TestSets[DecodingTestsPerformance] = new TestSet(solver, "Decoding performance tests (Lab)");
            List<EncodingTriple> allA = new List<EncodingTriple>();
            for (int i = 0; i < 2000000; i++)
                allA.Add(new EncodingTriple(0, 0, 'a'));
            List<EncodingTriple> allA2 = new List<EncodingTriple>();
            for (int i = 0; i < 400000; i++)
                allA2.Add(new EncodingTriple(0, (i == 0 ? 0 : 9), 'a'));
            List<EncodingTriple> allA3 = new List<EncodingTriple>();
            for (int i = 0; i < 10; i++)
                allA3.Add(new EncodingTriple(0, (i == 0 ? 0 : 999999), 'a'));
            TestSets[DecodingTestsPerformance].TestCases.Add(new DecodingTestCase(60, "a^N (by single characters)", new string('a', 2000000), allA));
            TestSets[DecodingTestsPerformance].TestCases.Add(new DecodingTestCase(15, "a^N (multiple repetitions)", new string('a', 4000000 - 9), allA2));
            TestSets[DecodingTestsPerformance].TestCases.Add(new DecodingTestCase(1.55, "a^N (super long repetitions)", new string('a', 10000000 - 999999), allA3));
            TestSets[DecodingTestsPerformance].TestCases.Add(new DecodingTestCase(1.55, "23th Zimin word", makeZimin(23), makeZiminEncoded(23)));
            TestSets[DecodingTestsPerformance].TestCases.Add(new DecodingTestCase(30, "Big random 1", makeRandomLowEntropy(3000000, 2, 3, 5, 13), makeRandomLowEntropyEncoded(3000000, 2, 3, 5, 13)));
            TestSets[DecodingTestsPerformance].TestCases.Add(new DecodingTestCase(1.4, "Big random 2", makeRandomLowEntropy(8000000, 5, 3, 500000, 14), makeRandomLowEntropyEncoded(8000000, 5, 3, 500000, 14)));



            string Correctness = "CorrectnessLab";
            TestSets[Correctness] = new TestSet(solver, "Encoding correctness tests (Lab)");
            TestSets[Correctness].TestCases.Add(new EncodingTestCase(1, "a", "a", 1, 1));
            TestSets[Correctness].TestCases.Add(new EncodingTestCase(1, "aa", "aa", 1, 2));
            TestSets[Correctness].TestCases.Add(new EncodingTestCase(1, "abc", "abc", 1, 3));
            TestSets[Correctness].TestCases.Add(new EncodingTestCase(1, "aab", "aab", 5, 2));
            TestSets[Correctness].TestCases.Add(new EncodingTestCase(1, "a^31", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", 14, 2));
            TestSets[Correctness].TestCases.Add(new EncodingTestCase(1, "xaabaabcaabaa", "xaabaabcaabaa", 6, 5));
            TestSets[Correctness].TestCases.Add(new EncodingTestCase(1, "xaabaabcaabaa (maxP=5)", "xaabaabcaabaa", 5, 6));
            TestSets[Correctness].TestCases.Add(new EncodingTestCase(1, "aababaababaabaababaab", "aababaababaabaababaab", 14, 5));
            TestSets[Correctness].TestCases.Add(new EncodingTestCase(1, "aaabcabcabdabcabdx", "aaabcabcabdabcabdx", 9, 5));
            TestSets[Correctness].TestCases.Add(new EncodingTestCase(1, "aababaaaaxaaabbaaay", "aababaaaaxaaabbaaay", 12, 6));
            TestSets[Correctness].TestCases.Add(new EncodingTestCase(1, "Random 1", makeRandom(3000, 2, 13), 50, 423));
            TestSets[Correctness].TestCases.Add(new EncodingTestCase(1, "Random 2", makeRandom(200, 3, 14), 50, 45));
            TestSets[Correctness].TestCases.Add(new EncodingTestCase(1, "Random 3", makeRandom(200, 4, 15), 50, 58));
            TestSets[Correctness].TestCases.Add(new EncodingTestCase(1, "Random low entropy 1", makeRandomLowEntropy(200, 2, 20, 50, 16), 50, 6));
            TestSets[Correctness].TestCases.Add(new EncodingTestCase(1, "4th Zimin word", makeZimin(4), 32, 5));


            string Performance = "PerformanceLab";
            TestSets[Performance] = new TestSet(solver, "Basic encoding performance tests (Lab)");
            TestSets[Performance].TestCases.Add(new EncodingTestCase(5.6, "19th Zimin word", makeZimin(19), int.MaxValue, 20));
            TestSets[Performance].TestCases.Add(new EncodingTestCase(7.7, "Big random 1", makeRandom(10000, 3, 18), 2000, 1300));
            TestSets[Performance].TestCases.Add(new EncodingTestCase(3.5, "(ababc)^n+x+(ababc)^n", repeat("ababc", 100000) + "x" + repeat("ababc", 100000), 50000, 6));
            TestSets[Performance].TestCases.Add(new EncodingTestCase(7, "Mostly a, rare b", makeRandomMostlyA(100000, 1000, 17), 50000, 99));
            TestSets[Performance].TestCases.Add(new EncodingTestCase(7, "Random islands", makeIslands(250000, 2, 2000, 19), int.MaxValue, 42));
            TestSets[Performance].TestCases.Add(new EncodingTestCase(7, "Expanded islands", makeExpandedIslands(250000, 50, 30000, 20), int.MaxValue, 19));


        }

        string makeExpandedIslands(int n, int islandSize, int distance, int seed)
        {
            Random rand = new Random(seed);
            string pom = makeIslands(n / (islandSize + distance) * (distance + 1), 1, distance, rand.Next());
            string bIsland = makeRandom(islandSize, 2, rand.Next());
            string cIsland = makeRandom(islandSize, 2, rand.Next());
            return makeSubstitution(pom, "ab", bIsland, cIsland);
        }

        string makeIslands(int n, int islandSize, int distance, int seed)
        {
            Random rand = new Random(seed);
            char[] tab = new char[n];
            for (int i = 0; i < n; i++)
                if (i % (islandSize + distance) < distance)
                    tab[i] = 'a';
                else
                    tab[i] = (char)(rand.Next(2) + 'b');
            return new string(tab);
        }

        string makeSubstitution(string s, params string[] parts)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in s)
                sb.Append(parts[c - 'a']);
            return sb.ToString();
        }

        string randomSubstitutionWithRepetitions(int n, int repLen, int repNum, int seed)
        {
            Random rand = new Random(seed);
            return makeSubstitution(makeRandom(n / repLen / repNum, 2, rand.Next()), repeat(makeRandom(repLen, 3, rand.Next()), repNum),
                repeat(makeRandom(repLen, 3, rand.Next()), repNum));
        }

        string makeRandomMostlyA(int n, int expectedSpaceBetweenB, int seed)
        {
            char[] tab = new char[n];
            Random rand = new Random(seed);

            int i = 0;
            while (i < n)
            {
                int s = rand.Next(expectedSpaceBetweenB / 2 + rand.Next(expectedSpaceBetweenB));
                while (s-- > 0 && i < n)
                    tab[i++] = 'a';
                if (i < n)
                    tab[i++] = 'b';
            }

            return new string(tab);
        }

        string repeat(string s, int n)
        {
            char[] ret = new char[s.Length * n];
            for (int i = 0; i < ret.Length; i++)
                ret[i] = s[i % s.Length];
            return new string(ret);
        }

        string makeRandomLowEntropy(int n, int symbols, int maxP, int maxC, int seed)
        {
            Random rand = new Random(seed);
            char[] s = new char[n];
            int i = 0;
            while (i < n)
            {
                int p = Math.Min(rand.Next(maxP + 1), i - 1);
                if (p == -1) p = 0;
                int c = rand.Next(maxC + 1);
                if (i == 0)
                    c = 0;
                while (c-- > 0)
                {
                    if (i == n - 1)
                        break;
                    s[i] = s[i - p - 1];
                    i++;
                }
                s[i++] = (char)('a' + rand.Next(symbols));
            }
            return new string(s);
        }

        List<EncodingTriple> makeRandomLowEntropyEncoded(int n, int symbols, int maxP, int maxC, int seed)
        {
            Random rand = new Random(seed);
            List<EncodingTriple> ret = new List<EncodingTriple>();
            int i = 0;
            while (i < n)
            {
                int p = Math.Min(rand.Next(maxP + 1), i - 1);
                if (p == -1) p = 0;
                int c = rand.Next(maxC + 1);
                if (i == 0)
                    c = 0;
                c = Math.Min(c, n - i - 1);
                char s = (char)('a' + rand.Next(symbols));
                ret.Add(new EncodingTriple(p, c, s));
                i += c + 1;
            }
            return ret;
        }

        List<EncodingTriple> makeSillyEncoding(string s)
        {
            List<EncodingTriple> ret = new List<EncodingTriple>();
            foreach (char c in s)
                ret.Add(new EncodingTriple(0, 0, c));
            return ret;
        }

        string makeRandom(int n, int symbols, int seed)
        {
            Random rand = new Random(seed);
            char[] s = new char[n];
            for (int i = 0; i < n; i++)
                s[i] = (char)('a' + rand.Next(symbols));
            return new string(s);
        }

        //Zimin word on n symbols
        string makeZimin(int n)
        {
            if (n <= 1)
                return "a";
            else
            {
                string prev = makeZimin(n - 1);
                return prev + (char)('a' + n - 1) + prev;
            }
        }

        List<EncodingTriple> makeZiminEncoded(int n)
        {
            List<EncodingTriple> ret = new List<EncodingTriple>();
            int curLen = 1;
            ret.Add(new EncodingTriple(0, 0, 'a'));
            for (int i = 2; i <= n; i++)
            {
                ret.Add(new EncodingTriple(Math.Max(0, curLen - 1), curLen - 1, (char)('a' + i - 1)));
                curLen = 2 * curLen;
            }
            ret.Add(new EncodingTriple(Math.Max(0, curLen - 1), curLen - 2, 'a'));
            return ret;
        }

    }

    class EncodingTestCase : TestCase
    {
        string originalString;
        int maxP;
        int expectedLength;
        List<EncodingTriple> result;

        public EncodingTestCase(double timeLimit, string description, string encodedString, int maxP, int expectedLength) : base(timeLimit, null, description)
        {
            this.originalString = encodedString;
            this.expectedLength = expectedLength;
            this.maxP = maxP;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            result = ((LZ77)prototypeObject).Encode((string)originalString.Clone(), maxP);
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            Result resultCode;
            string message;
            if (result == null)
            {
                resultCode = Result.WrongResult;
                message = "Result = null";
                return (resultCode, message);
            }

            if (result.Count != expectedLength)
            {
                resultCode = Result.WrongResult;
                message = "Wrong number of triples! Expected = " + expectedLength.ToString() + ", returned = " + result.Count.ToString()
                    + $"(czas:{PerformanceTime,6:#0.000} jednostek)";
                return (resultCode, message);
            }
            int resLen = 0;
            if (result[0].c > 0)
            {
                resultCode = Result.WrongResult;
                message = "Error: result[0].c=" + result[0].c.ToString() + ", should be 0";
                return (resultCode, message);
            }
            for (int i = 0; i < result.Count; i++)
            {
                if (result[i].p < 0 || result[i].p > maxP)
                {
                    resultCode = Result.WrongResult;
                    message = "Error: result[" + i.ToString() + "].p=" + result[i].p.ToString() + (result[i].p > maxP ? ", maxP = " + maxP.ToString() : "");
                    return (resultCode, message);
                }
                if (resLen > 0 && result[i].p >= resLen)
                {
                    resultCode = Result.WrongResult;
                    message = "Error: result[" + i.ToString() + "].p=" + result[i].p.ToString() + ", should be at most " + (resLen - 1).ToString();
                    return (resultCode, message);
                }
                if (result[i].c < 0)
                {
                    resultCode = Result.WrongResult;
                    message = "Error: result[" + i.ToString() + "].c=" + result[i].c.ToString();
                    return (resultCode, message);
                }
                resLen += result[i].c + 1;
                if (i == result.Count - 1 && resLen != originalString.Length)
                {
                    resultCode = Result.WrongResult;
                    message = "Error: decoded string has length " + resLen.ToString() + " (should be " + originalString.Length.ToString() + ")";
                    return (resultCode, message);
                }
            }
            string res = (new LZ77()).Decode(result);
            for (int i = 0; i < originalString.Length; i++)
                if (res[i] != originalString[i])
                {
                    resultCode = Result.WrongResult;
                    message = "Wrong encoding! DecodedString[" + i.ToString() + "]=" + res[i] + ", should be " + originalString[i];
                    return (resultCode, message);
                }
            resultCode = Result.Success;
            message = $"OK (czas:{PerformanceTime,6:#0.000} jednostek)";
            return (resultCode, message);
        }
    }

    class DecodingTestCase : TestCase
    {
        string originalString;
        List<EncodingTriple> encodedString;
        string result;

        public DecodingTestCase(double timeLimit, string description, string originalString, List<EncodingTriple> encoding) : base(timeLimit, null, description)
        {
            this.originalString = originalString;
            this.encodedString = encoding;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            result = ((LZ77)prototypeObject).Decode(encodedString);
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            Result resultCode;
            string message;
            if (result == null)
            {
                resultCode = Result.WrongResult;
                message = "Result = null";
                return (resultCode, message);
            }

            if (result.Length != originalString.Length)
            {
                resultCode = Result.WrongResult;
                message = "Wrong length of decoded string! Expected = " + originalString.Length.ToString() + ", returned = " + result.Length.ToString();
                //+ $"(czas:{PerformanceTime,6:#0.000} jednostek)";
                return (resultCode, message);
            }
            for (int i = 0; i < originalString.Length; i++)
                if (result[i] != originalString[i])
                {
                    resultCode = Result.WrongResult;
                    message = "Wrong decoding! DecodedString[" + i.ToString() + "]=" + result[i].ToString() + ", should be " + originalString[i].ToString();
                    return (resultCode, message);
                }
            resultCode = Result.Success;
            message = $"OK (czas:{PerformanceTime,6:#0.000} jednostek)";
            return (resultCode, message);
        }
    }
}