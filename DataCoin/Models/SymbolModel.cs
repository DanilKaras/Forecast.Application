using Newtonsoft.Json;

namespace DataCoin.Models
{
    public class SymbolModel
    {    
        [JsonProperty(PropertyName = "symbol_id")]
        public string SymbolId { get; set; }

        [JsonProperty(PropertyName = "exchange_id")]
        public string ExchangeId { get; set; } 
        
        [JsonProperty(PropertyName = "symbol_type")]
        public string SymbolType { get; set; } 
        
        [JsonProperty(PropertyName = "asset_id_base")]
        public string AssetIdBase { get; set; } 
        
        [JsonProperty(PropertyName = "asset_id_quote")]
        public string AssetIdQuote { get; set; } 
    }
    
}