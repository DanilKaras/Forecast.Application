using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AymanMVCProject.Models;
using DataCoin;
using Microsoft.Extensions.Options;

namespace AymanMVCProject.Controllers
{
    public class HomeController : Controller
    {
        private IOptions<ApplicationSettings> _appSettings;

        public HomeController(IOptions<ApplicationSettings> appSettings)
        {
            _appSettings = appSettings;
           
        }
        
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public async Task<IActionResult> UpdateAssets()
        {
            var assets = new Logic(_appSettings);
            assets.UpdateAseets();
            return Json(new {message = "Done"});
        }

        [HttpGet]
        public async Task<IActionResult> SymbolsList()
        {
            try
            {
                var logic = new Logic(_appSettings);
                var symbols = logic.GetAllSymbols().OrderBy(x => x).ToList();
                return Json(new {symbols = symbols});
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Index(string symbol, int dataHours, bool useSeasonality)
        {
            try
            {
                var coinHistory = new Logic(_appSettings, symbol, dataHours);
                coinHistory.GenerateCsvFile();
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw new Exception();
            }
            return View();
        }
    }
}
