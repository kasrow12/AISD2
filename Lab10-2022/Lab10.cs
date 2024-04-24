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
        // int curCost = 0;
        List<int> bestS = [];
        // int bestSum = int.MinValue;
        // int bestCost = int.MaxValue;

        bool[] delivered = new bool[n];
        int ndelivered = 0;

        bool FindPath(int last)
        {
            if (ndelivered == n - 1 && last == 0)
            {
                // if (curCost < bestCost)
                // {
                    bestS = S[..];
                    // bestCost = curCost;
                    // bestSum = curSum;
                // }
                // else
                // {
                    return true;
                // }
            }

            foreach (var e in railway.OutEdges(last))
            {
                if (delivered[e.To] || curSteam - e.Weight < 0 || curEggs < eggDemand[e.To])
                    continue;

                S.Add(e.To);
                if (e.To != 0)
                {
                    delivered[e.To] = true;
                    ndelivered++;
                }

                int lastSteam = curSteam;
                int lastEggs = curEggs;
                
                curSteam -= e.Weight;
                if (isRefuelStation[e.To])
                    curSteam = tankEngineRange;
                
                curEggs -= eggDemand[last];
                if (e.To == 0)
                    curEggs = truckCapacity;

                if (FindPath(e.To))
                    return true;

                if (e.To != 0)
                {
                    delivered[e.To] = false;
                    ndelivered--;
                }

                curSteam = lastSteam;
                curEggs = lastEggs;
                
                S.RemoveAt(S.Count - 1); // remove last                
            }

            return false;
        }

        bool ret = FindPath(0);
        
        // Console.WriteLine(String.Join(',', bestS));
        return (ret, bestS.ToArray());
    }
}