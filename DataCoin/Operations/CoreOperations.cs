using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        
        public bool FillModel(string url, DateTime dtMin, DateTime dtMax, string apiKey, ref List<List<AssetModel>> modelSet, DirSwitcher switcher)
        {
            var counter = 0;
            while (dtMin < dtMax.AddDays(4))
            {
                var dateStartStr = dtMin.ToString("s");
                var dateEndStr = dtMin.AddDays(4).ToString("s");
                var coinUrl = BuildCoinUrl(url, coinName, dateStartStr, dateEndStr);
                switch (switcher)
                {
                    case DirSwitcher.Auto:
                        if (!Build(coinUrl, apiKey, ref modelSet, switcher))
                        {
                            return false;
                        }
                        break;
                    case DirSwitcher.Manual:
                        Build(coinUrl, apiKey, ref modelSet, switcher);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(switcher), switcher, null);
                }
                
                
                dtMin = dtMin.AddDays(4);
                counter++;
            }

            StaticUtility.AddRequestCount(counter);
            return true;
            //manager.UpdateRequests(counter);

        }
        
        public static bool IsMissingData(int period, List<List<AssetModel>> model)
        {
            return CountArrElements(model) != period;
        }
        
        public bool FillMissingData(int period, DateTime startDate, string apiKey, ref List<List<AssetModel>> model, DirSwitcher switcher)
        {
            if (CountArrElements(model) > period)
            {
                RemoveExcess(period, ref model);
                return true;
            }
            if (CountArrElements(model) < period)
            {
                return FillGaps(period, startDate, apiKey, ref model, switcher);
            }

            return false;
        }
        
        private static bool Build(string url, string key, ref List<List<AssetModel>> modelSet, DirSwitcher switcher)
        {

            var response = StaticUtility.GenerateRestUrl(url, key);
            List<AssetModel> model = null;
            
            switch(switcher)
                {
                    case DirSwitcher.Auto:
                        if (response.StatusDescription == "OK")
                        {
                            model = JsonConvert.DeserializeObject<List<AssetModel>>(response.Content);
                            if (model.Any())
                            {
                                modelSet.Add(model);
                            }
                        }
                        else
                        {
                            return false;
                        }
                        break;
                    
                    case DirSwitcher.Manual:
                        if (response.StatusDescription != "OK")
                        {
                            throw new Exception(response.Content);
                        }
                        model = JsonConvert.DeserializeObject<List<AssetModel>>(response.Content);
                        if (model.Any())
                        {
                            modelSet.Add(model);
                            return true;
                        }
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException(nameof(switcher), switcher, null);
                }

            return true;
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
        
        private bool FillGaps(int period, DateTime dateStart, string apiKey, ref List<List<AssetModel>> model, DirSwitcher switcher)
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
                switch (switcher)
                {
                    case DirSwitcher.Auto:
                        if (!Build(url, apiKey, ref tmpModelSet, switcher))
                        {
                            return false;
                        }
                        break;
                    case DirSwitcher.Manual:
                        Build(url, apiKey, ref tmpModelSet, switcher);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(switcher), switcher, null);
                }                
                tmpDate = tmpDate.AddDays(-4);
                requestsCount--;
            }
           
           

            StaticUtility.AddRequestCount(counter);
            //manager.UpdateRequests(counter);

            if (!tmpModelSet.Any()) return false;
            
            var tmpCounterfForModel = CountArrElements(tmpModelSet);
            if (tmpCounterfForModel <= difference) return false;
            model.Insert(0, (RemoveExcessFromEnd(period, tmpModelSet, model)));
            return true;

        }
        
        public CoinPerformance Indicatior(IEnumerable<TableRow> table)
        {
            var tableRows = table.ToList();
            var result = new CoinPerformance();
            
            if (!decimal.TryParse(appSettings.Value.Border, out var border))
            {
                throw new Exception("Wrong Value of Border in App Settings!");
            }
            
            if (!decimal.TryParse(appSettings.Value.BorderUp, out var borderUp))
            {
                throw new Exception("Wrong Value of BorderUp in App Settings!");
            }

            var upper = tableRows.First().Yhat;
            var lower = tableRows.Last().Yhat;
            var max = tableRows.First().MaxVal;
            var min = tableRows.First().MinVal;

           if (max < upper)
            {
                max = upper;
            }

            if (min > lower)
            {
                min = lower;
            }
            
            decimal length;
            if (upper - lower > 0)
            {
                length =  1 / (max - min) * (upper - lower) * 100;
            }
            else if(upper - lower < 0)
            {
                length = -1 * (1 / (max - min) * (lower - upper) * 100);
            }
            else
            {
                result.Indicator = Indicator.Neutral;
                result.Rate = 0;
                return result;
            }

            
            if (length > borderUp)
            {
                result.Indicator = Indicator.StrongPositive;
                result.Rate = length/100;
                return result;
            }
            
            if (length > 0 && length <= borderUp)
            {
                result.Indicator = Indicator.Positive;
                result.Rate = length/100;
                return result;
            }
            
            if (length < 0 && border < length)
            {
                result.Indicator = Indicator.Neutral;
                result.Rate = -1 * length/100;
                return result;
            }
            
            result.Indicator = Indicator.Negative;
            result.Rate =  - 1 * length / 100;
            return result;
            
        }
    }
}