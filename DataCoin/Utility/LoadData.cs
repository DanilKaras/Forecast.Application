using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using System.Linq;
using AymanMVCProject.Models;
using DataCoin.Models;
using DataCoin.Operations;
using Microsoft.Extensions.Options;

namespace DataCoin.Utility
{
    public class LoadData
    {
        private List<List<AssetModel>> modelSet;
        readonly DateTime dateStart;
        readonly DateTime dateEnd;
        readonly string coinName;
        private readonly int period;
        private readonly string url;
        private readonly string key;
        private readonly CoreOperations operations;
        private readonly string fileName;
        private IOptions<ApplicationSettings> _service;
        private DirectoryManager manager;
        
        public LoadData(IOptions<ApplicationSettings> service, string coinName, int period, string currentLocation)
        {
            _service = service;
            
            modelSet = new List<List<AssetModel>>();
            url = _service.Value.CoinApiUrl;
            key = _service.Value.ApiKey;
            this.period = period;
            this.coinName = coinName;
            dateEnd = DateTime.Now;
            dateStart = DateTime.Now.AddHours(-1 * period);
            operations = new CoreOperations(url, coinName, currentLocation, service);
            manager = new DirectoryManager(service, currentLocation);
            fileName = "data.csv";
        }

        public void UploadHistoryFromServer()
        {
            var dtMin = dateStart;
            var dtMax = dateEnd;
            operations.FillModel(url, dtMin, dtMax, key, ref modelSet);

            var isSomethingMissing = CoreOperations.IsMissingData(period, modelSet);
            
            if (isSomethingMissing)
            {
                operations.FillMissingData(period, dateStart, key, ref modelSet);
            }
        }

        public void LoadToCsv(string path)
        {   
            var csv = new StringBuilder();
            const string fs = "Time";
            const string sc = "avg";    
            var newLine1 = $"{fs},{sc}{Environment.NewLine}";
            
            var location = manager.GenerateForecastFolder(coinName, period);
            
            csv.Append(newLine1);
            var counter = 0;
            var maxcount = 0;
            if (modelSet.Count == 0)
            {
                throw new Exception("File Is Empty");
            }
            foreach (var model in modelSet)
            {
                foreach (var item in model)
                {
                    maxcount++;
                }
            }
            foreach (var node in modelSet)
            {
                foreach (var item in node)
                {
                    counter++;
             
                    var d2 = StaticUtility.TimeConverter(item.TimeClose).ToLocalTime();

                    var formattedDate = d2.ToString("u").Replace("Z", "");// + " UTC";
                    var avg = (item.PriceClose + item.PriceHigh + item.PriceLow) / 3;
                    
                    var second = avg.ToString(CultureInfo.CurrentCulture);
                    var newLine = counter < maxcount ? $"{formattedDate},{second}{Environment.NewLine}" : $"{formattedDate},{second}";
                    csv.Append(newLine);
                }
            }

            var saveTo = Path.Combine(location, fileName);
            
            File.WriteAllText(saveTo, csv.ToString());
        }
    }
}
