using System;
using System.Collections.Generic;

namespace ASD
{
    class Lab12Main
    {

        static void Main(string[] args)
        {
            Lab12TestModule lab12test = new Lab12TestModule();
            lab12test.PrepareTestSets();

            foreach (var ts in lab12test.TestSets)
            {
                ts.Value.PerformTests(verbose: true, checkTimeLimit: ts.Key.ToLower().Contains("performance"));
            }
        }
    }

    class Lab12TestModule : TestModule
    {
        public override void PrepareTestSets()
        {

            WaterCalculator solver = new WaterCalculator();



            string SimpleDepths = "SimpleDepths";
            TestSets[SimpleDepths] = new TestSet(solver, "PointDepths - simple version");
            TestSets[SimpleDepths].TestCases.Add(new DepthTestCase(1, "Triangle up", new Point[] { new Point(2, 3), new Point(3, 4), new Point(4, 3) }, new double[] { 0, 0, 0 }));
            TestSets[SimpleDepths].TestCases.Add(new DepthTestCase(1, "Triangle down", new Point[] { new Point(2, 3), new Point(3, 1), new Point(4, 3) }, new double[] { 0, 2, 0 }));

            TestSets[SimpleDepths].TestCases.Add(new DepthTestCase(1, "Polyline up", new Point[] { new Point(0, 0), new Point(3, 4), new Point(6, 5) }, new double[] { 0, 0, 0 }));
            TestSets[SimpleDepths].TestCases.Add(new DepthTestCase(1, "Polyline down", new Point[] { new Point(0, 5), new Point(3, 4), new Point(6, 2) }, new double[] { 0, 0, 0 }));
            TestSets[SimpleDepths].TestCases.Add(new DepthTestCase(1, "Straight line", new Point[] { new Point(0, 5), new Point(3, 5), new Point(6, 5) }, new double[] { 0, 0, 0 }));

            TestSets[SimpleDepths].TestCases.Add(new DepthTestCase(1, "Water steps", new Point[] { new Point(0, 3), new Point(1, 1), new Point(3, 5), new Point(4, 3), new Point(6, 7), new Point(7, 5), new Point(8, 7), new Point(10, 3), new Point(11, 5) },
                new double[] { 0, 2, 0, 2, 0, 2, 0, 2, 0 }));
            TestSets[SimpleDepths].TestCases.Add(new DepthTestCase(1, "Wave", new Point[] { new Point(0, 3), new Point(1, 6), new Point(2, 8), new Point(3, 9), new Point(9, 3), new Point(10, 4), new Point(11, 6), new Point(12, 9) },
    new double[] { 0, 0, 0, 0, 6, 5, 3, 0 }));
            TestSets[SimpleDepths].TestCases.Add(new DepthTestCase(1, "FlatBottom", new Point[] { new Point(0, 4), new Point(1, 3), new Point(2, 3), new Point(3, 3), new Point(4, 3), new Point(5, 3), new Point(6, 3), new Point(7, 3), new Point(8, 3), new Point(9, 4) }, new double[] { 0, 1, 1, 1, 1, 1, 1, 1, 1, 0 }));


            int n = 8000;

            Point[] minusSin = new Point[n + n / 5];
            double[] positiveSin = new double[n + n / 5];
            for (int i = 0; i < n + n / 5; i++)
            {
                minusSin[i].x = (Math.PI * i) / n;
                minusSin[i].y = -Math.Sin((Math.PI * i) / n);
                positiveSin[i] = Math.Max(0, Math.Sin((Math.PI * i) / n));
            }
            TestSets[SimpleDepths].TestCases.Add(new DepthTestCase(1, "-sin(x)", minusSin,
    positiveSin));
            TestSets[SimpleDepths].TestCases.Add(new DepthTestCase(1, "Fractional polygon", makeCircularLake(5, 10), new double[] { 0, 5 * Math.Sqrt(2), 10, 5 * Math.Sqrt(2), 0 }));



            string SimpleVolume = "SimpleVolume";
            TestSets[SimpleVolume] = new TestSet(solver, "WaterVolume - simple version");
            TestSets[SimpleVolume].TestCases.Add(new VolumeTestCase(1, "Triangle up", new Point[] { new Point(2, 3), new Point(3, 4), new Point(4, 3) }, 0));
            TestSets[SimpleVolume].TestCases.Add(new VolumeTestCase(1, "Triangle down", new Point[] { new Point(2, 3), new Point(3, 1), new Point(4, 3) }, 2));

            TestSets[SimpleVolume].TestCases.Add(new VolumeTestCase(1, "W letter", new Point[] { new Point(0, 5), new Point(1, 0), new Point(2, 2), new Point(3, 0), new Point(4, 5) }, 13));
            TestSets[SimpleVolume].TestCases.Add(new VolumeTestCase(1, "W letter - flat bottom", new Point[] { new Point(0, 5), new Point(1, 0), new Point(2, 2), new Point(3, 2), new Point(4, 0), new Point(6, 5) }, 18.5));
            TestSets[SimpleVolume].TestCases.Add(new VolumeTestCase(1, "Steps", new Point[] { new Point(0, 0), new Point(1, 0), new Point(2, 1), new Point(3, 1), new Point(4, 2), new Point(5, 2), new Point(6, 3), new Point(7, 3) }, 0));
            TestSets[SimpleVolume].TestCases.Add(new VolumeTestCase(1, "Steps with lake", new Point[] { new Point(0, 0), new Point(1, 0), new Point(2, 1), new Point(3, 1), new Point(4, 0), new Point(5, 1), new Point(6, 1), new Point(7, 2), new Point(8, 2), new Point(9, 3) }, 1));

            TestSets[SimpleVolume].TestCases.Add(new VolumeTestCase(1, "Water steps", new Point[] { new Point(0, 3), new Point(1, 1), new Point(3, 5), new Point(4, 3), new Point(6, 7), new Point(7, 5), new Point(8, 7), new Point(10, 3), new Point(11, 5) }, 8));
            TestSets[SimpleVolume].TestCases.Add(new VolumeTestCase(1, "Wave", new Point[] { new Point(0, 3), new Point(1, 6), new Point(2, 8), new Point(3, 9), new Point(9, 3), new Point(10, 4), new Point(11, 6), new Point(12, 9) }, 29));
            TestSets[SimpleVolume].TestCases.Add(new VolumeTestCase(1, "FlatBottom", new Point[] { new Point(0, 4), new Point(1, 3), new Point(2, 3), new Point(3, 3), new Point(4, 3), new Point(5, 3), new Point(6, 3), new Point(7, 3), new Point(8, 3), new Point(9, 4) }, 8));


            TestSets[SimpleVolume].TestCases.Add(new VolumeTestCase(1, "-sin(x)", minusSin, 2));
            TestSets[SimpleVolume].TestCases.Add(new VolumeTestCase(1, "Fractional polygon", makeCircularLake(5, 10), 400 * Math.Sin(Math.PI / 8) * Math.Cos(Math.PI / 8)));

            string AdvancedDepths = "AdvancedDepths";
            TestSets[AdvancedDepths] = new TestSet(solver, "PointsDepths - advanced version");
            TestSets[AdvancedDepths].TestCases.Add(new DepthTestCase(1, "Two lakes vertical", new Point[] { new Point(0, 4), new Point(1, 4), new Point(2, 2), new Point(3, 4), new Point(3, 7), new Point(2, 5), new Point(1, 8), new Point(2, 6), new Point(3, 8), new Point(4, 5) }, new double[] { 0, 0, 2, 0, 0, 0, 0, 2, 0, 0 }));
            TestSets[AdvancedDepths].TestCases.Add(new DepthTestCase(1, "Two lakes vertical 2", new Point[] { new Point(0, 4), new Point(1, 5), new Point(3, 2), new Point(6, 11), new Point(2, 9), new Point(3, 8), new Point(4, 9), new Point(3, 6), new Point(1, 9), new Point(7, 14) }, new double[] { 0, 0, 3, 0, 0, 1, 0, 0, 0, 0 }));
            TestSets[AdvancedDepths].TestCases.Add(new DepthTestCase(1, "Right angles", new Point[] { new Point(1, 1), new Point(1, 0), new Point(5, 0), new Point(5, 5), new Point(2, 5), new Point(2, 3), new Point(3, 3), new Point(3, 4), new Point(4, 4), new Point(4, 2), new Point(1, 2), new Point(1, 7), new Point(2, 7), new Point(2, 6), new Point(5, 6), new Point(5, 7) }, new double[] { 0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 0 }));
            TestSets[AdvancedDepths].TestCases.Add(new DepthTestCase(1, "Satans Tail", new Point[] { new Point(0, 0), new Point(10, 0), new Point(10, 10), new Point(5, 3), new Point(6, 2), new Point(7, 3), new Point(8, 2), new Point(9, 3), new Point(8, 1), new Point(7, 2), new Point(6, 1), new Point(4, 3), new Point(11, 12) }, new double[] { 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0 }));
            TestSets[AdvancedDepths].TestCases.Add(new DepthTestCase(1, "Stalagmite", new Point[] { new Point(-6, 4.5), new Point(-5, 5), new Point(-4, 5.5), new Point(-2, 6), new Point(-1, 6), new Point(-2, 5), new Point(-3, 4), new Point(-4, 5), new Point(-5, 4), new Point(-4, 3), new Point(-3, 2), new Point(-2, 3), new Point(-1, 0) }, new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0 }));

