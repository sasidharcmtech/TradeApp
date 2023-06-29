using System.ComponentModel.DataAnnotations;

namespace TradeApp
{
    public class TradeModel
    {
        [Key]
        public string TradeID { get; set; }
        public string ISIN { get; set; }
        public string Notional { get; set; }
    }
}
