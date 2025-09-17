namespace haggling_interfaces;

public class RarityHunterCustomer : Customer
{
    protected override IProduct? DecideOnProduct(IVendor vendor)
    {
        var all = vendor.Products?
            .Where(p => Inventory.All(i => i.Type != p.Type))
            .ToList() ?? new();

        if (all.Count == 0) return null;

        var mh = all
            .Where(p => MustHaves != null && MustHaves.Contains(p.Type))
            .OrderByDescending(p => p.Rarity.Value)
            .FirstOrDefault();
        if (mh != null) return mh;

        var liked = all
            .Where(p => Likes.Contains(p.Type))
            .OrderByDescending(p => p.Rarity.Value)
            .FirstOrDefault();
        if (liked != null) return liked;

        var neutral = all
            .Where(p => !Likes.Contains(p.Type) && !Dislikes.Contains(p.Type))
            .OrderByDescending(p => p.Rarity.Value)
            .FirstOrDefault();

        return neutral ?? all.FirstOrDefault();
    }

    protected override OfferDecision EvaluateOfferDecision(IOffer offer)
    {
        var baseDecision = base.EvaluateOfferDecision(offer);
        if (Patience == 0) return OfferDecision.Decline;

        var r = offer.Product.Rarity.Value; // 0..100

        if (baseDecision == OfferDecision.Counter && r >= 80)
        {
            var mustHave = (MustHaves ?? new()).Contains(offer.Product.Type);
            var liked    = Likes.Contains(offer.Product.Type);

            if (mustHave && offer.Price <= Budget * (MustHaveAcceptThreshold + 0.10m))
                return OfferDecision.Accept;

            if (liked && offer.Price <= Budget * (LikeAcceptThreshold + 0.10m))
                return OfferDecision.Accept;
        }

        return baseDecision;
    }

    protected override IOffer CreateOffer(IProduct product)
    {
        var offer = base.CreateOffer(product);

        if (LastVendorOffer != null && LastCustomerOffer != null)
        {
            var rarity = Math.Clamp(product.Rarity.Value / 100.0, 0.0, 1.0); // 0..1
            var extraConcession = 0.05 * rarity; 

            var target = (double)LastCustomerOffer.Price +
                        ((double)LastVendorOffer.Price - (double)LastCustomerOffer.Price) * extraConcession;

            var boosted = Math.Min((decimal)target + offer.Price - LastCustomerOffer.Price, Budget);
            offer.Price = boosted >= MinCounterPrice ? boosted : MinCounterPrice;
        }

        return offer;
    }
}