            string AdvancedVolume = "AdvancedVolume";

            TestSets[AdvancedVolume] = new TestSet(solver, "WaterVolume - advanced version");
            TestSets[AdvancedVolume].TestCases.Add(new VolumeTestCase(1, "Two lakes vertical", new Point[] { new Point(0, 4), new Point(1, 4), new Point(2, 2), new Point(3, 4), new Point(3, 7), new Point(2, 5), new Point(1, 8), new Point(2, 6), new Point(3, 8), new Point(4, 5) }, 4));
            TestSets[AdvancedVolume].TestCases.Add(new VolumeTestCase(1, "Two lakes vertical 2", new Point[] { new Point(0, 4), new Point(1, 5), new Point(3, 2), new Point(6, 11), new Point(2, 9), new Point(3, 8), new Point(4, 9), new Point(3, 6), new Point(1, 9), new Point(7, 14) }, 5.5));
            TestSets[AdvancedVolume].TestCases.Add(new VolumeTestCase(1, "Right angles", new Point[] { new Point(1, 1), new Point(1, 0), new Point(5, 0), new Point(5, 5), new Point(2, 5), new Point(2, 3), new Point(3, 3), new Point(3, 4), new Point(4, 4), new Point(4, 2), new Point(1, 2), new Point(1, 7), new Point(2, 7), new Point(2, 6), new Point(5, 6), new Point(5, 7) }, 8));
            TestSets[AdvancedVolume].TestCases.Add(new VolumeTestCase(1, "Satans Tail", new Point[] { new Point(0, 0), new Point(10, 0), new Point(10, 10), new Point(5, 3), new Point(6, 2), new Point(7, 3), new Point(8, 2), new Point(9, 3), new Point(8, 1), new Point(7, 2), new Point(6, 1), new Point(4, 3), new Point(11, 12) }, 2));
            TestSets[AdvancedVolume].TestCases.Add(new VolumeTestCase(1, "Stalagmite", new Point[] { new Point(-6, 4.5), new Point(-5, 5), new Point(-4, 5.5), new Point(-2, 6), new Point(-1, 6), new Point(-2, 5), new Point(-3, 4), new Point(-4, 5), new Point(-5, 4), new Point(-4, 3), new Point(-3, 2), new Point(-2, 3), new Point(-1, 0) }, 1));

