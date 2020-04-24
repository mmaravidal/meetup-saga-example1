using System.Collections.Generic;

namespace Example2.Http.Models
{
    public class CreateOrderResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Dictionary<string, int> Items { get; set; }
    }
}
