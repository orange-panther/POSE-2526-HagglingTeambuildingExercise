namespace haggling_interfaces;

using System.Reflection;
public class Customer : ICustomer
{
    public string Name { get; init; } = string.Empty;
    public int Age { get; init; }
    public Percentage Patience { get; set; } = 100;

    protected decimal Budget { get; set; }                                   // Budget des Kunden
    protected List<ProductType> Likes { get; set; } = new();                    // Produkte, die der Kunde mag
    protected List<ProductType> Dislikes { get; set; } = new();                 // Produkte, die der Kunde nicht mag
    protected List<ProductType>? MustHaves { get; set; } = new();               // Produkte, die der Kunde unbedingt will
    protected Percentage Elasticity { get; set; } = 50;                      // Preisempfindlichkeit (0–100)
    protected List<IProduct> Inventory { get; set; } = new();                // Bereits gekaufte Items
    protected IOffer? LastVendorOffer { get; set; }                          // letztes Angebot des Vendors
    protected IOffer? LastCustomerOffer { get; set; }                        // letztes Gegenangebot des Customers

    protected const decimal MustHaveAcceptThreshold = 0.80m;                 // 80% des Budgets
    protected const decimal LikeAcceptThreshold = 0.70m;                 // 70% des Budgets
    protected const int LowPatienceThreshold = 30;                    // <30% Geduld → eher Abbruch
    protected const decimal MinCounterPrice = 0.01m;                 // Gegenangebote min. 1 Cent

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

        bool likesProduct    = Likes.Contains(offer.Product.Type);
        bool mustHaveProduct = MustHaves.Contains(offer.Product.Type);
        bool dislikesProduct = Dislikes.Contains(offer.Product.Type);

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

    var availableProducts = vendor.Products
        .Where(p => !Inventory.Any(i => i.Type == p.Type))
        .ToList();

    var mustHave = availableProducts
        .FirstOrDefault(p => MustHaves != null && MustHaves.Contains(p.Type));
    if (mustHave != null)
    {
        LastVendorOffer = vendor.GetStartingOffer(mustHave, this);
        return mustHave;
    }

    var liked = availableProducts
        .FirstOrDefault(p => Likes.Contains(p.Type));
    if (liked != null)
    {
        LastVendorOffer = vendor.GetStartingOffer(liked, this);
        return liked;
    }

    var neutral = availableProducts
        .FirstOrDefault(p => !Likes.Contains(p.Type) &&
                             !Dislikes.Contains(p.Type));
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

        SetBudget(c, Random.Shared.Next(1500, 2501));
        SetElasticity(c, Random.Shared.Next(20, 81)); // 20–80

        var types = Enum.GetValues<ProductType>().ToList();
        Shuffle(types);

        var mustHaves = new List<ProductType> { types[0] };
        var likes     = new List<ProductType> { types[1], types[2] };
        var dislikes  = new List<ProductType> { types[3] };

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