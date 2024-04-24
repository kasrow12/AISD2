
using System;

namespace ASD
{

    abstract class ChangeMakingTestCase : TestCase
    {

        protected int amount;
        protected int[] coins;
        protected int? expectedRes;
        protected int[] expectedChange;

        protected int? res;
        protected int[] change;

        public ChangeMakingTestCase(double timeLimit, Exception expectedException, int a, int[] c, int[] ec) : base(timeLimit, expectedException, "")
        {
            amount = a;
            coins = c;
            expectedChange = ec;
            if (ec == null)
                expectedRes = null;
            if (ec != null)
            {
                expectedRes = 0;
                foreach (int cc in ec) expectedRes += cc;
            }
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings = null)
        {
            Result resultCode;
            string message;

            if (res != expectedRes)
            {
                resultCode = Result.WrongResult;
                message = string.Format("incorrect result: {0} (expected: {1})", res == null ? "null" : res.ToString(), expectedRes == null ? "null" : expectedRes.ToString());
                return (resultCode, message);
            }
            if ((res == null && change != null) || (res != null && change == null))
            {
                resultCode = Result.WrongResult;
                message = $"incorrect change table: {change.ToString()} (expected: {expectedChange.ToString()}";
                return (resultCode, message);
            }
            resultCode = Result.Success;
            message = "OK";
            if (res == null) return (resultCode, message);

            if (change.Length != expectedChange.Length)
            {
                resultCode = Result.WrongResult;
                message = $"incorrect change table length: {change.Length} (expected: {expectedChange.Length}";
                return (resultCode, message);
            }
            int cc = 0;
            int a = 0;
            for (int i = 0; i < coins.Length; ++i)
            {
                if (change[i] < 0)
                {
                    resultCode = Result.WrongResult;
                    message = $"negative coins number: {change.ToString()}";
                    return (resultCode, message);
                }
                cc += change[i];
                a += coins[i] * change[i];
            }
            if (cc != expectedRes)
            {
                resultCode = Result.WrongResult;
                message = $"icorrect coins count: {cc}, {change.ToString()} (expected {expectedRes}, {expectedChange.ToString()})";
                return (resultCode, message);
            }
            if (a != amount)
            {
                resultCode = Result.WrongResult;
                message = $"icorrect change amount: {a}, {change.ToString()} (expected {amount}, {expectedChange.ToString()})";
                return (resultCode, message);
            }
            return (resultCode, message);
        }

    }

    class ChangeMakingLimitsTestCase : ChangeMakingTestCase
    {

        protected int[] limits;

        public ChangeMakingLimitsTestCase(double timeLimit, Exception expectedException, int a, int[] c, int[] l, int[] ec)
                   : base(timeLimit, expectedException, a, c, ec)
        {
            limits = l;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            res = (prototypeObject as ChangeMaking).Dynamic(amount, coins, limits, out change);
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings = null)
        {
            (Result resultCode, string message) = base.VerifyTestCase(settings);
            if (message != "OK") return (resultCode, message);
            resultCode = Result.Success;
            message = "OK";
            if (res == null) return (resultCode, message);
            for (int i = 0; i < coins.Length; ++i)
                if (change[i] > limits[i])
                {
                    resultCode = Result.WrongResult;
                    message = $"coins limit break: {change.ToString()}";
                    return (resultCode, message);
                }
            return (resultCode, message);
        }

    }

    class ChangeMakingNoLimitsTestCase : ChangeMakingTestCase
    {

        public ChangeMakingNoLimitsTestCase(double timeLimit, Exception expectedException, int a, int[] c, int[] ec)
                   : base(timeLimit, expectedException, a, c, ec) { }

        protected override void PerformTestCase(object prototypeObject)
        {
            res = (prototypeObject as ChangeMaking).NoLimitsDynamic(amount, coins, out change);
        }

    }

