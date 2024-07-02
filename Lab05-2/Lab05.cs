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
        
        // Console.WriteLine(String.Join(',', set));

        int res = int.MaxValue;
        var list = new List<int>();

        foreach (int k in set)
        {
            int[] from = new int[g.VertexCount];
            int[] waitTimes = new int[g.VertexCount];
            for (int i = 0; i < g.VertexCount; i++)
            {
                waitTimes[i] = int.MaxValue;
            }

            from[end] = -1;

            var q = new PriorityQueue<int, (int, int)>();
            q.Insert((start, weights[start]), weights[start]);

            while (q.Count > 0)
            {
                (int v, int time) = q.Extract();

                if (time > waitTimes[v])
                    continue;

                foreach (var e in g.OutEdges(v))
                {
                    if (e.Weight > k)
                        continue;
                    
                    int curr = time + weights[e.To];
                    if (curr < waitTimes[e.To])
                    {
                        q.Insert((e.To, curr), curr);
                        from[e.To] = v;
                        waitTimes[e.To] = curr;
                    }
                }
            }

            if (from[end] > -1 && waitTimes[end] - k < res)
            {
                res = waitTimes[end] - k;
                
                list = new List<int> { end };
                int u = end;
                while (u != start)
                {
                    u = from[u];
                    list.Insert(0, u);
                }
            }

        }
        // Console.WriteLine(String.Join(',', list));

        return list;
    }
}