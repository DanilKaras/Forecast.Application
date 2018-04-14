using DataCoin.Models;

namespace AymanMVCProject.Models
{
    public class InstantForecastModal
    {
        public string AssetName { get; set; }
        public string Rate { get; set; }
        public Indicator Indicator { get; set; }
        public string ForecastPath { get; set; }
    }
}