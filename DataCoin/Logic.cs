﻿using System;
using System.Collections.Generic;
using AymanMVCProject.Models;
using DataCoin.Operations;
using DataCoin.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DataCoin
{
    public class Logic
    {
        private string symbolId;
        private string path;
        private IOptions<ApplicationSettings> _services;
        private int period;
        private string currentLocation;
        public Logic(IOptions<ApplicationSettings> services, string symbolId, int period, string currentLocation)
        {
            path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            this.symbolId = symbolId;
            this.period = period;
            _services = services;
            this.currentLocation = currentLocation; 
        }
        public Logic(IOptions<ApplicationSettings> services)
        {
            _services = services;
        }


//        public Logic(IOptions<ApplicationSettings> services, int period)
//        {
//            _services = services;
//            this.period = period;
//        }
        
        public void GenerateCsvFile()
        {
            var coinHistory = new LoadData(_services, symbolId, period, currentLocation);
            coinHistory.UploadHistoryFromServer();
            coinHistory.LoadToCsv(path);
        }

        public void UpdateAseets()
        {
            var assets = new SymbolsUpdater(_services);
            assets.UpdateAssetsInFile();
        }

        public IEnumerable<string> GetAllSymbols()
        {
            var assets = new SymbolsUpdater(_services);
            return assets.ReadSymbolsFromFile();
        }

        public void PythonExecutor(string path, int periods,  bool seasonalityHourly, bool seasonalityDaily)
        {
            var python = new PythonExec(path, periods, seasonalityHourly, seasonalityDaily, _services.Value.PythonLocation);
            python.RunPython();
        }
    }
}
