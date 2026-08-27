using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cavista.CTRecruita.Utilities.Configurations
{
    public class JWTConfig
    {
        public string Issuer { get; set; }
        public string SigningKey { get; set; }
        public int ExpiryTime { get; set; }
    }
}
