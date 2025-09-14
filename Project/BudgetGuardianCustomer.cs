namespace haggling_interfaces;

public class BudgetGuardianCustomer : Customer
{
    protected override OfferDecision EvaluateOfferDecision(IOffer offer)
    {
        if (Patience == 0) return OfferDecision.Decline;

        static bool Same(string a, string b) =>
            string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

        bool likes     = Likes.Any(l => Same(l.Name, offer.Product.Name));
        bool mustHave  = (MustHaves ?? new()).Any(m => Same(m.Name, offer.Product.Name));
        bool dislikes  = Dislikes.Any(d => Same(d.Name, offer.Product.Name));

        const decimal MustHaveTight = MustHaveAcceptThreshold - 0.10m;
        const decimal LikeTight     = LikeAcceptThreshold     - 0.10m;

        if (mustHave && Budget >= offer.Price)
            return offer.Price <= Budget * MustHaveTight ? OfferDecision.Accept : OfferDecision.Counter;

        if (likes && offer.Price <= Budget * LikeTight)
            return OfferDecision.Accept;

        if (offer.Price > Budget * 0.90m)
            return OfferDecision.Decline;

        if (likes && Budget >= offer.Price)
            return OfferDecision.Counter;

        if (dislikes || offer.Price > Budget || Patience < LowPatienceThreshold)
            return OfferDecision.Decline;

        return OfferDecision.Counter;
    }

    protected override IOffer CreateOffer(IProduct product)
    {
        var offer = base.CreateOffer(product);

        if (LastCustomerOffer == null)
        {
            var lowered = offer.Price * 0.95m;
            offer.Price = lowered >= MinCounterPrice ? lowered : MinCounterPrice;
            return offer;
        }

        if (LastVendorOffer != null && LastCustomerOffer != null)
        {
            double extraConcessionDown = 0.07; 
            double step = ((double)LastVendorOffer.Price - (double)LastCustomerOffer.Price) * extraConcessionDown;

            var next = (decimal)((double)offer.Price - step); 
            
            next = Math.Min(next, Budget);
            offer.Price = next >= MinCounterPrice ? next : MinCounterPrice;
        }

        return offer;
    }

    protected override void UpdatePatience(IOffer newVendorOffer)
    {
        // Budgettreu: reagiert gelassener auf Preisänderungen
        base.UpdatePatience(newVendorOffer);
        // Reduziere den Abzug leicht um 20% (Clamp inkl.)
        // (Patience ist Percentage -> implizit int)
        int recover = (int)Math.Round((100 - (int)Patience) * 0.20); // kleiner „Rückfedern“-Effekt
        Patience = Math.Clamp((int)Patience + recover, 0, 100);
    }
}
