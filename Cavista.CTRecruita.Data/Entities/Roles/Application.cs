using Cavista.CTRecruita.Data.Entities.Auth;
using Cavista.CTRecruita.Data.Entities.BaseEntites;
using Cavista.CTRecruita.Data.Entities.Enums;
using Cavista.CTRecruita.Data.Entities.Forms;
using Hangfire.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cavista.CTRecruita.Data.Entities.Roles
{
    public class Application : BaseEntity
    {
        [ForeignKey(nameof(Candidate))]
        public long CandidateId { get; set; }
        public Candidate Candidate { get; set; }
        [ForeignKey(nameof(JobRoleId))]
        public long JobRoleId { get; set; }
        public JobRole JobRole { get; set; }
        public ApplicationStage Stage { get; set; } = ApplicationStage.Applied;
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Active;
        public ApplicationSource Source { get; set; } = ApplicationSource.Direct;
        public DateTime AppliedOn { get; set; } = DateTime.UtcNow;
        [ForeignKey(nameof(AssignedRecruiter))]
        public long? AssignedRecruiterId { get; set; }
        public AppUser AssignedRecruiter { get; set; }
        public ICollection<ApplicationStageHistory> StageHistory { get; set; } = new List<ApplicationStageHistory>();
        public ICollection<Interview> Interviews { get; set; } = new List<Interview>();
        public ICollection<ApplicationAnswer> Answers { get; set; } = new List<ApplicationAnswer>();
    }
}
