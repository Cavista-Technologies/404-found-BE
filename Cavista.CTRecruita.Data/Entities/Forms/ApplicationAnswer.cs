using Cavista.CTRecruita.Data.Entities.BaseEntites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Cavista.CTRecruita.Data.Entities.Forms
{
    public class ApplicationAnswer : BaseEntity
    {
        public long ApplicationId { get; set; }
        public Application Application { get; set; }
        public long FormFieldId { get; set; }
        public FormField FormField { get; set; }
        public string Value { get; set; }   
    }
}
