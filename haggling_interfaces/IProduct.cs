namespace haggling_interfaces;

public interface IProduct
{
    public string Name { get; init; }

    public ProductType Type { get; init; }

    /// <summary>
    /// A measure of how rare the product is expressed as a <see cref="Percentage"/>.
    /// Implementations may use this to influence starting prices or customer interest.
    /// </summary>
    public Percentage Rarity { get; set; }
}

public enum ProductType
{
    Food,
    Electronics,
    Clothing,
    Furniture,
    Toys,
    Books,
    Tools,
    SportsEquipment,
    Jewelry,
    BeautyProducts
}
