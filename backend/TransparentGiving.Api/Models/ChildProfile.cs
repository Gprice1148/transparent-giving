namespace TransparentGiving.Api.Models
{
    public class ChildProfile
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Country { get; set; } = string.Empty;
        public string SurgeryType { get; set; } = string.Empty;
        public decimal CostTotal { get; set; }
        public decimal CostFunded { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int Priority { get; set; }  // Lower = higher priority
    }
}