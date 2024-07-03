using ASD.Graphs;

namespace ASD;

public class Lab06 : MarshalByRefObject
{
    public List<int> WidePath(DiGraph<int> g, int start, int end)
    {
        int[] from = new int[g.VertexCount];
        int[] maxWidths = new int[g.VertexCount];

        from[end] = -1;

        var q = new PriorityQueue<int, (int, int)>();
        q.Insert((start, int.MaxValue), 0);

        while (q.Count > 0)
        {
            (int v, int width) = q.Extract();

            if (width < maxWidths[v])
                continue;

            foreach (var e in g.OutEdges(v))
            {
                int curr = Math.Min(width, e.Weight);
                if (curr > maxWidths[e.To])
                {
                    // PriorityQueue bierze minimalne, my chcemy maksymalne
                    q.Insert((e.To, curr), int.MaxValue - curr);
                    from[e.To] = v;
                    maxWidths[e.To] = curr;
                }
            }
        }

        if (from[end] < 0)
            return [];

        var list = new List<int> { end };
        int u = end;
        while (u != start)
        {
            u = from[u];
            list.Insert(0, u);
        }

        return list;
    }


    public List<int> WeightedWidePath(DiGraph<int> g, int start, int end, int[] weights, int maxWeight)
    {
        var set = new HashSet<int>();
        foreach (var e in g.DFS().SearchAll())
            set.Add(e.Weight);

        int res = int.MinValue;
        var list = new List<int>();

        foreach (int k in set)
        {
            int[] from = new int[g.VertexCount];
            int[] waitTimeSum = new int[g.VertexCount];
            int[] minPathWidth = new int[g.VertexCount];
            for (int i = 0; i < g.VertexCount; i++)
            {
                // waitTimes[i] = int.MaxValue + int.MinValue;
                // minWi[i] = int.MaxValue;
                waitTimeSum[i] = 0;
                minPathWidth[i] = int.MinValue;
            }

            waitTimeSum[start] = weights[start];
            from[end] = -1;

            var q = new PriorityQueue<int, (int, int, int)>();
            q.Insert((start, weights[start], int.MaxValue), 0);

            while (q.Count > 0)
            {
                (int v, int time, int width) = q.Extract();

                // if (width > minWi[v] || time > waitTimes[v])
                    // continue;

                foreach (var e in g.OutEdges(v))
                {
                    if (e.Weight > k)
                        continue;

                    int currWaitSum = time + weights[e.To];
                    int currMinWidth = Math.Min(e.Weight, width);

                    if (currMinWidth - currWaitSum > minPathWidth[e.To] - waitTimeSum[e.To])
                    {
                        q.Insert((e.To, currWaitSum, currMinWidth), currWaitSum - currMinWidth); // -(currWidth - curr) bo kolejka min
                        from[e.To] = v;
                        waitTimeSum[e.To] = currWaitSum;
                        minPathWidth[e.To] = currMinWidth;
                    }
                }
            }

            if (from[end] > -1 && minPathWidth[end] - waitTimeSum[end] > res)
            {
                res = minPathWidth[end] - waitTimeSum[end];

                list = new List<int> { end };
                int u = end;
                while (u != start)
                {
                    u = from[u];
                    list.Insert(0, u);
                }
            }
        }

        return list;
    }
}