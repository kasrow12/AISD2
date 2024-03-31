using System;
using System.Linq;
using ASD.Graphs;
using ASD.Graphs.Testing;

namespace ASD
{
	public class Lab06Stage1Case : Lab06StageBase
	{
		readonly int start;

		public Lab06Stage1Case(DiGraph<int> c,
							   Graph<int> g,
							   int start,
							   int end,
							   int? expectedResult,
							   double timeLimit,
							   string description) : base(c, g, end, expectedResult, timeLimit, description)
		{
			this.start = start;
		}

		protected override bool CheckResultValue => false;

		protected override int[] PossibleStarts => new int[] { start };

		protected override void PerformTestCase(object prototypeObject)
		{
			var res = ((Lab06)prototypeObject).Stage1(c.VertexCount, cClone, gClone, end, start);
			foundResult = res.Item1;
			resultPath = res.Item2;
		}

	}
	public class Lab06Stage2Case : Lab06StageBase
	{
		readonly int[] starts;
		public Lab06Stage2Case(DiGraph<int> c,
							   Graph<int> g,
							   int start,
							   int end,
							   int? expectedResult,
							   double timeLimit,
							   string description) : base(c, g, end, expectedResult, timeLimit, description)
		{
			starts = new int[] { start };
		}

		public Lab06Stage2Case(DiGraph<int> c,
							   Graph<int> g,
							   int[] starts,
							   int end,
							   int? expectedResult,
							   double timeLimit,
							   string description) : base(c, g, end, expectedResult, timeLimit, description)
		{
			this.starts = starts;
		}

		protected override bool CheckResultValue => true;

		protected override int[] PossibleStarts => starts;

		protected override void PerformTestCase(object prototypeObject)
		{
			var res = ((Lab06)prototypeObject).Stage2(c.VertexCount, cClone, gClone, end, starts);
			foundResult = res.Item1.HasValue;
			if (foundResult.Value)
			{
				foundResultCost = res.Item1.Value;
			}
			resultPath = res.Item2;
		}

	}
	public abstract class Lab06StageBase : TestCase
	{
		protected readonly DiGraph<int> c;
		protected readonly Graph<int> g;
		protected readonly DiGraph<int> cClone;
		protected readonly Graph<int> gClone;
		protected readonly int end;
		protected readonly int? expectedResult;
		protected bool? foundResult = null;
		protected int foundResultCost = -1;
		protected int[] resultPath = null;
		public Lab06StageBase(DiGraph<int> c,
							   Graph<int> g,
							   int end,
							   int? expectedResult,
							   double timeLimit,
							   string description) : base(timeLimit, null, description)
		{
			this.c = c;
			this.g = g;
			this.cClone = new DiGraph<int>(c);
			this.gClone = new Graph<int>(g);
			this.end = end;
			this.expectedResult = expectedResult;
		}

		protected abstract bool CheckResultValue { get; }
		protected abstract int[] PossibleStarts { get; }

