using System.ComponentModel.DataAnnotations;

namespace SampleAPI.Entities
{
    public class Order
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime EntryDate { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public bool IsInvoiced { get; set; }
        public bool IsDeleted { get; set; }
    }
}
