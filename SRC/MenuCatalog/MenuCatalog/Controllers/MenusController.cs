using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MenuCatalog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace MenuCatalog.Controllers
{
    [Route("api/[controller]")]
	[Authorize]
    public class MenusController : Controller
    {
        
        public MenusController(MenuContext dbContext)
        {
            DbContext = dbContext;
        }

        public MenuContext DbContext { get; }



        // GET: /api/Menus
        [HttpGet("")]
        public async Task<List<Menu>> GetMenus()
        {
            var menus = await DbContext.Menus
                .ToListAsync();

            return menus;
        }
    }
}
