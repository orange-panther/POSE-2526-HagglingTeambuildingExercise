namespace haggling_interfaces;

public class Customer : ICustomer
{
    public string Name { get; init; } = string.Empty;
    public int Age { get; init; }
    public Percentage Patience { get; set; } = 100;

    protected decimal Budget { get; set; }                                   // Budget des Kunden
    protected List<IProduct> Likes { get; set; } = new();                    // Produkte, die der Kunde mag
    protected List<IProduct> Dislikes { get; set; } = new();                 // Produkte, die der Kunde nicht mag
    protected List<IProduct>? MustHaves { get; set; } = new();               // Produkte, die der Kunde unbedingt will
    protected Percentage Elasticity { get; set; } = 50;                      // Preisempfindlichkeit (0–100)
    protected List<IProduct> Inventory { get; set; } = new();                // Bereits gekaufte Items
    protected IOffer? LastVendorOffer { get; set; }                          // letztes Angebot des Vendors
    protected IOffer? LastCustomerOffer { get; set; }                        // letztes Gegenangebot des Customers

    protected const decimal MustHaveAcceptThreshold = 0.80m;                 // 80% des Budgets
    protected const decimal LikeAcceptThreshold     = 0.70m;                 // 70% des Budgets
    protected const int     LowPatienceThreshold    = 30;                    // <30% Geduld → eher Abbruch
    protected const decimal MinCounterPrice         = 0.01m;                 // Gegenangebote min. 1 Cent

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

        LastVendorOffer = offer;
        LastCustomerOffer = offer;

        Patience = 100;

        Console.WriteLine($"{Name} akzeptiert den Handel: {offer.Product.Name} für {offer.Price}.");
    }

    public IProduct ChooseProduct(IVendor vendor)
    {
        if (vendor == null) throw new ArgumentNullException(nameof(vendor));

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
            throw new ArgumentNullException(nameof(offer));
        if (vendor == null)
            throw new ArgumentNullException(nameof(vendor));

        UpdatePatience(offer);

        LastVendorOffer = offer;

        var decision = EvaluateOfferDecision(offer);

        switch (decision)
        {
            case OfferDecision.Accept:
                offer.Status = OfferStatus.Accepted;
                AcceptTrade(offer);
                return offer;

            case OfferDecision.Decline:
                offer.Status = OfferStatus.Stopped;
                StopTrade();
                return offer;

            case OfferDecision.Counter:
                var counterOffer = CreateOffer(offer.Product);
                LastCustomerOffer = counterOffer;
                return counterOffer;

            default:
                throw new InvalidOperationException("Unbekannte Angebotsentscheidung.");
        }
    }

    protected enum OfferDecision { Accept, Decline, Counter }

    protected virtual OfferDecision EvaluateOfferDecision(IOffer offer)
    {
        if (Patience == 0)
        {
            return OfferDecision.Decline;
        }

        static bool SameName(string a, string b) =>
            string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

        bool likesProduct     = Likes.Any(l => SameName(l.Name, offer.Product.Name));
        bool mustHaveProduct  = MustHaves != null && MustHaves.Any(m => SameName(m.Name, offer.Product.Name));
        bool dislikesProduct  = Dislikes.Any(d => SameName(d.Name, offer.Product.Name));

        if (mustHaveProduct && Budget >= offer.Price)
        {
            if (offer.Price <= Budget * MustHaveAcceptThreshold)
                return OfferDecision.Accept;
            else
                return OfferDecision.Counter;
        }

        if (likesProduct && offer.Price <= Budget * LikeAcceptThreshold)
            return OfferDecision.Accept;

        if (likesProduct && Budget >= offer.Price)
            return OfferDecision.Counter;

        if (dislikesProduct || offer.Price > Budget || Patience < LowPatienceThreshold)
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

    protected virtual IProduct? DecideOnProduct(IVendor vendor)
    {
        if (vendor.Products == null) return null;

        static bool SameName(string a, string b) =>
            string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

        var availableProducts = vendor.Products
            .Where(p => !Inventory.Any(i => SameName(i.Name, p.Name)))
            .ToList();

        var mustHave = availableProducts
            .FirstOrDefault(p => MustHaves != null && MustHaves.Any(m => SameName(m.Name, p.Name)));
        if (mustHave != null)
        {
            LastVendorOffer = vendor.GetStartingOffer(mustHave, this);
            return mustHave;
        }

        var liked = availableProducts
            .FirstOrDefault(p => Likes.Any(l => SameName(l.Name, p.Name)));
        if (liked != null)
        {
            LastVendorOffer = vendor.GetStartingOffer(liked, this);
            return liked;
        }

        var neutral = availableProducts
            .Where(p => !Likes.Any(l => SameName(l.Name, p.Name)) &&
                        !Dislikes.Any(d => SameName(d.Name, p.Name)))
            .FirstOrDefault();
        if (neutral != null)
        {
            LastVendorOffer = vendor.GetStartingOffer(neutral, this);
            return neutral;
        }

        return null;
    }

    protected virtual IOffer CreateOffer(IProduct product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));
        if (LastVendorOffer == null)
            throw new InvalidOperationException("Vendor must make the first offer.");

        IOffer newOffer = new CustomerOffer
        {
            Product = product,
            OfferedBy = PersonType.Customer,
            Status = OfferStatus.Ongoing
        };

        if (LastCustomerOffer == null)
        {
            double rarityWeight = Math.Clamp(product.Rarity.Value / 100.0, 0.0, 1.0);
            double startRatio = 0.3 + 0.5 * rarityWeight;
            double initialPrice = (double)LastVendorOffer.Price * startRatio;

            var price = Math.Min((decimal)initialPrice, Budget);
            newOffer.Price = price >= MinCounterPrice ? price : MinCounterPrice;
            return newOffer;
        }

        double baseConcession = 0.2; 
        double elasticityBump = Math.Min((int)Elasticity, 100) / 1000.0; 
        double concessionRate = Math.Clamp(baseConcession + elasticityBump, 0.05, 0.6);

        double nextPrice = (double)LastCustomerOffer.Price +
                           ((double)LastVendorOffer.Price - (double)LastCustomerOffer.Price) * concessionRate;

        var capped = Math.Min((decimal)nextPrice, Budget);
        newOffer.Price = capped >= MinCounterPrice ? capped : MinCounterPrice;

        return newOffer;
    }

    protected virtual void UpdatePatience(IOffer newVendorOffer)
    {
        if (LastVendorOffer == null)
        {
            int initDrop = Random.Shared.Next(0, 6);
            Patience = Math.Clamp((int)Patience - initDrop, 0, 100);
            return;
        }

        var prev = LastVendorOffer.Price;
        var delta = Math.Abs(newVendorOffer.Price - prev);
        var pct = prev > 0 ? (double)(delta / prev) : 1.0;

        int baseReduction = pct < 0.01
            ? Random.Shared.Next(0, 12)   
            : Random.Shared.Next(5, 25);  

        double elasticityMultiplier = 1.0 + Math.Min((int)Elasticity, 100) / 200.0; 
        int patienceReduction = (int)Math.Round(baseReduction * elasticityMultiplier);

        Patience = Math.Clamp((int)Patience - patienceReduction, 0, 100);
    }
}
