using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MenuRecommendation.Models;
using MenuRecommendation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;

namespace MenuRecommendation.Controllers
{
	[Authorize]
    [Route("api/[controller]")]
    public class MenuRecommenderController : Controller
    {
        IMenuCatalogService menuCatalogService;

        public MenuRecommenderController(IMenuCatalogService menuCatalogService)
        {
            this.menuCatalogService = menuCatalogService;
        }

        // GET: /api/MenuRecommender/MenuOfTheMoment
        [HttpGet("MenuOfTheMoment")]
        public async Task<ActionResult> GetMenuOfTheMoment()
        {
			
            var menus = await menuCatalogService.GetMenus();

            if (menus!=null && menus.Any())
            {
                int random = new Random().Next(0, menus.Count);

                var menu = menus[random];
                return new JsonResult(menu);
            }
            else
            {
                return NotFound();
            }

            
        }
    }
}
