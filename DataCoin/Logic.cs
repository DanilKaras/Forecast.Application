using System;
using System.Collections.Generic;
using AymanMVCProject.Models;
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
        public Logic(IOptions<ApplicationSettings> services, string symbolId, int period)
        {
            path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            this.symbolId = symbolId;
            this.period = period;
            _services = services;
        }
        public Logic(IOptions<ApplicationSettings> services)
        {
            _services = services;
        }
        
        public void GenerateCsvFile()
        {
            var coinHistory = new LoadData(_services, symbolId, period);
            coinHistory.LoadHistory();
            coinHistory.LoadToCsv(path);
        }

        public void UpdateAseets()
        {
            var assets = new SymbolsUpdater(_services);
            assets.UpdateAssetsInFile();
        }

        public List<string> GetAllSymbols()
        {
            var assets = new SymbolsUpdater(_services);
            return assets.ReadSymbolsFromFile();
        }
    }
}