		public (Result resultCode, string message) OkResult(string message) => (TimeLimit < PerformanceTime ? Result.LowEfficiency : Result.Success, $"{message} {PerformanceTime.ToString("#0.00")}s");


		protected override (Result resultCode, string message) VerifyTestCase(object settings)
		{
			if (foundResult is null)
			{
				return (Result.NotPerformed, "Test not performed. Internal error.");
			}
			else
			{
				if (foundResult != expectedResult.HasValue)
				{
					return (Result.WrongResult, foundResult.Value ? "Zwrócono, że rozwiązanie istnieje, choć tak nie jest" : "Zwrócono, że rozwiązanie nie istnieje mimo, że w rzeczywistośći istnieje");
				}
				else
				{
					if (resultPath is null)
					{
						return (Result.WrongResult, "Tablica z rozwiązaniem jest nullem");
					}
					else
					{
						if (!expectedResult.HasValue)
						{
							if (resultPath.Length == 0)
							{
								return OkResult("OK.");
							}
							else
							{
								return (Result.WrongResult, $"Tablica z rozwiązaniem powinna mieć długość 0, a ma {resultPath.Length}");
							}
						}
						else
						{
							return CheckPath(resultPath);
						}
					}
				}
			}
		}

		private (Result resultCode, string message) CheckPath(int[] path)
		{
			if (path.Length < 1)
			{
				return (Result.WrongResult, "Zwrócono tablicę o długości 0, a rozwiązanie istnieje");
			}
			else
			{
				if (!this.PossibleStarts.Contains(path[0]))
				{
					return (Result.WrongResult, $"Pierwszym elementem ścieżki nie jest wierzchołek ze zbioru [{String.Join(",", PossibleStarts)}], a wierzchołek {path[0]}");
				}
				else if (path.Last() != end)
				{
					return (Result.WrongResult, $"Ostatnim elementem ścieżki nie jest {end}, a wierzchołek {path.Last()}");
				}
				else
				{
					string resultMsg = null;
					int costSum = path.Length - 1; // Liczba krawędzi
					for (int i = 0; i < path.Length - 2 && resultMsg is null; i++)
					{
						var u = path[i];
						var v = path[i + 1];
						var w = path[i + 2];

						if (!g.HasEdge(u, v))
						{
							resultMsg = $"Rozwiązanie zawiera przejście z {u} do {v}, choć w grafie g nie ma takiej krawędzi";
						}
						else if (!g.HasEdge(v, w))
						{
							resultMsg = $"Rozwiązanie zawiera przejście z {v} do {w}, choć w grafie g nie ma takiej krawędzi";
						}
						else
						{
							var color1 = g.GetEdgeWeight(u, v);
							var color2 = g.GetEdgeWeight(v, w);
							if (color1 != color2 && !c.HasEdge(color1, color2))
							{
								resultMsg = $"W rozwiązaniu jest przejście {u}->{v} w kolorze {color1}, a następnie {v}->{w} w kolorze {color2}, ale w grafie c nie ma krawędzi {color1}->{color2}";
							}
							else if (color1 != color2)
							{
								costSum += c.GetEdgeWeight(color1, color2);
							}

						}
					}

					if (resultMsg is null)
					{
						if (CheckResultValue && (costSum != expectedResult.Value || costSum != foundResultCost))
						{
							return (Result.WrongResult, $"Wysiłek zwróconej trasy to {costSum}, wartość zwrócona to {foundResultCost},  a powinno być {expectedResult.Value}");
						}
						else
						{
							return OkResult("OK");
						}
					}
					else
					{
						return (Result.WrongResult, resultMsg);
					}
				}
			}
		}
	}

	class Lab06Tests : TestModule
	{
		public TestSet stage1Tests;
		public TestSet stage2Tests;

		public override void PrepareTestSets()
		{
			stage1Tests = new TestSet(new Lab06(), "Etap 1");
			stage2Tests = new TestSet(new Lab06(), "Etap 2");
			TestSets["Etap 1"] = stage1Tests;
			TestSets["Etap 2"] = stage2Tests;
			PrepareStage1();
		}

