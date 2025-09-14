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
       //var myOffer = CreateOffer();  
       //return myOffer;
   }


    public IOffer RespondToOffer(IOffer offer, IVendor vendor)
    {
        throw new NotImplementedException();
    }

    public void StopTrade()
    {
        throw new NotImplementedException();
    }

    private IProduct? DecideOnProduct(IVendor vendor)
    {

        var availableProducts = vendor.Products
            .Where(p => !Inventory.Any(i => i.Name == p.Name))
            .ToList();

        var mustHave = availableProducts
            .FirstOrDefault(p => Musthaves != null && Musthaves.Any(m => m.Name == p.Name));
        if (mustHave != null)
        {
            LastVendorOffer = vendor.GetStartingOffer(mustHave, this);
            return mustHave;
        }


        var liked = availableProducts
            .FirstOrDefault(p => Likes.Any(l => l.Name == p.Name));
        if (liked != null)
        {
            LastVendorOffer = vendor.GetStartingOffer(liked, this);
            return liked;
        }

    
        var neutral = availableProducts
            .Where(p => !Likes.Any(l => l.Name == p.Name) &&
                        !Dislikes.Any(d => d.Name == p.Name))
            .FirstOrDefault();
        if (neutral != null)
        {
            LastVendorOffer = vendor.GetStartingOffer(neutral, this);
            return neutral;
        }
        
        return null;
    }


}

