using Cavista.CTRecruita.Data.Entities.BaseEntites;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cavista.CTRecruita.Data.Entities.Auth
{
    public class AppUser : IdentityUser<long>
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public UserType UserType { get; set; }
        public UserStatus UserStatus { get; set; }
        public bool RequiresPasswordReset { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
    public class AppRole : IdentityRole<long> { }
}
