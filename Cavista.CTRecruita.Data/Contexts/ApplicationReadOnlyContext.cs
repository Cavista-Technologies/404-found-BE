using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cavista.CTRecruita.Data.Contexts
{
    public class ApplicationReadOnlyContext : ApplicationContextBase
    {
        public override int SaveChanges()
        {
            if (this.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
            {
                return base.SaveChanges();
            }
            throw new Exception("Save changes can not be executed on this context");
        }

        public ApplicationReadOnlyContext(DbContextOptions<ApplicationReadOnlyContext> dbContextOptions) : base(dbContextOptions)
        {
        }
    }
}
