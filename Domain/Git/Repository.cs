using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevRating.Domain.Git
{
    public interface Repository
    {
        IEnumerable<Commit> Commits(string since, string until);
    }
}