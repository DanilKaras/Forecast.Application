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
        private IOptions<ApplicationSettings> services;

        public CoreOperations(string apiUrl, string coinName, string currentLocation, IOptions<ApplicationSettings> services)
        {
            this.apiUrl = apiUrl;
            this.coinName = coinName;
            this.services = services;
            manager= new DirectoryManager(services, currentLocation); 
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
        
        public bool IsMissingData(int period, List<List<AssetModel>> model)
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
        
        private void Build(string url, string key, ref List<List<AssetModel>> modelSet)
        {

            var response = StaticUtility.GenerateRestUrl(url, key);
            var model = JsonConvert.DeserializeObject<List<AssetModel>>(response.Content);
            if (model.Any())
            {
                modelSet.Add(model);
            }
        }
        
        private string BuildCoinUrl(string url, string coinName, string dateStartStr, string dateEndStr)
        {
            return string.Format("{0}/ohlcv/{1}/history?period_id=1HRS&time_start={2}&time_end={3}",
                url,
                coinName,
                dateStartStr,
                dateEndStr);
        }
        
        private int CountArrElements(List<List<AssetModel>> model)
        {
            var counter = 0;
            if (model.Any())
            {
                foreach (var subSet in model)
                {
                    foreach (var coinRecord in subSet)
                    {
                        counter++;
                    }
                }
            }
            return counter;
        }
        
        private void RemoveExcess(int period, ref List<List<AssetModel>> model)
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
                    if (modelPosition <= modelElementsCount - period) 
                    {
                        model[i].RemoveRange(j, modelElementsCount - period);
                        stopFor = true;
                        break;
                    }        
                }
            }
        }
        
        private List<AssetModel> RemoveExcessFromEnd(int period,  List<List<AssetModel>> helpModel, List<List<AssetModel>> mainModel)
        {
            var modelPosition = 0;
            var getElementsCount = period - CountArrElements(mainModel);
            var tmpModel = new List<AssetModel>();
            helpModel.Reverse();
            var stopFor = false;
            var firstItemInModel = mainModel.First().First();
            
            try
            {
                for (int i = helpModel.Count; i > 0; i--)
                {
                    if(stopFor) break;
                    for (int j = helpModel[i-1].Count; j > 0; j--)
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
                    throw new Exception();
                }
            }
            else
            {
                throw new Exception();
            }
        }
    }
}