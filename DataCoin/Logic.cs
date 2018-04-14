using System;
using System.Collections.Generic;
using System.Linq;
using AymanMVCProject.Models;
using DataCoin.Models;
using DataCoin.Operations;
using DataCoin.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace DataCoin
{
    public class Logic
    {
        private readonly string symbolId;
        //private string path;
        private readonly IOptions<ApplicationSettings> _appSettings;
        private readonly int period;
        private readonly string currentLocation;
        private DirectoryManager manager;
        public Logic(IOptions<ApplicationSettings> appSettings, string symbolId, int period, string currentLocation)
        {
            
            this.symbolId = symbolId;
            this.period = period;
            _appSettings = appSettings;
            this.currentLocation = currentLocation; 
            manager = new DirectoryManager(appSettings, currentLocation);
            
        }
        
        public Logic(IOptions<ApplicationSettings> services)
        {
            _appSettings = services;
        }
        
        public bool GenerateCsvFile(string path)
        {
            var coinHistory = new LoadData(_appSettings, symbolId, period, currentLocation);
            if (!coinHistory.UploadHistoryFromServer()) return false;
            coinHistory.LoadToCsv(path);
            return true;
        }

        public bool GenerateCsvFileAuto(string path)
        {
            var coinHistory = new LoadData(_appSettings, symbolId, period, currentLocation);
            if (coinHistory.UploadHistoryFromServerAuto().Any())
            {              
                return coinHistory.LoadToCsv(path);
            }
            DirectoryManager.RemoveFolder(path);
            return false;
        }
        
        public void UpdateAseets()
        {
            var assets = new SymbolsUpdater(_appSettings);
            assets.UpdateAssetsInFile();
        }

        public IEnumerable<string> GetAllSymbols()
        {
            var assets = new SymbolsUpdater(_appSettings);
            return assets.ReadSymbolsFromFile();
        }

        public void PythonExecutor(string path, int periods, bool seasonalityHourly, bool seasonalityDaily)
        {
            var python = new PythonExec(path, periods, seasonalityHourly, seasonalityDaily, _appSettings.Value.PythonLocation);
            python.RunPython();
        }

        public CoinPerformance DefineTrend(IEnumerable<TableRow> table)
        {
            var indicator = new CoreOperations(_appSettings);
            return indicator.Indicatior(table);
        }
    }
}
