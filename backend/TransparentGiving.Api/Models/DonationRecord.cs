namespace TransparentGiving.Api.Models
{
    public class DonationRecord
    {
        public string DonorName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
        public List<ChildMatch> Matches { get; set; } = new();
    }

    public class ChildMatch
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Allocated { get; set; }
        public string Country { get; set; } = string.Empty;
        public string SurgeryType { get; set; } = string.Empty;
    }
}