            string DepthPerformance = "DepthPerformance";

            int bigN = 100000;

            Point[] bigFlatBottom = new Point[bigN];
            double[] bigFlatBottomResult = new double[bigN];
            for (int i = 0; i < bigN; i++)
            {
                bigFlatBottom[i] = new Point(i, -1);
                bigFlatBottomResult[i] = 1;
            }
            bigFlatBottom[0].y = bigFlatBottom[bigN - 1].y = 0;
            bigFlatBottomResult[0] = bigFlatBottomResult[bigN - 1] = 0;

            Point[] bigUpLadder;
            double[] bigUpLadderResult;
            Point[] singleUpPart = new Point[] { new Point(0, 4), new Point(1, 2), new Point(2, 6), new Point(1, 5) };
            double[] singleUpPartResult = new double[] { 0, 2, 0, 0 };
            List<Point> bigUpLadderList = new List<Point>();
            List<double> BigUpLadderResultList = new List<double>();
            for (int i = 0; i <= bigN / singleUpPart.Length; i++)
            {
                foreach (Point p in singleUpPart)
                    bigUpLadderList.Add(new Point(p.x, p.y + 5 * i));
                foreach (double d in singleUpPartResult)
                    BigUpLadderResultList.Add(d);
            }
            bigUpLadderList.Add(new Point(0, 5 * bigN / singleUpPart.Length + 10));
            bigUpLadderList.Add(new Point(3, 5 * bigN / singleUpPart.Length + 10));
            BigUpLadderResultList.Add(0);
            BigUpLadderResultList.Add(0);
            bigUpLadder = bigUpLadderList.ToArray();
            bigUpLadderResult = BigUpLadderResultList.ToArray();

