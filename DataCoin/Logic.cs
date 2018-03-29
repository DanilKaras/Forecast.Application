using System;
using System.Collections.Generic;
using AymanMVCProject.Models;
using DataCoin.Models;
using DataCoin.Operations;
using DataCoin.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DataCoin
{
    public class Logic
    {
        private string symbolId;
        //private string path;
        private IOptions<ApplicationSettings> _appSettings;
        private int period;
        private string currentLocation;
        
        public Logic(IOptions<ApplicationSettings> appSettings, string symbolId, int period, string currentLocation)
        {
            
            this.symbolId = symbolId;
            this.period = period;
            _appSettings = appSettings;
            this.currentLocation = currentLocation; 
        }
        
        public Logic(IOptions<ApplicationSettings> services)
        {
            _appSettings = services;
        }
        
        public void GenerateCsvFile(string path)
        {
            var coinHistory = new LoadData(_appSettings, symbolId, period, currentLocation);
            coinHistory.UploadHistoryFromServer();
            coinHistory.LoadToCsv(path);
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

        public Indicator DefineTrend(IEnumerable<TableRow> table)
        {
            var indicator = new CoreOperations(_appSettings);
            return indicator.Indicatior(table);
        }
    }
}
