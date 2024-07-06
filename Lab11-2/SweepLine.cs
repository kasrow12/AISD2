namespace ASD;

internal class SweepLine
{
    /// <summary>
    ///     Funkcja obliczająca długość teoriomnogościowej sumy pionowych odcinków
    /// </summary>
    /// <returns>Długość teoriomnogościowej sumy pionowych odcinków</returns>
    /// <param name="segments">Tablica z odcinkami, których teoriomnogościowej sumy długość należy policzyć</param>
    /// Każdy odcinek opisany jest przez dwa punkty: początkowy i końcowy
    public double VerticalSegmentsUnionLength(Geometry.Segment[] segments)
    {
        // kolejka min (od dołu do góry)
        var pq = new PriorityQueue<SweepEvent, double>();
        foreach (var segment in segments)
        {
            pq.Enqueue(new SweepEvent(segment.ps.y, segment.ps.y < segment.pe.y), segment.ps.y);
            pq.Enqueue(new SweepEvent(segment.pe.y, segment.ps.y >= segment.pe.y), segment.pe.y);
        }

        double result = 0;
        int count = 0;
        double startingY = 0;
        while (pq.Count > 0)
        {
            var e = pq.Dequeue();
            if (count == 0)
                startingY = e.Coord;

            if (e.IsStartingPoint)
                count++;
            else
                count--;

            if (count == 0)
                result += e.Coord - startingY;
        }

        return result;
    }

    /// <summary>
    ///     Funkcja obliczająca pole teoriomnogościowej sumy prostokątów
    /// </summary>
    /// <returns>Pole teoriomnogościowej sumy prostokątów</returns>
    /// <param name="rectangles">Tablica z prostokątami, których teoriomnogościowej sumy pole należy policzyć</param>
    /// Każdy prostokąt opisany jest przez cztery wartości: minimalna współrzędna X, minimalna współrzędna Y, 
    /// maksymalna współrzędna X, maksymalna współrzędna Y.
    public double RectanglesUnionArea(Geometry.Rectangle[] rectangles)
    {
        // kolejka min (od lewej do prawej)
        var pq = new PriorityQueue<RectEvent, double>();
        foreach (var rect in rectangles)
        {
            var start = new Geometry.Segment(new Geometry.Point(rect.MinX, rect.MinY),
                new Geometry.Point(rect.MinX, rect.MaxY));
            var end = new Geometry.Segment(new Geometry.Point(rect.MaxX, rect.MinY),
                new Geometry.Point(rect.MaxX, rect.MaxY));

            // jako 4-ty parametr podaję start, żeby móc później usunąć go z listy w evencie dla end
            // (bo startowy segment to inny segment niż końcowy)
            // dla startu bez znaczenia
            pq.Enqueue(new RectEvent(rect.MinX, start, true, start), rect.MinX);
            pq.Enqueue(new RectEvent(rect.MaxX, end, false, start), rect.MaxX);
        }

        var list = new List<Geometry.Segment>();
        double result = 0;
        double lastX = double.MinValue;
        double lastD = 0;

        while (pq.Count > 0)
        {
            var e = pq.Dequeue();

            if (e.IsStartingPoint)
                list.Add(e.Segment);
            else
                list.Remove(e.Start);

            double d = VerticalSegmentsUnionLength(list.ToArray());

            if (lastX < e.Coord)
                result += lastD * (e.Coord - lastX);

            lastD = d;
            lastX = e.Coord;
        }

        return result;
    }

    /// <summary>
    ///     Struktura pomocnicza opisująca zdarzenie
    /// </summary>
    /// <remarks>
    ///     Można jej użyć, przerobić, albo w ogóle nie używać i zrobić po swojemu
    /// </remarks>
    private struct SweepEvent(double c, bool sp)
    {
        /// <summary>
        ///     Współrzędna zdarzenia
        /// </summary>
        public readonly double Coord = c;

        /// <summary>
        ///     Czy zdarzenie oznacza początek odcinka/prostokąta
        /// </summary>
        public readonly bool IsStartingPoint = sp;
    }

    private struct RectEvent(double c, Geometry.Segment s, bool sp, Geometry.Segment start)
    {
        public readonly double Coord = c;
        public readonly Geometry.Segment Start = start;
        public readonly Geometry.Segment Segment = s;
        public readonly bool IsStartingPoint = sp;
    }
}