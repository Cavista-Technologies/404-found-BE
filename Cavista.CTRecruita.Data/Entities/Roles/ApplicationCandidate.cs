using Cavista.CTRecruita.Data.Entities.Auth;
using Cavista.CTRecruita.Data.Entities.BaseEntites;
using Cavista.CTRecruita.Data.Entities.Enums;
using Cavista.CTRecruita.Data.Entities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cavista.CTRecruita.Data.Entities.Roles
{
    public class ApplicationCandidate : BaseEntity
    {
        [ForeignKey(nameof(Application))]
        public long ApplicationId { get; set; }

        public Application Application { get; set; }

        [ForeignKey(nameof(Candidate))]
        public long CandidateId { get; set; }

        public Candidate Candidate { get; set; }

        public ApplicationStage Stage { get; set; } = ApplicationStage.Applied;

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Active;
        public ApplicationSource Source { get; set; } = ApplicationSource.Direct;

        public DateTime AppliedOn { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(AssignedRecruiter))]
        public long? AssignedRecruiterId { get; set; }

        public AppUser AssignedRecruiter { get; set; }

        public ICollection<ApplicationCandidateStageHistory> StageHistory { get; set; }
            = new List<ApplicationCandidateStageHistory>();

        public ICollection<Interview> Interviews { get; set; }
            = new List<Interview>();
        public ICollection<ApplicationAnswer> Answers { get; set; }  = new List<ApplicationAnswer>();
    }
}
