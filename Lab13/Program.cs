using ASD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lab15
{
    class Program
    {
        class PeriodTestCase : TestCase
        {
            string s;
            int expectedResult;
            int result;

            public PeriodTestCase(string s, int result, double timeLimit)
                : base(timeLimit, null, null)
            {
                this.s = s;
                expectedResult = result;
            }


            protected override void PerformTestCase(object prototypeObject)
            {
                result = s.Period();
            }

            protected override (Result resultCode, string message) VerifyTestCase(object settings)
            {
                Result resultCode;
                string message;
                if (expectedResult != result)
                {
                    resultCode = Result.WrongResult;
                    message = "Error: answer = " + result + "\texpected = " + expectedResult;
                }
                else
                {
                    resultCode = Result.Success;
                    message = "OK";
                }
                return (resultCode, message);
            }
        }

        class PowerTestCase : TestCase
        {
            string s;
            int expectedResult;
            int result, startI, endI;

            public PowerTestCase(string s, int result, double timeLimit)
                : base(timeLimit, null, null)
            {
                this.s = s;
                expectedResult = result;
            }

            protected override void PerformTestCase(object prototypeObject)
            {
                result = s.MaxPower(out startI, out endI);
            }

            protected override (Result resultCode, string message) VerifyTestCase(object settings)
            {
                Result resultCode;
                string message;
                if (expectedResult != result)
                {
                    resultCode = Result.WrongResult;
                    message = "Error: answer = " + result + "\texpected = " + expectedResult;
                }
                else
                {
                    bool ok = true;

                    if (startI < 0 || endI < startI || endI > s.Length)
                        ok = false;
                    else if ((endI - startI) % result != 0)
                        ok = false;
                    else
                    {
                        int len = (endI - startI) % result;
                        for (int i = startI; i < endI - len; i++)
                            if (s[i] != s[i + len])
                                ok = false;
                    }
                    if (ok)
                    {
                        resultCode = Result.Success;
                        message = "OK";
                    }
                    else
                    {
                        resultCode = Result.WrongResult;
                        message = "Answer ok, wrong indices [" + startI + "," + endI + ")";
                    }
                }

                return (resultCode, message);
            }
        }

        static string ntimes(string s, int n)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < n; i++)
                sb.Append(s);
            return sb.ToString();
        }

        static string makeRandom(int len, int seed, int alphabet)
        {
            StringBuilder sb = new StringBuilder();
            Random rand = new Random(seed);
            for (int i = 0; i < len; i++)
                sb.Append((char)('a' + rand.Next(alphabet)));
            return sb.ToString();
        }

        static void Main(string[] args)
        {
            // Console.WriteLine(stringExtender.KMP("abc", "abcababab"));
            // var x = stringExtender.ComputePrefix("ababacabab");
            
            
            // Console.WriteLine("Okres slowa - male testy");
            TestSet smallPeriod = new TestSet(null, "Okres slowa - male testy");
            smallPeriod.TestCases.Add(new PeriodTestCase("a", 1, 1));
            smallPeriod.TestCases.Add(new PeriodTestCase("aa", 1, 1));
            smallPeriod.TestCases.Add(new PeriodTestCase("ab", 2, 1));
            smallPeriod.TestCases.Add(new PeriodTestCase("abcdabc", 4, 1));
            smallPeriod.TestCases.Add(new PeriodTestCase("abcdef", 6, 1));
            smallPeriod.TestCases.Add(new PeriodTestCase("abacabaxabacaba", 8, 1));
            smallPeriod.TestCases.Add(new PeriodTestCase("abcdabcdabcde", 13, 1));
            smallPeriod.TestCases.Add(new PeriodTestCase("eabcdabcdabcd", 13, 1));
            smallPeriod.TestCases.Add(new PeriodTestCase("aaaabaaabaa", 9, 1));
            smallPeriod.PerformTests(true, false);

            // Console.WriteLine("Okres slowa - duze testy");
            TestSet bigPeriod = new TestSet(null, "Okres slowa - duze testy");
            int n1 = 50000;
            string rand1, rand2;
            rand1 = makeRandom(n1, 13, 32);
            rand2 = makeRandom(n1, 3218, 2);
            bigPeriod.TestCases.Add(new PeriodTestCase(rand1 + rand1.Substring(0, n1 / 2), n1, 1));
            bigPeriod.TestCases.Add(new PeriodTestCase(rand1.Substring(n1 / 2) + rand1, n1, 1));
            bigPeriod.TestCases.Add(new PeriodTestCase(rand2, 49985, 1));
            bigPeriod.TestCases.Add(new PeriodTestCase(ntimes("abc", n1 / 3), 3, 1));
            bigPeriod.TestCases.Add(new PeriodTestCase(ntimes(makeRandom(200, 12, 3), n1 / 200), 200, 1));
            bigPeriod.PerformTests(true, true);

            //Console.WriteLine("Najwieksza potega - male testy");
            TestSet smallPower = new TestSet(null, "Najwieksza potega - male testy");
            smallPower.TestCases.Add(new PowerTestCase("aaaxyzaxyzxyzaxyzxyzaxyzxyzaxyzxyzaaa", 4, 1));
            smallPower.TestCases.Add(new PowerTestCase("a", 1, 1));
            smallPower.TestCases.Add(new PowerTestCase("ab", 1, 1));
            smallPower.TestCases.Add(new PowerTestCase("aa", 2, 1));
            smallPower.TestCases.Add(new PowerTestCase("xyzabxyza", 1, 1));
            smallPower.TestCases.Add(new PowerTestCase("xyzabxyzabxyzabxyza", 3, 1));
            smallPower.TestCases.Add(new PowerTestCase("abacabadabacaba", 1, 1));
            smallPower.TestCases.Add(new PowerTestCase("aaaxyzaxyzxyzaxyzxyzaxyzxyzaxyzxyzaaa", 4, 1));
            smallPower.TestCases.Add(new PowerTestCase("abcabcabcaaxxyy", 3, 1));
            smallPower.TestCases.Add(new PowerTestCase("xyyxaabcabcabca", 3, 1));
            smallPower.PerformTests(true, false);

            //Console.WriteLine("Najwieksza potega - duze testy");
            TestSet bigPower = new TestSet(null, "Najwieksza potega - duze testy");
            int n2 = 5000;
            string rand3 = makeRandom(n2, 13, 32);
            string rand4 = makeRandom(n2, 14, 2);
            bigPower.TestCases.Add(new PowerTestCase(rand3, 3, 2));
            bigPower.TestCases.Add(new PowerTestCase(rand4, 11, 3));
            bigPower.TestCases.Add(new PowerTestCase(ntimes("abc", n2 / 3), n2 / 3, 2));
            bigPower.TestCases.Add(new PowerTestCase(makeRandom(n2 / 4, 11, 3) + ntimes("abc", n2 / 6) + makeRandom(n2 / 4, 11, 4), n2 / 6, 3));
            bigPower.TestCases.Add(new PowerTestCase(makeRandom(n2 / 4, 11, 3) + ntimes(makeRandom(70, 17, 3), n2 / 70) + makeRandom(n2 / 4, 11, 4), n2 / 70, 4));
            bigPower.PerformTests(false, true);

        }
    }
}