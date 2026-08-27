using Microsoft.EntityFrameworkCore;

namespace Cavista.CTRecruita.Data.Contexts
{
    public class ApplicationContext : ApplicationContextBase
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> dbContextOptions)
          : base(dbContextOptions)
        {
        }
    }
}
