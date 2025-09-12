namespace haggling_interfaces;

public interface IVendor
{
    public string Name { get; init; }
    public int Age { get; init; }

    /// <summary>
    /// The vendor's patience level as a <see cref="Percentage"/>. This can be
    /// used to determine how many counter-offers the vendor will make before
    /// accepting or stopping negotiations.
    /// </summary>
    public Percentage Patience { get; set; }

    public IProduct[] Products { get; init; }

    /// <summary>
    /// Create a starting <see cref="IOffer"/> for the specified
    /// <paramref name="product"/> when interacting with a <paramref name="customer"/>.
    /// Implementations should set an initial <see cref="IOffer.Price"/> and the
    /// <see cref="IOffer.OfferedBy"/> accordingly. The offer's status should be
    /// set to <see cref="OfferStatus.Ongoing"/> to indicate that negotiations
    /// are in progress.
    /// </summary>
    /// <param name="product">The product for which to create a starting offer.</param>
    /// <param name="customer">The customer the offer is for.</param>
    /// <returns>A new <see cref="IOffer"/> representing the vendor's opening position.</returns>
    public IOffer GetStartingOffer(IProduct product, ICustomer customer);

    /// <summary>
    /// Called when another party makes an offer to this vendor.
    /// Implementations should examine the <paramref name="offer"/>, the
    /// initiating <paramref name="customer"/> and return a modified
    /// counter-offer. To accept the offer, set the
    /// <see cref="IOffer.Status"/> to <see cref="OfferStatus.Accepted"/>.
    /// To stop the trade, set the <see cref="IOffer.Status"/> to
    /// <see cref="OfferStatus.Stopped"/>. To make a counter-offer,
    /// set (or keep) the <see cref="IOffer.Status"/> to <see cref="OfferStatus.Ongoing"/>
    /// and adjust the <see cref="IOffer.Price"/> accordingly. Also ensure to set
    /// the <see cref="IOffer.OfferedBy"/> to <see cref="PersonType.Vendor"/>.
    /// </summary>
    /// <param name="offer">The incoming offer to respond to.</param>
    /// <param name="customer">The customer who made the offer.</param>
    /// <returns>
    /// An <see cref="IOffer"/> representing this vendor's response as a counter-offer.
    /// </returns>
    public IOffer RespondToOffer(IOffer offer, ICustomer customer);

    /// <summary>
    /// Finalize and accept the agreed trade represented by <paramref name="offer"/>.
    /// Implementations should perform any state changes necessary to complete
    /// the transaction between this customer and vendor. e.g.: updating inventory,
    /// adjusting balances, resetting the patience, etc.
    /// </summary>
    /// <param name="offer">The offer that has been accepted.</param>
    public void AcceptTrade(IOffer offer);

    /// <summary>
    /// Stop the current trade negotiation with the customer.
    /// Implementations should reset any state related to the trade.
    /// </summary>
    public void StopTrade();
}
