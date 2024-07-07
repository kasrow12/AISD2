using System;
using System.Text;

namespace ASD
{
    /// <summary>
    /// Wspólna struktura dla danych wejściowych związanych z produkcją, magazynowaniem i sprzedażą oraz danych wyjściowych
    /// (wynikowej produkcji i wyznaczonego zysku).
    /// </summary>
    [Serializable]
    public struct PlanData
    {
        /// <summary>
        /// Liczba telewizorów.
        /// </summary>
        /// <remarks>
        /// Dla danych wejściowych, wartość ta reprezentuje limity (produkcji, magazynowania lub sprzedaży).
        /// W obiektach zwracanych przez metody <see cref="ProductionPlanner.CreateSimplePlan"/> i <see cref="ProductionPlanner.CreateComplexPlan"/>
        /// w tym polu powinna znaleźć się całkowita łączna liczba wyprodukowanych telewizorów.
        /// </remarks>
        public int Quantity;

        /// <summary>
        /// Wartość telewizorów.
        /// </summary>
        /// <remarks>
        /// Dla danych wejściowych, wartość ta reprezentuje odpowiednio: koszt produkcji, koszt magazynowania i cenę sprzedaży jednego telewizora.
        /// W obiektach zwracanych przez metody <see cref="ProductionPlanner.CreateSimplePlan"/> i <see cref="ProductionPlanner.CreateComplexPlan"/>
        /// w tym polu powinien znaleźć się łączny zysk fabryki (różnica przychodów ze sprzedaży i kosztów produkcji i magazynowania).
        /// </remarks>
        public double Value;
    }

    /// <summary>
    /// Tygodniowy plan produkcji dla pojedynczego kontrahenta.
    /// </summary>
    [Serializable]
    public struct SimpleWeeklyPlan
    {
        /// <summary>
        /// Liczba wyprodukowanych sztuk w danym tygodniu.
        /// </summary>
        public int UnitsProduced;

        /// <summary>
        /// Liczba sprzedanych sztuk w danym tygodniu.
        /// </summary>
        public int UnitsSold;

        /// <summary>
        /// Liczba sztuk przechowanych w magazynie na następny tydzień.
        /// </summary>
        public int UnitsStored;

        public override string ToString()
        {
            return $"Produced: {UnitsProduced,4}, sold: {UnitsSold,4}, stored: {UnitsStored,4}";
        }
    }

    /// <summary>
    /// Tygodniowy plan produkcji dla wielu kontrahentów.
    /// </summary>
    [Serializable]
    public struct WeeklyPlan
    {
        /// <summary>
        /// Liczba wyprodukowanych sztuk w danym tygodniu.
        /// </summary>
        public int UnitsProduced;

        /// <summary>
        /// Liczba sztuk sprzedanych w danym tygodniu poszczególnym kontrahentom w postaci tablicy.
        /// </summary>
        public int[] UnitsSold;

        /// <summary>
        /// Liczba sztuk przechowanych w magazynie na następny tydzień.
        /// </summary>
        public int UnitsStored;

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Produced: {UnitsProduced,4}, stored: {UnitsStored,4}");
            stringBuilder.AppendLine("\tSales plan:");
            for (int i = 0; i < UnitsSold.Length; ++i)
            {
                stringBuilder.AppendLine($"\t\tCustomer {i}: sold {UnitsSold[i]}");
            }
            return stringBuilder.ToString();
        }
    }
}
