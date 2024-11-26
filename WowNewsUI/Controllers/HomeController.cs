using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using WowNewsUI.Models;
using System.Text.Json;
using WowNewsAPI.Models;
using WowNewsUI.Controllers;
using static AuthService;

namespace WowNewsUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly AuthService _authService;
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;

        public HomeController(ILogger<HomeController> logger, HttpClient httpClient, AuthService authService)
        {
            _logger = logger;
            _httpClient = httpClient;
            _authService = authService;
        }

        public IActionResult Index()
        {
            return View();
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
        public async Task<IActionResult> Profile()
        {
            var profileData = await _authService.GetProfileAsync();
            return View("Profile", model: profileData);
        }

    }
}
