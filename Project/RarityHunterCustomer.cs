namespace haggling_interfaces;

public class RarityHunterCustomer : Customer
{
    protected override IProduct? DecideOnProduct(IVendor vendor)
    {
        if (vendor.Products == null) return null;

        var pool = vendor.Products
            .Where(p => Inventory.All(i => i.Type != p.Type))
            .Where(p => !Dislikes.Contains(p.Type))
            .ToList();
        if (pool.Count == 0) return null;

        IProduct? PickAffordable(IEnumerable<IProduct> seq)
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

        var must = pool
            .Where(p => MustHaves != null && MustHaves.Contains(p.Type))
            .OrderByDescending(p => p.Rarity.Value);

        var liked = pool
            .Where(p => Likes.Contains(p.Type))
            .OrderByDescending(p => p.Rarity.Value);

        var neutral = pool
            .Where(p => !Likes.Contains(p.Type) && (MustHaves == null || !MustHaves.Contains(p.Type)))
            .OrderByDescending(p => p.Rarity.Value);

        var chosen = PickAffordable(must)
                  ?? PickAffordable(liked)
                  ?? PickAffordable(neutral);

        if (chosen == null)
        {
            StopTrade();
            return null;
        }

        return chosen;
    }

    protected override OfferDecision EvaluateOfferDecision(IOffer offer)
    {
        var baseDecision = base.EvaluateOfferDecision(offer);
        if (Patience == 0) return OfferDecision.Decline;

        var rarity = offer.Product.Rarity.Value; 
        bool must  = (MustHaves ?? new()).Contains(offer.Product.Type);
        bool like  = Likes.Contains(offer.Product.Type);

        if (baseDecision == OfferDecision.Counter && rarity >= 90)
        {
            if (must && offer.Price <= Budget * 0.95m) return OfferDecision.Accept;
            if (like && offer.Price <= Budget * 0.90m) return OfferDecision.Accept;
        }

        return baseDecision;
    }

    protected override IOffer CreateOffer(IProduct product)
    {
        var offer = base.CreateOffer(product);

        if (LastVendorOffer == null) return offer;

        var r = Math.Clamp(product.Rarity.Value, 0, 100);
        decimal cushionPct = 0.05m + ((50 - r) / 100m) * 0.10m; 
        if (cushionPct < 0m)  cushionPct = 0m;
        if (cushionPct > 0.10m) cushionPct = 0.10m;

        decimal vendor = LastVendorOffer.Price;
        decimal cap    = R2(vendor * (1m - cushionPct)); 

        decimal price = offer.Price;
        if (price > cap) price = cap;

        decimal minOffer = MinMeaningfulOfferFor(vendor);
        if (price < minOffer) price = minOffer;
        if (price > Budget)   price = Budget;

        offer.Price = R2(price);
        return offer;
    }
}
