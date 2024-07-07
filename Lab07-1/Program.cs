using System;
using System.Collections.Generic;
using System.Linq;

namespace ASD
{
    public class SimpleProductionProfitTestCase : TestCase
    {
        private readonly PlanData[] production;
        private readonly PlanData[] sales;
        private readonly PlanData storageInfo;
        private readonly int expectedMaxProduction;
        private readonly double expectedMaxProfit;

        private int actualMaxProduction;
        private double actualMaxProfit;

        public SimpleProductionProfitTestCase(double timeLimit, Exception expectedException, string description, (PlanData[], PlanData[], PlanData) inputData, int expectedMaxProduction, double expectedMaxProfit) : base(timeLimit, expectedException, description)
        {
            (production, sales, storageInfo) = inputData;
            this.expectedMaxProduction = expectedMaxProduction;
            this.expectedMaxProfit = expectedMaxProfit;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            var simpleProductionPlan = ((ProductionPlanner)prototypeObject).CreateSimplePlan(production, sales, storageInfo, out _);
            actualMaxProduction = simpleProductionPlan.Quantity;
            actualMaxProfit = simpleProductionPlan.Value;
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            Result resultCode = Result.NotPerformed;
            string message = "";
            if (actualMaxProduction != expectedMaxProduction)
            {
                resultCode = Result.WrongResult;
                message = $"Wrong number of units produced: expected {expectedMaxProduction}, got {actualMaxProduction}";
                return (resultCode, message);
            }

            if (Math.Abs(actualMaxProfit - expectedMaxProfit) > 1e-5)
            {
                resultCode = Result.WrongResult;
                message = $"Wrong profit calculated: expected {expectedMaxProfit}, got {actualMaxProfit}";
                return (resultCode, message);
            }

            resultCode = Result.Success;
            message = $"OK, produced {actualMaxProduction}, profit {actualMaxProfit}," +
                      $" time: {PerformanceTime:F3}";
            return (resultCode, message);
        }
    }

    public class SimpleProductionPlanTestCase : TestCase
    {
        private readonly PlanData[] production;
        private readonly PlanData[] sales;
        private readonly PlanData storageInfo;
        private readonly int expectedMaxProduction;
        private readonly double expectedMaxProfit;

        private PlanData productionPlan;
        private SimpleWeeklyPlan[] weeklyPlan;

        public SimpleProductionPlanTestCase(double timeLimit, Exception expectedException, string description, (PlanData[], PlanData[], PlanData) inputData, int expectedMaxProduction, double expectedMaxProfit) : base(timeLimit, expectedException, description)
        {
            (production, sales, storageInfo) = inputData;
            this.expectedMaxProduction = expectedMaxProduction;
            this.expectedMaxProfit = expectedMaxProfit;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            var planner = (ProductionPlanner)prototypeObject;
            productionPlan = planner.CreateSimplePlan(production, sales, storageInfo, out weeklyPlan);
            if (planner.ShowDebug)
            {
                Console.WriteLine();
                Console.WriteLine("\n--- START OF DEBUG INFO ---");
                for (int i = 0; i < weeklyPlan.Length; ++i)
                {
                    Console.WriteLine($"Week {i,4}: {weeklyPlan[i]}");
                }
                Console.WriteLine("--- END OF DEBUG INFO ---\n");
            }
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            Result resultCode = Result.NotPerformed;
            string message = "";
            int weeks = production.Length;
            int unitsProduced = 0;
            int unitsInStorage = 0;
            double productionExpenses = 0;
            double storageExpenses = 0;
            double salesProfits = 0;

            if (weeklyPlan == null)
            {
                resultCode = Result.WrongResult;
                message = "No plan returned";
                return (resultCode, message);
            }
            if (weeks != weeklyPlan.Length)
            {
                resultCode = Result.WrongResult;
                message = $"Bad plan - wrong number of weeks; expected {weeks} weeks, WeeklyPlan has length {weeklyPlan.Length}";
                return (resultCode, message);
            }
            for (int i = 0; i < weeks; ++i)
            {
                var planStep = weeklyPlan[i];
                if (planStep.UnitsProduced < 0 || planStep.UnitsSold < 0 || planStep.UnitsStored < 0)
                {
                    resultCode = Result.WrongResult;
                    message = $"Bad plan - week {i} specifies negative amounts";
                    return (resultCode, message);
                }
                if (planStep.UnitsProduced > production[i].Quantity)
                {
                    resultCode = Result.WrongResult;
                    message = $"Bad plan - production in week {i} exceeds maximum supply limit";
                    return (resultCode, message);
                }
                if (planStep.UnitsStored > storageInfo.Quantity)
                {
                    resultCode = Result.WrongResult;
                    message = $"Bad plan - storage capacity exceeded in week {i}";
                    return (resultCode, message);
                }
                if (planStep.UnitsSold > sales[i].Quantity)
                {
                    resultCode = Result.WrongResult;
                    message = $"Bad plan - sales in week {i} exceed maximum demand limit";
                    return (resultCode, message);
                }
                // Sprawdzamy, czy są sztuki, z którymi nic nie zrobiono
                if (planStep.UnitsProduced + unitsInStorage > planStep.UnitsStored + planStep.UnitsSold)
                {
                    resultCode = Result.WrongResult;
                    message = $"Bad plan - some units in week {i} have not been either sold or stored";
                    return (resultCode, message);
                }
                if (planStep.UnitsProduced + unitsInStorage < planStep.UnitsStored + planStep.UnitsSold)
                {
                    resultCode = Result.WrongResult;
                    message = $"Bad plan - nonexistent units sold or stored in week {i}";
                    return (resultCode, message);
                }
                // Produkcja
                unitsProduced += planStep.UnitsProduced;
                productionExpenses += planStep.UnitsProduced * production[i].Value;
                // Sprzedaż
                salesProfits += planStep.UnitsSold * sales[i].Value;
                // Wstawienie do magazynu
                unitsInStorage = planStep.UnitsStored;
                // Naliczenie kosztu za przetrzymanie
                storageExpenses += planStep.UnitsStored * storageInfo.Value;
            }

            if (unitsProduced != productionPlan.Quantity)
            {
                resultCode = Result.WrongResult;
                message = $"Bad plan: total number of units produced is {productionPlan.Quantity}, " +
                          $"following plan results in {unitsProduced} units";
                return (resultCode, message);
            }
            if (unitsProduced != expectedMaxProduction)
            {
                resultCode = Result.WrongResult;
                message = $"Wrong number of units produced: expected {expectedMaxProduction}, got {unitsProduced}";
                return (resultCode, message);
            }

            var profit = salesProfits - productionExpenses - storageExpenses;
            if (Math.Abs(profit - productionPlan.Value) > 1e-5)
            {
                resultCode = Result.WrongResult;
                message = $"Bad plan: net profit is {productionPlan.Value}, " +
                          $"following plan results in profit of {profit}";
                return (resultCode, message);
            }
            if (Math.Abs(profit - expectedMaxProfit) > 1e-5)
            {
                resultCode = Result.WrongResult;
                message = $"Wrong profit calculated: expected {expectedMaxProfit}, got {profit}";
                return (resultCode, message);
            }
            resultCode = Result.Success;
            message = $"OK, time: {PerformanceTime:F3}";
            return (resultCode, message);
        }
    }

