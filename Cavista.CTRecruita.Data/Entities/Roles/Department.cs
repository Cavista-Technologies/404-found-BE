using Cavista.CTRecruita.Data.Entities.BaseEntites;
using Hangfire.Common;

namespace Cavista.CTRecruita.Data.Entities.Roles
{
    public class Department : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<JobRole> Jobs { get; set; } = new List<JobRole>();
    }
}
