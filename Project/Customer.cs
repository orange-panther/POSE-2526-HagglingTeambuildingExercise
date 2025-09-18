namespace haggling_interfaces;

using System.Reflection;

public class Customer : ICustomer
{
    public string Name { get; init; } = string.Empty;
    public int Age { get; init; }
    public Percentage Patience { get; set; } = 100;

    protected decimal Budget { get; set; }
    protected List<ProductType> Likes { get; set; } = new();
    protected List<ProductType> Dislikes { get; set; } = new();
    protected List<ProductType>? MustHaves { get; set; } = new();
    protected Percentage Elasticity { get; set; } = 50;
    protected List<IProduct> Inventory { get; set; } = new();
    protected IOffer? LastVendorOffer { get; set; }
    protected IOffer? LastCustomerOffer { get; set; }

    protected const decimal MustHaveAcceptThreshold = 0.80m;
    protected const decimal LikeAcceptThreshold = 0.70m;
    protected const int LowPatienceThreshold = 30;
    protected const decimal MinCounterPrice = 0.01m;

    protected int CounterOffersMade { get; set; } = 0;
    protected const int MinCountersBeforeAccept = 2;
    protected const decimal MinMeaningfulOffer = 5m;
    protected const decimal CloseEnoughTolerance = 0.05m;

    protected virtual decimal R2(decimal x) => Math.Round(x, 2, MidpointRounding.AwayFromZero);

    public void AcceptTrade(IOffer offer)
    {
        if (offer == null) throw new ArgumentNullException(nameof(offer));
        if (Budget < offer.Price)
        {
            Console.WriteLine($"{Name} kann das Angebot nicht akzeptieren – Budget zu klein.");
            return;
        }

        if (LastVendorOffer != null && R2(offer.Price) > R2(LastVendorOffer.Price))
        {
            StopTrade();
            return;
        }

        Inventory.Add(offer.Product);
        Budget -= offer.Price;
        LastVendorOffer = offer;
        LastCustomerOffer = offer;
        Patience = 100;
        CounterOffersMade = 0;

        Console.WriteLine($"{Name} akzeptiert den Handel: {offer.Product.Name} für {R2(offer.Price)}.");
    }

    public IProduct ChooseProduct(IVendor vendor)
    {
        if (vendor == null) throw new ArgumentNullException(nameof(vendor));

        var product = DecideOnProduct(vendor);
        if (product != null) return product;
        else throw new InvalidOperationException($"{Name} findet kein Produkt, das gekauft werden kann.");
    }

    protected decimal MinMeaningfulOfferFor(decimal vendorPrice)
    {
        return Math.Max(0.50m, R2(vendorPrice * 0.10m));
    }