    public class ComplexProductionProfitTestCase : TestCase
    {
        private readonly PlanData[] production;
        private readonly PlanData[,] sales;
        private readonly PlanData storageInfo;
        private readonly int expectedMaxProduction;
        private readonly double expectedMaxProfit;

        private int actualMaxProduction;
        private double actualMaxProfit;

        public ComplexProductionProfitTestCase(double timeLimit, Exception expectedException, string description, (PlanData[], PlanData[,], PlanData) inputData, int expectedMaxProduction, double expectedMaxProfit) : base(timeLimit, expectedException, description)
        {
            (production, sales, storageInfo) = inputData;
            this.expectedMaxProduction = expectedMaxProduction;
            this.expectedMaxProfit = expectedMaxProfit;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            var productionPlan = ((ProductionPlanner)prototypeObject).CreateComplexPlan(production, sales, storageInfo, out _);
            actualMaxProduction = productionPlan.Quantity;
            actualMaxProfit = productionPlan.Value;
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            Result resultCode = Result.NotPerformed;
            string message = "";
            //if (actualMaxProduction != expectedMaxProduction)
            //{
            //    resultCode = Result.WrongResult;
            //    message = $"Wrong number of units produced: expected {expectedMaxProduction}, got {actualMaxProduction}";
            //    return;
            //}

            if (Math.Abs(actualMaxProfit - expectedMaxProfit) > 1e-5)
            {
                resultCode = Result.WrongResult;
                message = $"Wrong profit calculated: expected {expectedMaxProfit}, got {actualMaxProfit}";
                return (resultCode, message);
            }

            resultCode = Result.Success;
            message = $"OK, produced {actualMaxProduction}, profit {actualMaxProfit}," +
                      $" time: {PerformanceTime:F3}";
            return (resultCode, message);
        }
    }

    public class ComplexProductionPlanTestCase : TestCase
    {
        private readonly PlanData[] production;
        private readonly PlanData[,] sales;
        private readonly PlanData storageInfo;
        private readonly int expectedMaxProduction;
        private readonly double expectedMaxProfit;

        private PlanData productionPlan;
        private WeeklyPlan[] weeklyPlan;

