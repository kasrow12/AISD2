using System;
using System.Linq;
using ASD;

namespace ASD_lab08
{
    public class Program
    {
        public abstract class Lab08TestCase : TestCase
        {
            protected readonly Cat[] cats;
            protected readonly Person[] people;
            protected readonly bool expectedIsPossible;
            protected readonly bool checkAssignment;
            protected readonly bool checkMinCost;
            protected readonly int expectedMinCost;

            protected (bool isPossible, int[][] assignment, int minCost) result;

            public Lab08TestCase(Cat[] cats, Person[] people, bool expectedIsPossible, bool checkAssignment, bool checkMinCost, int expectedMinCost, double timeLimit, string description) : base(timeLimit, null, description)
            {
                this.cats = cats;
                this.people = people;
                this.expectedIsPossible = expectedIsPossible;
                this.checkAssignment = checkAssignment;
                this.checkMinCost = checkMinCost;
                this.expectedMinCost = expectedMinCost;
            }

            protected override (Result resultCode, string message) VerifyTestCase(object settings)
            {
                var (code, msg) = CheckSolution(result.isPossible, result.assignment, result.minCost);
                return (code, $"{msg} [{Description}]");
            }

            private (Result resultCode, string message) CheckSolution(bool isPossible, int[][] assignment, int minCost)
            {
                if (expectedIsPossible != isPossible)
                {
                    return (Result.WrongResult, $"(istnienie trasy) Zwrócono {isPossible}, powinno być {expectedIsPossible}");
                }
                else if (!isPossible)
                {
                    return OkResult("OK");
                }

                if (checkMinCost && minCost != expectedMinCost)
                {
                    return (Result.WrongResult, $"(minimalny koszt) Zwrócono {minCost}, powinno być {expectedMinCost}");
                }

                if (!checkAssignment)
                {
                    return OkResult("OK");
                }

                if (assignment == null)
                {
                    return (Result.WrongResult, "(przypisanie) Zwrócono null zamiast przypisania");
                }

                if (assignment.Length != people.Length)
                {
                    return (Result.WrongResult, $"(przypisanie) Zwrócono {assignment.Length} osób, w zwróconym przypisaniu powinno znaleźć się {people.Length} osób");
                }

                bool[] usedCats = new bool[cats.Length];
                int totalCost = 0;
                for (int personId = 0; personId < assignment.Length; ++personId)
                {
                    if (assignment[personId].Length > people[personId].MaxCats)
                    {
                        return (Result.WrongResult, $"(przypisanie) Przekroczono ({assignment[personId].Length}) limit kotów dla osoby {personId} (limit {people[personId].MaxCats})");
                    }

                    foreach (int catId in assignment[personId])
                    {
                        if (usedCats[catId])
                        {
                            return (Result.WrongResult, $"(przypisanie) Przypisano wielokrotnie kota {catId}");
                        }
                        usedCats[catId] = true;

                        if (!cats[catId].AcceptablePeople.Contains(personId))
                        {
                            return (Result.WrongResult, $"(przypisanie) Błędne przypisanie kota, kot {catId} nie akceptuje osoby {personId}");
                        }

                        totalCost += people[personId].Salaries[catId];
                    }
                }

                if (checkMinCost && totalCost != minCost)
                {
                    return (Result.WrongResult, $"(przypisanie, minimalny koszt) Koszt wynikający z przypisania ({totalCost}) jest inny niż zwrócony minimalny koszt ({minCost})");
                }

                for (int catId = 0; catId < usedCats.Length; ++catId)
                {
                    if (!usedCats[catId])
                    {
                        return (Result.WrongResult, $"(przypisanie) Zapomniano o kocie {catId}");
                    }
                }

                return OkResult("OK");
            }

            public (Result resultCode, string message) OkResult(string message) => (TimeLimit < PerformanceTime ? Result.LowEfficiency : Result.Success, $"{message} {PerformanceTime:#0.00}s");
        }

        public class Stage1TestCase : Lab08TestCase
        {
            public Stage1TestCase(Cat[] cats, Person[] people, bool expectedIsPossible, bool checkAssignment, double timeLimit, string description)
                : base(cats: cats, people: people, expectedIsPossible: expectedIsPossible, checkAssignment: checkAssignment, checkMinCost: false, expectedMinCost: -1, timeLimit: timeLimit, description: description) { }

            protected override void PerformTestCase(object prototypeObject)
            {
                (bool isPossible, int[][] assignment) = ((Cats)prototypeObject).StageOne(cats, people);
                result = (isPossible, assignment, int.MaxValue);
            }
        }

        public class Stage2TestCase : Lab08TestCase
        {
            public Stage2TestCase(Cat[] cats, Person[] people, bool expectedIsPossible, bool checkAssignment, int expectedMinCost, double timeLimit, string description)
                : base(cats: cats, people: people, expectedIsPossible: expectedIsPossible, checkAssignment: checkAssignment, checkMinCost: true, expectedMinCost: expectedMinCost, timeLimit: timeLimit, description: description) { }

            protected override void PerformTestCase(object prototypeObject)
            {
                (bool isPossible, int[][] assignment, int minCost) = ((Cats)prototypeObject).StageTwo(cats, people);
                result = (isPossible, assignment, minCost);
            }
        }

        public class Lab08Tests : TestModule
        {
            TestSet Stage1a = new TestSet(prototypeObject: new Cats(), description: "Etap 1, odpowiedź czy przypisanie jest możliwe", settings: true);
            TestSet Stage1b = new TestSet(prototypeObject: new Cats(), description: "Etap 1, zwrócenie możliwego przypisania", settings: true);
            TestSet Stage2a = new TestSet(prototypeObject: new Cats(), description: "Etap 2, zwrócenie minimalnego kosztu", settings: true);
            TestSet Stage2b = new TestSet(prototypeObject: new Cats(), description: "Etap 2, zwrócenie możliwego przypisania dla minimalnego kosztu", settings: true);

