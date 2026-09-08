using Cavista.CTRecruita.Data.Entities.Auth;
using Cavista.CTRecruita.Data.Entities.BaseEntites;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cavista.CTRecruita.Data.Entities.Roles
{
    public class Interview : BaseEntity
    {
        [ForeignKey(nameof(ApplicationCandidate))]
        public long ApplicationCandidateId { get; set; }
        public ApplicationCandidate ApplicationCandidate { get; set; }
        public DateTime ScheduledAt { get; set; }
        public int DurationMinutes { get; set; }
        public InterviewMode Mode { get; set; }
        public string Location { get; set; }
        public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;
        [ForeignKey(nameof(Interviewer))]
        public long InterviewerId { get; set; }
        public AppUser Interviewer { get; set; }
        public string Feedback { get; set; }
        public int? Rating { get; set; }
    }
}