        public ComplexProductionPlanTestCase(double timeLimit, Exception expectedException, string description, (PlanData[], PlanData[,], PlanData) inputData, int expectedMaxProduction, double expectedMaxProfit) : base(timeLimit, expectedException, description)
        {
            (production, sales, storageInfo) = inputData;
            this.expectedMaxProduction = expectedMaxProduction;
            this.expectedMaxProfit = expectedMaxProfit;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            var planner = (ProductionPlanner)prototypeObject;
            productionPlan = planner.CreateComplexPlan(production, sales, storageInfo, out weeklyPlan);
            if (planner.ShowDebug)
            {
                Console.WriteLine();
                Console.WriteLine("\n--- START OF DEBUG INFO ---");
                for (int i = 0; i < weeklyPlan.Length; ++i)
                {
                    Console.WriteLine($"Week {i,4}: {weeklyPlan[i]}");
                }
                Console.WriteLine("--- END OF DEBUG INFO ---\n");
            }
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            Result resultCode = Result.NotPerformed;
            string message = "";
            int weeks = production.Length;
            int customers = sales.GetLength(0);
            int unitsProduced = 0;
            int unitsInStorage = 0;
            double productionExpenses = 0;
            double storageExpenses = 0;
            double salesProfits = 0;

            if (weeklyPlan == null)
            {
                resultCode = Result.WrongResult;
                message = "No plan returned";
                return (resultCode, message);
            }
            if (weeks != weeklyPlan.Length)
            {
                resultCode = Result.WrongResult;
                message = $"Bad plan - wrong number of weeks; expected {weeks} weeks, WeeklyPlan has length {weeklyPlan.Length}";
                return (resultCode, message);
            }
            for (int i = 0; i < weeks; ++i)
            {
                var planStep = weeklyPlan[i];
                if (planStep.UnitsSold.Length != customers)
                {
                    resultCode = Result.WrongResult;
                    message = $"Bad plan - UnitsSold array in week {i} has invalid length; expected {customers}, got {planStep.UnitsSold.Length}";
                    return (resultCode, message);
                }
                if (planStep.UnitsProduced < 0 || planStep.UnitsStored < 0 || planStep.UnitsSold.Any(sold => sold < 0))
                {
                    resultCode = Result.WrongResult;
                    message = $"Bad plan - week {i} specifies negative amounts";
                    return (resultCode, message);
                }
                if (planStep.UnitsProduced > production[i].Quantity)
                {
                    resultCode = Result.WrongResult;
                    message = $"Bad plan - production in week {i} exceeds maximum supply limit";
                    return (resultCode, message);
                }
                if (planStep.UnitsStored > storageInfo.Quantity)
                {
                    resultCode = Result.WrongResult;
                    message = $"Bad plan - storage capacity exceeded in week {i}";
                    return (resultCode, message);
                }

                for (int customer = 0; customer < customers; ++customer)
                {
                    if (planStep.UnitsSold[customer] > sales[customer, i].Quantity)
                    {
                        resultCode = Result.WrongResult;
                        message = $"Bad plan - sales in week {i} to customer {customer} exceed maximum demand limit";
                        return (resultCode, message);
                    }
                }
                var totalSold = planStep.UnitsSold.Sum();
                if (planStep.UnitsProduced + unitsInStorage > planStep.UnitsStored + totalSold)
                {
                    resultCode = Result.WrongResult;
                    message = $"Bad plan - some units in week {i} have not been either sold or stored";
                    return (resultCode, message);
                }
                if (planStep.UnitsProduced + unitsInStorage < planStep.UnitsStored + totalSold)
                {
                    resultCode = Result.WrongResult;
                    message = $"Bad plan - nonexistent units sold or stored in week {i}";
                    return (resultCode, message);
                }
                // Produkcja
                unitsProduced += planStep.UnitsProduced;
                productionExpenses += planStep.UnitsProduced * production[i].Value;
                // Sprzedaż
                for (int customer = 0; customer < customers; ++customer)
                {
                    salesProfits += planStep.UnitsSold[customer] * sales[customer, i].Value;
                }
                // Wstawienie do magazynu
                unitsInStorage = planStep.UnitsStored;
                // Naliczenie kosztu za przetrzymanie
                storageExpenses += planStep.UnitsStored * storageInfo.Value;
            }

            if (unitsProduced != productionPlan.Quantity)
            {
                resultCode = Result.WrongResult;
                message = $"Bad plan: total number of units produced is {productionPlan.Quantity}, " +
                          $"following plan results in {unitsProduced} units";
                return (resultCode, message);
            }

            //if (unitsProduced != expectedMaxProduction)
            //{
            //    resultCode = Result.WrongResult;
            //    message = $"Wrong number of units produced: expected {expectedMaxProduction}, got {unitsProduced}";
            //    return;
            //}

            var profit = salesProfits - productionExpenses - storageExpenses;
            if (Math.Abs(profit - productionPlan.Value) > 1e-5)
            {
                resultCode = Result.WrongResult;
                message = $"Bad plan: net profit is {productionPlan.Value}, " +
                          $"following plan results in profit of {profit}";
                return (resultCode, message);
            }
            if (Math.Abs(profit - expectedMaxProfit) > 1e-5)
            {
                resultCode = Result.WrongResult;
                message = $"Wrong profit calculated: expected {expectedMaxProfit}, got {profit}";
                return (resultCode, message);
            }
            resultCode = Result.Success;
            message = $"OK, time: {PerformanceTime:F3}";
            return (resultCode, message);
        }
    }

