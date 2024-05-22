using System;
using System.Collections.Generic;

namespace ASD
{
    public class WaterCalculator : MarshalByRefObject
    {

        /*
         * Metoda sprawdza, czy przechodząc p1->p2->p3 skręcamy w lewo 
         * (jeżeli idziemy prosto, zwracany jest fałsz).
         */
        private bool leftTurn(Point p1, Point p2, Point p3)
        {
            Point w1 = new Point(p2.x - p1.x, p2.y - p1.y);
            Point w2 = new Point(p3.x - p2.x, p3.y - p2.y);
            double vectProduct = w1.x * w2.y - w2.x * w1.y;
            return vectProduct > 0;
        }


        /*
         * Metoda wyznacza punkt na odcinku p1-p2 o zadanej współrzędnej y.
         * Jeżeli taki punkt nie istnieje (bo cały odcinek jest wyżej lub niżej), zgłaszany jest wyjątek ArgumentException.
         */
        private Point getPointAtY(Point p1, Point p2, double y)
        {
            if (p1.y != p2.y)
            {
                double newX = p1.x + (p2.x - p1.x) * (y - p1.y) / (p2.y - p1.y);
                if ((newX - p1.x) * (newX - p2.x) > 0)
                    throw new ArgumentException("Odcinek p1-p2 nie zawiera punktu o zadanej współrzędnej y!");
                return new Point(p1.x + (p2.x - p1.x) * (y - p1.y) / (p2.y - p1.y), y);
            }
            else
            {
                if (p1.y != y)
                    throw new ArgumentException("Odcinek p1-p2 nie zawiera punktu o zadanej współrzędnej y!");
                return new Point((p1.x + p2.x) / 2, y);
            }
        }


        /// <summary>
        /// Funkcja zwraca tablice t taką, że t[i] jest głębokością, na jakiej znajduje się punkt points[i].
        /// 
        /// Przyjmujemy, że pierwszy punkt z tablicy points jest lewym krańcem, a ostatni - prawym krańcem łańcucha górskiego.
        /// </summary>
        ///
        public double[] PointDepths(Point[] points)
        {
            int left = 0;
            int right = points.Length - 1;
            double[] water = new double[points.Length];
            
            while (true)
            {
                while (left < right && points[left].y < points[left + 1].y) left++;
                while (left < right && points[right - 1].y > points[right].y) right--;
                
                if (left >= right) break;

                double min;
                if (points[left].y < points[right].y)
                {
                    min = points[left++].y;
                    while (left < right && points[left].y < min)
                    {
                        water[left] = min - points[left].y;
                        left++;
                    }
                }
                else
                {
                    min = points[right--].y;
                    while (left < right && points[right].y < min)
                    {
                        water[right] = min - points[right].y;
                        right--;
                    }
                }
            }
            
            return water;
        }

        /// <summary>
        /// Funkcja zwraca objętość wody, jaka zatrzyma się w górach.
        /// 
        /// Przyjmujemy, że pierwszy punkt z tablicy points jest lewym krańcem, a ostatni - prawym krańcem łańcucha górskiego.
        /// </summary>
        public double WaterVolume(Point[] points)
        {
            int left = 0;
            int right = points.Length - 1;
            double[] water = new double[points.Length];

            double area = 0;
            while (true)
            {
                while (left < right && points[left].y < points[left + 1].y) left++;
                while (left < right && points[right - 1].y > points[right].y) right--;

                if (left >= right) break;

                double min;
                if (points[left].y < points[right].y)
                {
                    min = points[left++].y;
                    while (left < right && points[left].y < min)
                    {
                        water[left] = min - points[left].y;
                        area += (water[left - 1] + water[left]) * (points[left].x - points[left - 1].x) / 2;
                        left++;
                    }

                    var p = getPointAtY(points[left - 1], points[left], min);
                    area += water[left - 1] * (p.x - points[left - 1].x) / 2;
                }
                else
                {
                    min = points[right--].y;
                    while (left < right && points[right].y < min)
                    {
                        water[right] = min - points[right].y;
                        area += (water[right + 1] + water[right]) * (points[right + 1].x - points[right].x) / 2;
                        right--;
                    }

                    var p = getPointAtY(points[right], points[right + 1], min);
                    area += water[right + 1] * (points[right + 1].x - p.x) / 2;
                }
            }


            return area;
        }
    }

    [Serializable]
    public struct Point
    {
        public double x, y;
        public Point(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }
}