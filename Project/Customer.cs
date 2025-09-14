namespace haggling_interfaces;

public class Customer : ICustomer
{
    public string Name { get; init; }
    public int Age { get; init; }
    public Percentage Patience { get; set; }

    public double Budget { get; set; } //Idk ob mit DezimalZahlen gehandelt wird aber zur Sicherheit
    public List<IProduct> Likes { get; set; } //Liste der Produkte die der Kunde mag
    public List<IProduct> Dislikes { get; set; } //Liste der Produkte die der Kunde nicht mag
    public List<IProduct>? Musthaves { get; set; } //Liste der Produkte die der Kunde unbedigt besitzen will
    public Percentage Elasticity { get; set; } //Wie stark der Kunde auf Preisänderungen reagiert
    public List<IProduct> Inventory { get; set; } // Liste für alle bereits gekauften Items
    public IOffer? LastVendorOffer { get; set; } //Letzte Offer an den Customer von dem Vendor (nullable weil Vendor noch keine Offer gemacht haben könnte)
    public IOffer? LastCustomerOffer { get; set; } // Letzte Offer vom Customer (nullable weil Kunde noch keine Offer gemacht haben könnte)

    public void AcceptTrade(IOffer offer)
    {
        throw new NotImplementedException();
    }

   public IOffer ChooseProduct(IVendor vendor)
   {
       var product = DecideOnProduct(vendor);
       var myOffer = CreateOffer(product);  
       return myOffer;
   }


    public IOffer RespondToOffer(IOffer offer, IVendor vendor)
    {
        throw new NotImplementedException();
    }

    public void StopTrade()
    {
        throw new NotImplementedException();
    }

  private IProduct DecideOnProduct(IVendor vendor)
      {

          var preferred = vendor.Products.FirstOrDefault(p => Likes.Any(l => l.Name == p.Name));
          var fallback = vendor.Products.OrderByDescending(p => p.Rarity.Value).First();
          var chosen = preferred ?? fallback;
          LastVendorOffer = vendor.GetStartingOffer(chosen, this);

          return chosen;
      }

      private IOffer CreateCounterOffer(IProduct product)
{
    if (LastVendorOffer == null)
        throw new InvalidOperationException("Vendor must make the first offer.");

    var newOffer = new Offer ();

    // First customer offer: depends on rarity
    if (LastCustomerOffer == null)
    {
        double rarityWeight = product.Rarity.Value / 100.0; // 0.0 (common) → 1.0 (very rare)
        double startRatio = 0.3 + 0.5 * rarityWeight;       // 30%–80% of vendor price

        double initialPrice = LastVendorOffer.Price * startRatio;

        // Cap at budget
        newOffer.Price = Math.Min(initialPrice, Budget);
        return newOffer;
    }

    // Otherwise: move closer to vendor’s offer
    double concessionRate = 0.2; // could later depend on patience/elasticity
    double nextPrice = LastCustomerOffer.Price +
                       (LastVendorOffer.Price - LastCustomerOffer.Price) * concessionRate;

    // Cap at budget
    newOffer.Price = Math.Min(nextPrice, Budget);

    return newOffer;
}



}