    public static class SimpleTests
    {
        public static (PlanData[], PlanData[], PlanData) SmallTest()
        {
            var productionInfo = new[]
            {
                new PlanData {Quantity = 15, Value = 5},
                new PlanData {Quantity = 10, Value = 11},
                new PlanData {Quantity = 20, Value = 9}
            };
            var storageInfo = new PlanData { Quantity = 10, Value = 10 };
            var salesInfo = new[]
            {
                new PlanData {Quantity = 10, Value = 7},
                new PlanData {Quantity = 15, Value = 9},
                new PlanData {Quantity = 30, Value = 11}
            };
            return (productionInfo, salesInfo, storageInfo);
        }

        public static (PlanData[], PlanData[], PlanData) SmallTestWithStorageProfit()
        {
            var productionInfo = new[]
            {
                new PlanData {Quantity = 15, Value = 5},
                new PlanData {Quantity = 10, Value = 11},
                new PlanData {Quantity = 20, Value = 9}
            };
            var storageInfo = new PlanData { Quantity = 10, Value = 2 };
            var salesInfo = new[]
            {
                new PlanData {Quantity = 10, Value = 7},
                new PlanData {Quantity = 15, Value = 9},
                new PlanData {Quantity = 30, Value = 11}
            };
            return (productionInfo, salesInfo, storageInfo);
        }

        public static (PlanData[], PlanData[], PlanData) DemandBottleneck()
        {
            var productionInfo = new[]
            {
                new PlanData {Quantity = 30, Value = 5.0},
                new PlanData {Quantity = 30, Value = 7.0},
                new PlanData {Quantity = 30, Value = 10.0},
                new PlanData {Quantity = 30, Value = 8.0}
            };
            var storageInfo = new PlanData { Quantity = 10, Value = 2.0 };
            var salesInfo = new[]
            {
                new PlanData {Quantity = 10, Value = 8.0},
                new PlanData {Quantity = 20, Value = 6.0},
                new PlanData {Quantity = 30, Value = 17.0},
                new PlanData {Quantity = 15, Value = 6.0}
            };
            return (productionInfo, salesInfo, storageInfo);
        }

        public static (PlanData[], PlanData[], PlanData) SupplyBottleneck()
        {
            var productionInfo = new[]
            {
                new PlanData {Quantity = 5, Value = 10.0},
                new PlanData {Quantity = 15, Value = 15.0},
                new PlanData {Quantity = 5, Value = 12.0},
                new PlanData {Quantity = 0, Value = 15.0},
                new PlanData {Quantity = 10, Value = 20.0}
            };
            var storageInfo = new PlanData { Quantity = 20, Value = 5.0 };
            var salesInfo = new[]
            {
                new PlanData {Quantity = 20, Value = 12.0},
                new PlanData {Quantity = 25, Value = 8.0},
                new PlanData {Quantity = 50, Value = 22.0},
                new PlanData {Quantity = 50, Value = 25.0},
                new PlanData {Quantity = 40, Value = 16.0}
            };
            return (productionInfo, salesInfo, storageInfo);
        }

        public static (PlanData[], PlanData[], PlanData) FreeStorage()
        {
            var productionInfo = new[]
            {
                new PlanData {Quantity = 20, Value = 15.0},
                new PlanData {Quantity = 15, Value = 20.0},
                new PlanData {Quantity = 25, Value = 25.0},
                new PlanData {Quantity = 15, Value = 15.0}
            };
            var storageInfo = new PlanData { Quantity = 100, Value = 0.0 };
            var salesInfo = new[]
            {
                new PlanData {Quantity = 10, Value = 5.0},
                new PlanData {Quantity = 5, Value = 2.0},
                new PlanData {Quantity = 75, Value = 40.0},
                new PlanData {Quantity = 20, Value = 10.0}
            };
            return (productionInfo, salesInfo, storageInfo);
        }

        public static (PlanData[], PlanData[], PlanData) BigLoss()
        {
            var productionInfo = new[]
            {
                new PlanData {Quantity = 30, Value = 25.0},
                new PlanData {Quantity = 25, Value = 30.0},
                new PlanData {Quantity = 35, Value = 40.0},
                new PlanData {Quantity = 30, Value = 35.0},
                new PlanData {Quantity = 40, Value = 37.5},
                new PlanData {Quantity = 45, Value = 40.0}
            };
            var storageInfo = new PlanData { Quantity = 60, Value = 3.0 };
            var salesInfo = new[]
            {
                new PlanData {Quantity = 30, Value = 15.0},
                new PlanData {Quantity = 30, Value = 25.0},
                new PlanData {Quantity = 45, Value = 42.0},
                new PlanData {Quantity = 45, Value = 28.0},
                new PlanData {Quantity = 50, Value = 22.0},
                new PlanData {Quantity = 50, Value = 18.0}
            };
            return (productionInfo, salesInfo, storageInfo);
        }

