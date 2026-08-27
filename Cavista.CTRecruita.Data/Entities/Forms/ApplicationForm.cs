using Cavista.CTRecruita.Data.Entities.BaseEntites;
using Cavista.CTRecruita.Data.Entities.Enums;
using Cavista.CTRecruita.Data.Entities.Forms;
using Cavista.CTRecruita.Data.Entities.Roles;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cavista.CTRecruita.Data.Entities.Form
{
    public class ApplicationForm : BaseEntity
    {
        public long JobRoleId { get; set; }
        [ForeignKey(nameof(JobRoleId))]
        public JobRole JobRole { get; set; }
        public string Title { get; set; }
        public string IntroMessage { get; set; }
        public FormStatus Status { get; set; } = FormStatus.Draft;
        public string Slug { get; set; } 
        public ICollection<FormField> Fields { get; set; } = new List<FormField>();
    }
}
