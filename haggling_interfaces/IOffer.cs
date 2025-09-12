namespace haggling_interfaces;

public interface IOffer
{
    public OfferStatus Status { get; set; }
    public IProduct Product { get; set; }
    public decimal Price { get; set; }
    public PersonType OfferedBy { get; set; }
}

public enum PersonType
{
    Customer,
    Vendor
}

public enum OfferStatus
{
    /// <summary>
    /// The offer has been accepted and the trade should complete.
    /// </summary>
    Accepted,

    /// <summary>
    /// The negotiation was stopped and no trade will occur.
    /// </summary>
    Stopped,

    /// <summary>
    /// The negotiation is ongoing; parties may make counter-offers.
    /// </summary>
    Ongoing
}