        public static (PlanData[], PlanData[], PlanData) NoStorage()
        {
            var storageInfo = new PlanData { Quantity = 0, Value = 0 };
            var productionInfo = new PlanData[8];
            var salesInfo = new PlanData[8];
            for (int i = 0; i < 8; ++i)
            {
                productionInfo[i] = new PlanData { Quantity = 10 * (i + 1), Value = 5.0 + i % 2 };
                salesInfo[i] = new PlanData { Quantity = 80 - 10 * i, Value = 6.0 - i % 2 };
            }
            return (productionInfo, salesInfo, storageInfo);
        }

        public static (PlanData[], PlanData[], PlanData) NoProduction()
        {
            var storageInfo = new PlanData { Quantity = 10, Value = 10 };
            var productionInfo = new PlanData[6];
            var salesInfo = new PlanData[6];
            for (int i = 0; i < 6; ++i)
            {
                productionInfo[i] = new PlanData { Quantity = 0, Value = 50 };
                salesInfo[i] = new PlanData { Quantity = 30, Value = 60 };
            }
            return (productionInfo, salesInfo, storageInfo);
        }

        public static (PlanData[], PlanData[], PlanData) NoBuyers()
        {
            var storageInfo = new PlanData { Quantity = 10, Value = 10 };
            var productionInfo = new PlanData[10];
            var salesInfo = new PlanData[10];
            for (int i = 0; i < 10; ++i)
            {
                productionInfo[i] = new PlanData { Quantity = 50, Value = 30 };
                salesInfo[i] = new PlanData { Quantity = 0, Value = 40 };
            }
            return (productionInfo, salesInfo, storageInfo);
        }

        public static (PlanData[], PlanData[], PlanData) RandomTest(int size, int seed)
        {
            var rng = new Random(seed);
            var planData = new PlanData { Quantity = rng.Next(1000), Value = 5 + rng.Next(15) };
            var productionInfo = new PlanData[size];
            var salesInfo = new PlanData[size];
            for (int i = 0; i < size; ++i)
            {
                productionInfo[i] = new PlanData { Quantity = rng.Next(1000), Value = 25 + rng.Next(50) };
                salesInfo[i] = new PlanData { Quantity = rng.Next(1000), Value = 35 + rng.Next(40) };
            }
            return (productionInfo, salesInfo, planData);
        }
    }

    public static class ComplexTests
    {
        public static (PlanData[], PlanData[,], PlanData) SmallTest()
        {
            var productionInfo = new[]
            {
                new PlanData {Quantity = 10, Value = 8},
                new PlanData {Quantity = 15, Value = 9},
                new PlanData {Quantity = 5, Value = 12}
            };
            var storageInfo = new PlanData { Quantity = 5, Value = 10 };
            var salesInfo = new[,]
            {
                {
                    new PlanData {Quantity = 6, Value = 9},
                    new PlanData {Quantity = 10, Value = 10},
                    new PlanData {Quantity = 5, Value = 6}
                },
                {
                    new PlanData {Quantity = 4, Value = 10},
                    new PlanData {Quantity = 15, Value = 7},
                    new PlanData {Quantity = 5, Value = 12}
                }
            };
            return (productionInfo, salesInfo, storageInfo);
        }

        public static (PlanData[], PlanData[,], PlanData) SmallTestWithStorageProfit()
        {
            var productionInfo = new[]
            {
                new PlanData {Quantity = 10, Value = 8},
                new PlanData {Quantity = 15, Value = 9},
                new PlanData {Quantity = 5, Value = 12}
            };
            var storageInfo = new PlanData { Quantity = 5, Value = 2 };
            var salesInfo = new[,]
            {
                {
                    new PlanData {Quantity = 6, Value = 9},
                    new PlanData {Quantity = 10, Value = 10},
                    new PlanData {Quantity = 5, Value = 6}
                },
                {
                    new PlanData {Quantity = 4, Value = 10},
                    new PlanData {Quantity = 15, Value = 7},
                    new PlanData {Quantity = 5, Value = 12}
                }
            };
            return (productionInfo, salesInfo, storageInfo);
        }

        public static (PlanData[], PlanData[,], PlanData) DemandBottleneck()
        {
            var productionInfo = new[]
            {
                new PlanData {Quantity = 50, Value = 10.0},
                new PlanData {Quantity = 40, Value = 15.0},
                new PlanData {Quantity = 45, Value = 12.0},
                new PlanData {Quantity = 60, Value = 8.0},
                new PlanData {Quantity = 50, Value = 10.0}
            };
            var storageInfo = new PlanData { Quantity = 10, Value = 3.0 };
            var salesInfo = new PlanData[3, productionInfo.Length];
            for (int week = 0; week < productionInfo.Length; ++week)
            {
                var demand = productionInfo[week].Quantity / 5;
                var value = productionInfo[week].Value;
                for (int customer = 0; customer < salesInfo.GetLength(0); ++customer)
                {
                    salesInfo[customer, week] = new PlanData { Quantity = demand, Value = value + customer - 1 };
                }
            }
            return (productionInfo, salesInfo, storageInfo);
        }

