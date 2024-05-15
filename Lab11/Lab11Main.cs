using System;
using System.Collections.Generic;
using System.Linq;
using static ASD.TestCase;

namespace ASD
{
    class Utils
    {
        public static (Result resultCode, string message) VerifyHull((double, double)[] expected, (double, double)[] actual)
        {
            if (actual == null)
                return (Result.WrongResult, "[FAILED] expected solution, got null");

            var expectedAsSet = new HashSet<(double, double)>(expected);
            var resultAsSet = new HashSet<(double, double)>(actual);

            bool goodElements = expectedAsSet.IsSupersetOf(resultAsSet);
            bool allElements = expectedAsSet.SetEquals(resultAsSet);
            bool noDuplicates = expected.Length == actual.Length;
            bool goodOrder = VerifyOrder(expected, actual);

            if (!goodElements)
                return (Result.WrongResult, "[FAILED] Your solution has invalid vertices");

            if (!allElements)
                return (Result.WrongResult, "[FAILED] Your solution misses some vertices of a solution");

            if (!noDuplicates)
                return (Result.WrongResult, "[FAILED] Your solution has good elements but contains duplicates");

            if (!goodOrder)
                return (Result.WrongResult, "[FAILED] Your solution has good elements but in invalid order");

            return (Result.Success, "[PASSED] OK");
        }

        private static bool VerifyOrder((double, double)[] expected, (double, double)[] actual)
        {
            if (expected.Length != actual.Length)
                return false;
            for (int shift = 0; shift < expected.Length; shift++)
            {
                bool shiftedEqualsToExpected = true;
                for (int i = 0; i < expected.Length; i++)
                {
                    if (!expected[i].Equals(actual[(i + shift) % expected.Length]))
                    {
                        shiftedEqualsToExpected = false;
                        break;
                    }
                }
                if (shiftedEqualsToExpected)
                    return true;
            }
            return false;
        }
    }

    class ConvexHullTestCase : TestCase
    {
        private readonly (double, double)[] _points;
        private readonly (double, double)[] _expected;

        private (double, double)[] _result;

        public ConvexHullTestCase(double timeLimit, Exception expectedException, string description,
                                        (double, double)[] points,
                                        (double, double)[] expected) : base(timeLimit, expectedException, description)
        {
            _points = points;
            _expected = expected;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            _result = ((Lab11)prototypeObject).ConvexHull(_points);
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            return Utils.VerifyHull(_expected, _result);
        }
    }

    class ConvexHullTwoTestCase : TestCase
    {
        private readonly (double, double)[] _first;
        private readonly (double, double)[] _second;
        private readonly (double, double)[] _expected;

        private (double, double)[] _result;

        public ConvexHullTwoTestCase(double timeLimit, Exception expectedException, string description,
                                    (double, double)[] first, (double, double)[] second,
                                    (double, double)[] expected) : base(timeLimit, expectedException, description)
        {
            _first = first;
            _second = second;
            _expected = expected;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            _result = ((Lab11)prototypeObject).ConvexHullOfTwo(_first, _second);
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            return Utils.VerifyHull(_expected, _result);
        }
    }

    class PolygonsHullTester : TestModule
    {
        private const int TIME_MULTIPLIER = 1;

        public override void PrepareTestSets()
        {
            TestSets["ContexHull"] = ConvexHullTests();
            TestSets["ConvesHullOfTwo"] = ConvexHullOfTwoTests();
        }

        private TestSet ConvexHullTests()
        {
            var convexHullTestSet = new TestSet(new Lab11(), "Convex hull");

            convexHullTestSet.TestCases.Add(new ConvexHullTestCase(TIME_MULTIPLIER, null, "Single point",
                new[] { (0.0, 0.0) },
                new[] { (0.0, 0.0) }));
            convexHullTestSet.TestCases.Add(new ConvexHullTestCase(TIME_MULTIPLIER, null, "Segment",
                new[] { (0.0, 0.0), (1.0, 1.0) },
                new[] { (0.0, 0.0), (1.0, 1.0) }));
            convexHullTestSet.TestCases.Add(new ConvexHullTestCase(TIME_MULTIPLIER, null, "Triangle",
                new[] { (0.0, 0.0), (1.0, 1.0), (0.0, 1.0) },
                new[] { (0.0, 0.0), (1.0, 1.0), (0.0, 1.0) }));
            convexHullTestSet.TestCases.Add(new ConvexHullTestCase(TIME_MULTIPLIER, null, "Vertical edge",
                new[] { (0.0, 1.0), (0.0, 0.0), (1.0, 1.0), (0.0, 2.0) },
                new[] { (0.0, 2.0), (0.0, 0.0), (1.0, 1.0) }));
            convexHullTestSet.TestCases.Add(new ConvexHullTestCase(TIME_MULTIPLIER, null, "Horizontal edge",
                new[] { (1.0, 0.0), (0.0, 0.0), (1.0, 1.0), (2.0, 0.0) },
                new[] { (0.0, 0.0), (2.0, 0.0), (1.0, 1.0) }));
            convexHullTestSet.TestCases.Add(new ConvexHullTestCase(TIME_MULTIPLIER, null, "Skewed edge",
                new[] { (1.0, 0.0), (1.0, 1.0), (0.0, 0.0), (2.0, 2.0) },
                new[] { (0.0, 0.0), (1.0, 0.0), (2.0, 2.0) }));
            convexHullTestSet.TestCases.Add(new ConvexHullTestCase(TIME_MULTIPLIER, null, "Degenerated",
                new[] { (0.0, 0.0), (1.0, 0.0), (0.0, -1.0), (0.0, -1.0), (0.0, 3.0), (5.0, -1.0) },
                new[] { (0.0, 3.0), (0.0, -1.0), (5.0, -1.0) }));
            convexHullTestSet.TestCases.Add(LargeRandomNormalizePolygon());

            return convexHullTestSet;
        }

