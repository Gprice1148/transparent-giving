namespace TransparentGiving.Api.Models
{
    public class GroupedDonationDto
    {
        public string ChildId { get; set; } = string.Empty;
        public string ChildName { get; set; } = string.Empty;
        public decimal DonatedByUser { get; set; }
        public decimal TotalRaised { get; set; }
        public decimal Goal { get; set; }
    }
}
