using Cavista.CTRecruita.Data.Entities.Auth;
using Cavista.CTRecruita.Data.Entities.Form;
using Cavista.CTRecruita.Data.Entities.Forms;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace Cavista.CTRecruita.Data.Contexts
{
    public abstract class ApplicationContextBase : IdentityDbContext<AppUser, AppRole, long>
    {
        public ApplicationContextBase(DbContextOptions dbContextOptions) : base(dbContextOptions)
        {

        }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<AppRole> AppRoles { get; set; }
        public DbSet<ApplicationForm> ApplicationForms { get; set; }
        public DbSet<FormFields> FormFields { get; set; }
        public DbSet<ApplicationAnswer> ApplicationAnswers { get; set; }

    }
}
