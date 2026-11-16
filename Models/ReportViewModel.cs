namespace CarRentalApp.Models
{
    public class ReportViewModel
    {
        public string CategoryName { get; set; } = string.Empty;
        public int ReservationCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}