            public override void PrepareTestSets()
            {
                TestSets["Stage1a"] = Stage1a;
                TestSets["Stage1b"] = Stage1b;
                TestSets["Stage2a"] = Stage2a;
                TestSets["Stage2b"] = Stage2b;

                Prepare();
            }

            private void AddStage1a(Lab08TestCase s1aTestCase)
            {
                Stage1a.TestCases.Add(s1aTestCase);
            }

            private void AddStage1b(Lab08TestCase s1bTestCase)
            {
                Stage1b.TestCases.Add(s1bTestCase);
            }

            private void AddStage2a(Lab08TestCase s1aTestCase)
            {
                Stage2a.TestCases.Add(s1aTestCase);
            }

            private void AddStage2b(Lab08TestCase s1bTestCase)
            {
                Stage2b.TestCases.Add(s1bTestCase);
            }

            public bool AreAllTestsPassed()
            {
                return Stage1a.FailedCount + Stage1b.FailedCount + Stage2a.FailedCount + Stage2b.FailedCount == 0;
            }

            private void Prepare()
            {
                string name0 = "Pusty przykład";
                Cat[] cats0 = new Cat[] { };
                Person[] people0 = new Person[] { };
                AddStage1a(new Stage1TestCase(cats: cats0, people: people0, expectedIsPossible: true, checkAssignment: false, timeLimit: 2.5, description: name0));
                AddStage1b(new Stage1TestCase(cats: cats0, people: people0, expectedIsPossible: true, checkAssignment: true, timeLimit: 1, description: name0));
                AddStage2a(new Stage2TestCase(cats: cats0, people: people0, expectedIsPossible: true, checkAssignment: false, expectedMinCost: 0, timeLimit: 1, description: name0));
                AddStage2b(new Stage2TestCase(cats: cats0, people: people0, expectedIsPossible: true, checkAssignment: true, expectedMinCost: 0, timeLimit: 1, description: name0));


                string name1 = "Przykład z zadania";
                Cat[] cats1 = new Cat[]
                {
                    new Cat(acceptablePeople: new int[] { 1, 2 }),
                    new Cat(acceptablePeople: new int[] { 0 }),
                    new Cat(acceptablePeople: new int[] { 0, 1 }),
                    new Cat(acceptablePeople: new int[] { 1, 2 }),
                    new Cat(acceptablePeople: new int[] { 2 }),
                };
                Person[] people1 = new Person[]
                {
                    new Person(maxCats: 2, salaries: new int[] { 50, 100, 100, 100, 65 }),
                    new Person(maxCats: 3, salaries: new int[] { 50, 80, 50, 50, 60}),
                    new Person(maxCats: 1, salaries: new int[] { 200, 120, 180, 200, 300 }),
                };
                AddStage1a(new Stage1TestCase(cats: cats1, people: people1, expectedIsPossible: true, checkAssignment: false, timeLimit: 2.5, description: name1));
                AddStage1b(new Stage1TestCase(cats: cats1, people: people1, expectedIsPossible: true, checkAssignment: true, timeLimit: 1, description: name1));
                AddStage2a(new Stage2TestCase(cats: cats1, people: people1, expectedIsPossible: true, checkAssignment: false, expectedMinCost: 550, timeLimit: 1, description: name1));
                AddStage2b(new Stage2TestCase(cats: cats1, people: people1, expectedIsPossible: true, checkAssignment: true, expectedMinCost: 550, timeLimit: 1, description: name1));


                string name2 = "Brak kotów";
                Cat[] cats2 = new Cat[] { };
                Person[] people2 = new Person[]
                {
                    new Person(maxCats: 2, salaries: new int[] { }),
                    new Person(maxCats: 3, salaries: new int[] { }),
                    new Person(maxCats: 1, salaries: new int[] { }),
                };
                AddStage1a(new Stage1TestCase(cats: cats2, people: people2, expectedIsPossible: true, checkAssignment: false, timeLimit: 1, description: name2));
                AddStage1b(new Stage1TestCase(cats: cats2, people: people2, expectedIsPossible: true, checkAssignment: true, timeLimit: 1, description: name2));
                AddStage2a(new Stage2TestCase(cats: cats2, people: people2, expectedIsPossible: true, checkAssignment: false, expectedMinCost: 0, timeLimit: 1, description: name2));
                AddStage2b(new Stage2TestCase(cats: cats2, people: people2, expectedIsPossible: true, checkAssignment: true, expectedMinCost: 0, timeLimit: 1, description: name2));


                string name3 = "Brak opiekunów";
                Cat[] cats3 = new Cat[]
                {
                    new Cat(acceptablePeople: new int[] { }),
                    new Cat(acceptablePeople: new int[] { }),
                    new Cat(acceptablePeople: new int[] { }),
                    new Cat(acceptablePeople: new int[] { }),
                    new Cat(acceptablePeople: new int[] { }),
                };
                Person[] people3 = new Person[] { };
                AddStage1a(new Stage1TestCase(cats: cats3, people: people3, expectedIsPossible: false, checkAssignment: false, timeLimit: 1, description: name3));
                AddStage1b(new Stage1TestCase(cats: cats3, people: people3, expectedIsPossible: false, checkAssignment: true, timeLimit: 1, description: name3));
                AddStage2a(new Stage2TestCase(cats: cats3, people: people3, expectedIsPossible: false, checkAssignment: false, expectedMinCost: 0, timeLimit: 1, description: name3));
                AddStage2b(new Stage2TestCase(cats: cats3, people: people3, expectedIsPossible: false, checkAssignment: true, expectedMinCost: 0, timeLimit: 1, description: name3));


                string name4 = "Brak kotów i opiekunów";
                Cat[] cats4 = new Cat[] { };
                Person[] people4 = new Person[] { };
                AddStage1a(new Stage1TestCase(cats: cats4, people: people4, expectedIsPossible: true, checkAssignment: false, timeLimit: 1, description: name4));
                AddStage1b(new Stage1TestCase(cats: cats4, people: people4, expectedIsPossible: true, checkAssignment: true, timeLimit: 1, description: name4));
                AddStage2a(new Stage2TestCase(cats: cats4, people: people4, expectedIsPossible: true, checkAssignment: false, expectedMinCost: 0, timeLimit: 1, description: name4));
                AddStage2b(new Stage2TestCase(cats: cats4, people: people4, expectedIsPossible: true, checkAssignment: true, expectedMinCost: 0, timeLimit: 1, description: name4));


                string name5 = "Jedno możliwe przypisanie";
                Cat[] cats5 = new Cat[]
                {
                    new Cat(acceptablePeople: new int[] { 5 }),
                    new Cat(acceptablePeople: new int[] { 4 }),
                    new Cat(acceptablePeople: new int[] { 3 }),
                    new Cat(acceptablePeople: new int[] { 2 }),
                    new Cat(acceptablePeople: new int[] { 1 }),
                    new Cat(acceptablePeople: new int[] { 0 }),
                };
                Person[] people5 = new Person[]
                {
                    new Person(maxCats: 10, salaries: new int[] { 1, 1, 1, 1, 1, 1 }),
                    new Person(maxCats: 10, salaries: new int[] { 2, 2, 2, 2, 2, 2 }),
                    new Person(maxCats: 10, salaries: new int[] { 3, 3, 3, 3, 3, 3 }),
                    new Person(maxCats: 10, salaries: new int[] { 4, 4, 4, 4, 4, 4 }),
                    new Person(maxCats: 10, salaries: new int[] { 5, 5, 5, 5, 5, 5 }),
                    new Person(maxCats: 10, salaries: new int[] { 6, 6, 6, 6, 6, 6 }),
                };
                AddStage1a(new Stage1TestCase(cats: cats5, people: people5, expectedIsPossible: true, checkAssignment: false, timeLimit: 1, description: name5));
                AddStage1b(new Stage1TestCase(cats: cats5, people: people5, expectedIsPossible: true, checkAssignment: true, timeLimit: 1, description: name5));
                AddStage2a(new Stage2TestCase(cats: cats5, people: people5, expectedIsPossible: true, checkAssignment: false, expectedMinCost: 21, timeLimit: 1, description: name5));
                AddStage2b(new Stage2TestCase(cats: cats5, people: people5, expectedIsPossible: true, checkAssignment: true, expectedMinCost: 21, timeLimit: 1, description: name5));


                string name6 = "Kot nieakceptujący żadnego opiekuna";
                Cat[] cats6 = new Cat[]
                {
                    new Cat(acceptablePeople: new int[] { 1, 2 }),
                    new Cat(acceptablePeople: new int[] { 0 }),
                    new Cat(acceptablePeople: new int[] { 0, 1 }),
                    new Cat(acceptablePeople: new int[] { }),
                    new Cat(acceptablePeople: new int[] { 2 }),
                };
                Person[] people6 = new Person[]
                {
                    new Person(maxCats: 2, salaries: new int[] { 50, 100, 100, 100, 65 }),
                    new Person(maxCats: 3, salaries: new int[] { 50, 80, 50, 50, 60}),
                    new Person(maxCats: 1, salaries: new int[] { 200, 120, 180, 200, 300 }),
                };
                AddStage1a(new Stage1TestCase(cats: cats6, people: people6, expectedIsPossible: false, checkAssignment: false, timeLimit: 1, description: name6));
                AddStage1b(new Stage1TestCase(cats: cats6, people: people6, expectedIsPossible: false, checkAssignment: true, timeLimit: 1, description: name6));
                AddStage2a(new Stage2TestCase(cats: cats6, people: people6, expectedIsPossible: false, checkAssignment: false, expectedMinCost: 0, timeLimit: 1, description: name6));
                AddStage2b(new Stage2TestCase(cats: cats6, people: people6, expectedIsPossible: false, checkAssignment: true, expectedMinCost: 0, timeLimit: 1, description: name6));


                string name7 = "Najtańszy opiekun";
                Cat[] cats7 = new Cat[]
                {
                    new Cat(acceptablePeople: new int[] { 0, 1, 2 }),
                    new Cat(acceptablePeople: new int[] { 0, 1, 2 }),
                    new Cat(acceptablePeople: new int[] { 0, 1, 2 }),
                    new Cat(acceptablePeople: new int[] { 0, 1, 2 }),
                    new Cat(acceptablePeople: new int[] { 0, 1, 2 }),
                };
                Person[] people7 = new Person[]
                {
                    new Person(maxCats: 5, salaries: new int[] { 100, 100, 100, 100, 100 }),
                    new Person(maxCats: 5, salaries: new int[] { 10, 10, 10, 10, 10 }),
                    new Person(maxCats: 5, salaries: new int[] { 200, 200, 200, 200, 200 }),
                };
                AddStage1a(new Stage1TestCase(cats: cats7, people: people7, expectedIsPossible: true, checkAssignment: false, timeLimit: 1, description: name7));
                AddStage1b(new Stage1TestCase(cats: cats7, people: people7, expectedIsPossible: true, checkAssignment: true, timeLimit: 1, description: name7));
                AddStage2a(new Stage2TestCase(cats: cats7, people: people7, expectedIsPossible: true, checkAssignment: false, expectedMinCost: 50, timeLimit: 1, description: name7));
                AddStage2b(new Stage2TestCase(cats: cats7, people: people7, expectedIsPossible: true, checkAssignment: true, expectedMinCost: 50, timeLimit: 1, description: name7));


                string name8 = "Saturacja dwóch opiekunów";
                Cat[] cats8 = new Cat[]
                {
                    new Cat(acceptablePeople: new int[] { 0, 1, 2 }),
                    new Cat(acceptablePeople: new int[] { 0, 1, 2 }),
                    new Cat(acceptablePeople: new int[] { 0, 1, 2 }),
                    new Cat(acceptablePeople: new int[] { 0, 1, 2 }),
                    new Cat(acceptablePeople: new int[] { 0, 1, 2 }),
                    new Cat(acceptablePeople: new int[] { 0, 1, 2 }),
                    new Cat(acceptablePeople: new int[] { 0, 1, 2 }),
                };
                Person[] people8 = new Person[]
                {
                    new Person(maxCats: 2, salaries: new int[] { 10, 20, 30, 40, 50, 60, 70 }),
                    new Person(maxCats: 100, salaries: new int[] { 100, 200, 300, 400, 500, 600, 700 }),
                    new Person(maxCats: 1, salaries: new int[] { 1, 2, 3, 4, 5, 6, 7 }),
                };
                int cost8 = 100 + 200 + 300 + 400 + 50 + 60 + 7; // = 1117
                AddStage1a(new Stage1TestCase(cats: cats8, people: people8, expectedIsPossible: true, checkAssignment: false, timeLimit: 1, description: name8));
                AddStage1b(new Stage1TestCase(cats: cats8, people: people8, expectedIsPossible: true, checkAssignment: true, timeLimit: 1, description: name8));
                AddStage2a(new Stage2TestCase(cats: cats8, people: people8, expectedIsPossible: true, checkAssignment: false, expectedMinCost: cost8, timeLimit: 1, description: name8));
                AddStage2b(new Stage2TestCase(cats: cats8, people: people8, expectedIsPossible: true, checkAssignment: true, expectedMinCost: cost8, timeLimit: 1, description: name8));


                string name9 = "Przekroczenie limitu kotów";
                Cat[] cats9 = new Cat[]
                {
                    new Cat(acceptablePeople: new int[] { 0, 1, 2, 3, 4 }),
                    new Cat(acceptablePeople: new int[] { 0, 1, 2, 3, 4 }),
                    new Cat(acceptablePeople: new int[] { 0, 1, 2, 3, 4 }),
                    new Cat(acceptablePeople: new int[] { 0, 1, 2, 3, 4 }),
                    new Cat(acceptablePeople: new int[] { 0, 1, 2, 3, 4 }),
                    new Cat(acceptablePeople: new int[] { 0, 1, 2, 3, 4 }),
                    new Cat(acceptablePeople: new int[] { 0, 1, 2, 3, 4 }),
                    new Cat(acceptablePeople: new int[] { 0, 1, 2, 3, 4 }),
                    new Cat(acceptablePeople: new int[] { 0, 1, 2, 3, 4 }),
                    new Cat(acceptablePeople: new int[] { 0, 1, 2, 3, 4 }),
                };
                Person[] people9 = new Person[]
                {
                    new Person(maxCats: 1, salaries: new int[] { 100, 100, 100, 100, 100, 100, 100, 100, 100, 100 }),
                    new Person(maxCats: 2, salaries: new int[] { 100, 100, 100, 100, 100, 100, 100, 100, 100, 100 }),
                    new Person(maxCats: 1, salaries: new int[] { 200, 200, 200, 200, 200, 200, 200, 200, 200, 200 }),
                    new Person(maxCats: 2, salaries: new int[] { 200, 200, 200, 200, 200, 200, 200, 200, 200, 200 }),
                    new Person(maxCats: 1, salaries: new int[] { 300, 300, 300, 300, 300, 300, 300, 300, 300, 300 }),
                };
                AddStage1a(new Stage1TestCase(cats: cats9, people: people9, expectedIsPossible: false, checkAssignment: false, timeLimit: 1, description: name9));
                AddStage1b(new Stage1TestCase(cats: cats9, people: people9, expectedIsPossible: false, checkAssignment: true, timeLimit: 1, description: name9));
                AddStage2a(new Stage2TestCase(cats: cats9, people: people9, expectedIsPossible: false, checkAssignment: false, expectedMinCost: 0, timeLimit: 1, description: name9));
                AddStage2b(new Stage2TestCase(cats: cats9, people: people9, expectedIsPossible: false, checkAssignment: true, expectedMinCost: 0, timeLimit: 1, description: name9));


                string name10 = "Większy przykład 1";
                Cat[] cats10 = new Cat[]
                {
                    new Cat(acceptablePeople: new int[] { 2 }),
                    new Cat(acceptablePeople: new int[] { 7, 8 }),
                    new Cat(acceptablePeople: new int[] { 1, 2, 3, 5, 8 }),
                    new Cat(acceptablePeople: new int[] { 2 }),
                    new Cat(acceptablePeople: new int[] { 2, 3, 4, 5, 7, 9 }),
                    new Cat(acceptablePeople: new int[] { 1, 5, 8 }),
                    new Cat(acceptablePeople: new int[] { 0, 1, 5, 6, 7, 9 }),
                    new Cat(acceptablePeople: new int[] { 1, 3, 4, 5, 6, 7, 8, 9 }),
                    new Cat(acceptablePeople: new int[] { 4, 7 }),
                    new Cat(acceptablePeople: new int[] { 0, 2, 3, 4, 6, 8 }),
                    new Cat(acceptablePeople: new int[] { 2 }),
                    new Cat(acceptablePeople: new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }),
                    new Cat(acceptablePeople: new int[] { 0, 1, 2, 6, 7, 9 }),
                    new Cat(acceptablePeople: new int[] { 3 }),
                    new Cat(acceptablePeople: new int[] { 1, 2, 4, 5, 7, 8, 9 }),
                };
                Person[] people10 = new Person[]
                {
                    new Person(maxCats: 1, salaries: new int[] { 356, 543, 363, 203, 454, 179, 777, 660, 144, 129, 935, 227, 44, 788, 49 }),
                    new Person(maxCats: 5, salaries: new int[] { 642, 938, 611, 781, 191, 464, 657, 451, 514, 551, 395, 202, 788, 768, 855 }),
                    new Person(maxCats: 1, salaries: new int[] { 763, 26, 964, 817, 986, 873, 441, 560, 368, 751, 237, 915, 587, 119, 966 }),
                    new Person(maxCats: 9, salaries: new int[] { 665, 324, 654, 118, 8, 949, 643, 241, 280, 79, 502, 863, 162, 228, 632 }),
                    new Person(maxCats: 2, salaries: new int[] { 388, 745, 207, 1000, 543, 652, 969, 720, 834, 359, 780, 466, 453, 189, 172 }),
                    new Person(maxCats: 5, salaries: new int[] { 812, 945, 893, 294, 533, 681, 901, 915, 799, 120, 718, 224, 527, 249, 895 }),
                    new Person(maxCats: 4, salaries: new int[] { 114, 751, 368, 346, 441, 16, 612, 706, 35, 848, 625, 934, 188, 145, 934 }),
                    new Person(maxCats: 7, salaries: new int[] { 304, 614, 220, 935, 561, 551, 803, 378, 105, 856, 195, 771, 462, 657, 31 }),
                    new Person(maxCats: 3, salaries: new int[] { 564, 954, 504, 293, 253, 795, 392, 484, 479, 96, 471, 114, 929, 845, 663 }),
                    new Person(maxCats: 2, salaries: new int[] { 402, 875, 762, 88, 586, 700, 898, 721, 684, 46, 40, 906, 648, 944, 842 }),
                };
                AddStage1a(new Stage1TestCase(cats: cats10, people: people10, expectedIsPossible: false, checkAssignment: false, timeLimit: 1, description: name10));
                AddStage1b(new Stage1TestCase(cats: cats10, people: people10, expectedIsPossible: false, checkAssignment: true, timeLimit: 1, description: name10));
                AddStage2a(new Stage2TestCase(cats: cats10, people: people10, expectedIsPossible: false, checkAssignment: false, expectedMinCost: 0, timeLimit: 1, description: name10));
                AddStage2b(new Stage2TestCase(cats: cats10, people: people10, expectedIsPossible: false, checkAssignment: true, expectedMinCost: 0, timeLimit: 1, description: name10));


