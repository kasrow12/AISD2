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
        return new List<int>();
    }
}