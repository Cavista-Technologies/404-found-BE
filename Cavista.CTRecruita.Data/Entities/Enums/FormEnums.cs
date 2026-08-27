using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cavista.CTRecruita.Data.Entities.Enums
{
    public enum FormFieldType 
    { 
        ShortText = 1, 
        LongText, 
        Number, 
        Url, 
        Email, 
        Phone, 
        Dropdown, 
        FileUpload
    }
    public enum FormStatus 
    { 
        Draft = 1, 
        Published 
    }
    public enum ApplicationSource 
    { 
        Direct = 1, 
        LinkedIn, 
        JobBoard, 
        Referral, 
        Twitter, 
        Email, 
        Other 
    }
}