                string name11 = "Większy przykład 2";
                Cat[] cats11 = new Cat[]
                {
                    new Cat(acceptablePeople: new int[] { 0, 4, 5, 7, 10, 13 }),
                    new Cat(acceptablePeople: new int[] { 2, 3, 6, 7, 8, 13 }),
                    new Cat(acceptablePeople: new int[] { 0, 4, 6, 7, 9, 11, 14 }),
                    new Cat(acceptablePeople: new int[] { 4, 8 }),
                    new Cat(acceptablePeople: new int[] { 0, 1, 7, 10, 12 }),
                    new Cat(acceptablePeople: new int[] { 4 }),
                    new Cat(acceptablePeople: new int[] { 0, 1, 5, 6, 7, 9 }),
                    new Cat(acceptablePeople: new int[] { 0, 1, 2, 3, 5, 8, 11, 14 }),
                    new Cat(acceptablePeople: new int[] { 3, 4, 6 }),
                    new Cat(acceptablePeople: new int[] { 13, 14 }),
                    new Cat(acceptablePeople: new int[] { 0, 1, 2, 3, 4, 9, 12, 13 }),
                    new Cat(acceptablePeople: new int[] { 0, 6, 9, 11, 14 }),
                    new Cat(acceptablePeople: new int[] { 5, 12, 13 }),
                    new Cat(acceptablePeople: new int[] { 1, 5, 7, 8, 9, 13, 14 }),
                    new Cat(acceptablePeople: new int[] { 2, 3, 5, 7, 8, 9, 10, 12, 13, 14 }),
                    new Cat(acceptablePeople: new int[] { 0, 1, 3, 7 }),
                    new Cat(acceptablePeople: new int[] { 13, 14 }),
                    new Cat(acceptablePeople: new int[] { 1, 2, 4, 5, 7, 8, 10, 11, 14 }),
                    new Cat(acceptablePeople: new int[] { 0, 4, 6 }),
                    new Cat(acceptablePeople: new int[] { 3, 8, 12 }),
                };
                Person[] people11 = new Person[]
                {
                    new Person(maxCats: 1, salaries: new int[] { 776, 698, 277, 502, 934, 436, 31, 38, 734, 998, 633, 239, 845, 451, 662, 767, 907, 34, 990, 162 }),
                    new Person(maxCats: 5, salaries: new int[] { 740, 630, 66, 871, 229, 487, 315, 204, 870, 49, 534, 437, 715, 978, 323, 228, 933, 608, 535, 238 }),
                    new Person(maxCats: 2, salaries: new int[] { 895, 650, 489, 404, 157, 511, 223, 813, 695, 195, 887, 39, 741, 370, 571, 794, 500, 834, 731, 521 }),
                    new Person(maxCats: 9, salaries: new int[] { 161, 77, 574, 170, 904, 404, 237, 389, 36, 893, 416, 222, 595, 720, 912, 744, 988, 820, 153, 594 }),
                    new Person(maxCats: 2, salaries: new int[] { 494, 447, 378, 1000, 674, 319, 896, 382, 25, 703, 396, 835, 173, 310, 183, 37, 301, 114, 50, 889 }),
                    new Person(maxCats: 5, salaries: new int[] { 562, 529, 397, 294, 209, 829, 344, 404, 578, 71, 877, 307, 289, 765, 397, 733, 898, 472, 242, 330 }),
                    new Person(maxCats: 4, salaries: new int[] { 430, 737, 153, 882, 308, 639, 56, 76, 886, 975, 216, 277, 775, 46, 611, 225, 431, 818, 447, 462 }),
                    new Person(maxCats: 7, salaries: new int[] { 408, 931, 584, 700, 599, 692, 469, 493, 171, 295, 612, 594, 497, 641, 737, 413, 872, 851, 216, 917 }),
                    new Person(maxCats: 3, salaries: new int[] { 115, 966, 223, 668, 152, 260, 508, 505, 818, 689, 773, 64, 214, 193, 713, 344, 370, 291, 917, 488 }),
                    new Person(maxCats: 2, salaries: new int[] { 625, 630, 709, 687, 771, 750, 359, 937, 414, 382, 429, 956, 350, 863, 319, 661, 356, 592, 509, 3 }),
                    new Person(maxCats: 8, salaries: new int[] { 306, 441, 378, 420, 732, 301, 817, 467, 455, 256, 925, 388, 494, 709, 128, 737, 89, 512, 284, 34 }),
                    new Person(maxCats: 3, salaries: new int[] { 379, 707, 41, 289, 997, 681, 336, 429, 578, 215, 140, 268, 926, 509, 150, 977, 569, 959, 285, 191 }),
                    new Person(maxCats: 2, salaries: new int[] { 304, 47, 771, 873, 825, 182, 118, 846, 297, 595, 154, 459, 395, 53, 797, 609, 761, 9, 81, 201 }),
                    new Person(maxCats: 1, salaries: new int[] { 952, 960, 481, 632, 823, 187, 100, 272, 352, 756, 525, 117, 901, 980, 637, 65, 397, 500, 554, 575 }),
                    new Person(maxCats: 2, salaries: new int[] { 850, 326, 914, 691, 482, 674, 356, 897, 476, 676, 337, 786, 204, 180, 376, 710, 602, 34, 704, 780 }),
                };
                AddStage1a(new Stage1TestCase(cats: cats11, people: people11, expectedIsPossible: true, checkAssignment: false, timeLimit: 1, description: name11));
                AddStage1b(new Stage1TestCase(cats: cats11, people: people11, expectedIsPossible: true, checkAssignment: true, timeLimit: 1, description: name11));
                AddStage2a(new Stage2TestCase(cats: cats11, people: people11, expectedIsPossible: true, checkAssignment: false, expectedMinCost: 4388, timeLimit: 1, description: name11));
                AddStage2b(new Stage2TestCase(cats: cats11, people: people11, expectedIsPossible: true, checkAssignment: true, expectedMinCost: 4388, timeLimit: 1, description: name11));


