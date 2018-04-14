using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Schema;
using DataCoin.Models;
using DataCoin.Operations;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.FormulaParsing.Utilities;
using RestSharp;

namespace DataCoin.Utility
{
    public static class StaticUtility
    {
        private static readonly object locker;
        private static int requestCounter;
        private static List<ExcelLog> log;
        private static readonly string hiddenVal;
        private static readonly string logName;
        private static readonly NumberFormatInfo numFormat; //= new CultureInfo( "en-US", false ).NumberFormat;
        //nfi.PercentDecimalDigits = 3;
            
        public static int RequestCounter
        {
            get
            {
                lock(locker)
                {
                    return requestCounter;
                }
            }
            set
            {
                lock(locker)
                {
                    requestCounter = value;
                }
            }
        }

        public static string LogName => logName;
        
        static StaticUtility()
        {
            locker = new object();
            requestCounter = 0;
            log = new List<ExcelLog>();    
            logName = "AssetLog.xlsx";
            numFormat = new CultureInfo("en-US", false ).NumberFormat;
            numFormat.PercentDecimalDigits = 3;
        }
        
        public static IRestResponse GenerateRestUrl(string url, string apiKey)
        {
            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);

            request.AddHeader("X-CoinAPI-Key", apiKey);
            var response = client.Execute(request);

            return response;
        } 
        
        
        public static DateTime TimeConverter(string time)
        {
            return time.IndexOf('.') != -1 ? DateTime.Parse(time.Remove(time.IndexOf('.'), time.Length - time.IndexOf('.'))) : DateTime.MinValue;
        }

        private static int FindPosition(IEnumerable<string> array, string colName)
        {
            var pos = -1;
            foreach (var item in array)
            {
                pos++;
                if (item == colName)
                {
                    return pos;
                }
            }
            return 0;
        }
        
        public static async Task<bool> WaitForFile(string path, int timeout)
        {
            var timeoutAt = DateTime.Now.AddSeconds(timeout);
            while (true)
            {
                if (File.Exists(path)) return true;
                if (DateTime.Now >= timeoutAt) return false;
                await Task.Delay(10);
            }
        }

        private static string TruncateLongString(string str, int len)
        {
            return str.Substring(0, Math.Min(str.Length, len + 2));
        }
        
        public static IEnumerable<TableRow> BuildOutTableRows(string path, int period)
        {
            var table = new List<TableRow>();
            using (var reader = new StreamReader(File.OpenRead($"{path}")))
            {
                var counter = 0;

                var dsPos = 0;
                var yhatPos = 0;
                var yhatUpperPos = 0;
                var yhatLowerPos = 0;
                while (!reader.EndOfStream)
                {
                    counter++;
                    var line = reader.ReadLine();

                    if (line == null) continue;
                    var values = line.Split(',');
                    if (counter == 1)
                    {
                        dsPos = FindPosition(values, "ds");
                        yhatPos = FindPosition(values, "yhat");
                        yhatUpperPos = FindPosition(values, "yhat_upper");
                        yhatLowerPos = FindPosition(values, "yhat_lower");
                    }
                    else
                    {
                        var row = new TableRow()
                        {
                            ID = values[0],
                            DS = values[dsPos],
                            Yhat = decimal.Parse(values[yhatPos], NumberStyles.Any, CultureInfo.InvariantCulture),//(TruncateLongString(values[yhatPos], 7)),
                            YhatUpper = decimal.Parse(values[yhatUpperPos], NumberStyles.Any, CultureInfo.InvariantCulture),//(TruncateLongString(values[yhatUpperPos], 7)),
                            YhatLower = decimal.Parse(values[yhatLowerPos], NumberStyles.Any, CultureInfo.InvariantCulture) //(TruncateLongString(values[yhatLowerPos], 7))
                        };
                        table.Add(row);
                    }
                }
            }

            var test = table.Take(table.Count - period).ToList();
            var max = table.Take(table.Count - period).Select(x => x.Yhat).Max();
            var min = table.Take(table.Count - period).Select(x => x.Yhat).Min();
            var returnTable = table.Skip(Math.Max(0, table.Count() - period)).Reverse().ToList();
            returnTable.First().MaxVal = max;
            returnTable.First().MinVal = min;
            return table.Skip(Math.Max(0, table.Count() - period)).Reverse().ToList();
        }

        

