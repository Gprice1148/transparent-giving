using TransparentGiving.Api.Models;

namespace TransparentGiving.Api.Services
{
    public class MockChildService : IChildService
    {
        public List<ChildProfile> GetAllChildren()
        {
            return new List<ChildProfile>
            {
                new ChildProfile
                {
                    Id = "child-001",
                    Name = "Amina",
                    Age = 7,
                    Country = "Kenya",
                    SurgeryType = "Cleft Lip Repair",
                    CostTotal = 250,
                    CostFunded = 185,
                    ImageUrl = "/images/amina.jpg",
                    Priority = 1
                },
                new ChildProfile
                {
                    Id = "child-002",
                    Name = "Luis",
                    Age = 5,
                    Country = "Guatemala",
                    SurgeryType = "Clubfoot Correction",
                    CostTotal = 300,
                    CostFunded = 100,
                    ImageUrl = "/images/luis.jpg",
                    Priority = 2
                },
                new ChildProfile
                {
                    Id = "child-003",
                    Name = "Sana",
                    Age = 6,
                    Country = "Bangladesh",
                    SurgeryType = "Hernia Repair",
                    CostTotal = 200,
                    CostFunded = 200,
                    ImageUrl = "/images/sana.jpg",
                    Priority = 3
                }
            };
        }
    }
}
