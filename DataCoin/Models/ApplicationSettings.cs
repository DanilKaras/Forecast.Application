﻿using System.Dynamic;

namespace AymanMVCProject.Models
{
    public class ApplicationSettings
    {
        public string ApiKey { get; set; }
        public string CoinApiUrl { get; set; }
        public string FileName { get; set; }
        public string ForecastDir { get; set; }
        public string RootFolder { get; set; }
        public string PythonLocation { get; set; }
        public string Border { get; set; }

        public string BorderUp { get; set; }
        public string AssetFile { get; set; }
        public string CounterFile { get; set; }
        public string ManualFolder { get; set; }
        public string AutoFolder { get; set; }
        public string Exchange { get; set; }
        public string Currency { get; set; }
        public string InstantForecast { get; set; }
        public string InstantFolder { get; set; }
    }
}