            double r = 9.999;
            var circularLake = makeCircularLake(bigN, r);
            Point[] circularLakeReversed = new Point[bigN];
            double[] circularLakeResult = new double[bigN];
            for (int i = 0; i < bigN; i++)
            {
                circularLakeResult[i] = -circularLake[i].y;
                circularLakeReversed[i] = new Point(circularLake[i].x, -circularLake[i].y);
            }

            TestSets[DepthPerformance] = new TestSet(solver, "PointsDepths - performance tests");

            TestSets[DepthPerformance].TestCases.Add(new DepthTestCase(1, "Big flat bottom", bigFlatBottom, bigFlatBottomResult));
            TestSets[DepthPerformance].TestCases.Add(new DepthTestCase(1, "Big up ladder", bigUpLadder, bigUpLadderResult));
            TestSets[DepthPerformance].TestCases.Add(new DepthTestCase(1, "Random 1", makeRandomMountains(bigN, 123), null));
            TestSets[DepthPerformance].TestCases.Add(new DepthTestCase(1, "Random 2", makeFlatRandomMountains(bigN, 123), null));
            TestSets[DepthPerformance].TestCases.Add(new DepthTestCase(1, "Random 3", makeRandomLake(bigN, 124), null));
            TestSets[DepthPerformance].TestCases.Add(new DepthTestCase(1, "Random 4", makeFractionalMountains(bigN, 125), null));

            TestSets[DepthPerformance].TestCases.Add(new DepthTestCase(1, "Circular lake", circularLake, circularLakeResult));
            TestSets[DepthPerformance].TestCases.Add(new DepthTestCase(1, "Circular lake reversed", circularLakeReversed, new double[bigN]));

            string VolumePerformance = "VolumePerformance";

            TestSets[VolumePerformance] = new TestSet(solver, "WaterVolume - performance tests");



            TestSets[VolumePerformance].TestCases.Add(new VolumeTestCase(1, "Big flat bottom", bigFlatBottom, bigN - 2));
            TestSets[VolumePerformance].TestCases.Add(new VolumeTestCase(1, "Big up ladder", bigUpLadder, (bigN / singleUpPart.Length + 1) * 1.5));
            TestSets[VolumePerformance].TestCases.Add(new VolumeTestCase(2, "Random 1", makeRandomMountains(bigN, 123), 7345458.3516326));
            TestSets[VolumePerformance].TestCases.Add(new VolumeTestCase(1, "Random 2", makeFlatRandomMountains(bigN, 123), 50038.5));
            TestSets[VolumePerformance].TestCases.Add(new VolumeTestCase(1, "Random 3", makeRandomLake(bigN, 124), 457642));
            TestSets[VolumePerformance].TestCases.Add(new VolumeTestCase(1, "Random 4", makeFractionalMountains(bigN, 125), 49889.1671248877));

            TestSets[VolumePerformance].TestCases.Add(new VolumeTestCase(2, "Circular lake", circularLake, r * Math.Sin(Math.PI / (bigN - 1) / 2) * r * Math.Cos(Math.PI / (bigN - 1) / 2) * (bigN - 1)));
            TestSets[VolumePerformance].TestCases.Add(new VolumeTestCase(2, "Circular lake reversed", circularLakeReversed, 0));
        }

