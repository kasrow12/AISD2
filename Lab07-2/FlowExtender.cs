using ASD.Graphs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASD
{
    public static class FlowExtender
    {

        /// <summary>
        /// Metod wylicza minimalny s-t-przekrój.
        /// </summary>
        /// <param name="undirectedGraph">Nieskierowany graf</param>
        /// <param name="s">wierzchołek źródłowy</param>
        /// <param name="t">wierzchołek docelowy</param>
        /// <param name="minCut">minimalny przekrój</param>
        /// <returns>wartość przekroju</returns>
        public static double MinCut(this Graph<double> undirectedGraph, int s, int t, out Edge<double>[] minCut)
        {
            (double value, var flow) = Flows.FordFulkerson(undirectedGraph, s, t);

            var residual = new DiGraph<double>(undirectedGraph.VertexCount);
            foreach (var e in undirectedGraph.DFS().SearchAll())
            {
                double val = flow.HasEdge(e.From, e.To) ? flow.GetEdgeWeight(e.From, e.To) : 0;
                double rval = flow.HasEdge(e.To, e.From) ? flow.GetEdgeWeight(e.To, e.From) : 0;

                double c = e.Weight - val + rval;
                if (c > 0)
                    residual.AddEdge(e.From, e.To, c);

                if (val > 0)
                    residual.AddEdge(e.To, e.From, val);
            }

            bool[] marked = new bool[undirectedGraph.VertexCount];
            marked[s] = true;
            foreach (var e in residual.DFS().SearchFrom(s))
                marked[e.To] = true;

            var minCutList = new List<Edge<double>>();
            foreach (var e in undirectedGraph.DFS().SearchAll())
                if (marked[e.From] && !marked[e.To])
                    minCutList.Add(e);
            
            minCut = minCutList.ToArray();
            return value;
        }

        /// <summary>
        /// Metada liczy spójność krawędziową grafu oraz minimalny zbiór rozcinający.
        /// </summary>
        /// <param name="undirectedGraph">nieskierowany graf</param>
        /// <param name="cutingSet">zbiór krawędzi rozcinających</param>
        /// <returns>spójność krawędziowa</returns>
        public static int EdgeConnectivity(this Graph<double> undirectedGraph, out Edge<double>[] cutingSet)
        {
            double min = double.MaxValue;
            cutingSet = null;

            for (int i = 1; i < undirectedGraph.VertexCount; i++)
            {
                double val = undirectedGraph.MinCut(0, i, out var cut);
                if (val < min)
                {
                    min = val;
                    cutingSet = cut;
                }
            }
            
            return (int)min;
        }
        
    }
}