using TransparentGiving.Api.Models;

namespace TransparentGiving.Api.Services
{
    public interface IChildService
    {
        List<ChildProfile> GetAllChildren();
    }
}