                string name12 = "Większy przykład 3";
                Cat[] cats12 = new Cat[]
                {
                    new Cat(acceptablePeople: new int[] { 2, 3, 4, 5, 6, 8, 12, 15, 16, 17 }),
                    new Cat(acceptablePeople: new int[] { 1, 4, 7, 9, 11, 12, 13, 14, 16, 17, 18 }),
                    new Cat(acceptablePeople: new int[] { 0, 6, 11 }),
                    new Cat(acceptablePeople: new int[] { 4, 12, 13, 15, 19 }),
                    new Cat(acceptablePeople: new int[] { 19 }),
                    new Cat(acceptablePeople: new int[] { 3, 14, 15, 16, 18 }),
                    new Cat(acceptablePeople: new int[] { 2, 4, 10, 11, 12 }),
                    new Cat(acceptablePeople: new int[] { 0, 1, 2, 3, 4, 6, 8, 10, 11, 15, 17, 18, 19 }),
                    new Cat(acceptablePeople: new int[] { 0, 3, 4, 5, 6, 7, 8, 10, 13, 14, 15 }),
                    new Cat(acceptablePeople: new int[] { 2, 3, 4, 5, 6, 7, 10, 11, 12, 13, 15, 17, 19 }),
                    new Cat(acceptablePeople: new int[] { 14, 17, 18, 19 }),
                    new Cat(acceptablePeople: new int[] { 6, 11, 13 }),
                    new Cat(acceptablePeople: new int[] { 1, 2, 4, 6, 8, 12, 14, 17, 18 }),
                    new Cat(acceptablePeople: new int[] { 1, 7, 9, 10, 12, 13, 17 }),
                    new Cat(acceptablePeople: new int[] { 1, 3, 5, 10, 12, 13, 15, 18 }),
                    new Cat(acceptablePeople: new int[] { 0, 5, 8, 9, 10, 11, 12, 13, 14, 15 }),
                    new Cat(acceptablePeople: new int[] { 1, 3, 6, 7, 8, 11, 12, 14, 15, 16, 17, 18, 19 }),
                    new Cat(acceptablePeople: new int[] { 3, 6, 8, 17 }),
                    new Cat(acceptablePeople: new int[] { 0, 2, 5, 6, 8, 9, 10, 11, 12, 13, 14, 16, 17, 18, 19 }),
                    new Cat(acceptablePeople: new int[] { 2, 3, 4, 15, 18 }),
                    new Cat(acceptablePeople: new int[] { 0, 7 }),
                    new Cat(acceptablePeople: new int[] { 0, 2, 3, 4, 5, 6, 7, 9, 10, 12, 13, 14, 15, 16, 17, 18, 19 }),
                    new Cat(acceptablePeople: new int[] { 0, 4, 8, 10, 11 }),
                    new Cat(acceptablePeople: new int[] { 0, 1, 2, 3, 5, 6, 7, 8, 9, 10, 11, 13, 14, 15, 16, 17, 18, 19 }),
                    new Cat(acceptablePeople: new int[] { 2, 8, 9, 10, 13, 14, 15, 17 }),
                };
                Person[] people12 = new Person[]
                {
                    new Person(maxCats: 6, salaries: new int[] { 112, 510, 558, 630, 797, 319, 921, 830, 624, 31, 576, 432, 219, 564, 68, 720, 80, 884, 171, 24, 120, 490, 814, 231, 343 }),
                    new Person(maxCats: 4, salaries: new int[] { 960, 461, 585, 271, 433, 547, 736, 798, 940, 378, 786, 43, 403, 553, 129, 511, 644, 839, 660, 473, 563, 874, 103, 334, 208 }),
                    new Person(maxCats: 3, salaries: new int[] { 95, 712, 220, 937, 707, 904, 236, 362, 411, 433, 679, 153, 872, 457, 42, 959, 827, 942, 799, 940, 146, 994, 4, 17, 685 }),
                    new Person(maxCats: 4, salaries: new int[] { 857, 235, 147, 529, 399, 941, 987, 505, 334, 703, 839, 292, 289, 831, 10, 487, 294, 205, 440, 967, 282, 943, 237, 201, 672 }),
                    new Person(maxCats: 2, salaries: new int[] { 573, 865, 906, 534, 859, 715, 776, 991, 465, 772, 452, 256, 957, 233, 563, 604, 683, 674, 257, 401, 971, 790, 966, 476, 92 }),
                    new Person(maxCats: 1, salaries: new int[] { 715, 79, 400, 848, 864, 662, 997, 379, 713, 548, 831, 703, 206, 123, 853, 238, 487, 40, 520, 776, 585, 777, 854, 332, 520 }),
                    new Person(maxCats: 2, salaries: new int[] { 98, 689, 840, 648, 86, 873, 462, 945, 900, 283, 902, 535, 922, 679, 406, 505, 584, 420, 179, 731, 712, 342, 161, 905, 617 }),
                    new Person(maxCats: 4, salaries: new int[] { 435, 362, 372, 23, 230, 844, 578, 228, 104, 423, 405, 208, 934, 315, 313, 982, 997, 838, 459, 39, 717, 92, 275, 660, 451 }),
                    new Person(maxCats: 3, salaries: new int[] { 170, 802, 260, 37, 449, 451, 952, 918, 545, 394, 571, 637, 719, 403, 783, 870, 968, 679, 486, 764, 122, 443, 957, 552, 170 }),
                    new Person(maxCats: 2, salaries: new int[] { 277, 160, 948, 731, 951, 79, 945, 610, 586, 986, 467, 557, 843, 388, 261, 158, 875, 681, 654, 539, 35, 831, 688, 452, 738 }),
                    new Person(maxCats: 3, salaries: new int[] { 206, 663, 904, 55, 240, 508, 424, 539, 664, 117, 346, 465, 419, 168, 542, 819, 10, 552, 800, 264, 19, 332, 651, 767, 889 }),
                    new Person(maxCats: 5, salaries: new int[] { 384, 957, 935, 452, 593, 702, 707, 128, 736, 992, 397, 869, 861, 276, 41, 658, 252, 905, 35, 365, 750, 599, 210, 847, 413 }),
                    new Person(maxCats: 2, salaries: new int[] { 941, 962, 216, 208, 212, 555, 681, 139, 511, 644, 231, 546, 333, 68, 287, 994, 788, 762, 596, 794, 327, 950, 459, 557, 775 }),
                    new Person(maxCats: 4, salaries: new int[] { 754, 453, 652, 119, 241, 874, 529, 757, 964, 57, 827, 478, 67, 207, 268, 791, 217, 619, 688, 832, 132, 719, 502, 64, 337 }),
                    new Person(maxCats: 1, salaries: new int[] { 233, 820, 817, 344, 33, 126, 820, 218, 953, 455, 96, 521, 659, 226, 857, 156, 647, 712, 837, 332, 637, 475, 262, 342, 211 }),
                    new Person(maxCats: 8, salaries: new int[] { 829, 754, 359, 116, 411, 631, 509, 541, 658, 303, 502, 958, 445, 246, 376, 770, 205, 51, 906, 665, 393, 881, 261, 107, 115 }),
                    new Person(maxCats: 3, salaries: new int[] { 317, 683, 242, 156, 874, 50, 862, 407, 612, 26, 736, 325, 619, 247, 380, 729, 466, 402, 474, 445, 730, 179, 975, 969, 303 }),
                    new Person(maxCats: 2, salaries: new int[] { 377, 649, 287, 916, 116, 665, 987, 654, 975, 917, 770, 734, 892, 958, 93, 363, 459, 657, 544, 282, 966, 873, 135, 358, 336 }),
                    new Person(maxCats: 5, salaries: new int[] { 238, 327, 891, 96, 467, 35, 662, 779, 866, 116, 948, 39, 546, 633, 755, 442, 570, 395, 884, 757, 975, 740, 139, 173, 740 }),
                    new Person(maxCats: 2, salaries: new int[] { 993, 267, 312, 930, 181, 235, 181, 993, 489, 192, 276, 727, 577, 342, 128, 450, 482, 222, 327, 130, 722, 553, 538, 890, 826 }),
                };
                AddStage1a(new Stage1TestCase(cats: cats12, people: people12, expectedIsPossible: true, checkAssignment: false, timeLimit: 1, description: name12));
                AddStage1b(new Stage1TestCase(cats: cats12, people: people12, expectedIsPossible: true, checkAssignment: true, timeLimit: 1, description: name12));
                AddStage2a(new Stage2TestCase(cats: cats12, people: people12, expectedIsPossible: true, checkAssignment: false, expectedMinCost: 4213, timeLimit: 1, description: name12));
                AddStage2b(new Stage2TestCase(cats: cats12, people: people12, expectedIsPossible: true, checkAssignment: true, expectedMinCost: 4213, timeLimit: 1, description: name12));


