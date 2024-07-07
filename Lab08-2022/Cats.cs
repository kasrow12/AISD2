using ASD.Graphs;

namespace ASD_lab08;

[Serializable]
public struct Cat
{
    /// <summary>
    ///     Zawiera identyfikatory osób, które kot zaakceptuje
    /// </summary>
    public int[] AcceptablePeople { get; }

    public Cat(int[] acceptablePeople)
    {
        AcceptablePeople = acceptablePeople;
    }
}

[Serializable]
public struct Person
{
    /// <summary>
    ///     Maksymalna liczba kotów, którymi zajmie się opiekun
    /// </summary>
    public int MaxCats { get; }

    /// <summary>
    ///     Kwoty, które osoba życzy sobie za opiekę nad kotami (catId -> int)
    /// </summary>
    public int[] Salaries { get; }

    public Person(int maxCats, int[] salaries)
    {
        MaxCats = maxCats;
        Salaries = salaries;
    }
}

public class Cats : MarshalByRefObject
{
    /// <summary>
    ///     Zadanie pierwsze, w którym nie bierzemy pod uwagę pieniędzy jakie nam przyjdzie zapłacić opiekunom
    /// </summary>
    /// <param name="cats">Tablica zawierające nasze koty</param>
    /// <param name="people">Tablica zawierająca dostępnych opiekunów</param>
    /// <returns>
    ///     isPossible: wartość logiczna oznaczająca, czy przypisanie jest możliwe,
    ///     assignment: przypisanie kotów do opiekunów (personId -> [catId])
    /// </returns>
    public (bool isPossible, int[][] assignment) StageOne(Cat[] cats, Person[] people)
    {
        int n = people.Length;
        int m = cats.Length;
        var g = new DiGraph<int>(n + m + 2);
        int source = n + m;
        int sink = n + m + 1;

        // krawędzie do opiekunów
        for (int person = 0; person < n; person++)
            g.AddEdge(source, person, people[person].MaxCats);

        for (int cat = 0; cat < m; cat++)
        {
            // krawędzie do odpowiednich kotów
            foreach (int person in cats[cat].AcceptablePeople)
                g.AddEdge(person, n + cat, 1);

            // krawędzie od kotów
            g.AddEdge(n + cat, sink, 1);
        }

        (int cap, var flow) = Flows.FordFulkerson(g, source, sink);

        // czy przepływ przez wszystkie koty
        if (cap != m)
            return (false, []);

        int[][] assignment = new int[n][];
        for (int person = 0; person < n; person++)
        {
            assignment[person] = new int[flow.OutDegree(person)];
            int idx = 0;

            foreach (var e in flow.OutEdges(person))
                assignment[person][idx++] = e.To - n; // koty jako druga kolumna grafu (v = n + cat)
        }

        return (true, assignment);
    }

    /// <summary>
    ///     Zadanie drugie, w którym bierzemy pod uwagę kwoty jakie nam przyjdzie zapłacić
    /// </summary>
    /// <param name="cats">Tablica zawierające nasze koty</param>
    /// <param name="people">Tablica zawierająca dostępnych opiekunów</param>
    /// <returns>
    ///     isPossible: wartość logiczna oznaczająca, czy przypisanie jest możliwe,
    ///     assignment: przypisanie kotów do opiekunów (personId -> [catId]),
    ///     minCost: minimalna suma pieniędzy do zapłacenia opiekunom za opiekę nad wszystkimi kotami
    /// </returns>
    public (bool isPossible, int[][] assignment, int minCost) StageTwo(Cat[] cats, Person[] people)
    {
        // analogicznie jak etap 1
        int n = people.Length;
        int m = cats.Length;
        var g = new NetworkWithCosts<int, int>(n + m + 2);
        int source = n + m;
        int sink = n + m + 1;

        // krawędzie do opiekunów, koszt = 0
        for (int person = 0; person < n; person++)
            g.AddEdge(source, person, people[person].MaxCats, 0);

        for (int cat = 0; cat < m; cat++)
        {
            // krawędzie do odpowiednich kotów o koszcie wypłaty
            foreach (int person in cats[cat].AcceptablePeople)
                g.AddEdge(person, n + cat, 1, people[person].Salaries[cat]);

            // krawędzie od kotów, koszt 0
            g.AddEdge(n + cat, sink, 1, 0);
        }

        (int cap, int val, var flow) = Flows.MinCostMaxFlow(g, source, sink);

        // czy przepływ przez wszystkie koty
        if (cap != m)
            return (false, [], 0);

        int[][] assignment = new int[n][];
        for (int person = 0; person < n; person++)
        {
            assignment[person] = new int[flow.OutDegree(person)];

            int idx = 0;
            foreach (var e in flow.OutEdges(person))
                assignment[person][idx++] = e.To - n; // koty jako druga kolumna grafu (v = n + cat)
        }

        return (true, assignment, val);
    }
}