		private void PrepareStage1()
		{
			{//Test 1
				Graph<int> g = new Graph<int>(5);
				g.AddEdge(0, 1, 0);
				g.AddEdge(1, 2, 1);
				g.AddEdge(2, 4, 2);
				g.AddEdge(0, 3, 0);
				g.AddEdge(3, 4, 2);
				DiGraph<int> c = new DiGraph<int>(3);
				c.AddEdge(0, 1, 1);
				c.AddEdge(1, 2, 1);
				const int Start = 0;
				const int End = 4;
				const int ExpectedResult = 5;
				stage1Tests.TestCases.Add(new Lab06Stage1Case(c, g, Start, End, ExpectedResult, 1, "Przykład 1"));
				stage2Tests.TestCases.Add(new Lab06Stage2Case(c, g, Start, End, ExpectedResult, 1, "Przykład 1"));
			}
			{//Test 2
				Graph<int> g = new Graph<int>(5);
				g.AddEdge(0, 1, 0);
				g.AddEdge(1, 2, 1);
				g.AddEdge(2, 3, 2);
				g.AddEdge(3, 1, 2);
				g.AddEdge(1, 4, 3);
				DiGraph<int> c = new DiGraph<int>(4);
				c.AddEdge(0, 1, 1);
				c.AddEdge(1, 2, 1);
				c.AddEdge(2, 3, 0);
				const int Start = 0;
				const int End = 4;
				const int ExpectedResult = 7;
				stage1Tests.TestCases.Add(new Lab06Stage1Case(c, g, Start, End, ExpectedResult, 1, "Przykład 2"));
				stage2Tests.TestCases.Add(new Lab06Stage2Case(c, g, Start, End, ExpectedResult, 1, "Przykład 2"));
			}
			{//Test 3
				Graph<int> g = new Graph<int>(5);
				g.AddEdge(0, 1, 0);
				g.AddEdge(1, 2, 1);
				g.AddEdge(1, 4, 2);
				DiGraph<int> c = new DiGraph<int>(3);
				c.AddEdge(0, 1, 1);
				c.AddEdge(1, 2, 1);
				c.AddEdge(0, 2, 5);
				const int Start = 0;
				const int End = 4;
				const int ExpectedResult = 6;
				stage1Tests.TestCases.Add(new Lab06Stage1Case(c, g, Start, End, ExpectedResult, 1, "Przykład 3"));
				stage2Tests.TestCases.Add(new Lab06Stage2Case(c, g, Start, End, ExpectedResult, 1, "Przykład 3"));
			}
			{//Test 4
				Graph<int> g = new Graph<int>(7);
				g.AddEdge(0, 1, 0);
				g.AddEdge(1, 2, 1);
				g.AddEdge(2, 3, 2);
				g.AddEdge(3, 1, 2);
				g.AddEdge(1, 4, 3);
				g.AddEdge(1, 5, 4);
				g.AddEdge(1, 6, 5);
				DiGraph<int> c = new DiGraph<int>(6);
				c.AddEdge(0, 1, 1);
				c.AddEdge(1, 2, 1);
				c.AddEdge(2, 3, 1);
				c.AddEdge(3, 4, 1);
				c.AddEdge(4, 5, 1);

				const int Start = 0;
				const int End = 6;
				const int ExpectedResult = 14;
				stage1Tests.TestCases.Add(new Lab06Stage1Case(c, g, Start, End, ExpectedResult, 1, "Przykład 4"));
				stage2Tests.TestCases.Add(new Lab06Stage2Case(c, g, Start, End, ExpectedResult, 1, "Przykład 4"));
			}

			{//Test 5
				Graph<int> g = new Graph<int>(5);
				DiGraph<int> c = new DiGraph<int>(5);
				g.AddEdge(0, 1, 0);
				g.AddEdge(1, 2, 1);
				g.AddEdge(2, 4, 4);
				g.AddEdge(0, 3, 0);
				g.AddEdge(3, 4, 2);
				c.AddEdge(0, 1, 1);
				c.AddEdge(1, 2, 1);
				c.AddEdge(2, 3, 1);
				const int Start = 0;
				const int End = 4;
				int? ExpectedResult = null;
				stage1Tests.TestCases.Add(new Lab06Stage1Case(c, g, Start, End, ExpectedResult, 1, "Cykl -- niemożliwe zmiany kolorów"));
				stage2Tests.TestCases.Add(new Lab06Stage2Case(c, g, Start, End, ExpectedResult, 1, "Cykl -- niemożliwe zmiany kolorow"));
			}
			{//Test 6
				int w = 4;
				int h = 3;
				DiGraph<int> c = new DiGraph<int>(1);

				Graph<int> g = GridGraph(w, h);
				int start = 0;
				int end = w * h - 1;
				const int ExpectedResult = 5;
				stage1Tests.TestCases.Add(new Lab06Stage1Case(c, g, start, end, ExpectedResult, 1, "Jednokolorowa siatka 4×3"));
				stage2Tests.TestCases.Add(new Lab06Stage2Case(c, g, start, end, ExpectedResult, 1, "Jednokolorowa siatka 4×3"));
			}

			AddGridTest(11, 10, 1, 1);//Test 7
			AddGridTest(111, 110, 3, 12);//Test 8
			AddTreeBasedTest(5, 2, 5, 3, 8);//Test 9
			AddRandomTest(150, 10, 4523, 2, 1, 8); // Test 10
			AddRandomTest(150, 100, 1337, 5, 1, 8); // Test 11
			{//Test 12
				Graph<int> g = new Graph<int>(5);
				g.AddEdge(0, 1, 0);
				g.AddEdge(1, 2, 1);
				g.AddEdge(2, 4, 2);
				g.AddEdge(0, 3, 0);
				g.AddEdge(3, 4, 2);
				DiGraph<int> c = new DiGraph<int>(3);
				c.AddEdge(1, 0, 1);
				c.AddEdge(2, 1, 1);
				const int Start = 0;
				const int End = 4;
				int? ExpectedResult = null;
				stage1Tests.TestCases.Add(new Lab06Stage1Case(c, g, Start, End, ExpectedResult, 1, "Przykład 1 z odwróconymi krawędziami grafu c"));
				stage2Tests.TestCases.Add(new Lab06Stage2Case(c, g, Start, End, ExpectedResult, 1, "Przykład 1 z odwróconymi krawędziami grafu c"));
			}
			{//Test 13
				Graph<int> g = new Graph<int>(7);
				g.AddEdge(0, 1, 0);
				g.AddEdge(1, 2, 1);
				g.AddEdge(2, 3, 2);
				g.AddEdge(3, 1, 2);
				g.AddEdge(1, 4, 3);
				g.AddEdge(1, 5, 4);
				g.AddEdge(1, 6, 5);
				DiGraph<int> c = new DiGraph<int>(6);
				c.AddEdge(1, 0, 1);
				c.AddEdge(2, 1, 1);
				c.AddEdge(3, 2, 1);
				c.AddEdge(4, 3, 1);
				c.AddEdge(5, 4, 1);

				const int Start = 0;
				const int End = 6;
				int? ExpectedResult = null;
				stage1Tests.TestCases.Add(new Lab06Stage1Case(c, g, Start, End, ExpectedResult, 1, "Przykład 4 z odwróconymi krawędziami grafu c"));
				stage2Tests.TestCases.Add(new Lab06Stage2Case(c, g, Start, End, ExpectedResult, 1, "Przykład 4 z odwróconymi krawędziami grafu c"));
			}
			{//Test 14
				int w = 4;
				int h = 3;
				DiGraph<int> c = new DiGraph<int>(30);
				for (int i = 0; i < c.VertexCount; i++)
				{
					for (int j = 0; j < c.VertexCount; j++)
					{
						if (i != j)
						{
							c.AddEdge(i, j, 1);
						}
					}
				}

				Graph<int> g = GridGraph(w, h);
				int start = 0;
				int end = w * h - 1;
				const int ExpectedResult = 5;
				stage1Tests.TestCases.Add(new Lab06Stage1Case(c, g, start, end, ExpectedResult, 1, "Jednokolorowa siatka 4×3, dużo kolorów w grafie c"));
				stage2Tests.TestCases.Add(new Lab06Stage2Case(c, g, start, end, ExpectedResult, 1, "Jednokolorowa siatka 4×3, dużo kolorów w grafie c"));
			}
			{//Test 15
				int w = 4;
				int h = 3;
				DiGraph<int> c = new DiGraph<int>(30);
				for (int i = 0; i < c.VertexCount; i++)
				{
					for (int j = 0; j < c.VertexCount; j++)
					{
						if (i != j)
						{
							c.AddEdge(i, j, 1);
						}
					}
				}

				Graph<int> g = GridGraphMulticolor(w, h, c.VertexCount);
				int start = 0;
				int end = w * h - 1;
				const int ExpectedResult = 7;
				stage1Tests.TestCases.Add(new Lab06Stage1Case(c, g, start, end, ExpectedResult, 1, "Wielokolorowa siatka 4×3, dużo kolorów w grafie c"));
				stage2Tests.TestCases.Add(new Lab06Stage2Case(c, g, start, end, ExpectedResult, 1, "Wielokolorowa siatka 4×3, dużo kolorów w grafie c"));
			}

			AddMultiinputGridTest(11, 10, 1, 1); // Test 16
			AddMultiinputGridTest(111, 110, 1, 12); //Test 17
		}