                string name13 = "Duży losowy przykład 1";
                (Cat[] cats13, Person[] people13) = GetRandomInput(numCats: 25, numPeople: 50, maxCatsAllowed: 5, maxSalaryAllowed: 100, seed: 77);
                AddStage1a(new Stage1TestCase(cats: cats13, people: people13, expectedIsPossible: true, checkAssignment: false, timeLimit: 1, description: name13));
                AddStage1b(new Stage1TestCase(cats: cats13, people: people13, expectedIsPossible: true, checkAssignment: true, timeLimit: 1, description: name13));
                AddStage2a(new Stage2TestCase(cats: cats13, people: people13, expectedIsPossible: true, checkAssignment: false, expectedMinCost: 120, timeLimit: 5.5, description: name13));
                AddStage2b(new Stage2TestCase(cats: cats13, people: people13, expectedIsPossible: true, checkAssignment: true, expectedMinCost: 120, timeLimit: 5.5, description: name13));


                string name14 = "Duży losowy przykład 2";
                (Cat[] cats14, Person[] people14) = GetRandomInput(numCats: 50, numPeople: 25, maxCatsAllowed: 5, maxSalaryAllowed: 5, seed: 0);
                AddStage1a(new Stage1TestCase(cats: cats14, people: people14, expectedIsPossible: true, checkAssignment: false, timeLimit: 1, description: name14));
                AddStage1b(new Stage1TestCase(cats: cats14, people: people14, expectedIsPossible: true, checkAssignment: true, timeLimit: 1, description: name14));
                AddStage2a(new Stage2TestCase(cats: cats14, people: people14, expectedIsPossible: true, checkAssignment: false, expectedMinCost: 63, timeLimit: 2, description: name14));
                AddStage2b(new Stage2TestCase(cats: cats14, people: people14, expectedIsPossible: true, checkAssignment: true, expectedMinCost: 63, timeLimit: 2, description: name14));


