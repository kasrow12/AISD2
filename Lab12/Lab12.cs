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
        public double[] PointDepths(Point[] points)
        {
            // Tym razem trochę może być trochę brzydszy kod, zabrakło czasu na upiększanie
            int left = 0;
            double[] water = new double[points.Length];

            while (true)
            {
                // Szukamy nie malejącego podciągu x'ów, aby wykorzystać rozwiązanie z części A, w której to było założeniem 
                int right = left;
                while (right + 1 < points.Length && points[right].x <= points[right + 1].x)
                    right++;

                if (left >= right)
                    break;

                CalculateWater(points, left, right, water);

                // Skipujemy ten podciąg oraz idziemy do następnego (o ile istnieje)
                left = right + 1;
                while (left + 1 < points.Length && points[left].x > points[left + 1].x)
                    left++;
            }

            return water;
        }


        public double CalculateWater(Point[] points, int left, int right, double[] water)
        {
            double area = 0;
            // Zamiatanie z obu stron, woda będzie sięgać mniejszego boku. Później przechodzimy do pozostałej części
            // i ponownie sprawdzamy podproblemy.
            while (true)
            {
                // Skip ścianek/krawędzi z boków
                while (left < right && points[left].y < points[left + 1].y) left++;
                while (left < right && points[right - 1].y > points[right].y) right--;

                if (left >= right) break;

                double min;
                // Woda "zatrzymuje się na lewej krawędzi"
                if (points[left].y < points[right].y)
                {
                    min = points[left++].y;
                    while (left < right && points[left].y < min)
                    {
                        water[left] = min - points[left].y;
                        // Pole wody liczymy jako sumę trapezów (bądź trójkątów, gdy któreś water[]=0) o wysokościach deltaX
                        area += (water[left - 1] + water[left]) * (points[left].x - points[left - 1].x) / 2;
                        left++;
                    }

                    // Minimum było z lewej, teraz zostaje nam punkt przecięcia z prawej (trójkąt)
                    var intersect = getPointAtY(points[left - 1], points[left], min);
                    area += water[left - 1] * (intersect.x - points[left - 1].x) / 2;
                }
                else // analogicznie, gdy minimum z prawej -> idziemy do lewej
                {
                    min = points[right--].y;
                    while (left < right && points[right].y < min)
                    {
                        water[right] = min - points[right].y;
                        area += (water[right + 1] + water[right]) * (points[right + 1].x - points[right].x) / 2;
                        right--;
                    }

                    var intersect = getPointAtY(points[right], points[right + 1], min);
                    area += water[right + 1] * (points[right + 1].x - intersect.x) / 2;
                }
            }

            return area;
        }

        /// <summary>
        /// Funkcja zwraca objętość wody, jaka zatrzyma się w górach.
        /// 
        /// Przyjmujemy, że pierwszy punkt z tablicy points jest lewym krańcem, a ostatni - prawym krańcem łańcucha górskiego.
        /// </summary>
        public double WaterVolume(Point[] points)
        {
            int left = 0;
            double[] water = new double[points.Length];
            double area = 0;

            // Analogicznie do tamtego etapu, szukamy nie malejących po x podciągów
            while (true)
            {
                int right = left;
                while (right + 1 < points.Length && points[right].x <= points[right + 1].x)
                    right++;

                if (left >= right)
                    break;

                area += CalculateWater(points, left, right, water);

                left = right + 1;
                while (left + 1 < points.Length && points[left].x > points[left + 1].x)
                    left++;
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