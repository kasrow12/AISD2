using ASD.Graphs;

namespace ASD;

public class ProductionPlanner : MarshalByRefObject
{
    /// <summary>
    ///     Flaga pozwalająca na włączenie wypisywania szczegółów skonstruowanego planu na konsolę.
    ///     Wartość <code>true</code> spoeoduje wypisanie planu.
    /// </summary>
    public bool ShowDebug { get; } = false;

    /// <summary>
    ///     Część 1. zadania - zaplanowanie produkcji telewizorów dla pojedynczego kontrahenta.
    /// </summary>
    /// <remarks>
    ///     Do przeprowadzenia testów wyznaczających maksymalną produkcję i zysk wymagane jest jedynie zwrócenie obiektu
    ///     <see cref="PlanData" />.
    ///     Testy weryfikujące plan wymagają przypisania tablicy z planem do parametru wyjściowego <see cref="weeklyPlan" />.
    /// </remarks>
    /// <param name="production">
    ///     Tablica obiektów zawierających informacje o produkcji fabryki w kolejnych tygodniach.
    ///     Wartości pola <see cref="PlanData.Quantity" /> oznaczają limit produkcji w danym tygodniu,
    ///     a pola <see cref="PlanData.Value" /> - koszt produkcji jednej sztuki.
    /// </param>
    /// <param name="sales">
    ///     Tablica obiektów zawierających informacje o sprzedaży w kolejnych tygodniach.
    ///     Wartości pola <see cref="PlanData.Quantity" /> oznaczają maksymalną sprzedaż w danym tygodniu,
    ///     a pola <see cref="PlanData.Value" /> - cenę sprzedaży jednej sztuki.
    /// </param>
    /// <param name="storageInfo">
    ///     Obiekt zawierający informacje o magazynie.
    ///     Wartość pola <see cref="PlanData.Quantity" /> oznacza pojemność magazynu,
    ///     a pola <see cref="PlanData.Value" /> - koszt przechowania jednego telewizora w magazynie przez jeden tydzień.
    /// </param>
    /// <param name="weeklyPlan">
    ///     Parametr wyjściowy, przez który powinien zostać zwrócony szczegółowy plan sprzedaży.
    /// </param>
    /// <returns>
    ///     Obiekt <see cref="PlanData" /> opisujący wyznaczony plan.
    ///     W polu <see cref="PlanData.Quantity" /> powinna znaleźć się maksymalna liczba wyprodukowanych telewizorów,
    ///     a w polu <see cref="PlanData.Value" /> - wyznaczony maksymalny zysk fabryki.
    /// </returns>
    public PlanData CreateSimplePlan(PlanData[] production, PlanData[] sales, PlanData storageInfo,
        out SimpleWeeklyPlan[] weeklyPlan)
    {
        int n = production.Length;
        var g = new NetworkWithCosts<int, double>(n + 2);
        int source = n;
        int sink = n + 1;

        for (int i = 0; i < n; i++)
        {
            g.AddEdge(source, i, production[i].Quantity, production[i].Value);
            g.AddEdge(i, sink, sales[i].Quantity, -sales[i].Value);
            if (i + 1 < n)
                g.AddEdge(i, i + 1, storageInfo.Quantity, storageInfo.Value);
        }

        (int cap, double val, var flow) = Flows.MinCostMaxFlow(g, source, sink);

        weeklyPlan = new SimpleWeeklyPlan[n];
        for (int i = 0; i < n; i++)
        {
            weeklyPlan[i].UnitsProduced = flow.HasEdge(source, i) ? flow.GetEdgeWeight(source, i) : 0;
            weeklyPlan[i].UnitsSold = flow.HasEdge(i, sink) ? flow.GetEdgeWeight(i, sink) : 0;
            if (i + 1 < n)
                weeklyPlan[i].UnitsStored = flow.HasEdge(i, i + 1) ? flow.GetEdgeWeight(i, i + 1) : 0;
        }

        return new PlanData
        {
            Value = -val,
            Quantity = cap
        };
    }