		private void AddRandomTest(int gSize, int cSize, int seed, int? expectedResult, double timeLimitS1, double timeLimitS2)
		{
			var rgg = new RandomGraphGenerator(seed);
			Graph prawieG = rgg.Graph(gSize, 0.2);
			Graph<int> g = rgg.AssignWeights(prawieG, 0, cSize - 1);
			DiGraph prawieC = rgg.DiGraph(cSize, 0.4);
			DiGraph<int> c = rgg.AssignWeights(prawieC, 0, 15);

			int start = 0;
			int end = 1;

			stage1Tests.TestCases.Add(new Lab06Stage1Case(c, g, start, end, expectedResult, timeLimitS1, $"Losowy graf, seed {seed}"));
			stage2Tests.TestCases.Add(new Lab06Stage2Case(c, g, start, end, expectedResult, timeLimitS2, $"Losowy graf, seed {seed}"));

		}

		private void AddGridTest(int w, int h, double timeLimitS1, double timeLimitS2)
		{
			string name = $"Siatka {w}×{h} wierzchołków, kolory dopuszczają tylko przejście zygzakiem przez siatkę";
			DiGraph<int> c = new DiGraph<int>(h + 1);
			c.AddEdge(0, 1, 1);
			c.AddEdge(1, 0, 1);
			for (int i = 2; i < h; i++)
			{
				for (int j = i + 1; j < h; j++)
				{
					c.AddEdge(i, j, 0);
					c.AddEdge(j, i, 0);
				}
			}
			Graph<int> g = new Graph<int>(w * h);
			int vertexId(int x, int y) => x + y * w;
			for (int i = 0; i < w; i++)
			{
				for (int j = 0; j < h - 1; j++)
				{
					g.AddEdge(vertexId(i, j), vertexId(i, j + 1), 0);
				}
			}
			for (int i = 0; i < w - 1; i++)
			{
				for (int j = 0; j < h; j++)
				{
					g.AddEdge(vertexId(i, j), vertexId(i + 1, j), i % 2 == 0 ? h - j : j + 1);
				}
			}
			int start = 0;
			int end = w * h - 1; // Tylko jeśli w jest nieparzyste
			int ExpectedResult = w * h - 1 + 2 * (w - 1);
			stage1Tests.TestCases.Add(new Lab06Stage1Case(c, g, start, end, ExpectedResult, timeLimitS1, name));
			stage2Tests.TestCases.Add(new Lab06Stage2Case(c, g, start, end, ExpectedResult, timeLimitS2, name));
		}
		private void AddMultiinputGridTest(int w, int h, double timeLimitS1, double timeLimitS2)
		{
			string name = $"Siatka {w}×{h} wierzchołków, kolory dopuszczają tylko przejście zygzakiem przez siatkę, wejśćia na lewej krawędzi";
			DiGraph<int> c = new DiGraph<int>(h + 1);
			c.AddEdge(0, 1, 1);
			c.AddEdge(1, 0, 1);
			for (int i = 2; i < h; i++)
			{
				for (int j = i + 1; j < h; j++)
				{
					c.AddEdge(i, j, 0);
					c.AddEdge(j, i, 0);
				}
			}
			Graph<int> g = new Graph<int>(w * h);
			int vertexId(int x, int y) => x + y * w;
			for (int i = 0; i < w; i++)
			{
				for (int j = 0; j < h - 1; j++)
				{
					g.AddEdge(vertexId(i, j), vertexId(i, j + 1), 0);
				}
			}
			for (int i = 0; i < w - 1; i++)
			{
				for (int j = 0; j < h; j++)
				{
					g.AddEdge(vertexId(i, j), vertexId(i + 1, j), i % 2 == 0 ? h - j : j + 1);
				}
			}
			int[] starts = Enumerable.Range(0, w).ToArray();
			int end = w * h - 1; // Tylko jeśli w jest nieparzyste
			int ExpectedResult = h - 1;
			stage2Tests.TestCases.Add(new Lab06Stage2Case(c, g, starts, end, ExpectedResult, timeLimitS2, name));
		}

