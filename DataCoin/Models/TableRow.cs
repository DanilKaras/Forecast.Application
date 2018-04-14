using Microsoft.Extensions.DependencyInjection;

namespace DataCoin.Models
{
    public class TableRow
    {
        public string ID { get; set; }
        public string DS { get; set; }
        public decimal Yhat { get; set; }
        public decimal YhatLower { get; set; }
        public decimal YhatUpper { get; set; }
        public decimal MaxVal { get; set; }
        public decimal MinVal { get; set; }
    } 
}