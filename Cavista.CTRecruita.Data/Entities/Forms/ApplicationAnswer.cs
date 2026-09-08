using Cavista.CTRecruita.Data.Entities.BaseEntites;
using Cavista.CTRecruita.Data.Entities.Roles;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cavista.CTRecruita.Data.Entities.Forms
{
    public class ApplicationAnswer : BaseEntity
    {
        [ForeignKey(nameof(ApplicationCandidate))]
        public long ApplicationCandidateId { get; set; }

        public ApplicationCandidate ApplicationCandidate { get; set; }

        public long FormFieldId { get; set; }

        public FormField FormField { get; set; }

        public string Value { get; set; }
    }
}
