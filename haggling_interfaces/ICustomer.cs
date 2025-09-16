namespace haggling_interfaces;

public interface ICustomer
{
    public string Name { get; init; }

    public int Age { get; init; }

    /// <summary>
    /// The customer's patience level expressed as a <see cref="Percentage"/>.
    /// This value can be used to determine how willing the customer is to
    /// continue haggling before accepting an offer or walking away.
    /// </summary>
    public Percentage Patience { get; set; }

    /// <summary>
    /// Allows the customer to choose a product from the specified vendor.
    /// </summary>
    /// <param name="vendor">
    /// The vendor from whom the product is to be chosen.
    /// To get the list of products, use the property <see cref="IVendor.Products"/>.
    /// </param>
    /// <returns>
    /// The <see cref="IProduct"/> selected by the customer.
    /// </returns>
    public IProduct ChooseProduct(IVendor vendor);

    /// <summary>
    /// Called when another party makes an offer to this customer.
    /// Implementations should examine the <paramref name="offer"/>, the
    /// initiating <paramref name="vendor"/> and return a modified
    /// counter-offer. To accept the offer, set the
    /// <see cref="IOffer.Status"/> to <see cref="OfferStatus.Accepted"/>.
    /// To stop the trade, set the <see cref="IOffer.Status"/> to
    /// <see cref="OfferStatus.Stopped"/>. To make a counter-offer,
    /// set (or keep) the <see cref="IOffer.Status"/> to <see cref="OfferStatus.Ongoing"/>
    /// and adjust the <see cref="IOffer.Price"/> accordingly. Also ensure to set
    /// the <see cref="IOffer.OfferedBy"/> to <see cref="PersonType.Customer"/>.
    /// </summary>
    /// <param name="offer">The incoming offer to respond to.</param>
    /// <param name="vendor">The vendor who made the offer.</param>
    /// <returns>
    /// An <see cref="IOffer"/> representing this customer's response as a counter-offer.
    /// </returns>
    public IOffer RespondToOffer(IOffer offer, IVendor vendor);

    /// <summary>
    /// Finalize and accept the agreed trade represented by <paramref name="offer"/>.
    /// Implementations should perform any state changes necessary to complete
    /// the transaction between this customer and vendor. e.g.: updating inventory,
    /// adjusting balances, resetting the patience, etc.
    /// </summary>
    /// <param name="offer">The offer that has been accepted.</param>
    public void AcceptTrade(IOffer offer);

    /// <summary>
    /// Stop the current trade negotiation with the vendor.
    /// Implementations should reset any state related to the trade.
    /// </summary>
    public void StopTrade();
}

public interface ICustomerFactory
{
    public static abstract ICustomer CreateCustomer(string name, int age);
}
