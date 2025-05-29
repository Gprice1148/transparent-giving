using Microsoft.AspNetCore.Mvc;
using TransparentGiving.Api.Models;
using TransparentGiving.Api.Services;

namespace TransparentGiving.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DonationsController : ControllerBase
    {
        private static List<DonationRecord> _donationLog = new();
        private readonly IChildService _childService;

        public DonationsController(IChildService childService)
        {
            _childService = childService;
        }

        [HttpPost]
        public IActionResult Donate([FromBody] DonationRequest request)
        {
            var children = _childService.GetAllChildren();
            var matches = AllocateDonation(request.DonationAmount, children, out decimal donationRemaining);

            var enrichedMatches = matches.Select(m =>
                {
                    var full = children.FirstOrDefault(c => c.Id == m.Id);
                    return new ChildMatch
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Allocated = m.Allocated,
                        Country = full?.Country ?? "",
                        SurgeryType = full?.SurgeryType ?? ""
                    };
                }).ToList();

            LogDonation(request.DonorName, request.DonationAmount, enrichedMatches);

            return Ok(new
            {
                donorName = request.DonorName,
                donationAmount = request.DonationAmount,
                matches = matches.Select(m =>
                {
                    var full = children.FirstOrDefault(c => c.Id == m.Id);
                    return new
                    {
                        m.Id,
                        m.Name,
                        m.Allocated,
                        Age = full?.Age,
                        Country = full?.Country,
                        SurgeryType = full?.SurgeryType,
                        CostTotal = full?.CostTotal,
                        CostFunded = full?.CostFunded,
                        ImageUrl = full?.ImageUrl
                    };
                }),
                surplus = donationRemaining
            });
        }

        [HttpGet]
        public IActionResult GetDonations([FromQuery] string donorName)
        {
            if (string.IsNullOrWhiteSpace(donorName))
                return BadRequest("Missing donorName");

            var donations = _donationLog
                .Where(d => d.DonorName.Equals(donorName, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(d => d.Timestamp)
                .ToList();

            return Ok(donations);
        }

        [Authorize]
        [HttpGet("grouped")]
        public IActionResult GetGroupedDonations([FromQuery] string donorName)
        {
            if (string.IsNullOrEmpty(donorName))
                return BadRequest("donorName is required.");

            // Filter donations made by the specified donor
            var filteredDonations = _donations
                .Where(d => d.DonorName.Equals(donorName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Group all donations by ChildId
            var grouped = _donations
                .GroupBy(d => d.ChildId)
                .Select(group =>
                {
                    var first = group.First();
                    var totalRaised = group.Sum(d => d.Amount);
                    var donatedByUser = filteredDonations
                        .Where(d => d.ChildId == group.Key)
                        .Sum(d => d.Amount);

                    var child = _children.FirstOrDefault(c => c.Id == group.Key);

                    return new GroupedDonationDto
                    {
                        ChildId = group.Key,
                        ChildName = child?.Name ?? "Unknown",
                        DonatedByUser = donatedByUser,
                        TotalRaised = totalRaised,
                        Goal = child?.GoalAmount ?? 0
                    };
                });

            return Ok(grouped);
        }

        private List<ChildMatch> AllocateDonation(decimal donationAmount, List<ChildProfile> children, out decimal remaining)
        {
            remaining = donationAmount;
            var matches = new List<ChildMatch>();

            foreach (var child in children.OrderBy(c => c.Priority))
            {
                var need = child.CostTotal - child.CostFunded;
                if (need <= 0 || remaining <= 0) continue;

                var allocated = Math.Min(need, remaining);
                child.CostFunded += allocated;
                remaining -= allocated;

                matches.Add(new ChildMatch
                {
                    Id = child.Id,
                    Name = child.Name,
                    Allocated = allocated
                });
            }

            return matches;
        }

        private void LogDonation(string donorName, decimal amount, List<ChildMatch> matches)
        {
            var record = new DonationRecord
            {
                DonorName = donorName,
                Amount = amount,
                Timestamp = DateTime.UtcNow,
                Matches = matches
            };

            _donationLog.Add(record);

            Console.WriteLine($"[LOG] Total donations: {_donationLog.Count}");
            foreach (var r in _donationLog)
            {
                Console.WriteLine($"  Donor: {r.DonorName}, Amount: {r.Amount}, Time: {r.Timestamp}");
                foreach (var m in r.Matches)
                {
                    Console.WriteLine($"    - Helped: {m.Name} with ${m.Allocated}");
                }
            }
        }
    }

    public class DonationRequest
    {
        public decimal DonationAmount { get; set; }
        public string DonorName { get; set; }
    }
}
