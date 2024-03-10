using ASD.Graphs;

namespace ASD;

public class Lab03 : MarshalByRefObject
{
    // Część I
    // Funkcja zwracajaca kwadrat danego grafu.
    // Kwadratem grafu nazywamy graf o takim samym zbiorze wierzchołków jak graf pierwotny, w którym wierzchołki
    // połączone sa krawędzią jeśli w grafie pierwotnym były polączone krawędzia bądź ścieżką złożoną z 2 krawędzi
    // (ale pętli, czyli krawędzi o początku i końcu w tym samym wierzchołku, nie dodajemy!).
    public Graph Square(Graph graph)
    {
        var h = (Graph)graph.Clone();

        for (int v = 0; v < graph.VertexCount; v++)
        {
            foreach (int u in graph.OutNeighbors(v))
            {
                foreach (int w in graph.OutNeighbors(u))
                {
                    if (w != v)
                        h.AddEdge(v, w);
                }
            }
        }

        return h;
    }

    // Część II
    // Funkcja zwracająca Graf krawędziowy danego grafu.
    // Wierzchołki grafu krawędziwego odpowiadają krawędziom grafu pierwotnego, wierzcholki grafu krawędziwego
    // połączone sa krawędzią jeśli w grafie pierwotnym z krawędzi odpowiadającej pierwszemu z nich można przejść
    // na krawędź odpowiadającą drugiemu z nich przez wspólny wierzchołek.

    // Tablicę names tworzymy i wypełniamy według następującej zasady.
    // Każdemu wierzchołkowi grafu krawędziowego odpowiada element tablicy names (o indeksie równym numerowi wierzchołka)
    // zawierający informację z jakiej krawędzi grafu pierwotnego wierzchołek ten powstał.
    // Np.dla wierzchołka powstałego z krawedzi <0,1> do tablicy zapisujemy krotke (0, 1) - przyda się w dalszych etapach
    public Graph LineGraph(Graph graph, out (int x, int y)[] names)
    {
        names = new (int x, int y)[graph.EdgeCount];
        var h = new Graph(graph.EdgeCount, graph.Representation);
        var dict = new Dictionary<(int, int), int>();

        int index = 0;
        foreach (var e in graph.DFS().SearchAll())
        {
            (int f, int t) edge = e.From < e.To ? (e.From, e.To) : (e.To, e.From);

            if (!dict.ContainsKey(edge))
            {
                names[index] = edge;
                dict[edge] = index++;
            }

            foreach (int v in graph.OutNeighbors(edge.f))
            {
                if (v == edge.t)
                    continue;

                (int f, int t) ee = edge.f < v ? (edge.f, v) : (v, edge.f);

                if (!dict.ContainsKey(ee))
                {
                    names[index] = ee;
                    dict[ee] = index++;
                }

                h.AddEdge(dict[edge], dict[ee]);
            }

            foreach (int v in graph.OutNeighbors(edge.t))
            {
                if (v == edge.f)
                    continue;

                (int f, int t) ee = edge.t < v ? (edge.t, v) : (v, edge.t);

                if (!dict.ContainsKey(ee))
                {
                    names[index] = ee;
                    dict[ee] = index++;
                }

                h.AddEdge(dict[edge], dict[ee]);
            }
        }

        return h;
    }

    // Część III
    // Funkcja znajdujaca poprawne kolorowanie wierzchołków danego grafu nieskierowanego.
    // Kolorowanie wierzchołków jest poprawne, gdy każde dwa sąsiadujące wierzchołki mają różne kolory
    // Funkcja ma szukać kolorowania według następujacego algorytmu zachłannego:

    // Dla wszystkich wierzchołków v (od 0 do n-1)
    // pokoloruj wierzcholek v kolorem o najmniejszym możliwym numerze(czyli takim, na który nie są pomalowani jego sąsiedzi)
    // Kolory numerujemy począwszy od 0.

    // UWAGA: Podany opis wyznacza kolorowanie jednoznacznie, jakiekolwiek inne kolorowanie, nawet jeśli spełnia formalnie
    // definicję kolorowania poprawnego, na potrzeby tego zadania będzie uznane za błędne.

    // Funkcja zwraca liczbę użytych kolorów (czyli najwyższy numer użytego koloru + 1),
    // a w tablicy colors zapamiętuje kolory poszczególnych wierzchołkow.
    public int VertexColoring(Graph graph, out int[] colors)
    {
        int usedColors = 0;

        colors = new int[graph.VertexCount];
        for (int i = 0; i < graph.VertexCount; i++)
            colors[i] = -1;

        for (int v = 0; v < graph.VertexCount; v++)
        {
            var neighbors = new HashSet<int>();
            foreach (int u in graph.OutNeighbors(v))
            {
                if (colors[u] >= 0)
                    neighbors.Add(colors[u]);
            }

            int color = 0;
            while (neighbors.Contains(color))
                color++;

            colors[v] = color;
            if (color >= usedColors)
                usedColors++;
        }

        return usedColors;
    }

    // Funkcja znajduje silne kolorowanie krawędzi danego grafu.
    // Silne kolorowanie krawędzi grafu jest poprawne gdy każde dwie krawędzie, które są ze sobą sąsiednie
    // (czyli można przejść z jednej na drugą przez wspólny wierzchołek)
    // albo są połączone inną krawędzią(czyli można przejść z jednej na drugą przez ową inną krawędź), mają różne kolory.

    // Należy zwrocić nowy graf, który będzie miał strukturę identyczną jak zadany graf,
    // ale w wagach krawędzi zostaną zapisane przydzielone kolory.

    // Wskazówka - to bardzo proste.Należy wykorzystać wszystkie poprzednie funkcje.
    // Zastanowić się co możemy powiedzieć o kolorowaniu wierzchołków kwadratu grafu krawędziowego?
    // Jak się to ma do silnego kolorowania krawędzi grafu pierwotnego?
    public int StrongEdgeColoring(Graph graph, out Graph<int> coloredGraph)
    {
        var h = Square(LineGraph(graph, out (int, int)[] names));
        int usedColors = VertexColoring(h, out int[] colors);

        coloredGraph = new Graph<int>(graph.VertexCount, graph.Representation);

        for (int i = 0; i < names.Length; i++)
        {
            (int u, int v) = names[i];
            if (graph.HasEdge(u, v))
                coloredGraph.AddEdge(u, v, colors[i]);
        }

        return usedColors;
    }
}