                string name15 = "Duży losowy przykład 3";
                (Cat[] cats15, Person[] people15) = GetRandomInput(numCats: 50, numPeople: 30, maxCatsAllowed: 7, maxSalaryAllowed: 150, seed: 33);
                AddStage1a(new Stage1TestCase(cats: cats15, people: people15, expectedIsPossible: true, checkAssignment: false, timeLimit: 1, description: name15));
                AddStage1b(new Stage1TestCase(cats: cats15, people: people15, expectedIsPossible: true, checkAssignment: true, timeLimit: 1, description: name15));
                AddStage2a(new Stage2TestCase(cats: cats15, people: people15, expectedIsPossible: true, checkAssignment: false, expectedMinCost: 718, timeLimit: 13, description: name15));
                AddStage2b(new Stage2TestCase(cats: cats15, people: people15, expectedIsPossible: true, checkAssignment: true, expectedMinCost: 718, timeLimit: 13, description: name15));
            }

            private (Cat[] cats, Person[] people) GetRandomInput(int numCats, int numPeople, int maxCatsAllowed, int maxSalaryAllowed, int seed)
            {
                Random rng = new Random(seed);

                Cat[] cats = new Cat[numCats];
                for (int catId = 0; catId < numCats; ++catId)
                {
                    int[] shuffledPeopleIds = Enumerable.Range(0, numPeople).OrderBy(e => rng.Next()).ToArray();
                    int numAcceptablePeople = rng.Next(1, numPeople);
                    int[] acceptablePeople = new int[numAcceptablePeople];
                    for (int i = 0; i < numAcceptablePeople; ++i)
                    {
                        acceptablePeople[i] = shuffledPeopleIds[i];
                    }
                    cats[catId] = new Cat(acceptablePeople: acceptablePeople);
                }

                Person[] people = new Person[numPeople];
                for (int personId = 0; personId < numPeople; ++personId)
                {
                    int maxCats = rng.Next(1, maxCatsAllowed);
                    int[] salaries = new int[numCats];
                    for (int i = 0; i < numCats; ++i)
                    {
                        salaries[i] = rng.Next(1, maxSalaryAllowed);
                    }
                    people[personId] = new Person(maxCats, salaries);
                }

                return (cats, people);
            }
        }

        static void Main(string[] args)
        {
            var tests = new Lab08Tests();
            tests.PrepareTestSets();
            foreach (var ts in tests.TestSets)
            {
                ts.Value.PerformTests(verbose: true, checkTimeLimit: false);
            }

            if (tests.AreAllTestsPassed())
            {
                Console.WriteLine("Wesołych Świąt! :)");
                Console.WriteLine(" /) /)");
                Console.WriteLine("( ^.^ )");
                Console.WriteLine("C(“) (“)");
            }
        }
    }
}