        public static (PlanData[], PlanData[,], PlanData) SupplyBottleneck()
        {
            var productionInfo = new[]
            {
                new PlanData {Quantity = 15, Value = 30.0},
                new PlanData {Quantity = 10, Value = 35.0},
                new PlanData {Quantity = 20, Value = 25.0},
                new PlanData {Quantity = 30, Value = 20.0},
                new PlanData {Quantity = 10, Value = 20.0}
            };
            var storageInfo = new PlanData { Quantity = 10, Value = 3.0 };
            var salesInfo = new PlanData[5, productionInfo.Length];
            for (int week = 0; week < productionInfo.Length; ++week)
            {
                var demand = productionInfo[week].Quantity / 2;
                var value = productionInfo[week].Value;
                for (int customer = 0; customer < salesInfo.GetLength(0); ++customer)
                {
                    salesInfo[customer, week] = new PlanData
                    {
                        Quantity = demand,
                        Value = value + 5 + Math.Abs(customer - week)
                    };
                }
            }
            return (productionInfo, salesInfo, storageInfo);
        }

        public static (PlanData[], PlanData[,], PlanData) FreeStorage()
        {
            var productionInfo = new[]
            {
                new PlanData {Quantity = 30, Value = 30.0},
                new PlanData {Quantity = 40, Value = 5.0},
                new PlanData {Quantity = 35, Value = 30.0},
                new PlanData {Quantity = 50, Value = 35.0},
                new PlanData {Quantity = 15, Value = 40.0},
                new PlanData {Quantity = 40, Value = 40.0}
            };
            var storageInfo = new PlanData { Quantity = 100, Value = 0.0 };
            var salesInfo = new PlanData[4, productionInfo.Length];
            for (int week = 0; week < productionInfo.Length; ++week)
            {
                var demand = productionInfo[week].Quantity / 3;
                var cost = productionInfo[week].Value;
                for (int customer = 0; customer < salesInfo.GetLength(0); ++customer)
                {
                    salesInfo[customer, week] = new PlanData { Quantity = demand, Value = cost - 5 + (week == 4 ? 25 : 0) };
                }
            }
            return (productionInfo, salesInfo, storageInfo);
        }

        public static (PlanData[], PlanData[,], PlanData) BigLoss()
        {
            var productionInfo = new[]
            {
                new PlanData {Quantity = 50, Value = 40.0},
                new PlanData {Quantity = 30, Value = 50.0},
                new PlanData {Quantity = 35, Value = 50.0},
                new PlanData {Quantity = 27, Value = 33.0},
                new PlanData {Quantity = 34, Value = 28.0},
                new PlanData {Quantity = 19, Value = 35.0},
                new PlanData {Quantity = 38, Value = 33.0}
            };
            var storageInfo = new PlanData { Quantity = 50, Value = 10.0 };
            var salesInfo = new PlanData[10, productionInfo.Length];
            for (int week = 0; week < productionInfo.Length; ++week)
            {
                var demand = productionInfo[week].Quantity / 10;
                var cost = productionInfo[week].Value;
                for (int customer = 0; customer < salesInfo.GetLength(0); ++customer)
                {
                    salesInfo[customer, week] = new PlanData
                    {
                        Quantity = demand + customer,
                        Value = cost - 3 - customer
                    };
                }
            }
            return (productionInfo, salesInfo, storageInfo);
        }

        public static (PlanData[], PlanData[,], PlanData) NoStorage()
        {
            var productionInfo = new[]
            {
                new PlanData {Quantity = 25, Value = 25.0},
                new PlanData {Quantity = 50, Value = 33.0},
                new PlanData {Quantity = 30, Value = 18.0},
                new PlanData {Quantity = 14, Value = 23.5},
                new PlanData {Quantity = 17, Value = 13.7},
                new PlanData {Quantity = 20, Value = 20.0}
            };
            var storageInfo = new PlanData { Quantity = 0, Value = 0.0 };
            var salesInfo = new PlanData[8, productionInfo.Length];
            for (int week = 0; week < productionInfo.Length; ++week)
            {
                var demand = productionInfo[week].Quantity * 2 / 3;
                var cost = productionInfo[week].Value;
                for (int customer = 0; customer < salesInfo.GetLength(0); ++customer)
                {
                    salesInfo[customer, week] = new PlanData
                    {
                        Quantity = demand,
                        Value = cost + 2 * (customer % 3) - 3.5 * (week % 2)
                    };
                }
            }
            return (productionInfo, salesInfo, storageInfo);
        }

