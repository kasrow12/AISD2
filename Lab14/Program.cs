using Labratoria_ASD2_2024;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ASD
{
    class Lab14Main
    {

        static void Main(string[] args)
        {
            Lab14TestModule lab12test = new Lab14TestModule();
            lab12test.PrepareTestSets();

            foreach (var ts in lab12test.TestSets)
            {
                ts.Value.PerformTests(verbose: true, checkTimeLimit: true);
            }
        }
    }

    class Lab14TestModule : TestModule
    {
        public override void PrepareTestSets()
        {


            string onlyOddTests = "OnlyOddPalindromesTests";

            TestSets[onlyOddTests] = new TestSet(new Lab14(), "Lab tests, only odd palindromes");
            // TestSets[onlyOddTests].TestCases.Add(new PalindromesTestCase(1, "aba", "aaaaaaaaaaa", new (int, int)[] {  }));
            TestSets[onlyOddTests].TestCases.Add(new PalindromesTestCase(1, "aba", "aba", new (int, int)[] { (0, 3) }));
            TestSets[onlyOddTests].TestCases.Add(new PalindromesTestCase(1, "ababa", "ababa", new (int, int)[] { (0, 3), (0, 5), (2, 3) }));
            TestSets[onlyOddTests].TestCases.Add(new PalindromesTestCase(1, "abcbabcbabc", "abcbabcbabc", new (int, int)[] { (0, 5), (0, 9), (2, 9), (6, 5) }));
            TestSets[onlyOddTests].TestCases.Add(new PalindromesTestCase(1, "kobylamamalybok wasitacatisaw", "kobylamamalybok wasitacatisaw", new (int, int)[] { (5, 3), (7, 3), (0, 15), (16, 13) }));
            TestSets[onlyOddTests].TestCases.Add(new PalindromesTestCase(1, "Slowo Zimina", "abacabadabacabaeabacabadabacaba",
                new (int, int)[] { (0, 3), (0, 7), (0, 15), (0, 31), (4, 3), (8, 3), (8, 7), (12, 3), (16, 3), (16, 7), (16, 15), (20, 3), (24, 3), (24, 7), (28, 3) }));


            string onlyEvenTests = "OnlyEvenPalindromesTests";

            TestSets[onlyEvenTests] = new TestSet(new Lab14(), "Lab tests, only even palindromes");
            TestSets[onlyEvenTests].TestCases.Add(new PalindromesTestCase(1, "abcabcabc", "abcabcabc", new (int, int)[] { }));
            TestSets[onlyEvenTests].TestCases.Add(new PalindromesTestCase(1, "abba", "abba", new (int, int)[] { (0, 4) }));
            TestSets[onlyEvenTests].TestCases.Add(new PalindromesTestCase(1, "abbaabba", "abbaabba", new (int, int)[] { (0, 4), (0, 8), (4, 4) }));
            TestSets[onlyEvenTests].TestCases.Add(new PalindromesTestCase(1, "aabccbaabccbaabc", "aabccbaabccbaabc", new (int, int)[] { (0, 2), (0, 8), (0, 14), (4, 12), (10, 6) }));
            TestSets[onlyEvenTests].TestCases.Add(new PalindromesTestCase(1, "aabbaaccaabbaaddaabbaaccaabbaa", "aabbaaccaabbaaddaabbaaccaabbaa",
                new (int, int)[] { (0, 2), (0, 6), (0, 14), (0, 30), (4, 2), (8, 2), (8, 6), (12, 2), (16, 2), (16, 6), (16, 14), (20, 2), (24, 2), (24, 6), (28, 2) }));



            string mixedTests = "MixedPalindromesTests";

            TestSets[mixedTests] = new TestSet(new Lab14(), "Lab tests, mixed palindromes");
            TestSets[mixedTests].TestCases.Add(new PalindromesTestCase(1, "aaba", "aaba", new (int, int)[] { (0, 2), (1, 3) }));
            TestSets[mixedTests].TestCases.Add(new PalindromesTestCase(1, "ababababaa", "ababababaa", new (int, int)[] { (0, 3), (0, 5), (0, 7), (0, 9), (2, 7), (4, 5), (6, 3), (8, 2) }));
            TestSets[mixedTests].TestCases.Add(new PalindromesTestCase(1, "abcaaddbabaababddaacadabcaddaabdabacabcd", "abcaaddbabaababddaacadabcaddaabdabacabcd",
                new (int, int)[] { (3, 2), (5, 2), (2, 18), (15, 2), (17, 2), (25, 4), (28, 2), (7, 3), (8, 3), (11, 3), (12, 3), (18, 3), (20, 3), (32, 3), (33, 5) }));
            TestSets[mixedTests].TestCases.Add(new PalindromesTestCase(1, "Dlugie slowo", "jkljkjlkjkljlljljkljlkjljljkljkljkljjkkljkljkljkllkklkjkkjkllkjjklkjjljkllkjjklljkkjjjkljkj",
                new (int, int)[] { (10, 6), (35, 2),(37, 2),(47, 4),(49, 4),(52, 8),(57, 6),(60, 6),(67, 2),(70, 6),(72, 8),(78, 2),(80, 4),(83, 2),(84, 2),(0, 9),(5, 7),(10, 3),(13, 3),
(14, 3),(13, 13),(22, 3),(19, 11),(24, 3),(51, 3),(53, 3),(56, 3),(62, 7),(68, 3),(82, 5),(88, 3) }));
            TestSets[mixedTests].TestCases.Add(new PalindromesTestCase(1, "Dlugie slowo 2", "abaccabadabbacabbaeeabbacabbadabaccabaff",
                new (int, int)[] { (0, 8), (9, 4), (14, 4), (0, 38), (20, 4), (25, 4), (30, 8), (38, 2), (0, 3), (5, 3), (6, 5), (9, 9), (20, 9), (27, 5), (30, 3), (35, 3) }));
        }
    }

    class PalindromesTestCase : TestCase
    {
        string text;
        (int startIndex, int length)[] ExpectedResult;
        (int startIndex, int length)[] result;

        public PalindromesTestCase(double timeLimit, string description, string text, (int startIndex, int length)[] result) : base(timeLimit, null, description)
        {
            this.text = text;
            this.ExpectedResult = result;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            result = ((Lab14)prototypeObject).FindPalindromes(text);
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
            if (ExpectedResult == null)
            {
                resultCode = Result.Success;
                message = $"Probably OK (czas:{PerformanceTime,6:#0.000} jednostek)";
                return (resultCode, message);
            }
            if (result.Length != ExpectedResult.Length)
            {
                resultCode = Result.WrongResult;
                message = "Error: incorrect return array size (expected = " + ExpectedResult.Length.ToString() + ", returned = " + result.Length.ToString() + ")";
                return (resultCode, message);
            }

            int[] oddLengths = new int[text.Length - 1];
            int[] evenLengths = new int[text.Length];

            foreach ((int start, int length) in ExpectedResult)
            {
                if (length % 2 == 0)
                    evenLengths[start + length / 2] = length;
                else
                    oddLengths[start + length / 2] = length;
            }

            foreach ((int start, int length) in result)
            {
                if (start < 0 || start + length > text.Length)
                {
                    message = $"Error: palindrome of length {length} starting at index {start} in text of length {text.Length}!";
                    return (Result.WrongResult, message);
                }
                if (length < 2)
                {
                    message = $"Error: returned palindrome of length {length}!";
                    return (Result.WrongResult, message);
                }
                if (length % 2 == 0 && evenLengths[start + length / 2] != length)
                {
                    message = $"Error: incorrect even palindrome of length {length} starting at index {start}!";
                    return (Result.WrongResult, message);
                }
                if (length % 2 == 1 && oddLengths[start + length / 2] != length)
                {
                    message = $"Error: incorrect odd palindrome of length {length} starting at index {start}!";
                    return (Result.WrongResult, message);
                }
            }
            resultCode = Result.Success;
            message = $"OK (czas:{PerformanceTime,6:#0.000} jednostek)";
            return (resultCode, message);
        }
    }
}