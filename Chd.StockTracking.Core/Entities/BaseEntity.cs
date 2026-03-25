namespace Chd.StockTracking.Core.Entities;

public abstract class BaseEntity
{
    public long Id { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
}