        public static (PlanData[], PlanData[,], PlanData) NoProduction()
        {
            int weeks = 7, customers = 4;
            var productionInfo = new PlanData[weeks];
            var salesInfo = new PlanData[customers, weeks];
            var storageInfo = new PlanData { Quantity = 5, Value = 15.0 };
            for (int week = 0; week < weeks; ++week)
            {
                productionInfo[week] = new PlanData { Quantity = 0, Value = 0 };
                for (int customer = 0; customer < customers; ++customer)
                {
                    salesInfo[customer, week] = new PlanData { Quantity = 15, Value = 50 };
                }
            }
            return (productionInfo, salesInfo, storageInfo);
        }

        public static (PlanData[], PlanData[,], PlanData) NoBuyers()
        {
            int weeks = 6, customers = 8;
            var productionInfo = new PlanData[weeks];
            var salesInfo = new PlanData[customers, weeks];
            var storageInfo = new PlanData { Quantity = 5, Value = 15.0 };
            for (int week = 0; week < weeks; ++week)
            {
                productionInfo[week] = new PlanData { Quantity = 20, Value = 50 };
                for (int customer = 0; customer < customers; ++customer)
                {
                    salesInfo[customer, week] = new PlanData { Quantity = 0, Value = 50 };
                }
            }
            return (productionInfo, salesInfo, storageInfo);
        }

        public static (PlanData[], PlanData[,], PlanData) RandomTest(int weeks, int customers, int seed)
        {
            var rng = new Random(seed);
            var storageInfo = new PlanData { Quantity = rng.Next(1000), Value = 5 + rng.Next(15) };
            var productionInfo = new PlanData[weeks];
            var salesInfo = new PlanData[customers, weeks];
            for (int week = 0; week < weeks; ++week)
            {
                productionInfo[week] = new PlanData { Quantity = rng.Next(1000), Value = 30 + rng.Next(60) };
                for (int customer = 0; customer < customers; ++customer)
                {
                    salesInfo[customer, week] = new PlanData
                    {
                        Quantity = rng.Next(2000 / customers),
                        Value = 25 + rng.Next(80)
                    };
                }
            }
            return (productionInfo, salesInfo, storageInfo);
        }
    }

