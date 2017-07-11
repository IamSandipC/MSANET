using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MenuCatalog.Models
{
    public class MenuContext : DbContext
    {
        public MenuContext(DbContextOptions options)
           : base(options)
        {
        }

        public DbSet<Menu> Menus { get; set; }

    }
}
