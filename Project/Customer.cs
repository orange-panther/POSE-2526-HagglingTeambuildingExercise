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
        if (offer == null)
        throw new ArgumentNullException(nameof(offer));

        if (Budget < offer.Price)
        {
            Console.WriteLine($"{Name} kann das Angebot nicht akzeptieren – Budget zu klein.");
            return;
        }

        Inventory.Add(offer.Product);
        Budget -= offer.Price;

        LastCustomerOffer = offer;
        LastVendorOffer = null;
        Patience = 100;

        Console.WriteLine($"{Name} akzeptiert den Handel: {offer.Product.Name} für {offer.Price}.");
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
        LastVendorOffer = null;
        LastCustomerOffer = null;
        Patience = 100;

        Console.WriteLine($"{Name} hat die Verhandlung abgebrochen.");
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


      private IOffer CreateOffer(IProduct product)
{
    if (LastVendorOffer == null)
        throw new InvalidOperationException("Vendor must make the first offer.");

    IOffer newOffer = new CustomerOffer ();

    // First customer offer: depends on rarity
    if (LastCustomerOffer == null)
    {
        double rarityWeight = product.Rarity.Value / 100.0; // 0.0 (common) → 1.0 (very rare)
        double startRatio = 0.3 + 0.5 * rarityWeight;       // 30%–80% of vendor price

        double initialPrice = (double) LastVendorOffer.Price * startRatio;

        // Cap at budget
        newOffer.Price = (decimal) Math.Min(initialPrice, Budget);
        return newOffer;
    }

    // Otherwise: move closer to vendor’s offer
    double concessionRate = 0.2; // could later depend on patience/elasticity
    double nextPrice = (double) LastCustomerOffer.Price +
                      ( (double) LastVendorOffer.Price -  (double) LastCustomerOffer.Price) * concessionRate;

    // Cap at budget
    newOffer.Price = (decimal) Math.Min(nextPrice, Budget);

    return newOffer;
}



}

