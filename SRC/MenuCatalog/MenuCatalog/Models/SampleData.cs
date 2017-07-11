using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MenuCatalog.Models
{
    public class SampleData
    {
        public async static Task InitializeMenuDatabase(IServiceProvider serviceProvider)
        {
            if (ShouldDropCreateDatabase())
            {

                using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var db = serviceScope.ServiceProvider.GetService<MenuContext>();
                    
                    //returns true when database is being created. false if it already exists
                    if (db.Database.EnsureCreated())
                    {
                        await InsertTestData(serviceProvider);

                    }
                }
            }

        }

        private async static Task InsertTestData(IServiceProvider serviceProvider)
        {
            await AddOrUpdateAsync(serviceProvider, m => m.Name, Menus.Select(menu => menu.Value));
        }

        private async static Task AddOrUpdateAsync<TEntity>(IServiceProvider serviceProvider,
                                            Func<TEntity, object> propertyToMatch, 
                                            IEnumerable<TEntity> entities)   where TEntity : class
        {
            // Query in a separate context so that we can attach existing entities as modified
            List<TEntity> existingData;
            using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var db = serviceScope.ServiceProvider.GetService<MenuContext>();
                existingData = db.Set<TEntity>().ToList();
            }

            using (var serviceScope1 = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var db1 = serviceScope1.ServiceProvider.GetService<MenuContext>();
                foreach (var item in entities)
                {
                    
                    db1.Entry(item).State = existingData.Any(g => propertyToMatch(g).Equals(propertyToMatch(item)))
                       ? EntityState.Modified
                       : EntityState.Added;
                }

                
                await db1.SaveChangesAsync();

                //If using MySql.Data.EntityFrameworkCore Version=7.0.7-m61, SaveChangesAsync does not work. 
                //Need to use SaveChangesAsync()
                //db1.SaveChanges();
            }
        }

        private static bool ShouldDropCreateDatabase()
        {
            string index = Environment.GetEnvironmentVariable("INSTANCE_INDEX");
            if (string.IsNullOrEmpty(index))
            {
                return true;
            }
            int indx = -1;
            if (int.TryParse(index, out indx))
            {
                if (indx > 0) return false;
            }
            return true;
        }

#region Sample Data
        private static Dictionary<string, Menu> menus;
        public static Dictionary<string, Menu> Menus
        {
            get
            {
                if (menus == null)
                {
                    var menuList = new Menu[]
                    {
                        new Menu { Name = "chocolate peanut butter cake" },
                        new Menu { Name = "Broccoli Cheese & Cracker Casserole" },
                        new Menu { Name = "Braised Balsamic Chicken" },
                        new Menu { Name = "Creamy Avocado Pasta" },
                        new Menu { Name = "Spicy Lemon Garlic Shrimp" }
                    };

                    menus = new Dictionary<string, Menu>();
                    foreach (Menu menu in menuList)
                    {
                        menus.Add(menu.Name, menu);
                    }
                }

                return menus;
            }
        }
#endregion

    }

}

