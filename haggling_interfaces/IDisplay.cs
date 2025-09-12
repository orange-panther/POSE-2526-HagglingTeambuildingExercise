namespace haggling_interfaces;

public interface IDisplay
{
    /// <summary>
    /// Render the provided list of <see cref="IProduct"/>s for the given
    /// <see cref="IVendor"/> and <see cref="ICustomer"/>. Implementations
    /// decide how products are presented to the customer (console, GUI, logs, etc.).
    /// </summary>
    /// <param name="products">The products available from the vendor.</param>
    /// <param name="vendor">The vendor offering the products.</param>
    /// <param name="customer">The customer viewing the products.</param>
    public void ShowProducts(IProduct[] products, IVendor vendor, ICustomer customer);

    /// <summary>
    /// Display the current <see cref="IOffer"/> in the context of a negotiation
    /// between the specified <see cref="IVendor"/> and <see cref="ICustomer"/>.
    /// Implementations should present the offer details and any relevant state
    /// (e.g.: who made the offer, price, product) to the user or system.
    /// </summary>
    /// <param name="offer">The offer to display.</param>
    /// <param name="vendor">The vendor participating in the negotiation.</param>
    /// <param name="customer">The customer participating in the negotiation.</param>
    public void ShowOffer(IOffer offer, IVendor vendor, ICustomer customer);
}
