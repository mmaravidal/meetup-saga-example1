using System.Collections.Generic;

namespace Example2.Events.Models
{
    public class CreateOrderRequest
    {
        public string Name { get; set; }
        public Dictionary<string, int> Items { get; set; }
        public string Id { get; internal set; }
    }
}
