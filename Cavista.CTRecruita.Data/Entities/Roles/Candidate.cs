using Cavista.CTRecruita.Data.Entities.BaseEntites;

namespace Cavista.CTRecruita.Data.Entities.Roles
{
    public class Candidate : BaseEntity
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Source { get; set; }
        public string LinkedInUrl { get; set; }
        public ICollection<Application> Applications { get; set; } = new List<Application>();
    }
}
