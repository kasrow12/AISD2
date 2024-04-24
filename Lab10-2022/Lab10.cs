using ASD.Graphs;

namespace Lab10;

public class DeliveryPlanner : MarshalByRefObject
{
    /// <param name="railway">Graf reprezentujący sieć kolejową</param>
    /// <param name="eggDemand">Zapotrzebowanie na jajka na poszczególnyhc stacjach. Zerowy element tej tablicy zawsze jest 0</param>
    /// <param name="truckCapacity">Pojemność wagonu na jajka</param>
    /// <param name="tankEngineRange">Zasięg parowozu</param>
    /// <param name="isRefuelStation">na danym indeksie true, jeśli na danej stacji można uzupelnić węgiel i wodę</param>
    /// <param name="anySolution">Czy znaleźć jakiekolwiek rozwiązanie (true, etap 1), czy najkrótsze (false, etap 2)</param>
    /// <returns>
    ///     Informację czy istnieje trasa oraz tablicę reprezentującą kolejne wierzchołki w trasie (pierwszy i ostatni
    ///     element tablicy musi być 0). W przypadku, gdy zwracany jest false, wartość tego pola nie jest sprawdzana, może być
    ///     null.
    /// </returns>
    public (bool routeExists, int[] route) PlanDelivery(Graph<int> railway, int[] eggDemand, int truckCapacity,
        int tankEngineRange, bool[] isRefuelStation, bool anySolution)
    {
        int n = railway.VertexCount;
        List<int> S = [0];
        int curEggs = truckCapacity;
        int curSteam = tankEngineRange;
        int curTime = 0;
        List<int> bestS = [];
        int bestTime = int.MaxValue;

        bool possible = false;

        bool[] delivered = new bool[n];
        int ndelivered = 0;

        void FindPath(int last)
        {
            if (last == 0 && ndelivered == n - 1)
            {
                if (curTime < bestTime)
                {
                    bestS = S[..];
                    bestTime = curTime;
                }

                possible = true;
                return;
            }

            foreach (var e in railway.OutEdges(last))
            {
                if (delivered[e.To] || curTime + e.Weight > bestTime // czy mieścimy się w lepszym czasie
                                    || curSteam < e.Weight // czy starczy zasięgu parowozu
                                    || curEggs < eggDemand[e.To]) // każda stacja tylko raz -> trzeba rozwieźć od razu
                    continue;

                S.Add(e.To);
                if (e.To != 0)
                {
                    delivered[e.To] = true;
                    ndelivered++;
                }

                curTime += e.Weight;

                int lastSteam = curSteam;
                if (isRefuelStation[e.To])
                    curSteam = tankEngineRange;
                else
                    curSteam -= e.Weight;

                int lastEggs = curEggs;
                if (e.To == 0)
                    curEggs = truckCapacity;
                else
                    curEggs -= eggDemand[e.To];

                FindPath(e.To);
                if (possible && anySolution)
                    return;

                if (e.To != 0)
                {
                    delivered[e.To] = false;
                    ndelivered--;
                }

                curTime -= e.Weight;
                curSteam = lastSteam;
                curEggs = lastEggs;
                S.RemoveAt(S.Count - 1);
            }
        }

        FindPath(0);

        return (possible, bestS.ToArray());
    }
}