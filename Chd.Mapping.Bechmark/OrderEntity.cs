namespace Chd.Mapping.Bechmark
{

    public partial class OrderEntity
    {
        public decimal Price { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal NetTotal { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string StatusText { get; set; }
        public bool IsActive { get; set; }
    }
}
