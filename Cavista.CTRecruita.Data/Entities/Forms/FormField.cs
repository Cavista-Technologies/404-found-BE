using Cavista.CTRecruita.Data.Entities.BaseEntites;
using Cavista.CTRecruita.Data.Entities.Enums;
using Cavista.CTRecruita.Data.Entities.Form;

namespace Cavista.CTRecruita.Data.Entities.Forms
{
    public class FormField : BaseEntity
    {
        public long ApplicationFormId { get; set; }
        public ApplicationForm ApplicationForm { get; set; }
        public string Label { get; set; }
        public string Placeholder { get; set; }
        public FormFieldType FieldType { get; set; }
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
        public bool IsStandard { get; set; } = false;
        public string OptionsJson { get; set; }  
    }
}
