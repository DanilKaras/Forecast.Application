using System.Collections.Generic;
using DataCoin.Models;
using Microsoft.AspNetCore.ApplicationInsights.HostingStartup;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AymanMVCProject.Models
{
    public class MainViewModel
    {
        public IEnumerable<TableRow> Table { get; set; }
        public string ComponentsPath { get; set; }
        public string ForecastPath { get; set; }
        public string AssetName { get; set; }
        public int RequestsPerDay { get; set; }
        public Indicator Indicator { get; set; }
    }
}