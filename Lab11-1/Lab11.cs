using System;
using System.Collections.Generic;
using System.Linq;

namespace ASD
{
    public class Lab11 : System.MarshalByRefObject
    {

        // iloczyn wektorowy
        private int Cross((double, double) o, (double, double) a, (double, double) b)
        {
            double value = (a.Item1 - o.Item1) * (b.Item2 - o.Item2) - (a.Item2 - o.Item2) * (b.Item1 - o.Item1);
            return Math.Abs(value) < 1e-10 ? 0 : value < 0 ? -1 : 1;
        }
        

        // Etap 1
        // po prostu otoczka wypukła
        public (double, double)[] ConvexHull((double, double)[] points)
        {
            if (points.Length < 3)
                return points;

            (double, double) minY = points[0];
            foreach (var point in points)
            {
                if (point.Item2 < minY.Item2
                    || point.Item2 == minY.Item2 && point.Item1 > minY.Item1)
                {
                    minY = point;
                }
            }
            
            Array.Sort(points, (pA, pB) =>
            {
                if (pA == minY)
                    return 1;
                if (pB == minY)
                    return -1;

                int ret = Cross(minY, pA, pB);
                if (ret == 0)
                {
                    if (pA.Item1 == pB.Item1)
                        return pA.Item2 < pB.Item2 ? 1 : -1;
                    return pB.Item1 < pA.Item1 ? 1 : -1;
                }
                return ret;
            } );
            
            // Console.WriteLine($"MinY: {minY}");
            // foreach (var p in points)
            // {
            //     Console.WriteLine($"{p} [{Math.Atan2(p.Item2 - minY.Item2, p.Item1 - minY.Item1)}]");
            // }

            var stack = new Stack<(double, double)>();
            stack.Push(points[0]); // minY
            stack.Push(points[1]); // guess

            for (int i = 2; i < points.Length; i++)
            {
                var p = stack.Pop();
                var next = points[i];

                while (stack.Count > 0 && Cross(stack.Peek(), p, next) >= 0)
                {
                    p = stack.Pop();
                }
                
                stack.Push(p);
                stack.Push(next);
            }
            
            // Console.WriteLine($"x {stack.Peek()}");
            // var top = stack.Pop();
            // if (Cross(stack.Peek(), top, minY) > 0)
            // {
            //     stack.Push(top);
            // } co jeśli są współliniowe na koniec
            
            // Console.WriteLine(String.Join(',', stack.ToArray()));
            return stack.ToArray();
        }

        // Etap 2
        // oblicza otoczkę dwóch wielokątów wypukłych
        public (double, double)[] ConvexHullOfTwo((double, double)[] poly1, (double, double)[] poly2)
        {
            return null;
        }

    }
}