using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataCoin.Models;
using OfficeOpenXml;
using RestSharp;

namespace DataCoin.Utility
{
    public static class StaticUtility
    {
        public static IRestResponse GenerateRestUrl(string url, string apiKey)
        {
            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);

            request.AddHeader("X-CoinAPI-Key", apiKey);
            var response = client.Execute(request);
            if (response.StatusDescription != "OK")
            {
                throw new Exception(response.StatusDescription);
            }
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
                if (System.IO.File.Exists(path)) return true;
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
                        dsPos = StaticUtility.FindPosition(values, "ds");
                        yhatPos = StaticUtility.FindPosition(values, "yhat");
                        yhatUpperPos = StaticUtility.FindPosition(values, "yhat_upper");
                        yhatLowerPos = StaticUtility.FindPosition(values, "yhat_lower");
                    }
                    else
                    {
                        var row = new TableRow()
                        {
                            ID = values[0],
                            DS = values[dsPos],
                            Yhat = StaticUtility.TruncateLongString(values[yhatPos], 7),
                            YhatUpper = StaticUtility.TruncateLongString(values[yhatUpperPos], 7),
                            YhatLower = StaticUtility.TruncateLongString(values[yhatLowerPos], 7)
                        };
                        table.Add(row);
                    }
                }
            }

            return table.Skip(Math.Max(0, table.Count() - period)).Reverse();
        }

        public static List<string> ReadFromExcel(string path)
        {
            var file = new FileInfo(path);
            var rawText = new List<string>();
            using (var package = new ExcelPackage(file))
            {       
                var worksheet = package.Workbook.Worksheets[1];
                var rowCount = worksheet.Dimension.Rows;
                var colCount = worksheet.Dimension.Columns;           
                for (var row = 1; row <= rowCount; row++)
                {
                    for (var col = 1; col <= colCount; col++)
                    {   
                        rawText.Add(worksheet.Cells[row, col].Value.ToString());    
                    }
                    
                }
            }
            return rawText;
        }
    }
}