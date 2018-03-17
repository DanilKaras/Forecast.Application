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
using DataCoin.Operations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite.Internal.ApacheModRewrite;
using Microsoft.Extensions.Options;

namespace AymanMVCProject.Controllers
{
    public class HomeController : Controller
    {
        private IOptions<ApplicationSettings> _appSettings;
        private string currentLocation;

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

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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
        public async Task<ActionResult> Index(string symbol, int dataHours, int periods, bool hourlySeasonality, bool dailySeasonality)
        {
            var viewModel = new MainViewModel();
            var coinHistory = new Logic(_appSettings, symbol, dataHours, currentLocation);
            var manager = new DirectoryManager(_appSettings, currentLocation);
            var pythonRun = new Logic(_appSettings); 
            try
            {
                coinHistory.GenerateCsvFile();

                pythonRun.PythonExecutor(manager.LastFolder, periods, hourlySeasonality, dailySeasonality);

                var pathToOut = Path.Combine(manager.LastFolder, manager.OutFile);
                var pathToComponents = Path.Combine(manager.LastFolder, manager.OutComponents);
                var pathToForecast = Path.Combine(manager.LastFolder, manager.OutForecast);
               
                var pathToComponentsForImg = Path.Combine(_appSettings.Value.ForecastDir, manager.FolderForImg, manager.OutComponents);
                var pathToForecastForImg = Path.Combine(_appSettings.Value.ForecastDir,manager.FolderForImg, manager.OutForecast);
                
                var outCreated =  await WaitForFile(pathToOut, 60);
                var componentsCreated = await WaitForFile(pathToComponents, 10);
                var forecastCreated = await WaitForFile(pathToForecast, 10);

                if (outCreated)
                {
                    viewModel.Table = BuildOutTableRows(pathToOut, periods);
                }
                else 
                {
                    return NotFound(new { message = "out.csv not found", requestCount = manager.CurrentCounts });
                }

                if (forecastCreated)
                {
                    viewModel.ForecastPath = pathToForecastForImg;
                }
                else
                {
                    return NotFound(new { message = "forecast.png not found", requestCount = manager.CurrentCounts});
                }
                
                if (componentsCreated)
                {
                    viewModel.ComponentsPath = pathToComponentsForImg;
                }
                else
                {
                    return NotFound(new { message = "components.png not found", requestCount = manager.CurrentCounts});
                }

                viewModel.RequestsPerDay = manager.CurrentCounts;
                viewModel.AssetName = symbol;
                
                
            }
            catch (Exception e)
            {
                return NotFound(new {message = "Not enough data to process!", requestCount = manager.CurrentCounts});
            }
            
            return Json(viewModel);
        }

        public IActionResult TestLink()
        {
            var manager = new DirectoryManager(_appSettings, currentLocation);
            manager.UpdateRequests(100);
           //pythonRun.PythonExecutor(DirectoryManager.GetLastFolder(pyPath), 72, false, false);
//            var test = new DirectoryManager(_appSettings);
//            var lastFolger = test.LastFolder;
//            try
//            {
//                //var pythonRun = new Logic(_appSettings); 
//                //pythonRun.PythonExecutor(lastFolger, 24, true, false);
//                var data = BuildOutTableRows(lastFolger, 24);
//            }
//            catch (Exception e)
//            {
//                throw new Exception();
//            }
              return Ok();
        }

        private static IEnumerable<TableRow> BuildOutTableRows(string path, int period)
        {
            var table = new List<TableRow>();
            var reader = new StreamReader(System.IO.File.OpenRead($"{path}"));
            var counter = 0;

            var dsPos = 0;
            var yhatPos = 0;
            var yhatUpperPos = 0;
            var yhatLowerUpper = 0;
            while (!reader.EndOfStream)
            {
                counter++;
                var line = reader.ReadLine();
                
                var values = line.Split(',');
                if (counter == 1)
                {
                    dsPos = FindPosition(values, "ds");
                    yhatPos = FindPosition(values, "yhat");
                    yhatUpperPos = FindPosition(values, "yhat_lower");
                    yhatLowerUpper = FindPosition(values, "yhat_upper");
                }
                else
                {
                    var row = new TableRow()
                    {
                        ID = values[0],
                        DS = values[dsPos],
                        Yhat = Convert.ToDecimal(values[yhatPos]).ToString("E2"),
                        YhatUpper = Convert.ToDecimal(values[yhatUpperPos]).ToString("E2"),
                        YhatLower = Convert.ToDecimal(values[yhatLowerUpper]).ToString("E2"),  
                    };
                    table.Add(row);
                }  
            }
               
            return table.Skip(Math.Max(0, table.Count() - period)).Reverse();
        }

        private static int FindPosition(IEnumerable<string> array, string colName)
        {
            var pos = -1;
            foreach (var item in array)
            {
                pos++;
                if (item == colName)
                {
                    return pos;
                }
            }
            return 0;
        }
        
        private static async Task<bool> WaitForFile(string path, int timeout)
        {
            var timeoutAt = DateTime.Now.AddSeconds(timeout);
            while (true)
            {
                if (System.IO.File.Exists(path)) return true;
                if (DateTime.Now >= timeoutAt) return false;
                await Task.Delay(10);
            }
        }
    }
    
}
