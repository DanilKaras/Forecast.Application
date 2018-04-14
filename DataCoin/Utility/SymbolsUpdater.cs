using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using AymanMVCProject.Models;
using DataCoin.Models;
using DataCoin.Operations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DataCoin.Utility
{
    public class SymbolsUpdater
    {
        private readonly string fileName;
        private readonly string currentPath;
        private readonly string filePath;
        private readonly string url;
        private readonly string key;
        private readonly IOptions<ApplicationSettings> _service;
        private readonly DirectoryManager directoryManager;
        public SymbolsUpdater(IOptions<ApplicationSettings> service)
        {
            _service = service;
            fileName = _service.Value.FileName;
            url = _service.Value.CoinApiUrl;
            key = _service.Value.ApiKey;
            currentPath = Directory.GetCurrentDirectory();
            filePath = Path.Combine(currentPath, fileName);
            directoryManager = new DirectoryManager();
        }
        
        public void UpdateAssetsInFile()
        {
            
            var requestString = url + "/symbols";
            var response = StaticUtility.GenerateRestUrl(requestString, key);
            
            var model = JsonConvert.DeserializeObject<List<SymbolModel>>(response.Content);

            var assetList = ReadAssets(model);
            
            if (!assetList.Any())
            {
                throw new Exception("No assets has been found");
            }
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            directoryManager.WriteAssetsToExcel(filePath, assetList);
            //File.AppendAllText(filePath, stringToFile);
        }

        
        public IEnumerable<string> ReadSymbolsFromFile()
        {
            if (!File.Exists(filePath)) return null;

            var readText = DirectoryManager.ReadAssetsFromExcel(filePath);//File.ReadAllText(filePath);
            var strArray = readText.ToList();
            return strArray;
        }
        
        private IEnumerable<string> ReadAssets(IEnumerable<SymbolModel> model)
        {
            var quote = _service.Value.Exchange.ToLower();
            var quoteId = _service.Value.Currency.ToLower();
            if (quote == "all")
            {
                return model.Where(x => x.AssetIdQuote.ToLower() == quoteId).Select(x => x.SymbolId).ToList();
            }
            
            return model.Where(x => x.ExchangeId.ToLower().Contains(quote) && x.AssetIdQuote.ToLower() == quoteId).Select(x => x.SymbolId).ToList();
            
            
        }
    }
}