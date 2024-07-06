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
            int[] diffs = new int[g.VertexCount];
            for (int i = 0; i < g.VertexCount; i++)
                diffs[i] = int.MinValue;

            from[end] = -1;
            diffs[start] = weights[start];

            var q = new PriorityQueue<int, (int, int, int)>();
            q.Insert((start, int.MaxValue, weights[start]), 0);

            while (q.Count > 0)
            {
                (int v, int width, int time) = q.Extract();

                if (width - time < diffs[v])
                    continue;

                foreach (var e in g.OutEdges(v))
                {
                    if (e.Weight > k)
                        continue;

                    int wait = time + weights[e.To];
                    int minWidth = Math.Min(e.Weight, width);
                    int diff = minWidth - wait;

                    if (diff > diffs[e.To])
                    {
                        q.Insert((e.To, minWidth, wait), -diff); // - bo kolejka min
                        from[e.To] = v;
                        diffs[e.To] = diff;
                    }
                }
            }

            if (from[end] > -1 && diffs[end] > res)
            {
                res = diffs[end];
                list = [end];
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