    /// <summary>
    ///     Część 2. zadania - zaplanowanie produkcji telewizorów dla wielu kontrahentów.
    /// </summary>
    /// <remarks>
    ///     Do przeprowadzenia testów wyznaczających produkcję dającą maksymalny zysk wymagane jest jedynie zwrócenie obiektu
    ///     <see cref="PlanData" />.
    ///     Testy weryfikujące plan wymagają przypisania tablicy z planem do parametru wyjściowego <see cref="weeklyPlan" />.
    /// </remarks>
    /// <param name="production">
    ///     Tablica obiektów zawierających informacje o produkcji fabryki w kolejnych tygodniach.
    ///     Wartość pola <see cref="PlanData.Quantity" /> oznacza limit produkcji w danym tygodniu,
    ///     a pola <see cref="PlanData.Value" /> - koszt produkcji jednej sztuki.
    /// </param>
    /// <param name="sales">
    ///     Dwuwymiarowa tablica obiektów zawierających informacje o sprzedaży w kolejnych tygodniach.
    ///     Pierwszy wymiar tablicy jest równy liczbie kontrahentów, zaś drugi - liczbie tygodni w planie.
    ///     Wartości pola <see cref="PlanData.Quantity" /> oznaczają maksymalną sprzedaż w danym tygodniu,
    ///     a pola <see cref="PlanData.Value" /> - cenę sprzedaży jednej sztuki.
    ///     Każdy wiersz tablicy odpowiada jednemu kontrachentowi.
    /// </param>
    /// <param name="storageInfo">
    ///     Obiekt zawierający informacje o magazynie.
    ///     Wartość pola <see cref="PlanData.Quantity" /> oznacza pojemność magazynu,
    ///     a pola <see cref="PlanData.Value" /> - koszt przechowania jednego telewizora w magazynie przez jeden tydzień.
    /// </param>
    /// <param name="weeklyPlan">
    ///     Parametr wyjściowy, przez który powinien zostać zwrócony szczegółowy plan sprzedaży.
    /// </param>
    /// <returns>
    ///     Obiekt <see cref="PlanData" /> opisujący wyznaczony plan.
    ///     W polu <see cref="PlanData.Quantity" /> powinna znaleźć się optymalna liczba wyprodukowanych telewizorów,
    ///     a w polu <see cref="PlanData.Value" /> - wyznaczony maksymalny zysk fabryki.
    /// </returns>
    public PlanData CreateComplexPlan(PlanData[] production, PlanData[,] sales, PlanData storageInfo,
        out WeeklyPlan[] weeklyPlan)
    {
        int buyers = sales.GetLength(0);
        int weeks = sales.GetLength(1);
        var g = new NetworkWithCosts<int, double>(2 * weeks + buyers + 2);
        int source = 2 * weeks + buyers;
        int sink = 2 * weeks + buyers + 1;

        for (int i = 0; i < weeks; i++)
        {
            // rodzielamy krawędź produkcji na source -> v -> tydzień
            // source -> v [capacity: produkcja w tygodniu, cost: 0]
            // v -> tydzień [capacity: inf, cost: koszt produkcji]
            // niewyprodukowanie pójdzie krawędzią v -> sink [capacity: inf, cost: 0]

            // ograniczenie produkcji
            g.AddEdge(source, i, production[i].Quantity, 0);
            // ilość niewyprodukowanych
            g.AddEdge(i, sink, int.MaxValue, 0);
            // koszt
            g.AddEdge(i, weeks + i, int.MaxValue, production[i].Value);

            // magazynowanie
            if (i + 1 < weeks)
                g.AddEdge(weeks + i, weeks + i + 1, storageInfo.Quantity, storageInfo.Value);

            // sprzedaż do kontrahentów
            for (int j = 0; j < buyers; j++)
                g.AddEdge(weeks + i, 2 * weeks + j, sales[j, i].Quantity, -sales[j, i].Value);
        }

        // podłączenie kontrahentów (nie daliśmy od razu, żeby nie powstały krawędzie wielokrotne i żeby ogarnąć
        // do kogo ile idzie)
        for (int j = 0; j < buyers; j++)
            g.AddEdge(2 * weeks + j, sink, int.MaxValue, 0);

        (int _, double val, var flow) = Flows.MinCostMaxFlow(g, source, sink);

        int quantity = 0;
        weeklyPlan = new WeeklyPlan[weeks];
        for (int i = 0; i < weeks; i++)
        {
            weeklyPlan[i].UnitsProduced = flow.HasEdge(i, weeks + i) ? flow.GetEdgeWeight(i, weeks + i) : 0;
            quantity += weeklyPlan[i].UnitsProduced;

            weeklyPlan[i].UnitsSold = new int[buyers];
            for (int j = 0; j < buyers; j++)
                weeklyPlan[i].UnitsSold[j] = flow.HasEdge(weeks + i, 2 * weeks + j)
                    ? flow.GetEdgeWeight(weeks + i, 2 * weeks + j)
                    : 0;

            if (i + 1 < weeks)
                weeklyPlan[i].UnitsStored = flow.HasEdge(weeks + i, weeks + i + 1)
                    ? flow.GetEdgeWeight(weeks + i, weeks + i + 1)
                    : 0;
        }

        return new PlanData
        {
            Value = -val,
            Quantity = quantity
        };
    }
}