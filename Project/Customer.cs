namespace haggling_interfaces;

public class Customer : ICustomer
{
    public string Name { get; init; }
    public int Age { get; init; }
    public Percentage Patience { get; set; }

    public decimal Budget { get; set; } //Idk ob mit DezimalZahlen gehandelt wird aber zur Sicherheit
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

    public IProduct ChooseProduct(IVendor vendor)
    {
        var product = DecideOnProduct(vendor);
        if (product != null)
        {
            return product;
        }
        else
        {
            throw new InvalidOperationException($"{Name} findet kein Produkt, das gekauft werden kann.");
        }
    }


    public IOffer RespondToOffer(IOffer offer, IVendor vendor)
    {
        if (offer == null)
        {
            throw new ArgumentNullException(nameof(offer));
        }

        decimal priceChange = LastVendorOffer != null ? Math.Abs(offer.Price - LastVendorOffer.Price) : offer.Price;
        int patienceReduction = priceChange < 0.01m * (LastVendorOffer?.Price ?? offer.Price) ? Random.Shared.Next(5,25) : Random.Shared.Next(0,12);
        Patience = Math.Max(0, Patience - patienceReduction);

        LastVendorOffer = offer;

        var decision = EvaluateOfferDecision(offer);
        switch (decision)
        {
            case OfferDecision.Accept:
                AcceptTrade(offer);
                return offer;
            case OfferDecision.Decline:
                StopTrade();
                return null;
            case OfferDecision.Counter:
                var counterOffer = CreateOffer(offer.Product);
                LastCustomerOffer = counterOffer;
                return counterOffer;
            default:
                StopTrade();
                return null;
        }
    }
    private enum OfferDecision { Accept, Decline, Counter }

    private OfferDecision EvaluateOfferDecision(IOffer offer)
    {
        if (Patience == 0)
        {
            return OfferDecision.Decline;
        }

        bool likesProduct = Likes.Any(l => l.Name == offer.Product.Name);
        bool mustHaveProduct = Musthaves != null && Musthaves.Any(m => m.Name == offer.Product.Name);
        bool dislikesProduct = Dislikes.Any(d => d.Name == offer.Product.Name);

        if (mustHaveProduct && Budget >= offer.Price)
        {
            if (offer.Price <= Budget * 0.8m)
                return OfferDecision.Accept;
            else
                return OfferDecision.Counter;
        }

        if (likesProduct && offer.Price <= Budget * 0.7m)
            return OfferDecision.Accept;

        if (likesProduct && Budget >= offer.Price)
            return OfferDecision.Counter;

        if (dislikesProduct || offer.Price > Budget || Patience < 30)
            return OfferDecision.Decline;

        return OfferDecision.Counter;
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
        newOffer.Price = Math.Min((decimal)initialPrice, Budget);
        return newOffer;
    }

    // Otherwise: move closer to vendor’s offer
    double concessionRate = 0.2; // could later depend on patience/elasticity
    double nextPrice = (double) LastCustomerOffer.Price +
                      ( (double) LastVendorOffer.Price -  (double) LastCustomerOffer.Price) * concessionRate;

    // Cap at budget
    newOffer.Price = Math.Min((decimal)nextPrice, Budget);

    return newOffer;
}



}

