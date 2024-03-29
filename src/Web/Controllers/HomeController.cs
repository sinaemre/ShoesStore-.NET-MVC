﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;
using Web.Interfaces;
using Web.Models;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHomeViewModelService _homeViewModelService;

        public HomeController(ILogger<HomeController> logger, IHomeViewModelService homeViewModelService)
        {
            _logger = logger;
            _homeViewModelService = homeViewModelService;
        }

        public async Task<IActionResult> Index(int? categoryId, int? brandId, int p = 1)
        {
            return View(await _homeViewModelService.GetHomeViewModelAsync(categoryId, brandId, p));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
