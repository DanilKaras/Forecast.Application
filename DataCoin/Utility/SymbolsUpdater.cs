using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using AymanMVCProject.Models;
using DataCoin.Models;
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

        public SymbolsUpdater(IOptions<ApplicationSettings> service)
        {
            _service = service;
            fileName = _service.Value.FileName;
            url = _service.Value.CoinApiUrl;
            key = _service.Value.ApiKey;
            currentPath = System.IO.Directory.GetCurrentDirectory();
            filePath = Path.Combine(currentPath, fileName);
        }
        
        public void UpdateAssetsInFile()
        {
            var requestString = url + "/symbols";
            var response = StaticUtility.GenerateRestUrl(requestString, key);
            
            var model = JsonConvert.DeserializeObject<List<SymbolModel>>(response.Content);

            var stringToFile = GenerateString(model);
           
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            
            File.AppendAllText(filePath, stringToFile);
        }

        
        public List<string> ReadSymbolsFromFile()
        {
            string readText = File.ReadAllText(filePath);
            var strArray = readText.Split(',').ToList();
            return strArray;
        }
        
        private string GenerateString(List<SymbolModel> model)
        {
            var onlySymbols = model.Where(x => x.AssetIdQuote == "BTC").Select(x => x.SymbolId).ToList();

            var sb = new StringBuilder();

            foreach (var symbol in onlySymbols)
            {
                sb.Append(symbol).Append(",");
            }
            
            return sb.Remove(sb.Length - 1, 1).ToString();
        }
    }
}