    class ChangeMakingTestModule : TestModule
    {
        public override void PrepareTestSets()
        {

            TestSet changeMakingNoLimits = new TestSet(new ChangeMaking(), "ChangeMaking without coin limits, lab test");

            changeMakingNoLimits.TestCases.Add(new ChangeMakingNoLimitsTestCase(5, null, 10, new int[] { 5, 1, 2 }, new int[] { 2, 0, 0 }));
            changeMakingNoLimits.TestCases.Add(new ChangeMakingNoLimitsTestCase(5, null, 123, new int[] { 1, 2, 5, 10, 20, 50, 100, 200 }, new int[] { 1, 1, 0, 0, 1, 0, 1, 0 }));
            changeMakingNoLimits.TestCases.Add(new ChangeMakingNoLimitsTestCase(5, null, 123, new int[] { 2, 5, 10, 20, 50, 100, 200 }, new int[] { 4, 1, 1, 0, 0, 1, 0 }));
            changeMakingNoLimits.TestCases.Add(new ChangeMakingNoLimitsTestCase(5, null, 23, new int[] { 1 }, new int[] { 23 }));
            changeMakingNoLimits.TestCases.Add(new ChangeMakingNoLimitsTestCase(5, null, 23, new int[] { 2 }, null));
            changeMakingNoLimits.TestCases.Add(new ChangeMakingNoLimitsTestCase(5, null, 23, new int[] { 50 }, null));

            TestSet changeMakingLimits = new TestSet(new ChangeMaking(), "ChangeMaking with coin limits, lab tests");
            changeMakingLimits.TestCases.Add(new ChangeMakingLimitsTestCase(5, null, 10, new int[] { 5, 1, 2 }, new int[] { 1, 20, 10 }, new int[] { 1, 1, 2 }));

            changeMakingLimits.TestCases.Add(new ChangeMakingLimitsTestCase(5, null, 123, new int[] { 1, 2, 5, 10, 20, 50, 100, 200 },
                                                                                          new int[] { 99999, 99999, 99999, 99999, 99999, 99999, 99999, 99999 },
                                                                                          new int[] { 1, 1, 0, 0, 1, 0, 1, 0 }));

            changeMakingLimits.TestCases.Add(new ChangeMakingLimitsTestCase(5, null, 123, new int[] { 2, 5, 10, 20, 50, 100, 200 },
                                                                                          new int[] { 99999, 99999, 99999, 99999, 99999, 99999, 99999 },
                                                                                          new int[] { 4, 1, 1, 0, 0, 1, 0 }));

            changeMakingLimits.TestCases.Add(new ChangeMakingLimitsTestCase(5, null, 123, new int[] { 1, 2, 5, 10, 20, 50, 100, 200 },
                                                                                          new int[] { 0, 99999, 99999, 99999, 99999, 99999, 99999, 99999 },
                                                                                          new int[] { 0, 4, 1, 1, 0, 0, 1, 0 }));

            changeMakingLimits.TestCases.Add(new ChangeMakingLimitsTestCase(5, null, 123, new int[] { 1, 2, 5, 10, 20, 50, 100, 200 },
                                                                                          new int[] { 99999, 99999, 3, 4, 3, 0, 0, 99999 },
                                                                                          new int[] { 0, 4, 3, 4, 3, 0, 0, 0 }));

            changeMakingLimits.TestCases.Add(new ChangeMakingLimitsTestCase(5, null, 23, new int[] { 1, 2, 5 },
                                                                                          new int[] { 99999, 99999, 3 },
                                                                                          new int[] { 0, 4, 3 }));

            changeMakingLimits.TestCases.Add(new ChangeMakingLimitsTestCase(5, null, 23, new int[] { 5, 1, 2 },
                                                                                         new int[] { 2, 3, 5 },
                                                                                         new int[] { 2, 3, 5 }));

            changeMakingLimits.TestCases.Add(new ChangeMakingLimitsTestCase(5, null, 23, new int[] { 5, 5, 1, 2 },
                                                                                         new int[] { 3, 2, 3, 3 },
                                                                                         new int[] { 3, 1, 1, 1 }));

            changeMakingLimits.TestCases.Add(new ChangeMakingLimitsTestCase(5, null, 23, new int[] { 2 }, new int[15], null));
            changeMakingLimits.TestCases.Add(new ChangeMakingLimitsTestCase(5, null, 23, new int[] { 50 }, new int[1], null));
            changeMakingLimits.TestCases.Add(new ChangeMakingLimitsTestCase(5, null, 23, new int[] { 1 }, new int[20], null));

            changeMakingLimits.TestCases.Add(new ChangeMakingLimitsTestCase(5, null, 141, new int[] { 2, 137, 65, 35, 30, 9, 123, 81, 71 },
                                                                                          new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                                                                                          new int[] { 1, 0, 1, 1, 1, 1, 0, 0, 0 }));

            TestSets["changeMakingNoLimits"] = changeMakingNoLimits;
            TestSets["changeMakingLimits"] = changeMakingLimits;
        }
    }

    class Lab02
    {

        static void Main()
        {
            ChangeMakingTestModule tests = new ChangeMakingTestModule();
            tests.PrepareTestSets();
            foreach (var ts in tests.TestSets)
            {
                ts.Value.PerformTests(false, true);
            }
        }
    }
}
