using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cavista.CTRecruita.Utilities.Configurations
{
    public class ConnectionStrings
    {
        public string DefaultConnection { get; set; }
        public string ReadOnlyConnection { get; set; }
        public string HangfireConn { get; set; }
    }
}