        public static void Log(string assetName, Indicator result, decimal rate)
        {
            lock (locker)
            {
                log.Add(new ExcelLog()
                {
                    AssetName = assetName,
                    Log = result.ToString(),
                    Rate = rate.ToString()
                });             
            }
        }

        public static void AddRequestCount(int number)
        {
            lock(locker)
            {
                requestCounter += number;
            }
        }

        public static void WriteLogExcel(string path)
        {
            // Create the file using the FileInfo object
            
            var file = new FileInfo(Path.Combine(path, logName));
            var query = (from p in log
                         group p by p.Log into g
                         select new { key = g.Key, list = g.Select(x=> new {Asset = x.AssetName, Rate = x.Rate}).ToList() }).ToList();


            var positiveGroup = query.Where(x => x.key == Indicator.Positive.ToString()).Select(x => x).SingleOrDefault();
            var neutralGroup = query.Where(x => x.key == Indicator.Neutral.ToString()).Select(x => x).SingleOrDefault();
            var negativeGroup = query.Where(x => x.key == Indicator.Negative.ToString()).Select(x => x).SingleOrDefault();
            var zeroGroup = query.Where(x =>x.key == Indicator.ZeroRezults.ToString()).Select(x => x).SingleOrDefault();
            var strongPositiveGroup = query.Where(x =>x.key == Indicator.StrongPositive.ToString()).Select(x => x).SingleOrDefault();          
            
            var sortedLog = new List<ExcelLog>();
            
            if (strongPositiveGroup != null)
            {
                var strongPositive = strongPositiveGroup.list.Select(x => new { x.Asset, x.Rate }).OrderByDescending(x => x.Rate).ToList();
                foreach (var item in strongPositive)
                {
                    sortedLog.Add(new ExcelLog(){AssetName = item.Asset, Rate = Convert.ToDouble(item.Rate).ToString("P", numFormat), Log = Indicator.StrongPositive.ToString()});
                }
            }
            
            if (positiveGroup != null)
            {
                var positive = positiveGroup.list.Select(x => new { x.Asset, x.Rate }).OrderByDescending(x => Convert.ToDecimal(x.Rate)).ToList();
                foreach (var item in positive)
                {
                    sortedLog.Add(new ExcelLog(){AssetName = item.Asset, Rate = Convert.ToDouble(item.Rate).ToString("P", numFormat), Log = Indicator.Positive.ToString()});
                }
            }

            if (neutralGroup != null)
            {
                var neutral = neutralGroup.list.Select(x => new { x.Asset, x.Rate }).OrderBy(x => Convert.ToDecimal(x.Rate)).ToList();
                foreach (var item in neutral)
                {
                    sortedLog.Add(new ExcelLog(){AssetName = item.Asset, Rate = Convert.ToDouble(item.Rate).ToString("P", numFormat), Log = Indicator.Neutral.ToString()});
                }
            }

            if (negativeGroup != null)
            {
                var negative = negativeGroup.list.Select(x => new { x.Asset, x.Rate }).OrderBy(x => Convert.ToDecimal(x.Rate)).ToList();
                foreach (var item in negative)
                {
                    sortedLog.Add(new ExcelLog(){AssetName = item.Asset, Rate = Convert.ToDouble(item.Rate).ToString("P", numFormat), Log = Indicator.Negative.ToString()});
                }
            }

            if (zeroGroup != null)
            {
                var zero = zeroGroup.list.Select(x => new { x.Asset, x.Rate }).ToList();
                foreach (var item in zero)
                {
                    sortedLog.Add(new ExcelLog(){AssetName = item.Asset, Rate = "Unknown", Log = Indicator.ZeroRezults.ToString()});
                }
            }

            var manager = new DirectoryManager();
            
            DirectoryManager.WriteLogToExcel(file, sortedLog);

            ClearLog();
        }

        private static void ClearLog()
        {
            lock (locker)
            {
                log.Clear();
            }
        }
        
    }
}