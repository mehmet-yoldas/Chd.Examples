namespace Chd.Mapping.Bechmark
{

    public partial class OrderEntity
    {
        public decimal Price { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal NetTotal { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string StatusText { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