		private void AddTreeBasedTest(int branching, int shift, int depth, double timeLimitS1, double timeLimitS2)
		{
			string name = $"Graf oparty o drzewo, branching {branching}, głębokość {depth}";
			Graph<int> g = new Graph<int>((1 - (int)Math.Pow(branching, depth)) / (1 - branching) + 1);
			DiGraph<int> c = new DiGraph<int>((int)Math.Pow(branching, depth - 1) + 1);

			int zeroidx = shift % (int)Math.Pow(branching, depth - 1);
			for (int level = depth - 1; level > 0; level--)
			{
				int levelOffset = (1 - (int)Math.Pow(branching, level) / (1 - branching)) - 1;
				int prevLevelOffset = (1 - (int)Math.Pow(branching, level - 1) / (1 - branching)) - 1;
				int levelSize = (int)Math.Pow(branching, level);
				for (int i = 0; i < levelSize; i++)
				{
					g.AddEdge(i + levelOffset, prevLevelOffset + (i / branching), (depth - level) + (levelSize - zeroidx + i) % levelSize);

				}
				zeroidx /= 3;
			}

			int leavesCount = (1 - (int)Math.Pow(branching, depth - 1) / (1 - branching));
			int lastLevelOffset = (1 - (int)Math.Pow(branching, depth - 1) / (1 - branching)) - 1;
			for (int i = 0; i < leavesCount; i++)
			{
				g.AddEdge(i + lastLevelOffset, g.VertexCount - 1, 0);
			}

			for (int i = 0; i < c.VertexCount; i++)
			{
				for (int j = 0; j < c.VertexCount; j++)
				{
					if (i != j)
					{
						c.AddEdge(i, j, Math.Abs(i - j));
					}
				}
			}
			int start = 0;
			int end = g.VertexCount - 1;
			int expectedResult = depth * 2 - 1;
			stage1Tests.TestCases.Add(new Lab06Stage1Case(c, g, start, end, expectedResult, timeLimitS1, name));
			stage2Tests.TestCases.Add(new Lab06Stage2Case(c, g, start, end, expectedResult, timeLimitS2, name));
		}

