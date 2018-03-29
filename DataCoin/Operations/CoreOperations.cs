using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using AymanMVCProject.Models;
using DataCoin.Models;
using DataCoin.Utility;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DataCoin.Operations
{
    public class CoreOperations
    {
        private readonly string apiUrl;
        private readonly string coinName;
        private readonly DirectoryManager manager;
        private IOptions<ApplicationSettings> appSettings;

        public CoreOperations(string apiUrl, string coinName, string currentLocation, IOptions<ApplicationSettings> appSettings)
        {
            this.apiUrl = apiUrl;
            this.coinName = coinName;
            this.appSettings = appSettings;
            manager= new DirectoryManager(appSettings, currentLocation); 
        }

        public CoreOperations(IOptions<ApplicationSettings> appSettings)
        {
            this.appSettings = appSettings;
        }
        
        public void FillModel(string url, DateTime dtMin, DateTime dtMax, string apiKey, ref List<List<AssetModel>> modelSet)
        {
            var counter = 0;
            while (dtMin < dtMax.AddDays(4))
            {
                var dateStartStr = dtMin.ToString("s");
                var dateEndStr = dtMin.AddDays(4).ToString("s");
                var coinUrl = BuildCoinUrl(url, coinName, dateStartStr, dateEndStr);
                
                Build(coinUrl, apiKey, ref modelSet);
                
                dtMin = dtMin.AddDays(4);
                counter++;
            }

            manager.UpdateRequests(counter);

        }
        
        public static bool IsMissingData(int period, List<List<AssetModel>> model)
        {
            return CountArrElements(model) != period;
        }
        
        public void FillMissingData(int period, DateTime startDate, string apiKey, ref List<List<AssetModel>> model)
        {
            if (CountArrElements(model) > period)
            {
                RemoveExcess(period, ref model);
            }
            else if (CountArrElements(model) < period)
            {
                FillGaps(period, startDate, apiKey, ref model);
            }
        }
        
        private static void Build(string url, string key, ref List<List<AssetModel>> modelSet)
        {

            var response = StaticUtility.GenerateRestUrl(url, key);
            var model = JsonConvert.DeserializeObject<List<AssetModel>>(response.Content);
            if (model.Any())
            {
                modelSet.Add(model);
            }
        }
        
        private static string BuildCoinUrl(string url, string coinName, string dateStartStr, string dateEndStr)
        {
            return string.Format("{0}/ohlcv/{1}/history?period_id=1HRS&time_start={2}&time_end={3}",
                url,
                coinName,
                dateStartStr,
                dateEndStr);
        }
        
        private static int CountArrElements(List<List<AssetModel>> model)
        {
            var counter = 0;
            
            if (!model.Any()) return counter;
            
            foreach (var subSet in model)
            {
                foreach (var coinRecord in subSet)
                {
                    counter++;
                }
            }
            return counter;
        }
        
        private static void RemoveExcess(int period, ref List<List<AssetModel>> model)
        {
            var modelPosition = 0;
            var modelElementsCount = CountArrElements(model);
            var stopFor = false;
            for (var i = 0; i < model.Count; i++)
            {
                if(stopFor) break;
                    
                for (var j = 0; j < model[i].Count; j++)
                {
                    modelPosition++;
                    if (modelPosition > modelElementsCount - period) continue;
                    model[i].RemoveRange(j, modelElementsCount - period);
                    stopFor = true;
                    break;
                }
            }
        }
        
        private static List<AssetModel> RemoveExcessFromEnd(int period,  List<List<AssetModel>> helpModel, List<List<AssetModel>> mainModel)
        {
            var modelPosition = 0;
            var getElementsCount = period - CountArrElements(mainModel);
            var tmpModel = new List<AssetModel>();
            helpModel.Reverse();
            var stopFor = false;
            var firstItemInModel = mainModel.First().First();
            
            try
            {
                for (var i = helpModel.Count; i > 0; i--)
                {
                    if(stopFor) break;
                    for (var j = helpModel[i-1].Count; j > 0; j--)
                    {
                        if (modelPosition >= getElementsCount)
                        {
                            stopFor = true;
                            break;
                        }
                        tmpModel.Add(helpModel[i-1][j-1]);                    
                        modelPosition++;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
           
            tmpModel.Reverse();
            return tmpModel;
        }
        
        private void FillGaps(int period, DateTime dateStart, string apiKey, ref List<List<AssetModel>> model)
        {
            var difference = period - CountArrElements(model);
            var requestsCount = Convert.ToInt32(difference / 100) + 1;
            var tmpDate = dateStart;
            var tmpModelSet = new List<List<AssetModel>>();
            var counter = 0;    
            while (requestsCount != 0)
            {
                counter++;
                var firstItemInModel = StaticUtility.TimeConverter(model.First().First().TimeClose);
                var dtMax = tmpDate;
                
                if (firstItemInModel.Subtract(tmpDate).TotalMinutes > 60)
                {
                    dtMax = dtMax.AddHours(1);
                }
                var dtMin = dtMax.AddDays(-4);
                
                var url = BuildCoinUrl(apiUrl, coinName, dtMin.ToString("s"), dtMax.ToString("s"));
                Build(url, apiKey, ref tmpModelSet);
                tmpDate = tmpDate.AddDays(-4);
                requestsCount--;
            }

            manager.UpdateRequests(counter);
            
            if (tmpModelSet.Any())
            {
                var tmpCounterfForModel = CountArrElements(tmpModelSet);
                if (tmpCounterfForModel > difference)
                {
                    model.Insert(0, (RemoveExcessFromEnd(period, tmpModelSet, model)));
                }
                else
                {
                    throw new Exception("Something's wrong with a coin");
                }
            }
            else
            {
                throw new Exception("Something's wrong with a coin");
            }
        }
        
        public Indicator Indicatior(IEnumerable<TableRow> table)
        {
            var tableRows = table.ToList();

            if (!decimal.TryParse(appSettings.Value.Border, out var border))
            {
                throw new Exception("Wrong Value of Border in App Settings!");
            }
            
            var lowest = Convert.ToDecimal(tableRows.Last().Yhat);
            var highest = Convert.ToDecimal(tableRows.First().Yhat);
            var differense = (1 - 1 / (highest / lowest)) * 100;
            
            if (differense > 0)
            {
                return Indicator.Positive;
            }

            if (border < differense && differense <= 0)
            {
                return Indicator.Neutral;
            }

            return Indicator.Negative;
        }
    }
}