using Cavista.CTRecruita.Data.Entities.Auth;
using Cavista.CTRecruita.Data.Entities.BaseEntites;
using Cavista.CTRecruita.Data.Entities.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cavista.CTRecruita.Data.Entities.Roles
{
    public class ApplicationStageHistory : BaseEntity
    {
        [ForeignKey(nameof(Application))]
        public long ApplicationId { get; set; }
        public Application Application { get; set; }
        public ApplicationStage FromStage { get; set; }
        public ApplicationStage ToStage { get; set; }
        public DateTime ChangedOn { get; set; } = DateTime.UtcNow;
        [ForeignKey(nameof(ChangedBy))]
        public long? ChangedById { get; set; }
        public AppUser ChangedBy { get; set; }
        public string? Reason { get; set; }
    }
}
