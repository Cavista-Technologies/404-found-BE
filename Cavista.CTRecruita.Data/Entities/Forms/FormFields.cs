using Cavista.CTRecruita.Data.Entities.Enums;
using Cavista.CTRecruita.Data.Entities.Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cavista.CTRecruita.Data.Entities.Forms
{
    public class FormFields : BaseEntity
    {
        public long ApplicationFormId { get; set; }
        public ApplicationForm ApplicationForm { get; set; }
        public string Label { get; set; }
        public string Placeholder { get; set; }
        public FormFieldType FieldType { get; set; }
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
        public bool IsStandard { get; set; }     // Full Name / Email / Phone / Resume map to Candidate
        public string OptionsJson { get; set; }  // JSON string[] — only for Dropdown
    }
}
