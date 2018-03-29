using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AymanMVCProject.Models;
using DataCoin;
using DataCoin.Models;
using DataCoin.Operations;
using DataCoin.Utility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite.Internal.ApacheModRewrite;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Extensions.Options;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;

namespace AymanMVCProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly IOptions<ApplicationSettings> _appSettings;
        private readonly string currentLocation;

        public HomeController(IOptions<ApplicationSettings> appSettings, IHostingEnvironment env)
        {
            _appSettings = appSettings;
            currentLocation = env.ContentRootPath;
        }
        
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        
        [HttpGet]
        public IActionResult ManualForecast()
        {
            return View();
        }

        [HttpGet]
        public IActionResult RequestsForToday()
        {
            var manager = new DirectoryManager(_appSettings, currentLocation);
            return Json(new {requestCount  = manager.CurrentCounts});
        }
        
        [HttpGet]
        public IActionResult UpdateAssets()
        {
            var assets = new Logic(_appSettings);
            assets.UpdateAseets();
            return Json(new {message = "Done"});
        }

        [HttpGet]
        public IActionResult SymbolsList()
        {
            try
            {
                var logic = new Logic(_appSettings);
                var symbols = logic.GetAllSymbols();
                
                symbols = symbols?.OrderBy(x => x).ToList();

                return Json(new {symbols = symbols});
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> ManualForecast(string symbol, int dataHours, int periods, bool hourlySeasonality, bool dailySeasonality)
        {
            var viewModel = new MainViewModel();
            var coin = new Logic(_appSettings, symbol, dataHours, currentLocation);
            var manager = new DirectoryManager(_appSettings, currentLocation);
            var pythonRun = new Logic(_appSettings);
            
            try
            {
                var pathToFolder = manager.GenerateForecastFolder(symbol, periods, DirSwitcher.Manual);
                coin.GenerateCsvFile(pathToFolder);
                pythonRun.PythonExecutor(manager.GetLastFolder(DirSwitcher.Manual), periods, hourlySeasonality, dailySeasonality);

                var pathToOut = Path.Combine(manager.GetLastFolder(DirSwitcher.Manual), manager.OutFile);
                var pathToComponents = Path.Combine(manager.GetLastFolder(DirSwitcher.Manual), manager.OutComponents);
                var pathToForecast = Path.Combine(manager.GetLastFolder(DirSwitcher.Manual), manager.OutForecast);
               
                var pathToComponentsForImg = Path.Combine(_appSettings.Value.ForecastDir, manager.DirForImages(DirSwitcher.Manual), manager.OutComponents);
                var pathToForecastForImg = Path.Combine(_appSettings.Value.ForecastDir, manager.DirForImages(DirSwitcher.Manual), manager.OutForecast);
                
                var outCreated =  await StaticUtility.WaitForFile(pathToOut, 60);
                var componentsCreated = await StaticUtility.WaitForFile(pathToComponents, 10);
                var forecastCreated = await StaticUtility.WaitForFile(pathToForecast, 10);

                if (outCreated)
                {
                    viewModel.Table = StaticUtility.BuildOutTableRows(pathToOut, periods);
                }
                else 
                {
                    return NotFound(new { message = "out.csv not found", requestCount = manager.CurrentCounts });
                }

                if (forecastCreated)
                {
                    viewModel.ForecastPath = "/" + pathToForecastForImg;
                }
                else
                {
                    return NotFound(new { message = "forecast.png not found", requestCount = manager.CurrentCounts});
                }
                
                if (componentsCreated)
                {
                    viewModel.ComponentsPath = "/" + pathToComponentsForImg;
                }
                else
                {
                    return NotFound(new { message = "components.png not found", requestCount = manager.CurrentCounts});
                }

                viewModel.RequestsPerDay = manager.CurrentCounts;
                viewModel.AssetName = symbol;

                viewModel.Indicator = coin.DefineTrend(viewModel.Table);
            }
            catch (Exception e)
            {
                return NotFound(new {message = e.Message, requestCount = manager.CurrentCounts});
            }
            
            return Json(viewModel);
        }

        [HttpGet]
        public IActionResult AutomaticForecast()
        {   
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AutomaticForecastPost(int dataHours, int periods, bool hourlySeasonality, bool dailySeasonality)
        {
            var manager = new DirectoryManager(_appSettings, currentLocation);
            var assets = StaticUtility.ReadFromExcel(manager.AsstesLocation);
            var symbol = assets.First();
            var coin = new Logic(_appSettings, symbol, dataHours, currentLocation);
            var pathToFolder = manager.GenerateForecastFolder(symbol, periods, DirSwitcher.Auto);           
            //test purpose
            try
            {
                manager.SpecifyDirByTrend(Indicator.Negative, pathToFolder);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
            
            //var pythonRun = new Logic(_appSettings);
            return Json(new {assets = assets}); 
        }
        
        public IActionResult TestLink()
        {
            return Ok();
        }
    }
}
