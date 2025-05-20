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


        [HttpGet("grouped")]
        public IActionResult GetGroupedDonations([FromQuery] string donorName)
        {
            if (string.IsNullOrWhiteSpace(donorName))
                return BadRequest("Missing donorName");

            var grouped = _donationLog
                .Where(d => d.DonorName.Equals(donorName, StringComparison.OrdinalIgnoreCase))
                .GroupBy(d => d.Timestamp.Date)
                .OrderByDescending(g => g.Key)
                .Select(g => new
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Donations = g.OrderByDescending(d => d.Timestamp)
                })
                .ToList();

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
