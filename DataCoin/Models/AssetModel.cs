using Newtonsoft.Json;
namespace DataCoin.Models
{
    public class AssetModel
    {
   
        [JsonProperty(PropertyName = "time_period_start")]
        public string TimePeriodStart { get; set; }

        [JsonProperty(PropertyName = "time_period_end")]
        public string TimePeriodEnd { get; set; }

        [JsonProperty(PropertyName = "time_open")]
        public string TimeOpen { get; set; }

        [JsonProperty(PropertyName = "time_close")]
        public string TimeClose { get; set; }

        [JsonProperty(PropertyName = "price_open")]
        public decimal PriceOpen { get; set; }

        [JsonProperty(PropertyName = "price_high")]
        public decimal PriceHigh { get; set; }

        [JsonProperty(PropertyName = "price_low")]
        public decimal PriceLow { get; set; }

        [JsonProperty(PropertyName = "price_close")]
        public decimal PriceClose { get; set; }

        [JsonProperty(PropertyName = "volume_traded")]
        public decimal VolumeTraded { get; set; }

        [JsonProperty(PropertyName = "trades_count")]
        public int TradesCount { get; set; }

    }
}
