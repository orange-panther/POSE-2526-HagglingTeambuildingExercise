namespace haggling_interfaces;

public class BudgetGuardianCustomer : Customer
{

    private int StubbornRounds { get; set; } = Random.Shared.Next(3, 6);

    protected override OfferDecision EvaluateOfferDecision(IOffer offer)
    {
        if (Patience == 0) return OfferDecision.Decline;

        bool likes    = Likes.Contains(offer.Product.Type);
        bool mustHave = MustHaves != null && MustHaves.Contains(offer.Product.Type);
        bool dislikes = Dislikes.Contains(offer.Product.Type);

        const decimal MustHaveTight = MustHaveAcceptThreshold - 0.10m; // 70%
        const decimal LikeTight     = LikeAcceptThreshold     - 0.10m; // 60%

        StubbornRounds = Math.Max(0, StubbornRounds - 1);

        if (mustHave && Budget >= offer.Price)
            return offer.Price <= Budget * MustHaveTight ? OfferDecision.Accept : OfferDecision.Counter;

        if (likes && offer.Price <= Budget * LikeTight)
            return OfferDecision.Accept;

        if (likes && Budget >= offer.Price)
            return OfferDecision.Counter;

        if (StubbornRounds <= 0 && (offer.Price > Budget * 0.90m || dislikes) || Patience < LowPatienceThreshold)
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
        base.UpdatePatience(newVendorOffer);

        int recover = (int)Math.Round((100 - (int)Patience) * 0.20);
        Patience = Math.Clamp((int)Patience + recover, 0, 100);
    }
}
