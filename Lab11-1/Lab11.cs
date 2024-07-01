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

        List<(double, double)> MergeListInOrder(List<(double, double)> above1, List<(double, double)> above2, bool rev)
        {
            var above = new List<(double, double)>();

            int i = 0;
            int j = 0;
            while (i < above1.Count && j < above2.Count)
            {
                if ((!rev && above1[i].Item1 < above2[j].Item1) || (rev && above1[i].Item1 > above2[j].Item1))
                {
                    above.Add(above1[i]);
                    i++;
                }
                else if ((!rev && above1[i].Item1 > above2[j].Item1) || (rev && above1[i].Item1 < above2[j].Item1))
                {
                    // above.Add(above1[i]);
                    // i++;
                    
                    // if (above1[i].Item2 != above2[j].Item2)
                        above.Add(above2[j]);
                    j++;   
                }
                else
                {
                    if (above1[i].Item2 > above2[j].Item2)
                    {
                        above.Add(above2[j++]);
                    }
                    else if (above1[i].Item2 < above2[j].Item2)
                    {
                        above.Add(above1[i++]);
                    }
                    else
                    {
                        j++;
                    }
                    
                }
                // else
                // {
                //     if (above1[i].Item2 == above2[j].Item2)
                //     {
                //         above.Add(above1[i]);
                //     }
                //     else if (above1[i].Item2 < above2[j].Item2)
                //     {
                //         above.Add(above2[j]);
                //     }
                //     else
                //     {
                //         above.Add(above1[i]);
                //     }
                //     i++;
                //     j++;
                // }
                // else// if (above1[i].Item1 > above2[j].Item1)
                // {
                //     above.Add(above2[j]);
                //     j++;   
                // }
            }

            while (i < above1.Count)
            {
                above.Add(above1[i]);
                i++;
            }
            while (j < above2.Count)
            {
                above.Add(above2[j]);
                j++;
            }

            return above;
        }

        // Etap 2
        // oblicza otoczkę dwóch wielokątów wypukłych
        public (double, double)[] ConvexHullOfTwo((double, double)[] poly1, (double, double)[] poly2)
        {
            (int min1, int max1) = FindMinMaxX(poly1);
            (int min2, int max2) = FindMinMaxX(poly2);
            
            // Console.WriteLine($"{min1} {max1} {min2} {max2}");

            var (above1, below1) = SplitConvexHull(poly1, min1, max1);
            var (above2, below2) = SplitConvexHull(poly2, min2, max2);

            // if (above1.Count + below1.Count != poly1.Length
            //     || above2.Count + below2.Count != poly2.Length)
            //     throw new Exception();

            var above = MergeListInOrder(above1, above2, true);
            var below = MergeListInOrder(below1, below2, false);

            // var aboveCH = ConvexHull(above.ToArray());
            // var belowCH = ConvexHull(below.ToArray());
            
            
            var stack = new Stack<(double, double)>();
            stack.Push(above[0]);
            stack.Push(above[1]);
            
            for (int i = 2; i < above.Count; i++)
            {
                var p = stack.Pop();
                var next = above[i];
            
                while (stack.Count > 0 && Cross(stack.Peek(), p, next) <= 0)
                {
                    p = stack.Pop();
                }
                
                stack.Push(p);
                stack.Push(next);
            }
            var aboveCH = stack.ToArray();
            // Console.WriteLine(String.Join('|', stack.ToArray()));
            
            stack = new Stack<(double, double)>();
            stack.Push(below[0]);
            stack.Push(below[1]);
            
            for (int i = 2; i < below.Count; i++)
            {
                var p = stack.Pop();
                var next = below[i];
            
                while (stack.Count > 0 && Cross(stack.Peek(), p, next) <= 0)
                {
                    p = stack.Pop();
                }
                
                stack.Push(p);
                stack.Push(next);
            }

            var belowCH = stack.ToArray();
            // Console.WriteLine(String.Join('|', stack.ToArray()));

            var res = new (double, double)[aboveCH.Length + belowCH.Length];
            belowCH.CopyTo(res, 0);
            aboveCH.CopyTo(res, belowCH.Length);

            // var xd = new (double, double)[poly1.Length + poly2.Length];
            // poly1.CopyTo(xd, 0);
            // poly2.CopyTo(xd, poly1.Length);
            // var xd2 = ConvexHull(res);
            var list = new List<(double, double)>();
            foreach (var p in belowCH)
                list.Insert(0,p);
            
            if (belowCH[^1] != aboveCH[0])
                list.Insert(0,aboveCH[0]);

            for (int i = 1; i < aboveCH.Length - 1; i++)
                list.Insert(0,aboveCH[i]);

            // Console.WriteLine(String.Join('|', res.ToArray()));
            Console.WriteLine(String.Join('|', list));
            // Console.WriteLine(String.Join('|', xd2));
            return list.ToArray();

            return res;
            // i = 1;
            // while (i < above.Count)
            // {
            //     if (above[i - 1].Item1 < above[i].Item1)
            //         throw new Exception();
            //     
            //     i++;
            // }
            
            
            
            return null;
        }

    }
}