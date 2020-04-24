namespace Example2.Events.Models
{
    public class StockLeaseDeniedEvent
    {
        public string Id { get; set; }
        public string Item { get; set; }
        public int Quantity { get; set; }
    }
}
