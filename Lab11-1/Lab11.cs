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

        (int, int) FindMinMaxX((double, double)[] poly)
        {
            int min = 0;
            int max = 0;
            for (int i = 1; i < poly.Length; i++)
            {
                if (poly[i].Item1 < poly[min].Item1)
                    min = i;

                if (poly[i].Item1 > poly[max].Item1)
                    max = i;
            }

            return (min, max);
        }

        (List<(double, double)>, List<(double, double)>) SplitConvexHull((double, double)[] poly, int min, int max)
        {
            var above = new List<(double, double)>();
            var below = new List<(double, double)>();
            
            int i = min;
            while (i != max)
            {
                below.Add(poly[i]);
                i = (i + 1) % poly.Length;
            }
            below.Add(poly[i]);

            while (i != min)
            {
                above.Add(poly[i]);
                i = (i + 1) % poly.Length;
            }
            above.Add(poly[i]);

            return (above, below);
        }

        List<(double, double)> MergeListInOrder(List<(double, double)> list1, List<(double, double)> list2, bool rev)
        {
            var list = new List<(double, double)>();

            int i = 0;
            int j = 0;
            while (i < list1.Count && j < list2.Count)
            {
                if ((!rev && list1[i].Item1 < list2[j].Item1) || (rev && list1[i].Item1 > list2[j].Item1))
                {
                    list.Add(list1[i++]);
                }
                else if ((!rev && list1[i].Item1 > list2[j].Item1) || (rev && list1[i].Item1 < list2[j].Item1))
                {
                    list.Add(list2[j++]);
                }
                else
                {
                    if (list1[i].Item2 > list2[j].Item2)
                    {
                        if (rev)
                            list.Add(list1[i++]);
                        else 
                            list.Add(list2[j++]);
                    }
                    else if (list1[i].Item2 < list2[j].Item2)
                    {
                        if (rev)
                            list.Add(list2[j++]);
                        else 
                            list.Add(list1[i++]);
                    }
                    else
                    {
                        i++;
                    }
                }
            }

            while (i < list1.Count)
                list.Add(list1[i++]);
            
            while (j < list2.Count)
                list.Add(list2[j++]);

            return list;
        }
        
        Stack<(double, double)> Grahamize(List<(double, double)> points)
        {
            var stack = new Stack<(double, double)>();
            stack.Push(points[0]);
            stack.Push(points[1]);
            
            for (int i = 2; i < points.Count; i++)
            {
                var p = stack.Pop();
                var next = points[i];
            
                while (stack.Count > 0 && Cross(stack.Peek(), p, next) <= 0)
                    p = stack.Pop();
                
                stack.Push(p);
                stack.Push(next);
            }

            return stack;
        }

        // Etap 2
        // oblicza otoczkę dwóch wielokątów wypukłych
        public (double, double)[] ConvexHullOfTwo((double, double)[] poly1, (double, double)[] poly2)
        {
            (int min1, int max1) = FindMinMaxX(poly1);
            (int min2, int max2) = FindMinMaxX(poly2);
            
            var (above1, below1) = SplitConvexHull(poly1, min1, max1);
            var (above2, below2) = SplitConvexHull(poly2, min2, max2);

            var above = MergeListInOrder(above1, above2, true);
            var below = MergeListInOrder(below1, below2, false);

            var hull = new List<(double, double)>();

            foreach (var p in Grahamize(below))
                hull.Add(p);
            
            var aboveCH = Grahamize(above).ToArray();
            // Skip zerowego i ostatniego elementu, bo duplikaty odpowiednio min i maxa
            for (int i = 1; i < aboveCH.Length - 1; i++)
                hull.Add(aboveCH[i]);

            var result = hull.ToArray();
            // Stack odwraca kolejność 
            Array.Reverse(result);
            
            return result;
        }
    }
}