    public IOffer RespondToOffer(IOffer offer, IVendor vendor)
    {
        if (offer == null) throw new ArgumentNullException(nameof(offer));
        if (vendor == null) throw new ArgumentNullException(nameof(vendor));

        UpdatePatience(offer);
        LastVendorOffer = offer;

        var decision = EvaluateOfferDecision(offer);

        switch (decision)
        {
            case OfferDecision.Accept:
                offer.Status = OfferStatus.Accepted;
                offer.OfferedBy = PersonType.Customer;
                AcceptTrade(offer);
                return offer;
            case OfferDecision.Decline:
                offer.Status = OfferStatus.Stopped;
                offer.OfferedBy = PersonType.Customer;
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
        if (Patience == 0) return OfferDecision.Decline;

        bool likes = Likes.Contains(offer.Product.Type);
        bool mustHave = MustHaves != null && MustHaves.Contains(offer.Product.Type);

        if (((offer.Price > Budget || Patience <= 0 ) && CounterOffersMade >= MinCountersBeforeAccept)
        || LastVendorOffer != null && offer.Price > LastVendorOffer.Price)
            return OfferDecision.Decline;

        if (LastCustomerOffer != null && CounterOffersMade >= MinCountersBeforeAccept)
        {
            var gap = Math.Abs(offer.Price - LastCustomerOffer.Price);
            var rel = LastCustomerOffer.Price > 0 ? (gap / LastCustomerOffer.Price) : 1.0m;
            if (rel <= CloseEnoughTolerance) return OfferDecision.Accept;
        }

        if (CounterOffersMade < MinCountersBeforeAccept)
            return OfferDecision.Counter;

        decimal valueFactor = mustHave ? 0.80m : (likes ? 0.75m : 0.70m);
        bool valueDeal = offer.Price <= (LastVendorOffer?.Price ?? offer.Price) * valueFactor;

        return OfferDecision.Counter;
    }

    public void StopTrade()
    {
        LastVendorOffer = null;
        LastCustomerOffer = null;
        Patience = 100;
        CounterOffersMade = 0;
        Console.WriteLine($"{Name} hat die Verhandlung abgebrochen.");
    }

    protected virtual IProduct? DecideOnProduct(IVendor vendor)
    {
        if (vendor.Products == null) return null;

        var availableProducts = vendor.Products
            .Where(p => !Inventory.Any(i => i.Type == p?.Type))
            .ToList();

        IProduct? FirstAffordable(IEnumerable<IProduct> seq)
        {
            foreach (var p in seq)
            {
                var start = vendor.GetStartingOffer(p, this);
                if (start.Price <= Budget)
                {
                    LastVendorOffer = start;
                    return p;
                }
            }
            return null;
        }

        var mustHave = MustHaves != null ? availableProducts.Where(p => MustHaves.Contains(p.Type)) : Enumerable.Empty<IProduct>();
        var liked = availableProducts.Where(p => Likes.Contains(p.Type));
        var neutral = availableProducts.Where(p => !Likes.Contains(p.Type) && !Dislikes.Contains(p.Type));

        var chosen = (FirstAffordable(mustHave)
                ?? FirstAffordable(liked)
                ?? FirstAffordable(neutral))
                ?? null;
        return chosen;
    }

    protected virtual IOffer CreateOffer(IProduct product)
    {
        if (product == null) throw new ArgumentNullException(nameof(product));
        if (LastVendorOffer == null) throw new InvalidOperationException("Vendor must make the first offer.");

        var offer = new CustomerOffer
        {
            Product = product,
            OfferedBy = PersonType.Customer,
            Status = OfferStatus.Ongoing,
            
        };

        decimal vendor = LastVendorOffer.Price;

        if (LastCustomerOffer == null)
        {
            decimal basePct = 0.75m;
            if (MustHaves != null && MustHaves.Contains(product.Type)) basePct = 0.85m;
            else if (Likes.Contains(product.Type)) basePct = 0.80m;

            decimal first = R2(vendor * basePct);

            decimal fiveUnder = R2(vendor * 0.95m);
            if (first > fiveUnder) first = fiveUnder;

            decimal minOffer = MinMeaningfulOfferFor(vendor);
            first = Math.Min(first, Budget);
            first = Math.Max(first, minOffer);

            offer.Price = R2(first);
            CounterOffersMade++;
            return offer;
        }

        decimal prev = LastCustomerOffer.Price;

        decimal elasticityFactor = (decimal)Elasticity / 100m;
        decimal patienceFactor = 1 - (decimal)Patience / 100m;
        decimal stepFactor = 0.15m + 0.25m * elasticityFactor + 0.20m * patienceFactor;

        if (stepFactor < 0.15m) stepFactor = 0.15m;
        if (stepFactor > 0.55m) stepFactor = 0.55m;

        decimal targetCeil = R2(vendor * 0.95m);

        decimal gap = targetCeil - prev;
        decimal next = prev + gap * stepFactor;

        if (next < prev) next = prev;
        if (next > targetCeil) next = targetCeil;
        next = Math.Min(next, Budget);

        decimal minOffer2 = MinMeaningfulOfferFor(vendor);
        if (next < minOffer2) next = minOffer2;

        offer.Price = R2(next);
        CounterOffersMade++;
        return offer;
    }


    protected virtual void UpdatePatience(IOffer newVendorOffer)
    {
        if (LastVendorOffer == null)
        {
            int initDrop = Random.Shared.Next(4, 20);
            Patience = Math.Clamp((int)Patience - initDrop, 0, 100);
            return;
        }

        var prev = LastVendorOffer.Price;
        var delta = Math.Abs(newVendorOffer.Price - prev);
        var pct = prev > 0 ? (double)(delta / prev) : 1.0;

        int baseReduction = pct < 0.01 ? Random.Shared.Next(2, 6) : Random.Shared.Next(6, 12);

        double elasticityMultiplier = 1.0 - Math.Min((int)Elasticity, 100) / 200.0;
        if (elasticityMultiplier < 0.1) elasticityMultiplier = 0.1;

        int patienceReduction = (int)Math.Round(baseReduction * elasticityMultiplier);
        Patience = Math.Clamp((int)Patience - patienceReduction, 0, 100);
    }
}

public class CustomerFactory : ICustomerFactory
{
    public static ICustomer CreateCustomer(string name, int age)
    {
        var kind = Random.Shared.Next(0, 3); // 0=Standard, 1=RarityHunter, 2=BudgetGuardian
        Customer c = kind switch
        {
            1 => new RarityHunterCustomer { Name = name, Age = age },
            2 => new BudgetGuardianCustomer { Name = name, Age = age },
            _ => new Customer { Name = name, Age = age }
        };

        SetBudget(c, Random.Shared.Next(3000, 10001)); // 3.000–10.000
        SetElasticity(c, Random.Shared.Next(20, 81)); // 20–80

        var types = Enum.GetValues<ProductType>().ToList();
        Shuffle(types);

        var mustHaves = new List<ProductType> { types[0] };
        var likes = new List<ProductType> { types[1], types[2] };
        var dislikes = new List<ProductType> { types[3] };

        SetMustHaves(c, mustHaves);
        SetLikes(c, likes);
        SetDislikes(c, dislikes);

        return c;
    }

    private static void SetBudget(Customer c, decimal value) =>
        c.GetType().GetProperty("Budget", BindingFlags.Instance | BindingFlags.NonPublic)!
         .SetValue(c, value);

    private static void SetLikes(Customer c, List<ProductType> value) =>
        c.GetType().GetProperty("Likes", BindingFlags.Instance | BindingFlags.NonPublic)!
         .SetValue(c, value);

    private static void SetDislikes(Customer c, List<ProductType> value) =>
        c.GetType().GetProperty("Dislikes", BindingFlags.Instance | BindingFlags.NonPublic)!
         .SetValue(c, value);

    private static void SetMustHaves(Customer c, List<ProductType> value) =>
        c.GetType().GetProperty("MustHaves", BindingFlags.Instance | BindingFlags.NonPublic)!
         .SetValue(c, value);

    private static void SetElasticity(Customer c, int percentageAsInt) =>
        c.GetType().GetProperty("Elasticity", BindingFlags.Instance | BindingFlags.NonPublic)!
         .SetValue(c, (Percentage)percentageAsInt);

    private static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Shared.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}