using MenuRecommendation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MenuRecommendation.Services
{
    public interface IMenuCatalogService
    {
        Task<List<Menu>> GetMenus();
    }
}
