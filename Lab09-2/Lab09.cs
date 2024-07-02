using ASD.Graphs;

namespace ASD;

public class Lab08 : MarshalByRefObject
{
    /// <summary>
    ///     Znajduje cykl rozpoczynający się w stolicy, który dla wybranych miast,
    ///     przez które przechodzi ma największą sumę liczby ludności w tych wybranych
    ///     miastach oraz minimalny koszt.
    /// </summary>
    /// <param name="cities">
    ///     Graf miast i połączeń między nimi.
    ///     Waga krawędzi jest kosztem przejechania między dwoma miastami.
    ///     Koszty transportu między miastami są nieujemne.
    /// </param>
    /// <param name="citiesPopulation">Liczba ludności miast</param>
    /// <param name="meetingCosts">
    ///     Koszt spotkania w każdym z miast.
    ///     Dla części pierwszej koszt spotkania dla każdego miasta wynosi 0.
    ///     Dla części drugiej koszty są nieujemne.
    /// </param>
    /// <param name="budget">Budżet do wykorzystania przez kandydata.</param>
    /// <param name="capitalCity">Numer miasta będącego stolicą, z której startuje kandydat.</param>
    /// <param name="path">
    ///     Tablica dwuelementowych krotek opisująca ciąg miast, które powinen odwiedzić kandydat.
    ///     Pierwszy element krotki to numer miasta do odwiedzenia, a drugi element decyduje czy
    ///     w danym mieście będzie organizowane spotkanie wyborcze.
    ///     Pierwszym miastem na tej liście zawsze będzie stolica (w której można, ale nie trzeba
    ///     organizować spotkania).
    ///     Zakładamy, że po odwiedzeniu ostatniego miasta na liście kandydat wraca do stolicy
    ///     (na co musi mu starczyć budżetu i połączenie między tymi miastami musi istnieć).
    ///     Jeżeli kandydat nie wyjeżdża ze stolicy (stolica jest jedynym miastem, które odwiedzi),
    ///     to lista `path` powinna zawierać jedynie jeden element: stolicę (wraz z informacją
    ///     czy będzie tam spotkanie czy nie). Nie są wtedy ponoszone żadne koszty podróży.
    ///     W pierwszym etapie drugi element krotki powinien być zawsze równy `true`.
    /// </param>
    /// <returns>
    ///     Liczba mieszkańców, z którymi spotka się kandydat.
    /// </returns>
    public int ComputeElectionCampaignPath(Graph<int> cities, int[] citiesPopulation,
        double[] meetingCosts, double budget, int capitalCity, out (int, bool)[] path)
    {
        int n = cities.VertexCount;
        List<(int, bool)> S = [];
        List<(int, bool)> bestS = [(capitalCity, false)]; // baza
        int curSum = 0;
        int bestSum = 0;
        double curCost = 0;
        double bestCost = 0;

        bool[] used = new bool[n];
        bool afterCapital = false;

        void MaxCampaign(int last, bool organize)
        {
            // Nie dodawaj miasta startowego drugi raz na końcu
            if (!used[last])
            {
                S.Add((last, organize));
                if (organize)
                {
                    curSum += citiesPopulation[last];
                    curCost += meetingCosts[last];
                }

                used[last] = true;
            }

            if (capitalCity == last)
            {
                if (curSum > bestSum
                    || (curSum == bestSum && curCost < bestCost))
                {
                    bestS = S[..];
                    bestCost = curCost;
                    bestSum = curSum;
                }

                if (afterCapital)
                    return;

                afterCapital = true;
            }

            foreach (var e in cities.OutEdges(last))
            {
                // Do stolicy musimy wejść drugi raz
                if ((used[e.To] && e.To != capitalCity) || curCost + e.Weight > budget)
                    continue;

                curCost += e.Weight;

                if (curCost + meetingCosts[e.To] <= budget)
                    MaxCampaign(e.To, true);

                if (meetingCosts[e.To] > 0)
                    MaxCampaign(e.To, false);

                curCost -= e.Weight;
            }

            if (organize && used[last])
            {
                curSum -= citiesPopulation[last];
                curCost -= meetingCosts[last];
            }

            used[last] = false;
            S.RemoveAt(S.Count - 1); // remove last
        }

        // Organizuj w stolicy
        if (meetingCosts[capitalCity] <= budget)
            MaxCampaign(capitalCity, true);

        // Nie organizuj w stolicy, jeśli koszt jest niezerowy (wpp. zorganizowano wyżej)
        if (meetingCosts[capitalCity] > 0)
        {
            afterCapital = false;
            // used, S, curSum i curCost raczej się resetują
            MaxCampaign(capitalCity, false);
        }

        path = bestS.ToArray();
        return bestSum;
    }
}