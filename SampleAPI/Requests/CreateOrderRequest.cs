using System.ComponentModel.DataAnnotations;

namespace SampleAPI.Requests
{
    public class CreateOrderRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsInvoiced { get; set; } = true;
    }
}
