using Chd.Mapping.Abstractions;
namespace ChdRoslynMappingTest
{
    // 1. Mark your DTO with [MapTo] and expression mapping
    [MapTo(typeof(OrderEntity))]
    public partial class OrderDto
    {
        public decimal Price { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }

        // Expression-based calculated property!
        [MapProperty("Price * (Tax + 100) / 100 - Discount")]
        public decimal NetTotal { get; set; }
    }

    // 2. Define your Entity (must be partial)
    public partial class OrderEntity
    {
        public decimal Price { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal NetTotal { get; set; }
    }
    // 3. Use implicit operators - that's it!
    internal class Program
    {
        static void Main(string[] args)
        {
            var dto = new OrderDto { Price = 100, Tax = 18, Discount = 2 };
            OrderEntity entity = dto;  // DTO → Entity with calculation!
            Console.WriteLine($"NetTotal: {entity.NetTotal}");  // Output: 116
        }
    }
}