		private static Graph<int> GridGraph(int w, int h)
		{
			Graph<int> g = new Graph<int>(w * h);
			for (int i = 0; i < w; i++)
			{
				for (int j = 1; j < h; j++)
				{
					g.AddEdge(i + (j - 1) * w, i + j * w, 0);
				}
			}

			for (int i = 1; i < w; i++)
			{
				for (int j = 0; j < h; j++)
				{
					g.AddEdge(i - 1 + j * w, i + j * w, 0);
				}
			}

			return g;
		}

		private static Graph<int> GridGraphMulticolor(int w, int h, int c)
		{
			Graph<int> g = new Graph<int>(w * h);
			for (int i = 0; i < w; i++)
			{
				for (int j = 1; j < h; j++)
				{
					g.AddEdge(i + (j - 1) * w, i + j * w, (i + j * w) % c);
				}
			}

			for (int i = 1; i < w; i++)
			{
				for (int j = 0; j < h; j++)
				{
					g.AddEdge(i - 1 + j * w, i + j * w, 0);
				}
			}

			return g;
		}

		public override double ScoreResult()
		{
			if (stage1Tests.FailedCount > 0 || stage1Tests.TimeoutsCount > 0 || stage1Tests.LowEfficiencyCount > 0 ||
			   stage2Tests.FailedCount > 0 || stage2Tests.TimeoutsCount > 0 || stage2Tests.LowEfficiencyCount > 0)
			{
				return -2.5;
			}
			else
			{
				return 2.5;
			}
		}

	}


	class Program
	{

		public static void Main(string[] args)
		{
			var tests = new Lab06Tests();
			tests.PrepareTestSets();
			foreach (var ts in tests.TestSets)
			{
				ts.Value.PerformTests(verbose: true, checkTimeLimit: false);
			}

		}
	}
}
