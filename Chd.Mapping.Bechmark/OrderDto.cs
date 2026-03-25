using Chd.Mapping.Abstractions;

namespace Chd.Mapping.Bechmark
{

    [MapTo(typeof(OrderEntity))]
    public partial class OrderDto
    {
        public decimal Price { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }

        [MapProperty("Price*(Tax+100)/100 - Discount")]
        public decimal NetTotal { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;

        [MapProperty("Name + ' ' + Surname")]
        public string FullName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        [MapProperty("IsActive ? 'Active' : 'Passive'")]
        public string StatusText { get; set; } = string.Empty;
    }
}
