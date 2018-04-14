using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AymanMVCProject.Models;
using DataCoin;
using DataCoin.Models;
using DataCoin.Operations;
using DataCoin.Utility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

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
            var logic = new Logic(_appSettings);
            var symbols = logic.GetAllSymbols();
            symbols = symbols?.OrderBy(x => x).ToList();

            return Json(new {symbols});
        }

        [HttpPost]
        public async Task<IActionResult> ManualForecast(string symbol, int dataHours, int periods, bool hourlySeasonality, bool dailySeasonality)
        {
            var viewModel = new MainViewModel();
            var coin = new Logic(_appSettings, symbol, dataHours, currentLocation);
            var manager = new DirectoryManager(_appSettings, currentLocation);
            var pythonRun = new Logic(_appSettings);
            StaticUtility.RequestCounter = (manager.GetRequestCount());
            try
            {
                var pathToFolder = manager.GenerateForecastFolder(symbol, periods, DirSwitcher.Manual);
                if (!coin.GenerateCsvFile(pathToFolder))
                {
                    throw new Exception("Something's wrong with a coin");
                }
                
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
                    manager.UpdateRequests(StaticUtility.RequestCounter);
                    return NotFound(new { message = "out.csv not found", requestCount = StaticUtility.RequestCounter });
                }

                if (forecastCreated)
                {
                    viewModel.ForecastPath = Path.DirectorySeparatorChar + pathToForecastForImg;
                }
                else
                {
                    manager.UpdateRequests(StaticUtility.RequestCounter);
                    return NotFound(new { message = "forecast.png not found", requestCount = StaticUtility.RequestCounter});
                }
                
                if (componentsCreated)
                {
                    viewModel.ComponentsPath = Path.DirectorySeparatorChar + pathToComponentsForImg;
                }
                else
                {
                    manager.UpdateRequests(StaticUtility.RequestCounter);
                    return NotFound(new { message = "components.png not found", requestCount = StaticUtility.RequestCounter});
                }
                
                manager.UpdateRequests(StaticUtility.RequestCounter);
                viewModel.RequestsPerDay = StaticUtility.RequestCounter;//manager.CurrentCounts;
                viewModel.AssetName = symbol;
                var performance = coin.DefineTrend(viewModel.Table);
                viewModel.Indicator = performance.Indicator;
            }
            catch (Exception e)
            {
                manager.UpdateRequests(StaticUtility.RequestCounter);
                return NotFound(new {message = e.Message, requestCount = StaticUtility.RequestCounter});
            }
            
            return Json(viewModel);
        }

        [HttpGet]
        public IActionResult AutoForecast()
        {   
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AutoForecastPost(int dataHours, int periods, bool hourlySeasonality, bool dailySeasonality)
        {
            var viewModel = new AutoForecastViewModel();
            var manager = new DirectoryManager(_appSettings, currentLocation);
            var assets = DirectoryManager.ReadAssetsFromExcel(manager.AsstesLocation);
            //var symbol = assets.First();
            StaticUtility.RequestCounter = manager.GetRequestCount();//StaticUtility.AddRequestCount(manager.GetRequestCount());
            //var pythonRun = new Logic(_appSettings);
            var catchAsset = string.Empty;
            try
            {
                Parallel.ForEach(assets, symbol =>
                    {
                        catchAsset = symbol;
                        var pythonRun = new Logic(_appSettings);
                        var coin = new Logic(_appSettings, symbol, dataHours, currentLocation);
                        var pathToFolder = manager.GenerateForecastFolder(symbol, periods, DirSwitcher.Auto);
                        //coin.GenerateCsvFile(pathToFolder);
                        if (!coin.GenerateCsvFileAuto(pathToFolder))
                        {
                            StaticUtility.Log(symbol, Indicator.ZeroRezults, 0);
                            return;
                        }
                        pythonRun.PythonExecutor(pathToFolder, periods, hourlySeasonality, dailySeasonality);

                        var pathToOut = Path.Combine(pathToFolder, manager.OutFile);
                        var pathToComponents = Path.Combine(pathToFolder, manager.OutComponents);
                        var pathToForecast = Path.Combine(pathToFolder, manager.OutForecast);

                        var outCreated =  StaticUtility.WaitForFile(pathToOut, 20);
                        var componentsCreated =  StaticUtility.WaitForFile(pathToComponents, 10);
                        var forecastCreated =  StaticUtility.WaitForFile(pathToForecast, 10);
                        if (!outCreated.Result || !forecastCreated.Result || !componentsCreated.Result) return;
                        var table = StaticUtility.BuildOutTableRows(pathToOut, periods);
                        var performance = coin.DefineTrend(table);
                        StaticUtility.Log(symbol, performance.Indicator, performance.Rate);
                        manager.SpecifyDirByTrend(performance.Indicator, pathToFolder);
                    }
                );
                
                manager.UpdateRequests(StaticUtility.RequestCounter);
                var folder = manager.GetLastFolder(DirSwitcher.Auto);
                StaticUtility.WriteLogExcel(folder);
                var positiveDir = Path.Combine(folder, manager.DirPositive);
                var neutralDir = Path.Combine(folder, manager.DirNeutral);
                var negativeDir = Path.Combine(folder, manager.DirNegative);
                var strongPositiveDir = Path.Combine(folder, manager.DirStrongPositive);
                var pathToExcelLog = Path.Combine(folder, StaticUtility.LogName);  
                if (DirectoryManager.IsFolderExist(positiveDir))
                {
                    viewModel.PositiveAssets = DirectoryManager.GetFolderNames(positiveDir);
                }

                if (DirectoryManager.IsFolderExist(neutralDir))
                {
                    viewModel.NeutralAssets = DirectoryManager.GetFolderNames(neutralDir);
                }
                
                if (DirectoryManager.IsFolderExist(negativeDir))
                {
                    viewModel.NegativeAssets = DirectoryManager.GetFolderNames(negativeDir);
                }
                
                if (DirectoryManager.IsFolderExist(strongPositiveDir))
                {
                    viewModel.StrongPositiveAssets = DirectoryManager.GetFolderNames(strongPositiveDir);
                }

                viewModel.RequestCount = StaticUtility.RequestCounter;
                viewModel.Report = manager.ReadLog(pathToExcelLog);
            }
            catch (Exception e)
            {
                manager.UpdateRequests(StaticUtility.RequestCounter);
                StaticUtility.WriteLogExcel(manager.GetLastFolder(DirSwitcher.Auto));
                return NotFound(new {message = e.Message + " Assset: " + catchAsset, requestCount = manager.CurrentCounts});
            }

            return Json(viewModel); 
        }


        public IActionResult GetForecastData(Indicator indicator, string assetName, int periods)
        {
            var viewModel = new MainViewModel();
            
            var manager = new DirectoryManager(_appSettings, currentLocation);
            var folder = manager.GetLastFolder(DirSwitcher.Auto);
            try
            {
                string indicatorDir;
                string dir;
                switch (indicator)
                {
                    case Indicator.Positive:
                        indicatorDir = manager.DirPositive;
                        dir = Path.Combine(folder, indicatorDir);
                        break;
                    case Indicator.Neutral:
                        indicatorDir = manager.DirNeutral;
                        dir = Path.Combine(folder, indicatorDir);
                        break;
                    case Indicator.Negative:
                        indicatorDir = manager.DirNegative;
                        dir = Path.Combine(folder, indicatorDir);
                        break;
                    case Indicator.StrongPositive:
                        indicatorDir = manager.DirStrongPositive;
                        dir = Path.Combine(folder, indicatorDir);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(indicator), indicator, null);
                }

                var targetFolder = DirectoryManager.GetForecastFolderByName(dir, assetName);
                var pathToOut = Path.Combine(dir, targetFolder, manager.OutFile);
                var pathToComponentsForImg = Path.Combine(_appSettings.Value.ForecastDir, manager.DirForImages(DirSwitcher.Auto), indicatorDir, targetFolder, manager.OutComponents);
                var pathToForecastForImg = Path.Combine(_appSettings.Value.ForecastDir,  manager.DirForImages(DirSwitcher.Auto), indicatorDir, targetFolder, manager.OutForecast);
                
                viewModel.ComponentsPath = Path.DirectorySeparatorChar + pathToComponentsForImg;
                viewModel.ForecastPath = Path.DirectorySeparatorChar + pathToForecastForImg;
                viewModel.RequestsPerDay = manager.GetRequestCount();
                viewModel.AssetName = assetName;
                viewModel.Indicator = indicator;
                viewModel.Table = StaticUtility.BuildOutTableRows(pathToOut, periods);
    
            }
            catch (Exception e)
            {
                return NotFound(new { message = e.Message });
            }
            
            
            return Json(viewModel);
        }

        public IActionResult GetLatestAssets()
        {
            var viewModel = new AutoForecastViewModel();
            try
            {
                var manager = new DirectoryManager(_appSettings, currentLocation);
                var folder = manager.GetLastFolder(DirSwitcher.Auto);
                var positiveDir = Path.Combine(folder, manager.DirPositive);
                var neutralDir = Path.Combine(folder, manager.DirNeutral);
                var negativeDir = Path.Combine(folder, manager.DirNegative);
                var strongPositiveDir = Path.Combine(folder, manager.DirStrongPositive);
                var pathToExcelLog = Path.Combine(folder, StaticUtility.LogName);
                viewModel.PositiveAssets = DirectoryManager.GetFolderNames(positiveDir);
                viewModel.NeutralAssets = DirectoryManager.GetFolderNames(neutralDir);
                viewModel.NegativeAssets = DirectoryManager.GetFolderNames(negativeDir);
                viewModel.StrongPositiveAssets = DirectoryManager.GetFolderNames(strongPositiveDir); 
                viewModel.Report = manager.ReadLog(pathToExcelLog);
            }
            catch (Exception e)
            {
                return NotFound(new { message = e.Message });
            }
            
            return Json(viewModel); 
        }
        
        
        public async Task<IActionResult> InstantForecast()
        {
            var viewModel = new InstantForecastModal();
            const int periods = 24;
            const int dataHours = 230;
            const bool hourlySeasonality = false;
            const bool dailySeasonality = false;
            var numFormat = new CultureInfo("en-US", false ).NumberFormat;
            numFormat.PercentDecimalDigits = 3;
            
            var symbol = _appSettings.Value.InstantForecast;
            var coin = new Logic(_appSettings, symbol, dataHours, currentLocation);
            var manager = new DirectoryManager(_appSettings, currentLocation);
            var pythonRun = new Logic(_appSettings);
            
            StaticUtility.RequestCounter = (manager.GetRequestCount());
            try
            {
                var pathToFolder = manager.GenerateForecastFolder(symbol, periods, DirSwitcher.Instant);
                
                if (!coin.GenerateCsvFile(pathToFolder))
                {
                    throw new Exception("Something's wrong with a coin");
                }
                
                pythonRun.PythonExecutor(manager.GetLastFolder(DirSwitcher.Instant), periods, hourlySeasonality, dailySeasonality);
                
                var pathToOut = Path.Combine(manager.GetLastFolder(DirSwitcher.Instant), manager.OutFile);
                var pathToComponents = Path.Combine(manager.GetLastFolder(DirSwitcher.Instant), manager.OutComponents);
                var pathToForecast = Path.Combine(manager.GetLastFolder(DirSwitcher.Instant), manager.OutForecast);

                var pathToForecastForImg = Path.Combine(_appSettings.Value.ForecastDir, manager.DirForImages(DirSwitcher.Instant), manager.OutForecast);

                var outCreated =  await StaticUtility.WaitForFile(pathToOut, 60);
                var componentsCreated = await StaticUtility.WaitForFile(pathToComponents, 10);
                var forecastCreated = await StaticUtility.WaitForFile(pathToForecast, 10);
                
                var tableInstant = new List<TableRow>();
                if (outCreated)
                {
                    tableInstant = StaticUtility.BuildOutTableRows(pathToOut, periods).ToList();
                }
                else 
                {
                    manager.UpdateRequests(StaticUtility.RequestCounter);
                    return NotFound(new { message = "out.csv not found", requestCount = StaticUtility.RequestCounter });
                }
                if (forecastCreated)
                {
                    viewModel.ForecastPath = Path.DirectorySeparatorChar + pathToForecastForImg;
                }
                else
                {
                    manager.UpdateRequests(StaticUtility.RequestCounter);
                    return NotFound(new { message = "forecast.png not found", requestCount = StaticUtility.RequestCounter});
                }
                
                if (!componentsCreated)
                {
                    manager.UpdateRequests(StaticUtility.RequestCounter);
                    return NotFound(new { message = "components.png not found", requestCount = StaticUtility.RequestCounter});
                }
                manager.UpdateRequests(StaticUtility.RequestCounter);
                
                viewModel.AssetName = symbol;
                var performance = coin.DefineTrend(tableInstant);
                viewModel.Indicator = performance.Indicator;
                viewModel.Rate = performance.Rate.ToString("P", numFormat);
            }
            catch (Exception e)
            {
                manager.UpdateRequests(StaticUtility.RequestCounter);
               return NotFound(new {message = e.Message, requestCount = StaticUtility.RequestCounter});
            }
            return Json(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetIndicatorList()
        {
            var props = new List<string>();
            
            foreach (var property in Enum.GetValues(typeof(Indicator)))
            {
                props.Add(property.ToString());
            }

            return Json(props);
        }
        
        [HttpGet]
        public IActionResult TwoStepForecast()
        {   
            return View();
        }
    }
}