    public class Lab11TestModule : TestModule
    {
        public override void PrepareTestSets()
        {
            var subjectUnderTest = new ProductionPlanner();

            var simpleProfitTests = new TestSet(
                subjectUnderTest,
                "Część 1. - wyznaczenie maksymalnego zysku"
            );
            simpleProfitTests.TestCases.AddRange(new List<TestCase>
            {
                new SimpleProductionProfitTestCase(20, null, "Mały test", SimpleTests.SmallTest(), 45, 10.0),
                new SimpleProductionProfitTestCase(20, null, "Mały test - większy zysk z magazynem", SimpleTests.SmallTestWithStorageProfit(), 45, 50.0),
                new SimpleProductionProfitTestCase(20, null, "Wąskie gardło - popyt", SimpleTests.DemandBottleneck(), 75, 200.0),
                new SimpleProductionProfitTestCase(20, null, "Wąskie gardło - podaż", SimpleTests.SupplyBottleneck(), 35, 50.0),
                new SimpleProductionProfitTestCase(20, null, "Darmowy magazyn", SimpleTests.FreeStorage(), 75, 1100.0),
                new SimpleProductionProfitTestCase(20, null, "Duża strata", SimpleTests.BigLoss(), 205, -1870.0),
                new SimpleProductionProfitTestCase(20, null, "Brak magazynu", SimpleTests.NoStorage(), 200, 0.0),
                new SimpleProductionProfitTestCase(20, null, "Fabryka zamknięta", SimpleTests.NoProduction(), 0, 0.0),
                new SimpleProductionProfitTestCase(20, null, "Brak kupujących", SimpleTests.NoBuyers(), 0, 0.0),
                new SimpleProductionProfitTestCase(20, null, "Średni test losowy", SimpleTests.RandomTest(100, 42), 40233, 220922.0),
                new SimpleProductionProfitTestCase(20, null, "Duży test losowy", SimpleTests.RandomTest(150, 1337), 57375, 504271.0),
            });

            var simplePlanTests = new TestSet(
                subjectUnderTest,
                "Część 1. - wyznaczenie planu produkcji"
            );
            simplePlanTests.TestCases.AddRange(new List<TestCase>
            {
                new SimpleProductionPlanTestCase(20, null, "Mały test", SimpleTests.SmallTest(), 45, 10.0),
                new SimpleProductionPlanTestCase(20, null, "Mały test - większy zysk z magazynem", SimpleTests.SmallTestWithStorageProfit(), 45, 50.0),
                new SimpleProductionPlanTestCase(20, null, "Wąskie gardło - popyt", SimpleTests.DemandBottleneck(), 75, 200.0),
                new SimpleProductionPlanTestCase(20, null, "Wąskie gardło - podaż", SimpleTests.SupplyBottleneck(), 35, 50.0),
                new SimpleProductionPlanTestCase(20, null, "Darmowy magazyn", SimpleTests.FreeStorage(), 75, 1100.0),
                new SimpleProductionPlanTestCase(20, null, "Duża strata", SimpleTests.BigLoss(), 205, -1870.0),
                new SimpleProductionPlanTestCase(20, null, "Brak magazynu", SimpleTests.NoStorage(), 200, 0.0),
                new SimpleProductionPlanTestCase(20, null, "Fabryka zamknięta", SimpleTests.NoProduction(), 0, 0.0),
                new SimpleProductionPlanTestCase(20, null, "Brak kupujących", SimpleTests.NoBuyers(), 0, 0.0),
                new SimpleProductionPlanTestCase(20, null, "Średni test losowy", SimpleTests.RandomTest(100, 42), 40233, 220922.0),
                new SimpleProductionPlanTestCase(20, null, "Duży test losowy", SimpleTests.RandomTest(150, 1337), 57375, 504271.0),
            });

            var complexProfitTests = new TestSet(
                subjectUnderTest,
                "Część 2. - wyznaczenie maksymalnego zysku"
            );
            complexProfitTests.TestCases.AddRange(new List<TestCase>
            {
                new ComplexProductionProfitTestCase(20, null, "Mały test", ComplexTests.SmallTest(), 20, 24.0),
                new ComplexProductionProfitTestCase(20, null, "Mały test - większy zysk z magazynem", ComplexTests.SmallTestWithStorageProfit(), 25, 29.0),
                new ComplexProductionProfitTestCase(20, null, "Wąskie gardło - popyt", ComplexTests.DemandBottleneck(), 51, 69.0),
                new ComplexProductionProfitTestCase(20, null, "Wąskie gardło - podaż", ComplexTests.SupplyBottleneck(), 85, 652.0),
                new ComplexProductionProfitTestCase(20, null, "Darmowy magazyn", ComplexTests.FreeStorage(), 72, 1860.0),
                new ComplexProductionProfitTestCase(20, null, "Duża strata", ComplexTests.BigLoss(), 0, 0.0),
                new ComplexProductionProfitTestCase(20, null, "Brak magazynu", ComplexTests.NoStorage(), 156, 330.0),
                new ComplexProductionProfitTestCase(20, null, "Fabryka zamknięta", ComplexTests.NoProduction(), 0, 0.0),
                new ComplexProductionProfitTestCase(20, null, "Brak kupujących", ComplexTests.NoBuyers(), 0, 0.0),
                new ComplexProductionProfitTestCase(20, null, "Średni test losowy", ComplexTests.RandomTest(30, 10, 11111), 9862, 227625.0),
                new ComplexProductionProfitTestCase(20, null, "Duży test losowy", ComplexTests.RandomTest(45, 15, 22222), 16304, 532936.0),
            });

            var complexPlanTests = new TestSet(
                subjectUnderTest,
                "Część 2. - wyznaczenie planu produkcji"
            );
            complexPlanTests.TestCases.AddRange(new List<TestCase>
            {
                new ComplexProductionPlanTestCase(20, null, "Mały test", ComplexTests.SmallTest(), 20, 24.0),
                new ComplexProductionPlanTestCase(20, null, "Mały test - większy zysk z magazynem", ComplexTests.SmallTestWithStorageProfit(), 25, 29.0),
                new ComplexProductionPlanTestCase(20, null, "Wąskie gardło - popyt", ComplexTests.DemandBottleneck(), 51, 69.0),
                new ComplexProductionPlanTestCase(20, null, "Wąskie gardło - podaż", ComplexTests.SupplyBottleneck(), 85, 652.0),
                new ComplexProductionPlanTestCase(20, null, "Darmowy magazyn", ComplexTests.FreeStorage(), 72, 1860.0),
                new ComplexProductionPlanTestCase(20, null, "Duża strata", ComplexTests.BigLoss(), 0, 0.0),
                new ComplexProductionPlanTestCase(20, null, "Brak magazynu", ComplexTests.NoStorage(), 156, 330.0),
                new ComplexProductionPlanTestCase(20, null, "Fabryka zamknięta", ComplexTests.NoProduction(), 0, 0.0),
                new ComplexProductionPlanTestCase(20, null, "Brak kupujących", ComplexTests.NoBuyers(), 0, 0.0),
                new ComplexProductionPlanTestCase(20, null, "Średni test losowy", ComplexTests.RandomTest(30, 10, 11111), 9862, 227625.0),
                new ComplexProductionPlanTestCase(20, null, "Duży test losowy", ComplexTests.RandomTest(45, 15, 22222), 16304, 532936.0),
            });

            TestSets = new Dictionary<string, TestSet>
            {
                {"LabSimpleProfitTests", simpleProfitTests},
                {"LabSimplePlanTests", simplePlanTests},
                {"LabComplexProfitTests", complexProfitTests},
                {"LabComplexPlanTests", complexPlanTests}
            };
        }
    }

    public class Lab11Main
    {
        public static void Main()
        {
            var testModule = new Lab11TestModule();
            testModule.PrepareTestSets();
            foreach (var testSet in testModule.TestSets)
            {
                testSet.Value.PerformTests(verbose: true, checkTimeLimit: false);
            }
        }
    }
}