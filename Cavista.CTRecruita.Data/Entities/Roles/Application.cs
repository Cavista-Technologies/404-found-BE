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
        [ForeignKey(nameof(JobRole))]
        public long JobRoleId { get; set; }

        public JobRole JobRole { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public ICollection<ApplicationCandidate> Candidates { get; set; } = new List<ApplicationCandidate>();
    }
}
