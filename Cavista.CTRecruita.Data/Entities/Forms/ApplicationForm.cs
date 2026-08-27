using Cavista.CTRecruita.Data.Entities.BaseEntites;
using Cavista.CTRecruita.Data.Entities.Enums;
using Cavista.CTRecruita.Data.Entities.Forms;
using Hangfire.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cavista.CTRecruita.Data.Entities.Form
{
    public class ApplicationForm : BaseEntity
    {
        public long JobId { get; set; }
        public Job Job { get; set; }
        public string Title { get; set; }
        public string IntroMessage { get; set; }
        public FormStatus Status { get; set; } = FormStatus.Draft;
        public string Slug { get; set; } 
        public ICollection<FormFields> Fields { get; set; } = new List<FormFields>();
        public ApplicationSource Source { get; set; } = ApplicationSource.Direct;
        public ICollection<ApplicationAnswer> Answers { get; set; } 
    }
}
