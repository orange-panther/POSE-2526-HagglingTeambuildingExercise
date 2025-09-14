namespace haggling_interfaces;

public class Customer : ICustomer
{
    public string Name { get; init; }
    public int Age { get; init; }
    public Percentage Patience { get; set; }

    public double Budget { get; set; } //Idk ob mit DezimalZahlen gehandelt wird aber zur Sicherheit
    public List<IProduct> Likes { get; set; } //Liste der Produkte die der Kunde mag
    public List<IProduct> Dislikes { get; set; } //Liste der Produkte die der Kunde nicht mag
    public List<IProduct>? Musthaves { get; set; } //Liste der Produkte die der Kunde besitzt
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
       var myOffer = CreateOffer();  
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

}

