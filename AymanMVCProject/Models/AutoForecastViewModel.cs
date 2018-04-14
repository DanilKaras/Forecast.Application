using System.Collections.Generic;
using DataCoin.Models;

namespace AymanMVCProject.Models
{
    public class AutoForecastViewModel
    {
        public List<string> PositiveAssets { get; set; }
        public List<string> NeutralAssets { get; set; }
        public List<string> NegativeAssets { get; set; }
        public List<string> StrongPositiveAssets { get; set; }
        public List<ExcelLog> Report { get; set; }
        public int RequestCount { get; set; }
    }
}