        public override double ScoreResult()
        {
            return 1;
        }

        static Point[] makeRandomMountains(int n, int seed)
        {
            Random rand = new Random(seed);
            Point[] ret = new Point[n];
            for (int i = 0; i < n; i++)
            {
                int c = (int)Math.Sqrt(Math.Min(i, n - i));
                ret[i] = new Point(i, rand.Next(c));
            }
            return ret;
        }

        static Point[] makeFlatRandomMountains(int n, int seed)
        {
            Random rand = new Random(seed);
            Point[] ret = new Point[n];
            for (int i = 0; i < n; i++)
            {
                int c = (int)Math.Log10(Math.Min(i, n - i));
                if (c < 0)
                    c = 0;
                ret[i] = new Point(i, c + rand.Next(2));
            }
            return ret;
        }

        static Point[] makeRandomLake(int n, int seed)
        {
            Random rand = new Random(seed);
            Point[] ret = new Point[n];
            for (int i = 0; i < n; i++)
            {
                int c = (int)(2 * Math.Log10(Math.Min(i, n - i)));
                if (c < 0) c = 0;
                ret[i] = new Point(i, (c <= 4 ? c : 8 - c) + rand.Next(2));
            }
            return ret;
        }

        static Point[] makeFractionalMountains(int n, int seed)
        {
            Random rand = new Random(seed);
            Point[] ret = new Point[n];
            ret[0] = new Point(0, 0);
            for (int i = 1; i < n; i++)
            {
                ret[i] = new Point(i, (rand.NextDouble() - 0.5));
            }
            return ret;
        }

        static Point[] makeCircularLake(int n, double r)
        {
            double alphaStep = Math.PI / (n - 1);
            Point[] ret = new Point[n];
            for (int i = 0; i < n; i++)
            {
                double currentAngle = Math.PI + (Math.PI * i) / (n - 1);
                ret[i] = new Point(r * Math.Cos(currentAngle), r * Math.Sin(currentAngle));
            }
            return ret;
        }
    }

    class DepthTestCase : TestCase
    {
        Point[] points;
        double[] ExpectedResult;
        double[] result;
        double epsilon = 1e-7;

        public DepthTestCase(double timeLimit, string description, Point[] points, double[] result) : base(timeLimit, null, description)
        {
            this.points = points;
            this.ExpectedResult = result;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            result = ((WaterCalculator)prototypeObject).PointDepths(points);
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
                message = "Incorrect array size (expected = " + ExpectedResult.Length.ToString() + ", returned = " + result.Length.ToString() + ")";
                return (resultCode, message);
            }
            for (int i = 0; i < result.Length; i++)
                //if (result[i] != ExpectedResult[i])
                if (Math.Abs(result[i] - ExpectedResult[i]) > epsilon)
                {
                    resultCode = Result.WrongResult;
                    message = "Incorrect value (expected result[" + i.ToString() + "] = " + ExpectedResult[i].ToString() + ", returned result[" + i.ToString() + "]= " + result[i].ToString() + ")";
                    return (resultCode, message);
                }
            resultCode = Result.Success;
            message = $"OK (czas:{PerformanceTime,6:#0.000} jednostek)";
            return (resultCode, message);
        }
    }

    class VolumeTestCase : TestCase
    {
        Point[] points;
        double ExpectedResult;
        double result;
        static double epsilon = 1e-7;

        public VolumeTestCase(double timeLimit, string description, Point[] points, double result) : base(timeLimit, null, description)
        {
            this.points = points;
            this.ExpectedResult = result;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            result = ((WaterCalculator)prototypeObject).WaterVolume(points);
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            Result resultCode;
            string message;
            if (Math.Abs(result - ExpectedResult) > epsilon)
            {
                resultCode = Result.WrongResult;
                message = "Incorrect result (expected " + ExpectedResult.ToString() + ", returned " + result.ToString() + ")";
                return (resultCode, message);
            }
            resultCode = Result.Success;
            message = $"OK (czas:{PerformanceTime,6:#0.000} jednostek)";
            return (resultCode, message);
        }
    }
}
