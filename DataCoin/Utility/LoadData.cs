using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using System.Linq;
using AymanMVCProject.Models;
using DataCoin.Models;
using Microsoft.Extensions.Options;

namespace DataCoin.Utility
{
    public class LoadData
    {
        private List<List<AssetModel>> modelSet;
        readonly DateTime dateStart;
        readonly DateTime dateEnd;
        readonly string coinName;
        private int period;
        private readonly string url;
        private readonly string key;
        private IOptions<ApplicationSettings> _service;

        public LoadData(IOptions<ApplicationSettings> service, string coinName, int period)
        {
            modelSet = new List<List<AssetModel>>();
            _service = service;
            url = _service.Value.CoinApiUrl;
            key = _service.Value.ApiKey;
            this.period = period;
            this.coinName = coinName;
            dateEnd = DateTime.Now;
            dateStart = DateTime.Now.AddHours(-1 * period);
        }

        public void LoadHistory()
        {
            var dtMin = dateStart;
            var dtMax = dateEnd;

            while (dtMin < dtMax.AddDays(4))
            {
                var dateStartStr = dtMin.ToString("s");
                var dateEndStr = dtMin.AddDays(4).ToString("s");

                string ulr = string.Format("{0}/ohlcv/{1}/history?period_id=1HRS&time_start={2}&time_end={3}",
                                           url,
                                           coinName,
                                           dateStartStr,
                                           dateEndStr);
                Build(ulr);
                dtMin = dtMin.AddDays(4);
            }


            if (IsMissingData(modelSet))
            {
                FillMissingData(modelSet);
            }
        }

        public void LoadToCsv(string path)
        {   
            var csv = new StringBuilder();
            var fs = "Time";
            var sc = "avg";
                
            var newLine1 = $"{fs},{sc}{Environment.NewLine}";
            csv.Append(newLine1);
            int counter = 0;
            int maxcount = 0;
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
                    if (item.TimeClose.IndexOf('.') != -1)
                    {
                        item.TimeClose = item.TimeClose.Remove(item.TimeClose.IndexOf('.'),
                            item.TimeClose.Length - item.TimeClose.IndexOf('.'));
                    }
                    
                    DateTime d2 = DateTime.Parse(item.TimeClose);
                    d2 = d2.AddHours(-4);
                    var formattedDate = d2.ToString("u").Replace("Z", "") + " UTC";
                    var avg = (item.PriceClose + item.PriceHigh + item.PriceLow) / 3;
                    
                    var second = avg.ToString();
                    string newLine = null;
                    newLine = counter < maxcount ? $"{formattedDate},{second}{Environment.NewLine}" : $"{formattedDate},{second}";

                    csv.Append(newLine);
                }

            }
            var generatedName = coinName + "_" + DateTime.Now.ToString("T").Replace(':', '_') + ".csv";
            var fileName = Path.Combine(path, generatedName);
            File.WriteAllText(fileName, csv.ToString());
        }
        
        private void Build(string url)
        {

            var response = StaticUtility.GenerateRestUrl(url, key);
            var model = JsonConvert.DeserializeObject<List<AssetModel>>(response.Content);
            if (model.Any())
            {
                modelSet.Add(model);
            }
        }

        private List<AssetModel> FillModel(string url)
        {
            var response = StaticUtility.GenerateRestUrl(url, key);
            var model = JsonConvert.DeserializeObject<List<AssetModel>>(response.Content);
            return model.Any() ? model : null;
        }
        
        private bool IsMissingData(List<List<AssetModel>> model)
        {
            return CountArrElements(model) != period;
        }

        private void FillMissingData(List<List<AssetModel>> model)
        {
            if (CountArrElements(model) > period)
            {
                RemoveRest(ref modelSet);
            }
            else if (CountArrElements(model) < period)
            {
                var difference = period - CountArrElements(model);
                var requestsCount = Convert.ToInt32(difference / 100) + 1;
                var tmpDate = dateStart;
                var tmpCounterfForModel = 0;
                var tmpModelSet = new List<List<AssetModel>>();
                
                while (requestsCount != 0)
                {
                    var dtMax = tmpDate;
                    var dtMin = dtMax.AddDays(-4);
                    
                    string tmpUrl = string.Format("{0}/ohlcv/{1}/history?period_id=1HRS&time_start={2}&time_end={3}",
                        url,
                        coinName,
                        dtMin.ToString("s"),
                        dtMax.ToString("s"));
                    tmpModelSet.Add(FillModel(tmpUrl));
                    tmpDate = tmpDate.AddDays(-4);
                    requestsCount--;
                }

                if (tmpModelSet.Any())
                {
                    tmpCounterfForModel = CountArrElements(tmpModelSet);
                    if (tmpCounterfForModel > difference)
                    {
                        modelSet.Insert(0, (RemoveRestFromEnd(tmpModelSet)));
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

        private void RemoveRest(ref List<List<AssetModel>> model)
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
                    if (modelPosition <= period) continue;
                        
                    model[i].RemoveRange(j, modelElementsCount - period);
                    stopFor = true;
                    break;
                }
            }
        }

        private List<AssetModel> RemoveRestFromEnd(List<List<AssetModel>> model)
        {
            var modelPosition = 0;
            var modelElementsCount = CountArrElements(model);
            var getElementsCount = period - CountArrElements(modelSet);
            var tmpModel = new List<AssetModel>();
            model.Reverse();
            var stopFor = false;

            try
            {
                for (int i = model.Count; i > 0; i--)
                {
                    if(stopFor) break;
                    for (int j = model[i-1].Count; j > 0; j--)
                    {
                        tmpModel.Add(model[i-1][j-1]);                    
                        modelPosition++;
                        if (modelPosition >= getElementsCount)
                        {
                            stopFor = true;
                            break;
                        }
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
    }
}