        private ConvexHullTestCase LargeRandomNormalizePolygon()
        {
            var random = new Random(1337);
            var points = new List<(double, double)>();
            var vertices = new[] { (10.0, 0.0), (10.0, 10.0), (0.0, 10.0),(0.0, 0.0) };
            for (var i = 0; i < 1000; i++)
            {
                var point = (random.NextDouble() * 10, random.NextDouble() * 10);
                points.Add(point);
            }
            foreach (var v in vertices)
            {
                points.Insert(random.Next(points.Count), v);
            }

            return new ConvexHullTestCase(TIME_MULTIPLIER, null, "Large random", points.ToArray(), vertices);
        }

        private TestSet ConvexHullOfTwoTests()
        {
            var convexHullTwoTests = new TestSet(new Lab11(), "Convex hull of two polygons");
            convexHullTwoTests.TestCases.Add(new ConvexHullTwoTestCase(TIME_MULTIPLIER, null, "Disjoint",
                new (double, double)[] { (0, 1), (0, 0), (1, 0), (1, 1) },
                new (double, double)[] { (1, 3), (3, 2), (4, 2), (3, 3) },
                new (double, double)[] { (0, 0), (1, 0), (4, 2), (3, 3), (1, 3), (0, 1) }));
            convexHullTwoTests.TestCases.Add(new ConvexHullTwoTestCase(TIME_MULTIPLIER, null, "Common top point",
                new (double, double)[] { (2, 0), (1, 1), (0, 0) },
                new (double, double)[] { (1, 1), (3, 1), (4, 2), (0, 2) },
                new (double, double)[] { (0, 0), (2, 0), (4, 2), (0, 2) }));
            convexHullTwoTests.TestCases.Add(new ConvexHullTwoTestCase(TIME_MULTIPLIER, null, "Common side point",
                new (double, double)[] { (0, 0), (1, 1), (0, 2) },
                new (double, double)[] { (0, 4), (0, 3), (1, 1) },
                new (double, double)[] { (0, 0), (1, 1), (0, 4) }));
            convexHullTwoTests.TestCases.Add(new ConvexHullTwoTestCase(TIME_MULTIPLIER, null, "Common mid point",
                new (double, double)[] { (0, 0), (1, 1), (0, 2) },
                new (double, double)[] { (2.5, 3), (2, 4), (1, 1) },
                new (double, double)[] { (0, 0), (1, 1), (2.5, 3), (2, 4), (0, 2) }));
            convexHullTwoTests.TestCases.Add(new ConvexHullTwoTestCase(TIME_MULTIPLIER, null, "Extended skewed edge",
                new (double, double)[] { (2, -1), (3, 3), (1, 4), (0, 0) },
                new (double, double)[] { (5, 2), (4, 7), (3, 3) },
                new (double, double)[] { (2, -1), (5, 2), (4, 7), (1, 4), (0, 0) }));
            convexHullTwoTests.TestCases.Add(new ConvexHullTwoTestCase(TIME_MULTIPLIER, null, "Extended horizontal edge",
                new (double, double)[] { (1, -1), (2, 0), (0, 0) },
                new (double, double)[] { (2, 0), (3, 0), (3, 1), (2, 1) },
                new (double, double)[] { (1, -1), (3, 0), (3, 1), (2, 1), (0, 0) }));
            convexHullTwoTests.TestCases.Add(new ConvexHullTwoTestCase(TIME_MULTIPLIER, null, "Extended vertical edge",
                new (double, double)[] { (2, 2), (4, 2), (4, 4), (2, 4) },
                new (double, double)[] { (2, 4), (3, 5), (2, 6) },
                new (double, double)[] { (2, 2), (4, 2), (4, 4), (2, 6) }));
            convexHullTwoTests.TestCases.Add(new ConvexHullTwoTestCase(TIME_MULTIPLIER, null, "Common horizontal edge",
                new (double, double)[] { (2.5, -1), (3, 0), (2, 0) },
                new (double, double)[] { (2, 0), (3, 0), (3, 1), (2, 1) },
                new (double, double)[] { (2, 0), (2.5, -1), (3, 0), (3, 1), (2, 1) }));
            convexHullTwoTests.TestCases.Add(new ConvexHullTwoTestCase(TIME_MULTIPLIER, null, "Common vertical edge",
                new (double, double)[] { (0, 0), (1, 0), (1, 1), (0, 1) },
                new (double, double)[] { (0, 0), (0, 1), (-1, 0.5) },
                new (double, double)[] { (0, 0), (1, 0), (1, 1), (0, 1), (-1, 0.5) }));

            return convexHullTwoTests;
        }
    }

    class LabMain
    {
        static void Main(string[] args)
        {
            var tests = new PolygonsHullTester();
            tests.PrepareTestSets();
            foreach (var testSet in tests.TestSets.Values)
                testSet.PerformTests(false);
        }
    }
}
