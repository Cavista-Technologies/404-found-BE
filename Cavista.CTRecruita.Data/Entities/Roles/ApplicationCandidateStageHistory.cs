using Cavista.CTRecruita.Data.Entities.Auth;
using Cavista.CTRecruita.Data.Entities.BaseEntites;
using Cavista.CTRecruita.Data.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cavista.CTRecruita.Data.Entities.Roles
{
    public class ApplicationCandidateStageHistory : BaseEntity
    {
        [ForeignKey(nameof(ApplicationCandidate))]
        public long ApplicationCandidateId { get; set; }

        public ApplicationCandidate ApplicationCandidate { get; set; }

        public ApplicationStage FromStage { get; set; }

        public ApplicationStage ToStage { get; set; }

        public DateTime ChangedOn { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(ChangedBy))]
        public long? ChangedById { get; set; }

        public AppUser ChangedBy { get; set; }

        public string? Reason { get; set; }
    }
}
