namespace MiniTruckDelivery.Models
{
    public class ReportViewModel
    {
        public DateTime Date { get; set; }

        public List<Trip> Trips { get; set; } = new();


        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }

        public decimal TotalRevenue =>
            Trips.Sum(trip => trip.AgreedAmount);

        public decimal TotalExpenses =>
            Trips.Sum(trip => trip.Expenses.Sum(expense => expense.Amount));

        public decimal NetIncome =>
            TotalRevenue - TotalExpenses;
    }
}
